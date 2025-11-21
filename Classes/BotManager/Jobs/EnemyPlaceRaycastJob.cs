using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components;

public class EnemyPlaceRaycastJob : BotManagerBase
{
    public struct CalcEnemyPlaceJob : IJobFor
    {
        [ReadOnly] public NativeArray<Vector3> PlacePositions;
        [ReadOnly] public NativeArray<Vector3> BotPositions;
        [ReadOnly] public NativeArray<Vector3> EnemyPositions;
        [WriteOnly] public NativeArray<float> PlaceDistancesToBot;
        [WriteOnly] public NativeArray<float> PlaceDistancesToEnemy;

        public void Execute(int index)
        {
            Vector3 EnemyPlace = PlacePositions[index];
            Vector3 BotPosition = BotPositions[index];
            Vector3 EnemyPosition = EnemyPositions[index];
            PlaceDistancesToBot[index] = (BotPosition - EnemyPlace).magnitude;
            PlaceDistancesToEnemy[index] = (EnemyPosition - EnemyPlace).magnitude;
        }

        public void Dispose()
        {
            if (PlacePositions.IsCreated) PlacePositions.Dispose();
            if (BotPositions.IsCreated) BotPositions.Dispose();
            if (EnemyPositions.IsCreated) EnemyPositions.Dispose();
            if (PlaceDistancesToBot.IsCreated) PlaceDistancesToBot.Dispose();
            if (PlaceDistancesToEnemy.IsCreated) PlaceDistancesToEnemy.Dispose();
        }
    }

    public EnemyPlaceRaycastJob(BotManagerComponent botcontroller) : base(botcontroller)
    {
        botcontroller.StartCoroutine(EnemyPlaceJobLoop());
    }

    private JobHandle EnemyPlaceJobHandle;
    private CalcEnemyPlaceJob EnemyPlaceJob;
    private readonly List<EnemyPlace> PlacesToCheck = new();

    private IEnumerator EnemyPlaceJobLoop()
    {
        yield return null;

        while (true)
        {
            if (BotController == null)
            {
                yield return null;
                continue;
            }

            var bots = BotController.BotSpawnController?.SAINBots;
            if (bots == null || bots.Count == 0)
            {
                yield return null;
                continue;
            }

            if (BotController.BotGame?.Status == EFT.GameStatus.Stopping)
            {
                yield return null;
                continue;
            }

            PlacesToCheck.Clear();
            foreach (BotComponent bot in bots)
            {
                if (bot?.BotActive == true)
                {
                    foreach (Enemy enemy in bot.EnemyController.EnemiesArray)
                    {
                        if (enemy?.EnemyKnown == true)
                        {
                            if (enemy.KnownPlaces.LastHeardPlace != null)
                                PlacesToCheck.Add(enemy.KnownPlaces.LastHeardPlace);
                            if (enemy.KnownPlaces.LastSeenPlace != null)
                                PlacesToCheck.Add(enemy.KnownPlaces.LastSeenPlace);
                            if (enemy.KnownPlaces.LastSquadHeardPlace != null)
                                PlacesToCheck.Add(enemy.KnownPlaces.LastSquadHeardPlace);
                            if (enemy.KnownPlaces.LastSquadSeenPlace != null)
                                PlacesToCheck.Add(enemy.KnownPlaces.LastSquadSeenPlace);
                        }
                    }
                }
            }
            int Count = PlacesToCheck.Count;
            if (Count == 0)
            {
                yield return null;
                continue;
            }

            NativeArray<Vector3> PlacePositions = new(Count, Allocator.TempJob);
            NativeArray<Vector3> BotPositions = new(Count, Allocator.TempJob);
            NativeArray<Vector3> EnemyPositions = new(Count, Allocator.TempJob);
            for (int i = 0; i < Count; i++)
            {
                EnemyPlace Place = PlacesToCheck[i];
                PlacePositions[i] = Place.Position;
                BotPositions[i] = Place.PlaceData.Owner.Transform.EyePosition;
                EnemyPositions[i] = Place.PlaceData.OwnerEnemy.EnemyTransform.Position;
            }

            EnemyPlaceJob = new CalcEnemyPlaceJob {
                PlacePositions = PlacePositions,
                BotPositions = BotPositions,
                EnemyPositions = EnemyPositions,
                PlaceDistancesToBot = new NativeArray<float>(Count, Allocator.TempJob),
                PlaceDistancesToEnemy = new NativeArray<float>(Count, Allocator.TempJob)
            };

            EnemyPlaceJobHandle = EnemyPlaceJob.Schedule(Count, new JobHandle());


            _commands = new NativeArray<RaycastCommand>(Count, Allocator.TempJob);
            _hits = new NativeArray<RaycastHit>(Count, Allocator.TempJob);

            for (int i = 0; i < Count; i++)
            {
                EnemyPlace Place = PlacesToCheck[i];
                Vector3 HeadPosition = Place.PlaceData.Owner.Transform.EyePosition;
                Vector3 PlacePosition = Place.Position + Vector3.up;
                _commands[i] = new RaycastCommand(HeadPosition, PlacePosition - HeadPosition, new QueryParameters {
                    layerMask = Mask
                }, 1f);
            }

            RaycastJobHandle = RaycastCommand.ScheduleBatch(_commands, _hits, 32);

            yield return null;

            var handle = RaycastJobHandle;
            if (!handle.IsCompleted) handle.Complete();
            RaycastJobHandle = handle;

            handle = EnemyPlaceJobHandle;
            if (!handle.IsCompleted) handle.Complete();
            EnemyPlaceJobHandle = handle;

            for (int i = 0; i < Count; i++)
            {
                EnemyPlace Place = PlacesToCheck[i];
                if (Place != null)
                {
                    RaycastHit Hit = _hits[i];
                    Place.SetDistances(EnemyPlaceJob.PlaceDistancesToBot[i], EnemyPlaceJob.PlaceDistancesToEnemy[i], Place.PlaceData.Owner);
                    Place.SetVisibilityOfPlace(Hit.collider == null, Place.PlaceData.Owner);
                }
            }

            PlacesToCheck.Clear();
            EnemyPlaceJob.Dispose();
            _commands.Dispose();
            _hits.Dispose();
        }
    }

    public void Dispose()
    {
        if (!RaycastJobHandle.IsCompleted) RaycastJobHandle.Complete();
        if (!EnemyPlaceJobHandle.IsCompleted) EnemyPlaceJobHandle.Complete();
        EnemyPlaceJob.Dispose();
        if (_commands.IsCreated) _commands.Dispose();
        if (_hits.IsCreated) _hits.Dispose();
    }

    private NativeArray<RaycastHit> _hits;
    private NativeArray<RaycastCommand> _commands;
    private JobHandle RaycastJobHandle;

    private readonly LayerMask Mask = LayerMaskClass.HighPolyWithTerrainMaskAI;
}
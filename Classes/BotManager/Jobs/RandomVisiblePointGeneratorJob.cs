using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using SAIN.Types.Jobs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Components;

public class RandomVisiblePointGeneratorJob : SainJobTemplate, IDisposable
{
    public RandomVisiblePointGeneratorJob(MonoBehaviour botcontroller) : base("Random Visible Point Generator", botcontroller, true)
    {
        //Start();
    }

    protected readonly List<RaycastJob> RaycastJobs = [];

    protected override IEnumerator PrimaryFunction()
    {
        //RandomDir[] ShortRandomDirections = GenerateRandomDirections(100, 0.5f, 5.0f);
        //RandomDir[] MidRangeRandomDirections = GenerateRandomDirections(100, 5f, 12.0f);
        RandomDir[] LongRandomDirections = GenerateRandomDirections(500, 0.5f, 100.0f);
        foreach (var player in AlivePlayers.Values)
        {
            if (player?.IsActive == true)
            {
                //RaycastJobs.Add(new RaycastJob(ShortRandomDirections, player.Transform.HeadPosition, LayerMaskClass.HighPolyWithTerrainMask, player.Player, null));
                //RaycastJobs.Add(new RaycastJob(MidRangeRandomDirections, player.Transform.HeadPosition, LayerMaskClass.HighPolyWithTerrainMask, player.Player, null));
                RaycastJobs.Add(new RaycastJob(LongRandomDirections, player.Transform.EyePosition, LayerMaskClass.HighPolyWithTerrainMask, player.Player, null));
            }
        }
        int Total = RaycastJobs.Count;
        if (Total > 0)
        {
            ScheduleJobs(Total);
            yield return AwaitCompletion(Total);

            for (int i = 0; i < Total; i++)
            {
                RaycastJob Job = RaycastJobs[i];
                Job.Complete();
                NativeArray<RaycastHit> Hits = Job.Hits;
                NativeArray<RaycastCommand> Commands = Job.Commands;

                if (GameWorldComponent.TryGetPlayerComponent(Job.Owner, out PlayerComponent Player))
                {
                    for (int j = Hits.Length - 1; j >= 0; j--)
                    {
                        RaycastHit Hit = Hits[j];
                        if (Hit.collider == null)
                        {
                            RaycastCommand Command = Commands[j];
                            Vector3 Point = Command.from + Command.direction * Command.distance;
                            Color RandomColor = DebugGizmos.RandomColor;
                            if (Player.Player.IsYourPlayer)
                            {
                                DebugGizmos.DrawSphere(Point, 0.025f, RandomColor, 0.05f);
                                //DebugGizmos.Line(Command.from, Point, RandomColor, 0.01f, true, 0.05f);
                            }
                            if (Command.distance > 3)
                            {
                                if (NavMesh.SamplePosition(Point, out NavMeshHit NavHit, 1.5f, -1))
                                {
                                    if (Player.Player.IsYourPlayer)
                                    {
                                        DebugGizmos.DrawSphere(NavHit.position, 0.1f, RandomColor, 0.05f);
                                        DebugGizmos.DrawLine(NavHit.position, NavHit.position + Vector3.up * 1.5f, RandomColor, 0.025f, 0.05f);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Dispose();
            // Logger.LogDebug($"Completed {Total}");
        }
    }

    private void ScheduleJobs(int Total)
    {
        for (int i = 0; i < Total; i++)
            RaycastJobs[i].Schedule();
    }

    protected static RandomDir[] GenerateRandomDirections(int Count, float LengthMin, float LengthMax)
    {
        RandomDir[] Result = new RandomDir[Count];
        for (int i = 0; i < Count; i++)
        {
            Result[i] = new RandomDir(LengthMin, LengthMax);
        }
        return Result;
    }

    private IEnumerator AwaitCompletion(int Total)
    {
        int FramesWaited = 0;
        float DeltaTimeWaited = 0;
        const int MaxFramesToWait = 10;
        bool JobsComplete = false;
        while (!JobsComplete && FramesWaited < MaxFramesToWait)
        {
            for (int i = 0; i < Total; i++)
            {
                if (!RaycastJobs[i].IsCompleted)
                    continue;
                JobsComplete = true;
            }
            yield return null;
            FramesWaited++;
            DeltaTimeWaited += Time.deltaTime;
        }

        // Logger.LogDebug($"Took {FramesWaited} frames or {DeltaTimeWaited} seconds To Complete Navmesh Raycasts Jobs");
    }

    protected override bool CanProceed()
    {
        var bots = SAINBotController?.BotSpawnController?.BotDictionary;
        return bots != null && bots.Count > 0;
    }

    protected override bool LoopCondition()
    {
        return SAINGameWorld != null;
    }

    public void Dispose()
    {
        foreach (RaycastJob Job in RaycastJobs)
        {
            Job.Dispose();
        }
        RaycastJobs.Clear();
    }
}
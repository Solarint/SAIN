using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.Types.Jobs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace SAIN.Components;

public class FlashlightRaycastJob : SainJobTemplate, IDisposable
{
    private const float LaserTraceDistance = 75;

    private const float Wide_FlashLightBeamAngle = 16f;
    private const int Wide_FlashlightBeamPointCount = 32;
    private const float Wide_FlashlightTraceDistance = 30;

    private const float Tight_FlashLightBeamAngle = 8f;
    private const int Tight_FlashlightBeamPointCount = 16;
    private const float Tight_FlashlightTraceDistance = 60;

    public FlashlightRaycastJob(MonoBehaviour gameWorld) : base("Flashlight Detection Job", gameWorld, true, 0.1f)
    {
        GenerateRandomYawPitchRotationsNonAlloc(_rotationsList_Wide, Wide_FlashlightBeamPointCount, Wide_FlashLightBeamAngle);
        GenerateRandomYawPitchRotationsNonAlloc(_rotationsList_Tight, Tight_FlashlightBeamPointCount, Tight_FlashLightBeamAngle);
    }

    protected readonly List<RaycastJob> RaycastJobs = [];
    protected readonly List<Quaternion> _rotationsList_Wide = [];
    protected readonly List<Quaternion> _rotationsList_Tight = [];

    protected override IEnumerator PrimaryFunction()
    {
        CreateFlashlightJobs();
        int Total = RaycastJobs.Count;
        if (Total > 0)
        {
            ScheduleJobs(Total);
            yield return null;
            ReadFlashlightJobData(Total);
            Dispose();

            CreateLightDetectionJobs();
            Total = RaycastJobs.Count;
            if (Total > 0)
            {
                ScheduleJobs(Total);
                yield return null;
                ReadLightDetectionJobData(Total);
                Dispose();
            }
        }
    }

    private void CreateFlashlightJobs()
    {
        List<RandomDir> Directions = _directionsList;
        HashSet<PlayerComponent> players = GameWorldComponent.Instance.PlayerTracker.AlivePlayerArray;
        foreach (var player in players)
        {
            if (player != null && player.IsActive && player.Flashlight.DeviceActive)
            {
                Vector3 WeaponPointDir = player.Transform.WeaponData.PointDirection;
                if (player.Flashlight.Laser || player.Flashlight.IRLaser)
                {
                    Directions.Add(new(LaserTraceDistance, WeaponPointDir));
                }
                if (player.Flashlight.WhiteLight || player.Flashlight.IRLight)
                {
                    CreateFlashlightBeam(Directions, _rotationsList_Wide, WeaponPointDir, Wide_FlashlightTraceDistance);
                    CreateFlashlightBeam(Directions, _rotationsList_Tight, WeaponPointDir, Tight_FlashlightTraceDistance);
                }
                if (Directions.Count > 0)
                {
                    RaycastJobs.Add(new RaycastJob(Directions, player.Transform.WeaponData.PointDirection, LayerMaskClass.HighPolyWithTerrainMaskAI, player.Player, null));
                    Directions.Clear();
                }
            }
        }
    }

    private void ReadFlashlightJobData(int Total)
    {
        for (int i = 0; i < Total; i++)
        {
            RaycastJob Job = RaycastJobs[i];
            Job.Complete();
            NativeArray<RaycastHit> Hits = Job.Hits;
            if (GameWorldComponent.TryGetPlayerComponent(Job.Owner, out PlayerComponent Player))
            {
                List<Vector3> LightPoints = Player.Flashlight.LightDetection.LightPoints;
                LightPoints.Clear();
                for (int j = Hits.Length - 1; j >= 0; j--)
                {
                    RaycastHit Hit = Hits[j];
                    if (Hit.collider != null)
                    {
                        // Offset the hit point slightly away from the thing it hit to allow easy visibilty checking and simulate "glow"
                        LightPoints.Add(Hit.point + (Hit.normal * 0.05f));
                    }
                }

                //if (Player.Player.IsYourPlayer)
                //{
                //    Logger.LogDebug($"player has {LightPoints.Count} light points");
                //    foreach (var point in LightPoints)
                //    {
                //        DebugGizmos.Line(Player.Transform.WeaponFirePort, point, 0.025f, 0.02f, true);
                //    }
                //}
            }
        }
    }

    private void CreateLightDetectionJobs()
    {
        foreach (BotComponent Bot in AliveBots.Values)
        {
            if (Bot != null && Bot.BotActive)
            {
                foreach (Enemy Enemy in Bot.EnemyController.Enemies.Values)
                {
                    if (Enemy != null && Enemy.PlayerComponent.IsActive)
                    {
                        FlashLightClass EnemyLight = Enemy.EnemyPlayerComponent.Flashlight;
                        if (EnemyLight.DeviceActive &&
                            Bot.PlayerComponent.Flashlight.LightDetection.CheckIsBeamVisible(EnemyLight) &&
                            Enemy.RealDistance <= 125f)
                        {
                            RaycastJobs.Add(new RaycastJob(EnemyLight.LightDetection.LightPoints, Bot.Transform.EyePosition, LayerMaskClass.HighPolyWithTerrainMaskAI, Bot.Player, Enemy.Player));
                        }
                    }
                }
            }
        }
    }

    private void ReadLightDetectionJobData(int Total)
    {
        for (int i = 0; i < Total; i++)
        {
            RaycastJob Job = RaycastJobs[i];
            Job.Complete();
            NativeArray<RaycastHit> Hits = Job.Hits;
            if (GameWorldComponent.TryGetPlayerComponent(Job.Owner, out PlayerComponent Player))
            {
                bool VisiblePoint = false;
                foreach (RaycastHit Hit in Hits)
                {
                    if (Hit.collider == null)
                    {
                        VisiblePoint = true;
                        break;
                    }
                }
                if (VisiblePoint && Job.Target != null)
                {
                    Player.Flashlight.LightDetection.TryToInvestigate(Job.Target);
                }
            }
        }
    }

    private readonly List<RandomDir> _directionsList = [];

    private void ScheduleJobs(int Total)
    {
        for (int i = 0; i < Total; i++)
            RaycastJobs[i].Schedule();
    }

    /// <summary>
    /// Generates a list of random rotations with given angle.
    /// </summary>
    /// <param name="count">Number of quaternions to generate.</param>
    /// <param name="maxYaw">Max yaw in degrees (horizontal rotation around Y).</param>
    /// <param name="maxPitch">Max pitch in degrees (vertical rotation around right axis).</param>
    /// <returns>List of Quaternion rotations.</returns>
    public static void GenerateRandomYawPitchRotationsNonAlloc(List<Quaternion> nonAllocList, int count, float coneAngle)
    {
        for (int i = 0; i < count; i++)
        {
            float yaw = UnityEngine.Random.Range(-coneAngle, coneAngle);     // Y axis
            float pitch = UnityEngine.Random.Range(-coneAngle, coneAngle); // X axis
            float roll = UnityEngine.Random.Range(-coneAngle, coneAngle);   // Z axis

            nonAllocList.Add(Quaternion.Euler(pitch, yaw, roll)); // (X, Y, Z) = (Pitch, Yaw, Roll)
        }
    }

    private static void CreateFlashlightBeam(List<RandomDir> beamDirections, List<Quaternion> rotationsList, Vector3 weaponPointDir, float distance)
    {
        for (int i = 0; i < rotationsList.Count; i++)
        {
            beamDirections.Add(new(distance, (rotationsList[i] * weaponPointDir).normalized));
        }
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

    protected override bool CanProceed()
    {
        var bots = SAINBotController?.BotSpawnController?.BotDictionary;
        return bots != null && bots.Count > 0;
    }

    protected override bool LoopCondition()
    {
        return SAINGameWorld != null;
    }

    public override void Stop()
    {
        Dispose();
        base.Stop();
    }

    public void Dispose()
    {
        foreach (RaycastJob Job in RaycastJobs) Job.Dispose();
        RaycastJobs.Clear();
    }
}
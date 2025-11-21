using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.Types.Jobs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace SAIN.Components;

public class EnemyPathVisibilityRaycastJob : SainJobTemplate, IDisposable
{
    static EnemyPathVisibilityRaycastJob()
    {
        PresetHandler.OnPresetUpdated += updateSettings;
        updateSettings(PresetHandler.LoadedPreset);
    }

    private static void updateSettings(SAINPresetClass preset)
    {
        _commandsPerJob = Mathf.RoundToInt(preset.GlobalSettings.Steering.PathVisionMinCommandsPerJob);
    }

    protected readonly List<PathVisionJob> VisionJobs = [];
    protected readonly List<PathVisionJob> ShootJobs = [];
    private QueryParameters queryParams;

    private static int _commandsPerJob = 256;

    public EnemyPathVisibilityRaycastJob(MonoBehaviour botcontroller) : base("Path Visibility Job", botcontroller, true, 1f / 20f)
    {
        LayerMask HighPolyWithTerrain = LayerMaskClass.HighPolyWithTerrainMask;
        LayerMask DoorLayer = LayerMaskClass.DoorLayer;
        LayerMask Mask = HighPolyWithTerrain & ~(1 << DoorLayer);
        queryParams = new(Mask, false, QueryTriggerInteraction.Ignore);
    }

    protected override IEnumerator PrimaryFunction()
    {
        HashSet<BotComponent> bots = SAINBotController.BotSpawnController.SAINBots;
        CalcEnemyPaths(bots);
        yield return null;
        CreateJobs(bots);
        if (VisionJobs.Count > 0)
        {
            ScheduleJobs(VisionJobs, _commandsPerJob);
            yield return null;
            CheckVisionCommandsTest();
            //ScheduleShootCommands();
            //if (ShootJobs.Count > 0)
            //{
            //    ScheduleJobs(ShootJobs, _commandsPerJob);
            //    yield return null;
            //    ReadResults();
            //}
        }
    }

    private WaitForFixedUpdate waitForFixedUpdate = new();
    private WaitForEndOfFrame waitForEndOfFrame = new();

    private static void ScheduleJobs(List<PathVisionJob> jobs, int minCommandsPerJob = 256)
    {
        for (int i = 0; i < jobs.Count; i++)
        {
            PathVisionJob job = jobs[i];
            job.Handle = RaycastCommand.ScheduleBatch(job.Commands, job.Hits, minCommandsPerJob);
            jobs[i] = job;
        }
    }

    private void CreateJobs(HashSet<BotComponent> bots)
    {
        float currentTime = Time.time;
        foreach (BotComponent bot in bots)
        {
            if (bot != null && bot.SAINLayersActive)
            {
                EnemyList knownEnemies = bot.EnemyController.KnownEnemies;
                if (knownEnemies.Count > 0)
                {
                    //Vector3 botPosition = bot.Transform.Position;
                    //Vector3 eyePosition = bot.Transform.EyePosition;
                    //Vector3 neutralViewPosition = new(botPosition.x, eyePosition.y, botPosition.z);
                    Vector3 neutralViewPosition = bot.Transform.WeaponRoot;
                    foreach (Enemy enemy in bot.EnemyController.KnownEnemies)
                    {
                        if (enemy.IsVisible)
                        {
                            enemy.SetLastCornerAsVisiblePathPoint(enemy.EnemyPosition, 0);
                            continue;
                        }
                        if (enemy.Path.ShallCheckPathVision(currentTime, neutralViewPosition))
                        {
                            int nodeCount = enemy.Path.AllPathNodeCount;
                            if (nodeCount > 0)
                            {
                                VisionJobs.Add(new(enemy.Path.AllPathNodes, nodeCount, neutralViewPosition, enemy, queryParams));
                                continue;
                            }
                            //if (enemy.Path.PathCorners.Length > 1)
                            //{
                            //    enemy.SetLastCornerAsVisiblePathPoint(enemy.Path.PathCorners[1], 1);
                            //    continue;
                            //}
                            enemy.ClearVisiblePathPoint();
                        }
                    }
                }
            }
        }
    }

    private void ScheduleShootCommands()
    {
        for (int i = 0; i < VisionJobs.Count; i++)
        {
            PathVisionJob job = VisionJobs[i];
            if (!job.Handle.IsCompleted) job.Handle.Complete();
            Enemy enemy = job.Enemy;
            if (enemy.EnemyKnown)
            {
                var Hits = job.Hits;
                //int lastVisibleIndex = 0;
                var nodes = enemy.Path.AllPathNodes;
                var visibleNodes = enemy.Path.VisibleNodes;
                visibleNodes.Clear();
                for (int j = 0; j < Hits.Length; j++)
                {
                    BotVisiblePathNode node = nodes[j];
                    if (Hits[j].collider == null)
                    {
                        node.Visible = true;
                        visibleNodes.Add(node);
                    }
                    else
                    {
                        node.Visible = false;
                    }
                    nodes[j] = node;
                }
                if (visibleNodes.Count == 0)
                {
                    if (enemy.Path.PathCorners.Length > 1)
                    {
                        enemy.SetLastCornerAsVisiblePathPoint(enemy.Path.PathCorners[1], 1);
                    }
                    else
                    {
                        //Logger.LogDebug($"[{enemy.Bot.name}] No visible path point found for enemy {enemy.EnemyName}");
                        enemy.ClearVisiblePathPoint();
                    }
                }
                else
                {
                    ShootJobs.Add(new(visibleNodes, enemy.Bot.Transform.WeaponRoot, enemy, queryParams));
                }
            }
            job.Hits.Dispose();
            job.Commands.Dispose();
        }
        VisionJobs.Clear();
    }

    private void CheckVisionCommandsTest()
    {
        for (int i = 0; i < VisionJobs.Count; i++)
        {
            PathVisionJob job = VisionJobs[i];
            if (!job.Handle.IsCompleted) job.Handle.Complete();
            Enemy enemy = job.Enemy;
            if (enemy.EnemyKnown)
            {
                var Hits = job.Hits;
                //int lastVisibleIndex = 0;
                var nodes = enemy.Path.AllPathNodes;
                bool pointFound = false;
                for (int j = Hits.Length - 1; j >= 0; j--)
                {
                    BotVisiblePathNode node = nodes[j];
                    if (Hits[j].collider == null)
                    {
                        enemy.SetLastVisiblePathPoint(node);
                        pointFound = true;
                        break;
                    }
                }
                if (!pointFound)
                {
#if DEBUG
                    Logger.LogDebug($"[{enemy.Bot.name}] No visible path point found for enemy {enemy.EnemyName}");
#endif
                    enemy.ClearVisiblePathPoint();
                    //if (enemy.Path.PathCorners.Length > 1)
                    //{
                    //    enemy.SetLastCornerAsVisiblePathPoint(enemy.Path.PathCorners[1], 1);
                    //}
                    //else
                    //{
                    //    //Logger.LogDebug($"[{enemy.Bot.name}] No visible path point found for enemy {enemy.EnemyName}");
                    //    enemy.ClearVisiblePathPoint();
                    //}
                }
            }
            job.Hits.Dispose();
            job.Commands.Dispose();
        }
        VisionJobs.Clear();
    }

    private void ReadResults()
    {
        for (int i = 0; i < ShootJobs.Count; i++)
        {
            PathVisionJob job = ShootJobs[i];
            if (!job.Handle.IsCompleted)
                job.Handle.Complete();
            NativeArray<RaycastHit> hits = job.Hits;
            Enemy enemy = job.Enemy;
            if (enemy.EnemyKnown)
            {
                var visibleNodes = enemy.Path.VisibleNodes;
                bool PointFound = false;
                for (int j = hits.Length - 1; j >= 0; j--)
                {
                    BotVisiblePathNode node = visibleNodes[j];
                    if (hits[j].collider == null)
                    {
                        enemy.SetLastVisiblePathPoint(node);
                        PointFound = true;
                        break;
                    }
                }
                if (!PointFound)
                {
                    if (visibleNodes.Count > 0)
                    {
                        enemy.SetLastVisiblePathPoint(visibleNodes[visibleNodes.Count - 1]);
                    }
                    else
                    {
                        enemy.ClearVisiblePathPoint();
                        //Logger.LogDebug($"[{enemy.Bot.name}] No shootable path point found for enemy {enemy.EnemyName}");
                    }
                }
            }
            job.Hits.Dispose();
            job.Commands.Dispose();
        }
        ShootJobs.Clear();
    }

    private static void CalcEnemyPaths(HashSet<BotComponent> bots)
    {
        PathVisibilityConfig config = new(GlobalSettingsClass.Instance.Steering);
        float currentTime = Time.time;
        foreach (BotComponent bot in bots)
        {
            if (bot != null && bot.SAINLayersActive)
            {
                foreach (Enemy enemy in bot.EnemyController.KnownEnemies)
                {
                    enemy.Path.CheckCalcPath(config, currentTime);
                }
            }
        }
    }

    protected override bool CanProceed()
    {
        var bots = SAINBotController?.BotSpawnController?.SAINBots;
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
        foreach (var Job in VisionJobs) Job.Dispose();
        VisionJobs.Clear();
        foreach (var job in ShootJobs) job.Dispose();
        ShootJobs.Clear();
    }
}
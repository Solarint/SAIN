using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.VisionConfig;
using SAIN.Helpers;
using SAIN.Classes;

namespace SAIN.Components
{
    public class LineOfSightManager : SAINControl
    {
        public LineOfSightManager()
        {
        }

        private readonly float SpherecastRadius = 0.05f;
        private LayerMask SightLayers => LayerMaskClass.HighPolyWithTerrainMaskAI;
        private readonly int MinJobSize = 5;
        private List<Player> RegisteredPlayers => Singleton<GameWorld>.Instance.RegisteredPlayers;

        private int Frames = 0;

        public void Update()
        {
            if (EnableVisionJobs.Value)
            {
                Frames++;
                if (Frames == CheckFrameCount.Value)
                {
                    Frames = 0;
                    GlobalRaycastJob();
                    CheckEnemiesJobs();
                }
            }
        }

        private Vector3 HeadPos(Player player)
        {
            return player.MainParts[BodyPartType.head].Position;
        }

        private Vector3 BodyPos(Player player)
        {
            return player.MainParts[BodyPartType.body].Position;
        }

        private Vector3[] Parts(Player player)
        {
            return player.MainParts.Values.Select(item => item.Position).ToArray();
        }

        private void CheckEnemiesJobs()
        {
            if (BotController.SAINBots.Count > 0)
            {
                foreach (var bot in BotController.SAINBots)
                {
                    if (bot.Value != null && bot.Value.Enemies.Count > 0)
                    {
                        BotsWithEnemies.Add(bot.Value);
                        TotalEnemiesGlobal += bot.Value.Enemies.Count;
                    }
                }

                if (BotsWithEnemies.Count > 0)
                {
                    EnemyCanShootJob();
                    EnemyVisionJob();

                    if (DebugVision.Value && DebugDrawTimer < Time.time)
                    {
                        DebugDrawTimer = Time.time + 0.5f;
                        foreach (var bot in BotsWithEnemies)
                        {
                            var enemies = bot.Enemies;
                            foreach (var enemy in enemies)
                            {
                                if (enemy.Value.InLineOfSight)
                                {
                                    DebugGizmos.SingleObjects.Line(bot.HeadPosition, enemy.Value.EnemyChestPosition, Color.blue, 0.05f, true, 0.5f, true);
                                }
                                if (enemy.Value.CanShoot)
                                {
                                    DebugGizmos.SingleObjects.Line(bot.WeaponRoot, enemy.Value.EnemyChestPosition, Color.red, 0.05f, true, 0.5f, true);
                                }
                            }
                        }
                    }
                }

                BotsWithEnemies.Clear();
                TotalEnemiesGlobal = 0;
            }
        }

        private float DebugDrawTimer { get; set; }

        public List<SAINComponent> BotsWithEnemies = new List<SAINComponent>();
        private int TotalEnemiesGlobal = 0;
        public List<SAINEnemy> BotEnemiesList = new List<SAINEnemy>();

        private void EnemyVisionJob()
        {
            const int PartCount = 6;

            NativeArray<SpherecastCommand> spherecastCommands = new NativeArray<SpherecastCommand>(
                TotalEnemiesGlobal * PartCount,
                Allocator.TempJob
            );
            NativeArray<RaycastHit> raycastHits = new NativeArray<RaycastHit>(
                TotalEnemiesGlobal * PartCount,
                Allocator.TempJob
            );

            for (int i = 0; i < BotsWithEnemies.Count; i++)
            {
                var bot = BotsWithEnemies[i];
                var mask = SightLayers;
                float max = bot.BotOwner.Settings.Current.CurrentVisibleDistance;
                Vector3 head = bot.BotOwner.LookSensor._headPoint;

                BotEnemiesList.Clear();
                BotEnemiesList.AddRange(bot.Enemies.Values.ToList());

                for (int k = 0; k < BotEnemiesList.Count; k++)
                {
                    SAINEnemy enemy = BotEnemiesList[k];
                    Vector3[] bodyParts = Parts(enemy.EnemyPlayer);

                    for (int j = 0; j < PartCount; j++)
                    {
                        Vector3 target = bodyParts[j];
                        Vector3 direction = target - head;
                        float distance = direction.magnitude;
                        float rayDistance = Mathf.Clamp(distance, 0f, max);

                        if (rayDistance < 8f)
                        {
                            mask = LayerMaskClass.HighPolyWithTerrainMask;
                        }

                        spherecastCommands[i + k + j] = new SpherecastCommand(
                            head,
                            SpherecastRadius,
                            direction.normalized,
                            rayDistance,
                            mask
                        );
                    }
                }
            }

            JobHandle spherecastJob = SpherecastCommand.ScheduleBatch(
                spherecastCommands,
                raycastHits,
                MinJobSize
            );

            spherecastJob.Complete();

            for (int i = 0; i < BotsWithEnemies.Count; i++)
            {
                bool visible = false;
                var bot = BotsWithEnemies[i];

                BotEnemiesList.AddRange(bot.Enemies.Values.ToList());
                for (int k = 0; k < BotEnemiesList.Count; k++)
                {
                    int partVisCount = 0;
                    SAINEnemy enemy = BotEnemiesList[k];

                    for (int j = 0; j < PartCount; j++)
                    {
                        if (raycastHits[i + k + j].collider == null)
                        {
                            partVisCount++;
                            visible = true;
                        }
                    }

                    if (visible)
                    {
                        float percentage = ((float)partVisCount / PartCount) * 100f;
                        percentage = Mathf.Round(percentage);
                        enemy.OnGainSight(percentage);
                    }
                    else
                    {
                        enemy.OnLoseSight();
                    }
                }
                BotEnemiesList.Clear();
            }

            spherecastCommands.Dispose();
            raycastHits.Dispose();
        }

        private void EnemyCanShootJob()
        {
            const int PartCount = 6;

            NativeArray<SpherecastCommand> spherecastCommands = new NativeArray<SpherecastCommand>(
                TotalEnemiesGlobal * PartCount,
                Allocator.TempJob
            );
            NativeArray<RaycastHit> raycastHits = new NativeArray<RaycastHit>(
                TotalEnemiesGlobal * PartCount,
                Allocator.TempJob
            );

            for (int i = 0; i < BotsWithEnemies.Count; i++)
            {
                var bot = BotsWithEnemies[i];
                var mask = SightLayers;
                float max = bot.BotOwner.WeaponManager.CurrentWeapon.Template.bEffDist * 1.5f;
                Vector3 weapon = bot.BotOwner.WeaponRoot.position;

                BotEnemiesList.AddRange(bot.Enemies.Values.ToList());

                for (int k = 0; k < BotEnemiesList.Count; k++)
                {
                    SAINEnemy enemy = BotEnemiesList[k];
                    Vector3[] bodyParts = Parts(enemy.EnemyPlayer);

                    for (int j = 0; j < PartCount; j++)
                    {
                        Vector3 target = bodyParts[j];
                        Vector3 direction = target - weapon;
                        float distance = direction.magnitude;
                        float rayDistance = Mathf.Clamp(distance, 0f, max);

                        if (rayDistance < 8f)
                        {
                            mask = LayerMaskClass.HighPolyWithTerrainMask;
                        }

                        spherecastCommands[i + k + j] = new SpherecastCommand(
                            weapon,
                            SpherecastRadius,
                            direction.normalized,
                            rayDistance,
                            mask
                        );
                    }
                }
                BotEnemiesList.Clear();
            }

            JobHandle spherecastJob = SpherecastCommand.ScheduleBatch(
                spherecastCommands,
                raycastHits,
                MinJobSize
            );

            spherecastJob.Complete();

            for (int i = 0; i < BotsWithEnemies.Count; i++)
            {
                bool canShoot = false;
                var bot = BotsWithEnemies[i];

                BotEnemiesList.AddRange(bot.Enemies.Values.ToList());
                for (int k = 0; k < BotEnemiesList.Count; k++)
                {
                    int partsCanShoot = 0;
                    SAINEnemy enemy = BotEnemiesList[k];
                    for (int j = 0; j < PartCount; j++)
                    {
                        if (raycastHits[i + k + j].collider == null)
                        {
                            partsCanShoot++;
                            canShoot = true;
                        }
                    }

                    if (canShoot)
                    {
                        float percentage = ((float)partsCanShoot / PartCount) * 100f;
                        percentage = Mathf.Round(percentage);
                        enemy.UpdateCanShoot(true, percentage);
                    }
                    else
                    {
                        enemy.UpdateCanShoot(false, 0f);
                    }
                }
                BotEnemiesList.Clear();
            }

            spherecastCommands.Dispose();
            raycastHits.Dispose();
        }

        private void GlobalRaycastJob()
        {
            NativeArray<SpherecastCommand> allSpherecastCommands = new NativeArray<SpherecastCommand>(
                BotController.SAINBots.Values.Count * RegisteredPlayers.Count,
                Allocator.TempJob
            );
            NativeArray<RaycastHit> allRaycastHits = new NativeArray<RaycastHit>(
                BotController.SAINBots.Values.Count * RegisteredPlayers.Count,
                Allocator.TempJob
            );

            int currentIndex = 0;

            var list = BotController.SAINBots.Values.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                var bot = list[i];
                Vector3 head = HeadPos(bot.BotOwner.GetPlayer);

                for (int j = 0; j < RegisteredPlayers.Count; j++)
                {
                    Vector3 target = BodyPos(RegisteredPlayers[j]);
                    Vector3 direction = target - head;
                    float max = bot.BotOwner.Settings.Current.CurrentVisibleDistance;
                    float rayDistance = Mathf.Clamp(direction.magnitude, 0f, max);

                    allSpherecastCommands[currentIndex] = new SpherecastCommand(
                        head,
                        SpherecastRadius,
                        direction.normalized,
                        rayDistance,
                        SightLayers
                    );

                    currentIndex++;
                }
            }

            JobHandle spherecastJob = SpherecastCommand.ScheduleBatch(
                allSpherecastCommands,
                allRaycastHits,
                MinJobSize
            );
            int visiblecount = 0;
            spherecastJob.Complete();

            for (int i = 0; i < list.Count; i++)
            {
                int startIndex = i * RegisteredPlayers.Count;
                var visiblePlayers = list[i].VisiblePlayers;
                BotVisiblePlayers.AddRange(visiblePlayers);

                for (int v = 0; v < visiblePlayers.Count; v++)
                {
                    var visPlayer = visiblePlayers[v];
                    if (visPlayer == null || visPlayer.HealthController.IsAlive == false)
                    {
                        BotVisiblePlayers.RemoveAt(v);
                    }
                }

                visiblePlayers.Clear();
                visiblePlayers.AddRange(BotVisiblePlayers);
                BotVisiblePlayers.Clear();

                for (int j = 0; j < RegisteredPlayers.Count; j++)
                {
                    currentIndex = startIndex + j;
                    if (allRaycastHits[currentIndex].collider != null)
                    {
                        if (visiblePlayers.Contains(RegisteredPlayers[j]))
                        {
                            visiblePlayers.Remove(RegisteredPlayers[j]);
                        }
                    }
                    else
                    {
                        visiblecount++;
                        if (!visiblePlayers.Contains(RegisteredPlayers[j]))
                        {
                            visiblePlayers.Add(RegisteredPlayers[j]);
                        }
                    }
                }
            }

            allSpherecastCommands.Dispose();
            allRaycastHits.Dispose();
        }

        public List<Player> BotVisiblePlayers = new List<Player>();
    }
}
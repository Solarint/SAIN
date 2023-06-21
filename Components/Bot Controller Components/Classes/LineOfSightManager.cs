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
                TotalEnemiesGlobal = 0;
                BotEnemiesList.Clear();
                BotsWithEnemies.Clear();

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

                    BotEnemiesList.Clear();
                    BotsWithEnemies.Clear();
                    TotalEnemiesGlobal = 0;
                }
            }
        }

        public List<SAINComponent> BotsWithEnemies = new List<SAINComponent>();
        private int TotalEnemiesGlobal = 0;
        public List<SAINEnemy> BotEnemiesList = new List<SAINEnemy>();

        private void EnemyVisionJob()
        {
            const int PartCount = 6;
            int total = TotalEnemiesGlobal * PartCount;

            NativeArray<SpherecastCommand> spherecastCommands = new NativeArray<SpherecastCommand>(
                total,
                Allocator.TempJob
            );
            NativeArray<RaycastHit> raycastHits = new NativeArray<RaycastHit>(
                total,
                Allocator.TempJob
            );

            total = 0;
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

                        spherecastCommands[total] = new SpherecastCommand(
                            head,
                            SpherecastRadius,
                            direction.normalized,
                            rayDistance,
                            mask
                        );
                        total++;
                    }
                }
            }

            JobHandle spherecastJob = SpherecastCommand.ScheduleBatch(
                spherecastCommands,
                raycastHits,
                MinJobSize
            );

            spherecastJob.Complete();

            total = 0;
            for (int i = 0; i < BotsWithEnemies.Count; i++)
            {
                bool visible = false;
                var bot = BotsWithEnemies[i];

                BotEnemiesList.Clear();
                BotEnemiesList.AddRange(bot.Enemies.Values.ToList());
                for (int k = 0; k < BotEnemiesList.Count; k++)
                {
                    int partVisCount = 0;
                    SAINEnemy enemy = BotEnemiesList[k];

                    for (int j = 0; j < PartCount; j++)
                    {
                        if (raycastHits[total].collider == null)
                        {
                            partVisCount++;
                            visible = true;
                        }
                        total++;
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
            int total = TotalEnemiesGlobal * PartCount;

            NativeArray<SpherecastCommand> spherecastCommands = new NativeArray<SpherecastCommand>(
                total,
                Allocator.TempJob
            );
            NativeArray<RaycastHit> raycastHits = new NativeArray<RaycastHit>(
                total,
                Allocator.TempJob
            );

            total = 0;
            for (int i = 0; i < BotsWithEnemies.Count; i++)
            {
                var bot = BotsWithEnemies[i];
                var mask = SightLayers;
                float max = bot.BotOwner.WeaponManager.CurrentWeapon.Template.bEffDist * 1.5f;
                Vector3 weapon = bot.BotOwner.WeaponRoot.position;

                BotEnemiesList.Clear();
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

                        spherecastCommands[total] = new SpherecastCommand(
                            weapon,
                            SpherecastRadius,
                            direction.normalized,
                            rayDistance,
                            mask
                        );
                        total++;
                    }
                }
            }

            JobHandle spherecastJob = SpherecastCommand.ScheduleBatch(
                spherecastCommands,
                raycastHits,
                MinJobSize
            );

            spherecastJob.Complete();
            total = 0;

            for (int i = 0; i < BotsWithEnemies.Count; i++)
            {
                bool canShoot = false;
                var bot = BotsWithEnemies[i];

                BotEnemiesList.Clear();
                BotEnemiesList.AddRange(bot.Enemies.Values.ToList());
                for (int k = 0; k < BotEnemiesList.Count; k++)
                {
                    int partsCanShoot = 0;
                    SAINEnemy enemy = BotEnemiesList[k];
                    for (int j = 0; j < PartCount; j++)
                    {
                        if (raycastHits[total].collider == null)
                        {
                            partsCanShoot++;
                            canShoot = true;
                        }
                        total++;
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
            }

            spherecastCommands.Dispose();
            raycastHits.Dispose();
            BotEnemiesList.Clear();
        }

        private List<SAINBot> GlobalVisionBotList = new List<SAINBot>();

        private void GlobalRaycastJob()
        {
            var sainBots = BotController.SAINBots.Values.ToList();
            int total = sainBots.Count * RegisteredPlayers.Count;

            NativeArray<SpherecastCommand> allSpherecastCommands = new NativeArray<SpherecastCommand>(
                total,
                Allocator.TempJob
            );
            NativeArray<RaycastHit> allRaycastHits = new NativeArray<RaycastHit>(
                total,
                Allocator.TempJob
            );

            total = 0;
            for (int i = 0; i < sainBots.Count; i++)
            {
                var bot = sainBots[i];
                Vector3 head = HeadPos(bot.BotOwner.GetPlayer);

                for (int j = 0; j < RegisteredPlayers.Count; j++)
                {
                    Vector3 target = BodyPos(RegisteredPlayers[j]);
                    Vector3 direction = target - head;
                    float max = bot.BotOwner.Settings.Current.CurrentVisibleDistance;
                    float rayDistance = Mathf.Clamp(direction.magnitude, 0f, max);

                    allSpherecastCommands[total] = new SpherecastCommand(
                        head,
                        SpherecastRadius,
                        direction.normalized,
                        rayDistance,
                        SightLayers
                    );
                    total++;
                }
            }

            JobHandle spherecastJob = SpherecastCommand.ScheduleBatch(
                allSpherecastCommands,
                allRaycastHits,
                MinJobSize
            );

            spherecastJob.Complete();
            total = 0;

            for (int i = 0; i < sainBots.Count; i++)
            {
                sainBots[i].VisiblePlayers.Clear();
                for (int j = 0; j < RegisteredPlayers.Count; j++)
                {
                    if (allRaycastHits[total].collider == null && RegisteredPlayers[j] != null && RegisteredPlayers[j]?.HealthController?.IsAlive == true)
                    {
                        sainBots[i].VisiblePlayers.Add(RegisteredPlayers[j]);
                    }
                    total++;
                }
            }

            allSpherecastCommands.Dispose();
            allRaycastHits.Dispose();
        }
    }
}
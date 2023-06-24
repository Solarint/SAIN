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

        private readonly float SpherecastRadius = 0.025f;
        private LayerMask SightLayers => LayerMaskClass.HighPolyWithTerrainMaskAI;
        private readonly int MinJobSize = 6;
        private List<Player> RegisteredPlayers => Singleton<GameWorld>.Instance.RegisteredPlayers;

        private int Frames = 0;

        public void Update()
        {
            Frames++;
            if (Frames == CheckFrameCount.Value)
            {
                Frames = 0;
                GlobalRaycastJob();
                //CheckEnemiesJobs();
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
                foreach (var bot in BotController.SAINBots.Values)
                {
                    if (bot?.Enemy != null)
                    {
                        BotsWithEnemies.Add(bot);
                    }
                }

                if (BotsWithEnemies.Count > 0)
                {
                    EnemyVisionJobAlt();
                    //EnemyCanShootJob();
                    //EnemyVisionJob();

                    BotsWithEnemies.Clear();
                }
            }
        }

        public List<SAINComponent> BotsWithEnemies = new List<SAINComponent>();
        public List<Vector3> PartPositions = new List<Vector3>();
        public readonly List<BodyPartType> PartTypes = new List<BodyPartType> { BodyPartType.leftLeg, BodyPartType.rightLeg, BodyPartType.body, BodyPartType.leftArm, BodyPartType.rightArm, BodyPartType.head};

        private void EnemyVisionJobAlt()
        {
            int total = BotsWithEnemies.Count * PartTypes.Count * 2;

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
                var shootmask = LayerMaskClass.HighPolyWithTerrainMask;
                float max = bot.BotOwner.Settings.Current.CurrentVisibleDistance;
                float shootMax = bot.BotOwner.WeaponManager.CurrentWeapon.Template.bEffDist * 1.5f;

                Vector3 direction;
                float distance;
                float rayDistance;
                Vector3 target;
                for (int j = 0; j < bot.Enemy.BodyParts.Length; j++)
                {
                    target = bot.Enemy.BodyParts[j].PartPosition;

                    direction = target - bot.HeadPosition;
                    distance = direction.magnitude;
                    rayDistance = Mathf.Clamp(distance, 0f, max);
                    if (rayDistance < 8f)
                    {
                        mask = LayerMaskClass.HighPolyWithTerrainMask;
                    }
                    spherecastCommands[total] = new SpherecastCommand(
                        bot.HeadPosition,
                        SpherecastRadius,
                        direction.normalized,
                        rayDistance,
                        mask
                    );
                    total++;

                    direction = target - bot.WeaponRoot;
                    distance = direction.magnitude;
                    rayDistance = Mathf.Clamp(distance, 0f, shootMax);
                    spherecastCommands[total] = new SpherecastCommand(
                        bot.WeaponRoot,
                        SpherecastRadius,
                        direction.normalized,
                        rayDistance,
                        shootmask
                    );
                    total++;
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
                SAINComponent bot = BotsWithEnemies[i];
                var MainParts = bot.Enemy.BodyParts;
                float seenCoef = bot.Vision.GetSeenCoef(bot.Enemy.Person.Transform, bot.Enemy.Person.AIData, bot.Enemy.TimeLastSeen, bot.Enemy.PositionLastSeen);

                int partsCanShoot = 0;
                int partVisCount = 0;
                for (int j = 0; j < PartTypes.Count; j++)
                {
                    bool partVis = false;
                    bool partShoot = false;

                    if (raycastHits[total].collider == null)
                    {
                        partVisCount++;
                        partVis = true;
                    }
                    total++;
                    if (raycastHits[total].collider == null)
                    {
                        partsCanShoot++;
                        partShoot = true;
                    }
                    total++;

                    MainParts[j].UpdateCanShoot(partShoot, seenCoef);
                    MainParts[j].UpdateVisibilty(partVis, seenCoef);
                }
            }

            if (DebugTimer < Time.time)
            {
                DebugTimer = Time.time + 10f;
                System.Console.WriteLine($"Job Complete for {BotsWithEnemies.Count} bots. {total} raycasts.");
            }

            spherecastCommands.Dispose();
            raycastHits.Dispose();
        }

        private float DebugTimer;

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
                var visPlayers = sainBots[i].VisiblePlayers;
                var idList = sainBots[i].VisiblePlayerIds;
                visPlayers.Clear();
                for (int j = 0; j < RegisteredPlayers.Count; j++)
                {
                    Player player = RegisteredPlayers[j];
                    if (allRaycastHits[total].collider == null && player != null && player.HealthController.IsAlive)
                    {
                        visPlayers.Add(player);
                        string id = player.ProfileId;
                        if (!idList.Contains(id))
                        {
                            idList.Add(id);
                        }
                    }
                    total++;
                }
            }

            allSpherecastCommands.Dispose();
            allRaycastHits.Dispose();
        }
    }
}
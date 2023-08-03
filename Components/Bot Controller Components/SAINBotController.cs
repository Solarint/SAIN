using BepInEx.Logging;
using Comfort.Common;
using EFT;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;
using SAIN.Helpers;
using SAIN.Components.BotController;
using UnityEngine.UIElements;
using UnityEngine.Profiling;

namespace SAIN.Components
{
    public class SAINBotController : MonoBehaviour
    {
        public Action<SAINSoundType, Player, float> AISoundPlayed { get; private set; }
        public Action<EPhraseTrigger, ETagStatus, Player> PlayerTalk { get; private set; }

        public Dictionary<string, SAINComponent> Bots => BotSpawnController.Bots;
        public GameWorld GameWorld => Singleton<GameWorld>.Instance;
        public IBotGame BotGame => Singleton<IBotGame>.Instance;
        public Player MainPlayer => Singleton<GameWorld>.Instance?.MainPlayer;
        public BotControllerClass DefaultController { get; set; }
        public ManualLogSource Logger => LineOfSightManager.Logger;
        public BotSpawnerClass BotSpawnerClass { get; set; }

        public CoverManager CoverManager { get; private set; } = new CoverManager();
        public LineOfSightManager LineOfSightManager { get; private set; } = new LineOfSightManager();
        public BotExtractManager BotExtractManager { get; private set; } = new BotExtractManager();
        private TimeClass TimeClass { get; set; } = new TimeClass();
        private WeatherVisionClass WeatherClass { get; set; } = new WeatherVisionClass();
        public BotSpawnController BotSpawnController { get; private set; } = new BotSpawnController();

        public float WeatherVisibility => WeatherClass.WeatherVisibility;
        public float TimeOfDayVisibility => TimeClass.TimeOfDayVisibility;
        public DateTime GameDateTime => TimeClass.GameDateTime;

        public Vector3 MainPlayerPosition { get; private set; }
        private bool ComponentAdded { get; set; }
        private float UpdatePositionTimer { get; set; }


        private void Awake()
        {
            GameWorld.OnDispose += Dispose;

            BotSpawnController.Awake();
            TimeClass.Awake();
            LineOfSightManager.Awake();
            CoverManager.Awake();
            PathManager.Awake();
            BotExtractManager.Awake();

            Singleton<GClass635>.Instance.OnGrenadeThrow += GrenadeThrown;
            Singleton<GClass635>.Instance.OnGrenadeExplosive += GrenadeExplosion;
            AISoundPlayed += SoundPlayed;
            PlayerTalk += PlayerTalked;
        }

        private void Update()
        {
            if (GameWorld == null)
            {
                Dispose();
                return;
            }
            if (BotGame == null)
            {
                return;
            }

            BotSpawnController.Update();
            BotExtractManager.Update();
            UpdateMainPlayer();
            TimeClass.Update();
            WeatherClass.Update();
            LineOfSightManager.Update();
            //CoverManager.ManualUpdate();
            //PathManager.ManualUpdate();
            //AddNavObstacles();
            //UpdateObstacles();
        }

        void PlayerTalked(EPhraseTrigger phrase, ETagStatus mask, Player player)
        {
            if (player == null || Bots == null)
            {
                return;
            }
            foreach (var bot in Bots)
            {
                SAINComponent sain = bot.Value;
                if (sain != null && sain.BotOwner != null && bot.Key != player.ProfileId)
                {
                    if (phrase == EPhraseTrigger.OnFight && sain.BotOwner.EnemiesController.IsEnemy(player))
                    {
                        sain.Talk.EnemyTalk.SetEnemyTalk(player);
                    }
                }
            }
        }

        public void BotDeath(BotOwner bot)
        {
            if (bot?.GetPlayer != null && bot.IsDead)
            {
                DeadBots.Add(bot.GetPlayer);
            }
        }

        public List<Player> DeadBots { get; private set; } = new List<Player>();
        public List<BotDeathObject> DeathObstacles { get; private set; } = new List<BotDeathObject>();

        private readonly List<int> IndexToRemove = new List<int>();

        public void AddNavObstacles()
        {
            if (DeadBots.Count > 0)
            {
                const float ObstacleRadius = 1.5f;

                for (int i = 0; i < DeadBots.Count; i++)
                {
                    var bot = DeadBots[i];
                    if (bot == null || bot.GetPlayer == null)
                    {
                        IndexToRemove.Add(i);
                        continue;
                    }
                    bool enableObstacle = true;
                    Collider[] players = Physics.OverlapSphere(bot.Position, ObstacleRadius, LayerMaskClass.PlayerMask);
                    foreach (var p in players)
                    {
                        if (p == null) continue;
                        if (p.TryGetComponent<Player>(out var player))
                        {
                            if (player.IsAI && player.HealthController.IsAlive)
                            {
                                enableObstacle = false;
                                break;
                            }
                        }
                    }
                    if (enableObstacle)
                    {
                        if (bot != null && bot.GetPlayer != null)
                        {
                            var obstacle = new BotDeathObject(bot);
                            obstacle.Activate(ObstacleRadius);
                            DeathObstacles.Add(obstacle);
                        }
                        IndexToRemove.Add(i);
                    }
                }

                foreach (var index in IndexToRemove)
                {
                    DeadBots.RemoveAt(index);
                }

                IndexToRemove.Clear();
            }
        }

        private void UpdateObstacles()
        {
            if (DeathObstacles.Count > 0)
            {
                for (int i = 0; i < DeathObstacles.Count; i++)
                {
                    var obstacle = DeathObstacles[i];
                    if (obstacle?.TimeSinceCreated > 30f)
                    {
                        obstacle?.Dispose();
                        IndexToRemove.Add(i);
                    }
                }

                foreach (var index in IndexToRemove)
                {
                    DeathObstacles.RemoveAt(index);
                }

                IndexToRemove.Clear();
            }
        }

        public void SoundPlayed(SAINSoundType soundType, Player player, float range)
        {
            if (Bots.Count == 0 || player == null)
            {
                return;
            }

            foreach (var bot in Bots.Values)
            {
                if (player.ProfileId == bot.Player.ProfileId)
                {
                    continue;
                }
                var Enemy = bot.Enemy;
                if (Enemy?.Person != null && Enemy.Person.ProfileId == player.ProfileId)
                {
                    if (Enemy.RealDistance <= range)
                    {
                        if (soundType == SAINSoundType.GrenadePin || soundType == SAINSoundType.GrenadeDraw)
                        {
                            Enemy.EnemyStatus.EnemyHasGrenadeOut = true;
                            return;
                        }
                        if (soundType == SAINSoundType.Reload)
                        {
                            Enemy.EnemyStatus.EnemyIsReloading = true;
                            return;
                        }
                        if (soundType == SAINSoundType.Heal)
                        {
                            Enemy.EnemyStatus.EnemyIsHealing = true;
                            return;
                        }
                    }
                }
            }
        }

        private void UpdateMainPlayer()
        {
            if (MainPlayer == null)
            {
                ComponentAdded = false;
                return;
            }

            // AddorUpdateColorScheme Components to main player
            if (!ComponentAdded)
            {
                MainPlayer.GetOrAddComponent<FlashLightComponent>();
                ComponentAdded = true;
            }

            if (UpdatePositionTimer < Time.time)
            {
                UpdatePositionTimer = Time.time + 1f;
                MainPlayerPosition = MainPlayer.Position;
            }
        }

        public float MainPlayerSqrMagnitude(Vector3 position)
        {
            return (position - MainPlayerPosition).sqrMagnitude;
        }

        public float MainPlayerMagnitude(Vector3 position)
        {
            return (position - MainPlayerPosition).magnitude;
        }

        public bool IsMainPlayerLookAtMe(Vector3 botPos, float dotProductMin = 0.75f, bool distRestriction = true, float sqrMagLimit = 160000)
        {
            if (distRestriction && MainPlayerSqrMagnitude(botPos) > sqrMagLimit)
            {
                return false;
            }
            Vector3 botDir = botPos - MainPlayerPosition;
            float DotProd = Vector3.Dot(MainPlayer.LookDirection.normalized, botDir.normalized);
            return DotProd > dotProductMin;
        }

        private void GrenadeExplosion(Vector3 explosionPosition, string playerProfileID, bool isSmoke, float smokeRadius, float smokeLifeTime)
        {
            if (playerProfileID == null)
            {
                return;
            }
            Player player = EFTInfo.GetPlayer(playerProfileID);
            Vector3 position = player.Position;
            if (isSmoke)
            {
                HelpersGClass.PlaySound(player, explosionPosition, 50f, AISoundType.gun);
                float radius = smokeRadius * HelpersGClass.SMOKE_GRENADE_RADIUS_COEF;
                foreach (var keyValuePair in DefaultController.Groups())
                {
                    foreach (BotGroupClass botGroupClass in keyValuePair.Value.GetGroups(true))
                    {
                        botGroupClass.AddSmokePlace(explosionPosition, smokeLifeTime, radius, position);
                    }
                }
            }
            if (!isSmoke)
            {
                HelpersGClass.PlaySound(player, explosionPosition, 200f, AISoundType.gun);
            }
        }

        private void GrenadeThrown(Grenade grenade, Vector3 position, Vector3 force, float mass)
        {
            if (grenade == null)
            {
                return;
            }
            var danger = VectorHelpers.DangerPoint(position, force, mass);
            foreach (var bot in Bots.Values)
            {
                if (bot != null && (danger - bot.Position).sqrMagnitude < 200f * 200f)
                {
                    bot.Grenade.EnemyGrenadeThrown(grenade, danger);
                }
            }
        }

        public List<string> Groups = new List<string>();
        public PathManager PathManager { get; private set; } = new PathManager();

        private void Dispose()
        {
            try
            {
                StopAllCoroutines();

                GameWorld.OnDispose -= Dispose;

                AISoundPlayed -= SoundPlayed;
                PlayerTalk -= PlayerTalked;
                Singleton<GClass635>.Instance.OnGrenadeThrow -= GrenadeThrown;
                Singleton<GClass635>.Instance.OnGrenadeExplosive -= GrenadeExplosion;

                if (Bots.Count > 0)
                {
                    foreach (var bot in Bots)
                    {
                        bot.Value?.Dispose();
                    }
                }
                Bots.Clear();
                Destroy(this);
            }
            catch { }
        }

        public bool GetBot(string profileId, out SAINComponent bot)
        {
            return Bots.TryGetValue(profileId, out bot);
        }
    }

    public class BotDeathObject
    {
        public BotDeathObject(Player player)
        {
            Player = player;
            NavMeshObstacle = player.gameObject.AddComponent<NavMeshObstacle>();
            NavMeshObstacle.carving = false;
            NavMeshObstacle.enabled = false;
            Position = player.Position;
            TimeCreated = Time.time;
        }

        public void Activate(float radius = 2f)
        {
            if (NavMeshObstacle != null)
            {
                NavMeshObstacle.enabled = true;
                NavMeshObstacle.carving = true;
                NavMeshObstacle.radius = radius;
            }
        }

        public void Dispose()
        {
            if (NavMeshObstacle != null)
            {
                NavMeshObstacle.carving = false;
                NavMeshObstacle.enabled = false;
                GameObject.Destroy(NavMeshObstacle);
            }
        }

        public NavMeshObstacle NavMeshObstacle { get; private set; }
        public Player Player { get; private set; }
        public Vector3 Position { get; private set; }
        public float TimeCreated { get; private set; }
        public float TimeSinceCreated => Time.time - TimeCreated;
        public bool ObstacleActive => NavMeshObstacle.carving;
    }
}
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
using SAIN;
using SAIN.Components.BotController;

namespace SAIN.Components
{
    public class SAINBotController : MonoBehaviour
    {
        public Dictionary<string, SAINComponent> SAINBots = new Dictionary<string, SAINComponent>();
        public LineOfSightManager LineOfSightManager { get; private set; } = new LineOfSightManager();
        public CoverManager CoverManager { get; private set; } = new CoverManager();
        public GameWorld GameWorld => Singleton<GameWorld>.Instance;
        public IBotGame BotGame => Singleton<IBotGame>.Instance;
        public static Player MainPlayer => Singleton<GameWorld>.Instance?.MainPlayer;
        public BotControllerClass DefaultController { get; set; }
        public ManualLogSource Logger => LineOfSightManager.Logger;

        private TimeClass TimeClass { get; set; } = new TimeClass();
        private WeatherVisionClass WeatherClass { get; set; } = new WeatherVisionClass();

        public float WeatherVisibility => WeatherClass.WeatherVisibility;
        public float TimeOfDayVisibility => TimeClass.TimeOfDayVisibility;

        public static Vector3 MainPlayerPosition { get; private set; }

        private bool ComponentAdded { get; set; }
        private float UpdatePositionTimer { get; set; }

        private void Awake()
        {
            TimeClass.Awake();
            LineOfSightManager.Awake();
            CoverManager.Awake();
            PathManager.Awake();

            Singleton<GClass629>.Instance.OnGrenadeThrow += GrenadeThrown;
            Singleton<GClass629>.Instance.OnGrenadeExplosive += GrenadeExplosion;
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

            UpdateMainPlayer();
            TimeClass.Update();
            WeatherClass.Update();
            LineOfSightManager.Update();
            CoverManager.Update();
            PathManager.Update();
        }

        private void UpdateMainPlayer()
        {
            if (MainPlayer == null)
            {
                ComponentAdded = false;
                return;
            }

            // Add Components to main player
            if (!ComponentAdded)
            {
                MainPlayer.GetOrAddComponent<PlayerTalkComponent>();
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

        private void GrenadeExplosion(Vector3 explosionPosition, Player player, bool isSmoke, float smokeRadius, float smokeLifeTime)
        {
            if (player == null)
            {
                return;
            }
            Vector3 position = player.Position;
            if (isSmoke)
            {
                Singleton<GClass629>.Instance.PlaySound(player, explosionPosition, 50f, AISoundType.gun);
                float radius = smokeRadius * GClass560.Core.SMOKE_GRENADE_RADIUS_COEF;
                foreach (KeyValuePair<BotZone, GClass507> keyValuePair in DefaultController.Groups())
                {
                    foreach (BotGroupClass botGroupClass in keyValuePair.Value.GetGroups(true))
                    {
                        botGroupClass.AddSmokePlace(explosionPosition, smokeLifeTime, radius, position);
                    }
                }
            }
            if (!isSmoke)
            {
                Singleton<GClass629>.Instance.PlaySound(player, explosionPosition, 300f, AISoundType.gun);
            }
        }

        private void GrenadeThrown(Grenade grenade, Vector3 position, Vector3 force, float mass)
        {
            var danger = VectorHelpers.DangerPoint(position, force, mass);
            foreach (var bot in SAINBots.Values)
            {
                if (!bot.IsDead && bot.BotOwner.BotState == EBotState.Active)
                {
                    var player = grenade.Player;
                    bool sendInfo = false;
                    if (player != null && bot.Enemy != null && bot.Enemy.EnemyPlayer.ProfileId == player.ProfileId)
                    {
                        sendInfo = true;
                    }
                    else if ((danger - bot.Position).sqrMagnitude < 22500f) // 150 meters min distance
                    {
                        sendInfo = true;
                    }

                    if (sendInfo)
                    {
                        bot.Grenade.EnemyGrenadeThrown(grenade, danger);
                    }
                }
            }
        }

        public List<string> Groups = new List<string>();
        public PathManager PathManager { get; private set; } = new PathManager();

        private void Dispose()
        {
            StopAllCoroutines();

            Singleton<GClass629>.Instance.OnGrenadeThrow -= GrenadeThrown;
            Singleton<GClass629>.Instance.OnGrenadeExplosive -= GrenadeExplosion;

            SAINBots.Clear();

            Destroy(this);
        }

        public void AddBot(SAINComponent sain)
        {
            string Id = sain.ProfileId;
            if (!SAINBots.ContainsKey(Id))
            {
                SAINBots.Add(Id, sain);
            }
        }

        public void RemoveBot(SAINComponent sain)
        {
            string Id = sain.ProfileId;
            if (SAINBots.ContainsKey(Id))
            {
                SAINBots.Remove(Id);
            }
        }
    }
}
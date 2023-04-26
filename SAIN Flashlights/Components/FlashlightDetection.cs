using BepInEx.Logging;
using Comfort.Common;
using EFT;
using System.Collections;
using UnityEngine;
using static SAIN_Flashlights.Config.DazzleConfig;

namespace SAIN_Flashlights.Components
{
    public class FlashlightDetection
    {
        public static Vector3 PlayerPosition { get; private set; }
        public static Vector3 FlashLightPoint { get; private set; }
        public static SAIN_Flashlight_Component PlayerFlashComponent { get; private set; }
        private static Player LocalPlayer { get; set; }
        protected static ManualLogSource Logger { get; private set; }

        public void FindYourPlayer()
        {
            if (LocalPlayer == null)
            {
                Player player = Singleton<GameWorld>.Instance.RegisteredPlayers.Find(p => p.IsYourPlayer);

                if (PlayerFlashComponent == null)
                {
                    PlayerFlashComponent = player.GetComponent<SAIN_Flashlight_Component>();
                }
            }
        }
        public void CreateDetectionPoints(Player player)
        {
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(FlashlightDetection));
            }

            if (player == null || player.IsAI || !player.HealthController.IsAlive)
                return;

            Vector3 playerPosition = player.WeaponRoot.position;

            Vector3 playerLookDirection = player.LookDirection;

            float detectionDistance = 60f;

            if (Physics.Raycast(playerPosition, playerLookDirection, out RaycastHit hit, detectionDistance, LayerMaskClass.HighPolyWithTerrainMask))
            {
                FlashLightPoint = hit.point;
                ExpireDetectionPoint(0.25f);

                PlayerPosition = playerPosition;
                ExpirePlayerPoint(0.25f);
            }
        }

        public void DetectPoints(Player player)
        {
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(FlashlightDetection));
            }

            if (!player.IsAI || player?.AIData?.BotOwner == null)
            {
                return;
            }

            FindYourPlayer();

            if (PlayerFlashComponent == null)
            {
                Logger.LogWarning($"Could not find flashlight component for local player");
                return;
            }

            BotOwner bot = player.AIData.BotOwner;

            if (FlashLightPoint != Vector3.zero && PlayerPosition != Vector3.zero)
            {
                if (bot.LookSensor.IsPointInVisibleSector(FlashLightPoint))
                {
                    Vector3 botPosition = bot.MyHead.position;

                    Vector3 direction = (FlashLightPoint - botPosition).normalized;

                    float rayLength = (FlashLightPoint - botPosition).magnitude;

                    if (!Physics.Raycast(botPosition, direction, rayLength, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        float dispersion = rayLength / 10f;

                        float num2 = MathHelpers.Random(-dispersion, dispersion);
                        float num3 = MathHelpers.Random(-dispersion, dispersion);

                        Vector3 vector = new Vector3(PlayerPosition.x + num2, PlayerPosition.y, PlayerPosition.z + num3);

                        //var target = new GClass270(vector, PlaceForCheckType.suspicious);

                        bot.BotsGroup.AddPointToSearch(vector, 10f, bot, true);

                        //bot.Memory.GoalTarget.SetTarget(target);

                        if (DebugFlash.Value)
                        {
                            Helpers.DebugDrawer.Line(vector, botPosition, 0.005f, Color.red, 3f);
                            Helpers.DebugDrawer.Line(FlashLightPoint, botPosition, 0.005f, Color.red, 3f);
                        }
                    }
                }
            }
        }

        private IEnumerator ExpireDetectionPoint(float delay)
        {
            yield return new WaitForSeconds(delay);
            FlashLightPoint = Vector3.zero;
        }

        private IEnumerator ExpirePlayerPoint(float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayerPosition = Vector3.zero;
        }
    }
    public static class MathHelpers
    {
        public static float Random(float a, float b)
        {
            float num = (float)random_0.NextDouble();
            return a + (b - a) * num;
        }

        private static readonly System.Random random_0 = new System.Random();
    }
    public class TimedVector3
    {
        public TimedVector3(Vector3 point, float timestamp)
        {
            Point = point;
            Timestamp = timestamp;
        }

        public Vector3 Point { get; set; }
        public float Timestamp { get; set; }
    }

}
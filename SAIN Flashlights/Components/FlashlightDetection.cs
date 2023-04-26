using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN_Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static Mono.Security.X509.X520;
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

        private float SearchTime;

        private float RayCastFrequencyTime;

        /// <summary>
        /// Finds the local player and the SAIN_Flashlight_Component if it is not already set.
        /// </summary>
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

        /// <summary>
        /// Creates detection points for the flashlight detection system.
        /// </summary>
        /// <param name="player">The player to create the detection points for.</param>
        public void CreateDetectionPoints(Player player)
        {
            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(FlashlightDetection));

            if (player == null || !player.HealthController.IsAlive)
                return;

            Vector3 playerPosition = player.WeaponRoot.position;

            Vector3 playerLookDirection = player.LookDirection;

            float detectionDistance = 60f;

            // Define the cone angle (in degrees)
            float coneAngle = 10f;

            // Generate random angles within the cone range for yaw and pitch
            float randomYawAngle = UnityEngine.Random.Range(-coneAngle * 0.5f, coneAngle * 0.5f);
            float randomPitchAngle = UnityEngine.Random.Range(-coneAngle * 0.5f, coneAngle * 0.5f);

            // Create a Quaternion rotation based on the random yaw and pitch angles
            Quaternion randomRotation = Quaternion.Euler(randomPitchAngle, randomYawAngle, 0);

            // Rotate the player's look direction by the Quaternion rotation
            Vector3 rotatedLookDirection = randomRotation * playerLookDirection;

            if (Physics.Raycast(playerPosition, rotatedLookDirection, out RaycastHit hit, detectionDistance, LayerMaskClass.HighPolyWithTerrainMask))
            {
                FlashLightPoint = hit.point;
                ExpireDetectionPoint(0.1f);

                PlayerPosition = player.Transform.position;
                ExpirePlayerPoint(0.1f);

                if (DebugFlash.Value)
                    DebugDrawer.Sphere(hit.point, 0.1f, Color.red, 0.25f);
            }
        }

        /// <summary>
        /// Detects and investigates a flashlight for bots.
        /// </summary>
        public void DetectAndInvestigateFlashlight(Player player)
        {
            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(FlashlightDetection));

            if (!player.IsAI || player?.AIData?.BotOwner == null)
                return;

            FindYourPlayer();

            if (PlayerFlashComponent == null)
            {
                Logger.LogError($"Could not find flashlight component for local player");
                return;
            }

            BotOwner bot = player.AIData.BotOwner;
            Vector3 playerPos = PlayerPosition;
            Vector3 flashPos = FlashLightPoint;

            if (flashPos != Vector3.zero && playerPos != Vector3.zero)
            {
                Vector3 botPos = bot.MyHead.position;

                if ((botPos - flashPos).magnitude < 100f)
                {
                    if (CanISeeFlashlight(bot, flashPos))
                    {
                        Vector3 estimatedPosition = EstimateSearchPosition(playerPos, flashPos, botPos, 5f);

                        TryToInvestigate(bot, estimatedPosition, out Vector3 debugHitPos);

                        DebugSearchPosition(bot.name, debugHitPos, flashPos, botPos, 5f, 0.15f, 0.05f);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the bot can see the flashlight based on the bot's position, the player's position, and the flashlight's position.
        /// </summary>
        /// <param name="bot">The bot owner.</param>
        /// <param name="playerPos">The player's position.</param>
        /// <param name="flashPos">The flashlight's position.</param>
        /// <returns>True if the bot can see the flashlight, false otherwise.</returns>
        private bool CanISeeFlashlight(BotOwner bot, Vector3 flashPos)
        {
            if (bot.LookSensor.IsPointInVisibleSector(flashPos))
            {
                Vector3 botPos = bot.Transform.position;

                Vector3 direction = (flashPos - botPos).normalized;

                float rayLength = (flashPos - botPos).magnitude - 0.1f;

                if (RayCastFrequencyTime < Time.time && !Physics.Raycast(botPos, direction, rayLength, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    SetRayCastTimer(0.25f);

                    if (SearchTime < Time.time)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to investigate a visible flashlight beam by estimating a search position and adding it to the bot's group search points.
        /// </summary>
        /// <param name="bot">The bot owner.</param>
        /// <param name="playerPos">The player's position.</param>
        /// <param name="flashPos">The flashbang's position.</param>
        /// <param name="botPos">The bot's position.</param>
        private void TryToInvestigate(BotOwner bot, Vector3 estimatedPosition, out Vector3 debugHitPos)
        {
            debugHitPos = Vector3.zero;

            if (NavMesh.SamplePosition(estimatedPosition, out NavMeshHit hit, 1.0f, -1))
            {
                NavMeshPath searchpath = new NavMeshPath();
                NavMesh.CalculatePath(bot.Transform.position, hit.position, -1, searchpath);

                if (searchpath.status == NavMeshPathStatus.PathInvalid)
                {
                    SetSearchTimer(1f);
                    SetRayCastTimer(1f);

                    if (DebugFlash.Value)
                        Logger.LogDebug($"Path Invalid");
                }
                else
                {
                    SetSearchTimer(5f);
                    SetRayCastTimer(5f);

                    debugHitPos = hit.position;

                    bot.BotsGroup.AddPointToSearch(hit.position, 20f, bot, true);
                }
            }
        }

        /// <summary>
        /// Estimates a search position based on the player, flash, and bot positions, and a dispersion value.
        /// </summary>
        /// <param name="playerPos">The position of the player.</param>
        /// <param name="flashPos">The position of the flashlight point.</param>
        /// <param name="botPos">The position of the bot.</param>
        /// <param name="dispersion">What the distance to enemy is divided by to produce dispersion. Higher is more accurate</param>
        /// <returns>The estimated search position.</returns>
        private Vector3 EstimateSearchPosition(Vector3 playerPos, Vector3 flashPos, Vector3 botPos, float dispersion)
        {
            Vector3 estimatedPosition = Vector3.Lerp(playerPos, flashPos, Random.Range(0.0f, 0.25f));

            float distance = (playerPos - botPos).magnitude;

            float maxDispersion = Mathf.Clamp(distance, 0f, 20f);

            float positionDispersion = maxDispersion / dispersion;

            float x = SAIN_Math.Random(-positionDispersion, positionDispersion);
            float z = SAIN_Math.Random(-positionDispersion, positionDispersion);

            return new Vector3(estimatedPosition.x + x, estimatedPosition.y, estimatedPosition.z + z);
        }

        /// <summary>
        /// Sets the search timer to the current time plus the given duration.
        /// </summary>
        /// <param name="duration">The duration to add to the current time.</param>
        private void SetSearchTimer(float duration)
        {
            SearchTime = Time.time + duration;
        }

        /// <summary>
        /// Sets the RayCastFrequencyTime to the current time plus the given duration.
        /// </summary>
        /// <param name="duration">The duration to add to the current time.</param>
        private void SetRayCastTimer(float duration)
        {
            RayCastFrequencyTime = Time.time + duration;
        }

        /// <summary>
        /// Draws debug lines and spheres to visualize the search position of a bot.
        /// </summary>
        /// <param name="name">Name of the bot.</param>
        /// <param name="hitpos">Position of the hit.</param>
        /// <param name="flashpos">Position of the flashlight.</param>
        /// <param name="botpos">Position of the bot.</param>
        /// <param name="expiretime">Time until the debug lines and spheres expire.</param>
        /// <param name="spheresize">Size of the debug spheres.</param>
        /// <param name="linesize">Size of the debug lines.</param>
        private void DebugSearchPosition(string name, Vector3 hitpos, Vector3 flashpos, Vector3 botpos, float expiretime, float spheresize, float linesize)
        {
            if (DebugFlash.Value)
            {
                Logger.LogDebug($"{name} Is Investigating Flashlight Beam");

                Helpers.DebugDrawer.Sphere(hitpos, spheresize, Color.red, expiretime);
                Helpers.DebugDrawer.Sphere(flashpos, spheresize, Color.red, expiretime);

                Helpers.DebugDrawer.Line(flashpos, botpos, linesize, Color.red, expiretime);
                Helpers.DebugDrawer.Line(hitpos, botpos, linesize, Color.red, expiretime);
                Helpers.DebugDrawer.Line(hitpos, flashpos, linesize, Color.red, expiretime);
            }
        }

        /// <summary>
        /// Waits for a specified amount of time before setting the FlashLightPoint to Vector3.zero.
        /// </summary>
        /// <param name="delay">The amount of time to wait before setting the FlashLightPoint.</param>
        /// <returns>An IEnumerator object.</returns>
        private IEnumerator ExpireDetectionPoint(float delay)
        {
            yield return new WaitForSeconds(delay);
            FlashLightPoint = Vector3.zero;
        }

        /// <summary>
        /// Waits for a specified delay and then sets the PlayerPosition to Vector3.zero.
        /// </summary>
        /// <param name="delay">The delay to wait before setting the PlayerPosition.</param>
        /// <returns>An IEnumerator object.</returns>
        private IEnumerator ExpirePlayerPoint(float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayerPosition = Vector3.zero;
        }
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
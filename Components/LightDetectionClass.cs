using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Components
{
    public class LightDetectionClass
    {
        public LightDetectionClass()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);

            LocalPlayer = Singleton<GameWorld>.Instance.RegisteredPlayers.Find(p => p.IsYourPlayer) as Player;
            PlayerFlashComponent = LocalPlayer.GetComponent<SAINFlashLightComponent>();
        }

        public void CreateDetectionPoints(Player player)
        {
            if (player == null || !player.HealthController.IsAlive)
            {
                return;
            }

            Vector3 playerPosition = player.WeaponRoot.position;

            Vector3 playerLookDirection = player.LookDirection;

            float detectionDistance = 60f;

            // Define the cone angle (in degrees)
            float coneAngle = 10f;

            // Generate random angles within the cone range for yaw and pitch
            float randomYawAngle = Random.Range(-coneAngle * 0.5f, coneAngle * 0.5f);
            float randomPitchAngle = Random.Range(-coneAngle * 0.5f, coneAngle * 0.5f);

            // AddColor a Quaternion rotation based on the random yaw and pitch angles
            Quaternion randomRotation = Quaternion.Euler(randomPitchAngle, randomYawAngle, 0);

            // Rotate the player's look direction by the Quaternion rotation
            Vector3 rotatedLookDirection = randomRotation * playerLookDirection;

            if (Physics.Raycast(playerPosition, rotatedLookDirection, out RaycastHit hit, detectionDistance, LayerMaskClass.HighPolyWithTerrainMask))
            {
                FlashLightPoint = hit.point;
                ExpireDetectionPoint(0.1f);

                PlayerPosition = player.Transform.position;
                ExpirePlayerPoint(0.1f);

                if (DebugFlash)
                {
                    DebugGizmos.SingleObjects.Sphere(hit.point, 0.1f, Color.red, true, 0.25f);
                }
            }
        }

        static bool DebugFlash => SAINPlugin.LoadedPreset.GlobalSettings.Flashlight.DebugFlash;

        public void DetectAndInvestigateFlashlight(Player player)
        {
            if (!player.IsAI)
            {
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
                    }
                }
            }
        }

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

                    if (DebugFlash)
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
        /// Estimates a search DrawPosition based on the player, flash, and bot positions, and a dispersion rounding.
        /// </summary>
        /// <param name="playerPos">The DrawPosition of the player.</param>
        /// <param name="flashPos">The DrawPosition of the flashlight point.</param>
        /// <param name="botPos">The DrawPosition of the bot.</param>
        /// <param name="dispersion">What the Distance to enemy is divided by to produce dispersion. Higher is more accurate</param>
        /// <returns>The estimated search DrawPosition.</returns>
        private Vector3 EstimateSearchPosition(Vector3 playerPos, Vector3 flashPos, Vector3 botPos, float dispersion)
        {
            Vector3 estimatedPosition = Vector3.Lerp(playerPos, flashPos, Random.Range(0.0f, 0.25f));

            float distance = (playerPos - botPos).magnitude;

            float maxDispersion = Mathf.Clamp(distance, 0f, 20f);

            float positionDispersion = maxDispersion / dispersion;

            float x = EFTMath.Random(-positionDispersion, positionDispersion);
            float z = EFTMath.Random(-positionDispersion, positionDispersion);

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

        public static Vector3 PlayerPosition { get; private set; }
        public static Vector3 FlashLightPoint { get; private set; }
        public static SAINFlashLightComponent PlayerFlashComponent { get; private set; }
        private static Player LocalPlayer { get; set; }
        protected static ManualLogSource Logger { get; private set; }

        private float SearchTime;

        private float RayCastFrequencyTime;
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
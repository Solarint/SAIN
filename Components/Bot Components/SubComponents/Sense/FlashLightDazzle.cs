using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using UnityEngine;
using static SAIN.UserSettings.DazzleConfig;

namespace SAIN.Helpers
{
    public class FlashLightDazzle : SAINBot
    {
        public FlashLightDazzle(BotOwner owner) : base(owner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private readonly ManualLogSource Logger;

        public void CheckIfDazzleApplied(IAIDetails person)
        {
            if (person == null || !(person is Player player))
            {
                return;
            }
            if (player.TryGetComponent<FlashLightComponent>(out var flashlight))
            {
                if (flashlight.WhiteLight)
                {
                    EnemyWithFlashlight(person);
                }
                else if (flashlight.Laser)
                {
                    EnemyWithLaser(person);
                }
                else
                {
                    if (BotOwner.NightVision.UsingNow)
                    {
                        if (flashlight.IRLight)
                        {
                            EnemyWithFlashlight(person);
                        }
                        else if (flashlight.IRLaser)
                        {
                            EnemyWithLaser(person);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the enemy is within range of the flashlight and applies dazzle and gain sight modifiers if so.
        /// </summary>
        /// <param name="BotOwner">The BotOwner object.</param>
        /// <param name="person">The IAIDetails object.</param>
        public void EnemyWithFlashlight(IAIDetails person)
        {
            Vector3 position = BotOwner.MyHead.position;
            Vector3 weaponRoot = person.WeaponRoot.position;
            float enemyDist = (position - weaponRoot).magnitude;

            if (enemyDist < 80f)
            {
                if (FlashLightVisionCheck(person))
                {
                    if (!Physics.Raycast(weaponRoot, (position - weaponRoot).normalized, (position - weaponRoot).magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        float gainSight = GetGainSightModifier(enemyDist);

                        float dazzlemodifier = 1f;

                        if (enemyDist < MaxDazzleRange.Value)
                            dazzlemodifier = GetDazzleModifier(person);

                        ApplyDazzle(dazzlemodifier, gainSight);

                        DebugLogs(dazzlemodifier, gainSight, enemyDist);
                    }
                }
            }
        }

        /// <summary>
        /// Applies dazzle to the enemy if they are within the max dazzle range and the raycast between the BotOwner and the enemy is not blocked.
        /// </summary>
        public void EnemyWithLaser(IAIDetails person)
        {
            Vector3 position = BotOwner.MyHead.position;
            Vector3 weaponRoot = person.WeaponRoot.position;
            float enemyDist = (position - weaponRoot).magnitude;

            if (enemyDist < 80f)
            {
                if (LaserVisionCheck(person))
                {
                    if (!Physics.Raycast(weaponRoot, (position - weaponRoot).normalized, (position - weaponRoot).magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        float gainSight = GetGainSightModifier(enemyDist);

                        float dazzlemodifier = 1f;
                        if (enemyDist < MaxDazzleRange.Value)
                        {
                            dazzlemodifier = GetDazzleModifier(person);
                        }

                        ApplyDazzle(dazzlemodifier, gainSight);

                        DebugLogs(dazzlemodifier, gainSight, enemyDist);
                    }
                }
            }
        }

        /// <summary>
        /// Logs debug information about the BotOwner's settings and the current dazzle, gainsight, and Distance values.
        /// </summary>
        /// <param name="BotOwner">The BotOwner owner.</param>
        /// <param name="dazzlemod">The dazzle intensity.</param>
        /// <param name="gainsightmod">The gainsight modifier.</param>
        /// <param name="distance">The Distance.</param>
        private void DebugLogs(float dazzlemod, float gainsightmod, float distance)
        {
            if (DebugFlash.Value)
            {
                var current = BotOwner.Settings.Current;
                Logger.LogDebug($"" +
                    $"_scatteringCoef: [{current._scatteringCoef}], " +
                    $"_accuratySpeedCoef: [{current._accuratySpeedCoef}], " +
                    $"_precicingSpeedCoef: [{current._precicingSpeedCoef}], " +
                    $"_priorityScatteringCoef: [{current._priorityScatteringCoef}]");
                Logger.LogDebug($"" +
                    $"CurrentAccuratySpeed: [{current.CurrentAccuratySpeed}], " +
                    $"CurrentPrecicingSpeed: [{current.CurrentPrecicingSpeed}], " +
                    $"CurrentScattering: [{current.CurrentScattering}], " +
                    $"PriorityScatter10meter: [{current.PriorityScatter10meter}]");
                Logger.LogInfo($"Dazzle Intensity: [{dazzlemod}], GainSightModifier: [{gainsightmod}], distance : [{distance}]");
            }
        }

        /// <summary>
        /// Applies dazzle modifications to the BotOwner's settings for 0.1 seconds.
        /// </summary>
        /// <param name="dazzleModif">The dazzle modification.</param>
        /// <param name="gainSightModif">The gain sight modification.</param>
        /// <param name="BotOwner">The BotOwner to apply the modifications to.</param>
        private void ApplyDazzle(float dazzleModif, float gainSightModif)
        {
            var modif = new GClass561
            {
                PrecicingSpeedCoef = Mathf.Clamp(dazzleModif, 1f, 5f) * Effectiveness.Value,
                AccuratySpeedCoef = Mathf.Clamp(dazzleModif, 1f, 5f) * Effectiveness.Value,
                LayChanceDangerCoef = 1f,
                VisibleDistCoef = 1f,
                GainSightCoef = gainSightModif,
                ScatteringCoef = Mathf.Clamp(dazzleModif, 1f, 2.5f) * Effectiveness.Value,
                PriorityScatteringCoef = Mathf.Clamp(dazzleModif, 1f, 2.5f) * Effectiveness.Value,
                HearingDistCoef = 1f,
                TriggerDownDelay = 1f,
                WaitInCoverCoef = 1f
            };

            BotOwner.Settings.Current.Apply(modif, 0.1f);
        }

        /// <summary>
        /// Checks if the enemy is looking at the BotOwner using a flashlight vision check.
        /// </summary>
        /// <param name="BotOwner">The BotOwner to check.</param>
        /// <param name="person">The enemy to check.</param>
        /// <returns>True if the enemy is looking at the BotOwner, false otherwise.</returns>
        private bool FlashLightVisionCheck(IAIDetails person)
        {
            Vector3 position = BotOwner.MyHead.position;
            Vector3 weaponRoot = person.WeaponRoot.position;

            float flashAngle = Mathf.Clamp(0.9770526f, 0.8f, 1f);
            bool enemylookatme = VectorHelpers.IsAngLessNormalized(VectorHelpers.NormalizeFastSelf(position - weaponRoot), person.LookDirection, flashAngle);

            return enemylookatme;
        }

        /// <summary>
        /// Checks if the enemy is looking at the BotOwner using a laser vision check.
        /// </summary>
        /// <param name="bot">The BotOwner to check.</param>
        /// <param name="person">The enemy to check.</param>
        /// <returns>True if the enemy is looking at the BotOwner, false otherwise.</returns>
        private bool LaserVisionCheck(IAIDetails person)
        {
            Vector3 position = BotOwner.MyHead.position;
            Vector3 weaponRoot = person.WeaponRoot.position;

            float laserAngle = 0.990f;
            bool enemylookatme = VectorHelpers.IsAngLessNormalized(VectorHelpers.NormalizeFastSelf(position - weaponRoot), person.LookDirection, laserAngle);

            return enemylookatme;
        }

        /// <summary>
        /// Calculates the dazzle modifier for a given BotOwner and Enemy.
        /// </summary>
        /// <param name="___botOwner_0">The BotOwner to calculate the dazzle modifier for.</param>
        /// <param name="person">The Enemy Shining the flashlight</param>
        /// <returns>The calculated dazzle modifier.</returns>
        private float GetDazzleModifier(IAIDetails person)
        {
            Vector3 position = BotOwner.MyHead.position;
            Vector3 weaponRoot = person.WeaponRoot.position;
            float enemyDist = (position - weaponRoot).magnitude;

            float dazzlemodifier = 1f - (enemyDist / MaxDazzleRange.Value);
            dazzlemodifier = (2 * dazzlemodifier) + 1f;

            if (BotOwner.NightVision.UsingNow)
            {
                dazzlemodifier *= 1.5f;
            }

            return dazzlemodifier;
        }

        /// <summary>
        /// Calculates the gain sight modifier based on the Distance to the enemy.
        /// </summary>
        /// <param name="enemyDist">The Distance to the enemy.</param>
        /// <returns>The gain sight modifier.</returns>
        private float GetGainSightModifier(float enemyDist)
        {
            float gainsightdistance = Mathf.Clamp(enemyDist, 25f, 80f);
            float gainsightmodifier = gainsightdistance / 80f;
            float gainsightscaled = gainsightmodifier * 0.25f + 0.75f;
            return gainsightscaled;
        }
    }
}
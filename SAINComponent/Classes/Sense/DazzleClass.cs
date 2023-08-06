using BepInEx.Logging;
using EFT;
using SAIN.Components;
using UnityEngine;
using static SAIN.Helpers.HelpersGClass;
using SAIN.Helpers;

namespace SAIN.SAINComponent.Classes.Sense
{
    public class DazzleClass : SAINBase, ISAINClass
    {
        public DazzleClass(SAINComponentClass owner) : base(owner)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }


        public void CheckIfDazzleApplied(IAIDetails person)
        {
            var flashlight = SAIN.FlashLight;
            if (flashlight != null)
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

                        if (enemyDist < MaxDazzleRange)
                            dazzlemodifier = GetDazzleModifier(person);

                        ApplyDazzle(dazzlemodifier, gainSight);
                    }
                }
            }
        }

        /// <summary>
        /// Applies dazzle to the enemy if they are within the Max dazzle range and the raycast between the BotOwner and the enemy is not blocked.
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
                        if (enemyDist < MaxDazzleRange)
                        {
                            dazzlemodifier = GetDazzleModifier(person);
                        }

                        ApplyDazzle(dazzlemodifier, gainSight);
                    }
                }
            }
        }

        static float MaxDazzleRange => SAINPlugin.LoadedPreset.GlobalSettings.Flashlight.MaxDazzleRange;
        static float Effectiveness => SAINPlugin.LoadedPreset.GlobalSettings.Flashlight.DazzleEffectiveness;

        private void ApplyDazzle(float dazzleModif, float gainSightModif)
        {
            float PrecicingSpeedCoef = Mathf.Clamp(dazzleModif, 1f, 5f) * Effectiveness;
            float AccuratySpeedCoef = Mathf.Clamp(dazzleModif, 1f, 5f) * Effectiveness;
            float ScatteringCoef = Mathf.Clamp(dazzleModif, 1f, 2.5f) * Effectiveness;
            float PriorityScatteringCoef = Mathf.Clamp(dazzleModif, 1f, 2.5f) * Effectiveness;

            BotStatModifiers Modifiers = new BotStatModifiers
                (
                    PrecicingSpeedCoef,
                    AccuratySpeedCoef,
                    gainSightModif,
                    ScatteringCoef,
                    PriorityScatteringCoef
                );

            BotOwner.Settings.Current.Apply(Modifiers.Modifiers, 0.1f);
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

            float dazzlemodifier = 1f - enemyDist / MaxDazzleRange;
            dazzlemodifier = 2 * dazzlemodifier + 1f;

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
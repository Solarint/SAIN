using BepInEx.Logging;
using EFT;
using System;
using UnityEngine;
using static SAIN_Flashlights.Config.DazzleConfig;
using SAIN_Helpers;

namespace SAIN_Flashlights.Helpers
{
    public class FlashLight
    {
        protected static ManualLogSource Logger { get; private set; }

        /// <summary>
        /// Checks if the enemy is within range of the flashlight and applies dazzle and gain sight modifiers if so.
        /// </summary>
        /// <param name="bot">The BotOwner object.</param>
        /// <param name="person">The IAIDetails object.</param>
        public static void EnemyWithFlashlight(BotOwner bot, IAIDetails person)
        {
            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(FlashLight));

            Vector3 position = bot.MyHead.position;
            Vector3 weaponRoot = person.WeaponRoot.position;
            float enemyDist = (position - weaponRoot).magnitude;

            if (enemyDist < 80f)
            {
                if (FlashLightVisionCheck(bot, person))
                {
                    if (!Physics.Raycast(weaponRoot, (position - weaponRoot).normalized, (position - weaponRoot).magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        DebugDraw(weaponRoot, position);

                        if (SillyMode.Value)
                        {
                            FunnyHelpers.FunnyMode(bot, position);
                            return;
                        }

                        float gainSight = GetGainSightModifier(enemyDist);

                        float dazzlemodifier = 1f;

                        if (enemyDist < MaxDazzleRange.Value)
                            dazzlemodifier = GetDazzleModifier(bot, person);

                        ApplyDazzle(dazzlemodifier, gainSight, bot);

                        DebugLogs(bot, dazzlemodifier, gainSight, enemyDist);
                    }
                }
            }
        }

        /// <summary>
        /// Applies dazzle to the enemy if they are within the max dazzle range and the raycast between the bot and the enemy is not blocked.
        /// </summary>
        public static void EnemyWithLaser(BotOwner bot, IAIDetails person)
        {
            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(FlashLight));

            Vector3 position = bot.MyHead.position;
            Vector3 weaponRoot = person.WeaponRoot.position;
            float enemyDist = (position - weaponRoot).magnitude;

            if (enemyDist < 80f)
            {
                if (LaserVisionCheck(bot, person))
                {
                    if (!Physics.Raycast(weaponRoot, (position - weaponRoot).normalized, (position - weaponRoot).magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        if (SillyMode.Value)
                        {
                            FunnyHelpers.FunnyMode(bot, position);
                            return;
                        }

                        float gainSight = GetGainSightModifier(enemyDist);

                        float dazzlemodifier = 1f;
                        if (enemyDist < MaxDazzleRange.Value)
                        {
                            dazzlemodifier = GetDazzleModifier(bot, person);
                        }

                        ApplyDazzle(dazzlemodifier, gainSight, bot);

                        DebugLogs(bot, dazzlemodifier, gainSight, enemyDist);
                    }
                }
            }
        }

        /// <summary>
        /// Logs debug information about the bot's settings and the current dazzle, gainsight, and distance values.
        /// </summary>
        /// <param name="bot">The bot owner.</param>
        /// <param name="dazzlemod">The dazzle intensity.</param>
        /// <param name="gainsightmod">The gainsight modifier.</param>
        /// <param name="distance">The distance.</param>
        private static void DebugLogs(BotOwner bot, float dazzlemod, float gainsightmod, float distance)
        {
            if (DebugFlash.Value)
            {
                var current = bot.Settings.Current;
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
        /// Draws debug lines and spheres to visualize the position and weapon root of an enemy.
        /// </summary>
        /// <param name="position">The position of the enemy.</param>
        /// <param name="weaponRoot">The weapon root of the enemy.</param>
        /// <param name="enemylookatme">Whether the enemy is looking at the player.</param>
        private static void DebugDraw(Vector3 position, Vector3 weaponRoot)
        {
            if (DebugFlash.Value)
            {
                SAIN_Helpers.DebugDrawer.Sphere(position - weaponRoot, 0.05f, Color.white, 1f);
                SAIN_Helpers.DebugDrawer.Line(position, weaponRoot, 0.01f, Color.green, 1f);
                SAIN_Helpers.DebugDrawer.Line(weaponRoot, position, 0.01f, Color.red, 1f);
            }
        }

        /// <summary>
        /// Applies dazzle modifications to the bot's settings for 0.1 seconds.
        /// </summary>
        /// <param name="dazzleModif">The dazzle modification.</param>
        /// <param name="gainSightModif">The gain sight modification.</param>
        /// <param name="bot">The bot to apply the modifications to.</param>
        private static void ApplyDazzle(float dazzleModif, float gainSightModif, BotOwner bot)
        {
            GClass557 modif = new GClass557
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

            if (AIHatesFlashlights.Value)
            {
                FunnyHelpers.RandomVoiceLine(bot);
            }

            bot.Settings.Current.Apply(modif, 0.1f);
        }

        /// <summary>
        /// Checks if the enemy is looking at the bot using a flashlight vision check.
        /// </summary>
        /// <param name="bot">The bot to check.</param>
        /// <param name="person">The enemy to check.</param>
        /// <returns>True if the enemy is looking at the bot, false otherwise.</returns>
        private static bool FlashLightVisionCheck(BotOwner bot, IAIDetails person)
        {
            Vector3 position = bot.MyHead.position;
            Vector3 weaponRoot = person.WeaponRoot.position;

            float flashAngle = Mathf.Clamp(0.9770526f, 0.8f, 1f);
            bool enemylookatme = SAIN_Math.IsAngLessNormalized(SAIN_Math.NormalizeFastSelf(position - weaponRoot), person.LookDirection, flashAngle);

            return enemylookatme;
        }

        /// <summary>
        /// Checks if the enemy is looking at the bot using a laser vision check.
        /// </summary>
        /// <param name="bot">The bot to check.</param>
        /// <param name="person">The enemy to check.</param>
        /// <returns>True if the enemy is looking at the bot, false otherwise.</returns>
        private static bool LaserVisionCheck(BotOwner bot, IAIDetails person)
        {
            Vector3 position = bot.MyHead.position;
            Vector3 weaponRoot = person.WeaponRoot.position;

            float laserAngle = 0.990f;
            bool enemylookatme = SAIN_Math.IsAngLessNormalized(SAIN_Math.NormalizeFastSelf(position - weaponRoot), person.LookDirection, laserAngle);

            return enemylookatme;
        }

        /// <summary>
        /// Calculates the dazzle modifier for a given BotOwner and Enemy.
        /// </summary>
        /// <param name="___botOwner_0">The BotOwner to calculate the dazzle modifier for.</param>
        /// <param name="person">The Enemy Shining the flashlight</param>
        /// <returns>The calculated dazzle modifier.</returns>
        private static float GetDazzleModifier(BotOwner ___botOwner_0, IAIDetails person)
        {
            BotOwner bot = ___botOwner_0;
            Vector3 position = bot.MyHead.position;
            Vector3 weaponRoot = person.WeaponRoot.position;
            float enemyDist = (position - weaponRoot).magnitude;

            float dazzlemodifier = 1f - (enemyDist / MaxDazzleRange.Value);
            dazzlemodifier = (2 * dazzlemodifier) + 1f;

            if (bot.NightVision.UsingNow)
            {
                dazzlemodifier *= 1.5f;
            }

            return dazzlemodifier;
        }

        /// <summary>
        /// Calculates the gain sight modifier based on the distance to the enemy.
        /// </summary>
        /// <param name="enemyDist">The distance to the enemy.</param>
        /// <returns>The gain sight modifier.</returns>
        private static float GetGainSightModifier(float enemyDist)
        {
            float gainsightdistance = Mathf.Clamp(enemyDist, 25f, 80f);
            float gainsightmodifier = gainsightdistance / 80f;
            float gainsightscaled = gainsightmodifier * 0.4f + 0.6f;
            return gainsightscaled;
        }
    }

    public class FunnyHelpers
    {
        private static float funnytimer = 0f;

        public static void FunnyMode(BotOwner bot, Vector3 position)
        {
            if (funnytimer < Time.time)
            {
                funnytimer = Time.time + 0.25f;
                bot.FlashGrenade.AddBlindEffect(1f, position);
                bot.FlashGrenade.Activate();
                bot.FlashGrenade.ShallShoot();
                bot.FriendlyTilt.Activate();

                float randomfunny = UnityEngine.Random.value;
                if (randomfunny > 0.9f)
                {
                    bot.GetPlayer.Say(EPhraseTrigger.NeedHelp, false, 0.2f, ETagStatus.Combat, 100, true);
                }
                else if (randomfunny <= 0.9f && randomfunny > 0.8f)
                {
                    bot.GetPlayer.Say(EPhraseTrigger.LostVisual, false, 0.2f, ETagStatus.Combat, 100, true);
                }
                else if (randomfunny <= 0.8f && randomfunny > 0.7f)
                {
                    bot.GetPlayer.Say(EPhraseTrigger.FriendlyFire, false, 0.2f, ETagStatus.Combat, 100, true);
                }
                else if (randomfunny <= 0.7f && randomfunny > 0.6f)
                {
                    bot.GetPlayer.Say(EPhraseTrigger.LostVisual, false, 0.2f, ETagStatus.Combat, 100, true);
                }
                else if (randomfunny <= 0.6f && randomfunny > 0.5f)
                {
                    bot.GetPlayer.Say(EPhraseTrigger.NeedHelp, false, 0.2f, ETagStatus.Combat, 100, true);
                }
                else if (randomfunny <= 0.5f && randomfunny > 0.4f)
                {
                    bot.GetPlayer.Say(EPhraseTrigger.LostVisual, false, 0.2f, ETagStatus.Combat, 100, true);
                }
                else if (randomfunny <= 0.4f && randomfunny > 0.3f)
                {
                    bot.GetPlayer.Say(EPhraseTrigger.SniperPhrase, false, 0.2f, ETagStatus.Combat, 100, true);
                }
                else if (randomfunny <= 0.3f && randomfunny > 0.2f)
                {
                    bot.GetPlayer.Say(EPhraseTrigger.NeedHelp, true, 0.2f, ETagStatus.Combat, 100, false);
                }
                else if (randomfunny <= 0.2f && randomfunny > 0.1f)
                {
                    bot.GetPlayer.Say(EPhraseTrigger.Stop, true, 0.3f, ETagStatus.Combat, 100, false);
                }
                else
                {
                    bot.GetPlayer.Say(EPhraseTrigger.Stop, true, 0.4f, ETagStatus.Combat, 100, true);
                }
            }
        }

        public static void RandomVoiceLine(BotOwner bot)
        {
            float randomphrase = UnityEngine.Random.value;
            if (randomphrase > 0.97f)
            {
                bot.BotTalk.Say(EPhraseTrigger.InTheFront, false, ETagStatus.Combat);
            }
            else if (randomphrase < 0.03f)
            {
                bot.BotTalk.Say(EPhraseTrigger.MumblePhrase, false, ETagStatus.Combat);
            }
        }
    }
}
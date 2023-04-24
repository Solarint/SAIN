using BepInEx.Logging;
using EFT;
using System;
using System.Linq;
using UnityEngine;
using static SAIN.Flashlights.Config.DazzleConfig;

namespace SAIN.Flashlights.Helpers
{
    public class FlashLight
    {
        private GameObject _flashlight;

        private GameObject[] _modes;

        protected static ManualLogSource Logger { get; private set; }

        public static void EnemyWithFlashlight(BotOwner bot, IAIDetails person, float nexttimecheck)
        {
            if (Logger == null) Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(FlashLight));

            Vector3 position = bot.MyHead.position;
            Vector3 weaponRoot = person.WeaponRoot.position;
            float enemyDist = (position - weaponRoot).magnitude;

            if (enemyDist < 80f)
            {
                if (FlashLightVisionCheck(bot, person))
                {
                    if (!Physics.Raycast(weaponRoot, (position - weaponRoot).normalized, (position - weaponRoot).magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        if (SillyMode.Value)
                        {
                            NonStaticHelpers voice = new NonStaticHelpers();
                            voice.FunnyMode(bot, position);
                            return;
                        }

                        float gainSight = GetGainSightModifier(enemyDist);

                        float dazzlemodifier = 1f;
                        if (enemyDist < MaxDazzleRange.Value)
                        {
                            dazzlemodifier = GetDazzleModifier(bot, person);
                        }

                        ApplyDazzle(dazzlemodifier, gainSight, bot, nexttimecheck);

                        if (DebugFlash.Value)
                        {
                            Logger.LogDebug($"Dazzle Intensity: [{dazzlemodifier}], GainSightModifier: [{gainSight}], distance : [{enemyDist}]");
                        }
                    }
                }
            }
        }

        private static void ApplyDazzle(float dazzleModif, float gainSightModif, BotOwner bot, float nexttimecheck)
        {
            GClass557 modif = new GClass557
            {
                PrecicingSpeedCoef = Mathf.Clamp(dazzleModif, 1f, 2f),
                AccuratySpeedCoef = Mathf.Clamp(dazzleModif, 1f, 2f),
                LayChanceDangerCoef = 1f,
                VisibleDistCoef = 1f,
                GainSightCoef = gainSightModif,
                ScatteringCoef = Mathf.Clamp(dazzleModif, 1f, 1.5f),
                PriorityScatteringCoef = Mathf.Clamp(dazzleModif, 1f, 1.5f),
                HearingDistCoef = 1f,
                TriggerDownDelay = 1f,
                WaitInCoverCoef = 1f
            };

            bot.Settings.Current.Apply(modif, nexttimecheck);
        }

        private static bool FlashLightVisionCheck(BotOwner bot, IAIDetails person)
        {
            Vector3 position = bot.MyHead.position;
            Vector3 weaponRoot = person.WeaponRoot.position;

            float flashAngle = Mathf.Clamp(0.9770526f * Angle.Value, 0.8f, 1f);
            bool enemylookatme = IsAngLessNormalized(NormalizeFastSelf(position - weaponRoot), person.LookDirection, flashAngle);

            return enemylookatme;
        }

        private static float GetDazzleModifier(BotOwner ___botOwner_0, IAIDetails person)
        {
            BotOwner bot = ___botOwner_0;
            Vector3 position = bot.MyHead.position;
            Vector3 weaponRoot = person.WeaponRoot.position;
            float enemyDist = (position - weaponRoot).magnitude;

            float dazzlemodifier = 1f - (enemyDist / MaxDazzleRange.Value);
            dazzlemodifier = Mathf.Clamp(dazzlemodifier, 0.1f, 1.0f);
            dazzlemodifier += 1.33f;
            dazzlemodifier *= Effectiveness.Value;

            if (bot.NightVision.UsingNow)
            {
                dazzlemodifier *= 1.5f;
            }

            if (AIHatesFlashlights.Value)
            {
                NonStaticHelpers voice = new NonStaticHelpers();
                voice.RandomVoiceLine(bot);
            }

            return dazzlemodifier;
        }
        private static float GetGainSightModifier(float enemyDist)
        {
            float gainsightdistance = Mathf.Clamp(enemyDist, 25f, 80f);
            float gainsightmodifier = gainsightdistance / 80f;
            float gainsightscaled = gainsightmodifier * 0.4f + 0.6f;
            return gainsightscaled;
        }

        private static bool IsAngLessNormalized(Vector3 a, Vector3 b, float cos)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z > cos;
        }

        private static Vector3 NormalizeFastSelf(Vector3 v)
        {
            float num = (float)Math.Sqrt((double)(v.x * v.x + v.y * v.y + v.z * v.z));
            v.x /= num;
            v.y /= num;
            v.z /= num;
            return v;
        }

        private void FlashLightMode(Player player)
        {
            _flashlight = player.GetComponentInChildren<TacticalComboVisualController>()?.gameObject;

            _modes = Array.ConvertAll(_flashlight.GetComponentsInChildren<Transform>(true), y => y.gameObject).Where(x => x.name.Contains("mode_")).ToArray();

            int currentMode = -1;
            for (int i = 0; i < _modes.Length; i++)
            {
                if (_modes[i].activeSelf)
                {
                    currentMode = i;
                    break;
                }
            }
            if (currentMode != -1)
            {
                Console.WriteLine($"Current flashlight mode: {currentMode}");
            }
            else
            {
                Console.WriteLine("No mode is currently active.");
            }
        }
    }

    public class NonStaticHelpers
    {
        private static float funnytimer = 0f;

        public void FunnyMode(BotOwner bot, Vector3 position)
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

        public void RandomVoiceLine(BotOwner bot)
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
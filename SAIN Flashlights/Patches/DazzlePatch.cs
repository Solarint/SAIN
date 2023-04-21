using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using static SAIN.Flashlights.Config.DazzleConfig;

namespace SAIN.Flashlights.Patches
{
    public class FlashLightDazzle : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass475), "CheckLookEnemy");
        }
        [PatchPrefix]
        public static void Prefix(ref BotOwner ___botOwner_0, IAIDetails person, ref float ___NextTimeCheck)
        {
            if (___NextTimeCheck < Time.time)
            {
                BotOwner bot = ___botOwner_0;
                bool lightOn = person.AIData.UsingLight;
                float enemyDist = (bot.Transform.position - person.Transform.position).magnitude;

                if (lightOn && enemyDist < MaxDazzleRange.Value)
                {
                    Vector3 position = bot.MyHead.position;
                    Vector3 weaponRoot = person.WeaponRoot.position;

                    float flashAngle = Mathf.Clamp(0.9770526f * Angle.Value, 0.8f, 1f);

                    bool enemylookatme = GClass782.IsAngLessNormalized(GClass782.NormalizeFastSelf(position - weaponRoot), person.LookDirection, flashAngle);

                    if (enemylookatme)
                    {
                        Vector3 enemyDirection = (position - weaponRoot).normalized;
                        float raycastLength = (position - weaponRoot).magnitude;
                        LayerMask mask = LayerMaskClass.HighPolyWithTerrainMask;

                        if (!Physics.Raycast(weaponRoot, enemyDirection, raycastLength, mask))
                        {
                            float distancemodifier = 1f - (enemyDist / MaxDazzleRange.Value);

                            distancemodifier = Mathf.Clamp(distancemodifier, 0.1f, 1.0f);

                            distancemodifier += 1.33f;

                            distancemodifier *= Effectiveness.Value;

                            if (AIHatesFlashlights.Value)
                            {
                                float randomphrase = UnityEngine.Random.value;
                                if (randomphrase > 0.95f)
                                {
                                    bot.GetPlayer.Say(EPhraseTrigger.OnAgony, false, 0.2f, ETagStatus.Combat, 10, true);
                                }
                                else if (randomphrase < 0.05f)
                                {
                                    bot.GetPlayer.Say(EPhraseTrigger.OnFight, false, 0.2f, ETagStatus.Combat, 10, true);
                                }
                            }

                            float blindtime = 0.5f;

                            if (bot.NightVision.UsingNow)
                            {
                                distancemodifier *= 1.5f;
                                blindtime = 1f;
                            }

                            //bot.GetPlayer.ActiveHealthController.DoStun(0.5f, 1f);
                            //bot.GetPlayer.HandsAnimator.Gesture(EGesture.Hello);

                            GClass557 modif = new GClass557(
                                        distancemodifier, //PRECICING, 
                                        distancemodifier, //ACCURATY, 
                                        1.0f, //LAY_CHANCE, 
                                        1.0f, //VISION_DIST, 
                                        1.0f, //GAIN_SIGHT, 
                                        distancemodifier, //SCATTERING, 
                                        1.0f, //HEARING, 
                                        distancemodifier, //SCATTERING,
                                        1f);

                            // what am i doing with my life
                            if (SillyMode.Value)
                            {
                                funny.funnymode(bot, position);
                                return;
                            }
                            else
                            {
                                bot.Settings.Current.Apply(modif, blindtime);
                            }

                            if (DebugFlash.Value) System.Console.WriteLine($"Dazzle Intensity: [{distancemodifier}], distance : [{enemyDist}] Angle number: [{Angle.Value}]");
                        }
                    }
                }
            }
        }
    }
    public class funny
    {
        private static float funnytimer = 0f;
        public static void funnymode(BotOwner bot, Vector3 position)
        {
            if (funnytimer < Time.time)
            {
                funnytimer = Time.time + 0.25f;
                bot.SetTargetMoveSpeed(1f);
                bot.Mover.SetTargetMoveSpeed(1f);
                bot.Covers.FindClosestPoint(position, true);

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
    }
}

using Aki.Reflection.Patching;
using EFT;
using EFT.Weather;
using HarmonyLib;
using SAIN.Vision.Helpers;
using System;
using System.Reflection;
using UnityEngine;
using static SAIN.Vision.Config.VisionConfig;

namespace SAIN.Vision.Patches
{
    public class VisibleDistancePatch : ModulePatch
    {
        private static float DebugTimer = 0f;
        private static PropertyInfo _clearVisibleDistProperty;
        private static PropertyInfo _visibleDistProperty;
        protected override MethodBase GetTargetMethod()
        {
            _clearVisibleDistProperty = AccessTools.Property(typeof(GClass550), "ClearVisibleDist");
            _visibleDistProperty = AccessTools.Property(typeof(GClass550), "VisibleDist");
            return AccessTools.Method(typeof(GClass550), "method_2");
        }
        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref float ___float_3, GClass550 __instance)
        {
            if (!EnableMod.Value)
            {
                return true;
            }

            if (___float_3 < Time.time)
            {
                float timeMod = 1f;
                float weatherMod = 1f;

                // Checks to make sure a date and time is present
                if (___botOwner_0.GameDateTime != null)
                {
                    DateTime dateTime = ___botOwner_0.GameDateTime.Calculate();

                    timeMod = Modifiers.Time.Visibilty(dateTime);

                    __instance.HourServer = (int)((short)dateTime.Hour);
                }

                if (WeatherController.Instance?.WeatherCurve != null)
                {
                    weatherMod = Modifiers.Weather.Visibility();
                }

                float MaxVision;
                float currentVisionDistance;
                float visdistcoef = ___botOwner_0.Settings.Current._visibleDistCoef;
                float defaultvision = ___botOwner_0.Settings.Current.CurrentVisibleDistance;

                // User Toggle for if Global Fog is disabled
                if (NoGlobalFog.Value)
                {
                    // Uses default settings with a max vision cap for safety if a bot is flashed.
                    if (___botOwner_0.FlashGrenade.IsFlashed)
                    {
                        currentVisionDistance = defaultvision;
                        MaxVision = 30f;
                    }
                    // Unlocked Vision distance.
                    else
                    {
                        currentVisionDistance = AbsoluteMaxVisionDistance.Value * visdistcoef;
                        MaxVision = AbsoluteMaxVisionDistance.Value;
                    }
                }
                // If global fog toggle is off, use default settings. And Take SQRT of weather mod to reduce its intensity. Clamp at 0.5 as well.
                else
                {
                    currentVisionDistance = defaultvision;
                    MaxVision = defaultvision;
                    weatherMod = Mathf.Sqrt(weatherMod);
                    weatherMod = Mathf.Clamp(weatherMod, 0.5f, 1f);
                }

                // Sets a minimum cap based on weather conditions to avoid bots having too low of a vision distance while at peace in bad weather
                float currentVisionDistanceCapped = Mathf.Clamp(currentVisionDistance * weatherMod, 80f, MaxVision);

                // Applies Time Modifier to the final vision distance results
                float finalVisionDistance = currentVisionDistanceCapped * timeMod;

                _clearVisibleDistProperty.SetValue(__instance, finalVisionDistance);

                _visibleDistProperty.SetValue(__instance, ___botOwner_0.NightVision.UpdateVision(__instance.ClearVisibleDist));

                _visibleDistProperty.SetValue(__instance, ___botOwner_0.BotLight.UpdateLightEnable(__instance.VisibleDist));


                // Log Everything!
                if (DebugWeather.Value && DebugTimer < Time.time)
                {
                    DebugTimer = Time.time + 5f;
                    System.Console.WriteLine($"SAIN Weather: VisibleDist: [{__instance.VisibleDist}], ClearVisibleDist: [{__instance.ClearVisibleDist}]");
                }

                // Original Logic. Update Frequency - once a minute
                ___float_3 = Time.time + (float)(___botOwner_0.FlashGrenade.IsFlashed ? 3 : 60);
            }
            return false;
        }
    }
    public class GainSightPatch : ModulePatch
    {
        //static float Timer = 0f;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass475), "method_7");
        }
        [PatchPostfix]
        public static void PatchPostfix(ref float __result)
        {
            if (!EnableMod.Value)
            {
                return;
            }

            float weatherModifier = 1f;
            if (WeatherController.Instance?.WeatherCurve != null)
            {
                weatherModifier = Modifiers.Weather.Visibility();
            }

            float inverseMod = Mathf.Sqrt(2f - weatherModifier);

            float finalModifier = __result * inverseMod;

            __result = finalModifier;
        }
    }
    public class VisionOverridesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalLookData), "Update");
        }
        [PatchPostfix]
        public static void PatchPostfix(ref BotGlobalLookData __instance)
        {
            if (!EnableMod.Value)
            {
                return;
            }

            __instance.MAX_DIST_CLAMP_TO_SEEN_SPEED = 1000f;

            if (Experimental.Value)
            {
                __instance.NIGHT_VISION_ON = 75f;
                __instance.NIGHT_VISION_OFF = 125f;
                __instance.NIGHT_VISION_DIST = 125f;
                __instance.VISIBLE_ANG_NIGHTVISION = 90f;

                __instance.LOOK_THROUGH_PERIOD_BY_HIT = 0f;

                __instance.LightOnVisionDistance = 70f;
                __instance.ENEMY_LIGHT_ADD = 120f;
                __instance.ENEMY_LIGHT_START_DIST = 200f;
                __instance.VISIBLE_ANG_LIGHT = 30f;
                __instance.VISIBLE_DISNACE_WITH_LIGHT = 50f;

                __instance.GOAL_TO_FULL_DISSAPEAR = 0.2f;
                __instance.GOAL_TO_FULL_DISSAPEAR_GREEN = 0.35f;
                __instance.GOAL_TO_FULL_DISSAPEAR_SHOOT = 0.0001f;

                __instance.MAX_VISION_GRASS_METERS = 1f;
                __instance.MAX_VISION_GRASS_METERS_OPT = 1f;
                __instance.MAX_VISION_GRASS_METERS_FLARE = 4f;
                __instance.MAX_VISION_GRASS_METERS_FLARE_OPT = 0.25f;

                __instance.NO_GREEN_DIST = 1f;
                __instance.NO_GRASS_DIST = 1f;

                //__instance.MIDDLE_DIST_CAN_SHOOT_HEAD = false;
                //__instance.CHECK_HEAD_ANY_DIST = false;
            }
        }
    }
}

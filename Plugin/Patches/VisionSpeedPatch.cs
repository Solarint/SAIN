using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.UserSettings;
using System;
using System.Reflection;
using UnityEngine;
using static SAIN.Editor.EditorSettings;

namespace SAIN.Patches
{
    public class Math
    {
        public static float CalcVisSpeed(float dist)
        {
            float result = 1f;
            if (dist >= CloseFarThresh.Value)
            {
                result *= FarVisionSpeed.Value;
            }
            else
            {
                result *= CloseVisionSpeed.Value;
            }
            result *= VisionSpeed.Value;

            return result;
        }
    }

    public class VisibleDistancePatch : ModulePatch
    {
        private static float DebugTimer = 0f;
        private static PropertyInfo _LookSensor;
        private static PropertyInfo _clearVisibleDistProperty;
        private static PropertyInfo _visibleDistProperty;
        private static PropertyInfo _HourServerProperty;

        protected override MethodBase GetTargetMethod()
        {
            _LookSensor = AccessTools.Property(typeof(BotOwner), "LookSensor");
            Type lookSensorType = _LookSensor.PropertyType;

            _clearVisibleDistProperty = lookSensorType.GetProperty("ClearVisibleDist");
            _visibleDistProperty = lookSensorType.GetProperty("VisibleDist");
            _HourServerProperty = lookSensorType.GetProperty("HourServer");

            return AccessTools.Method(lookSensorType, "method_2");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref float ___float_3, object __instance)
        {
            if (___float_3 < Time.time)
            {
                float timeMod = 1f;

                // Checks to make sure a date and time is present
                if (___botOwner_0.GameDateTime != null)
                {
                    DateTime dateTime = SAINPlugin.BotController.GameDateTime;
                    timeMod = SAINPlugin.BotController.TimeOfDayVisibility;
                    // Set the value of the "HourServer" property to the hour from the DateTime object
                    _HourServerProperty.SetValue(__instance, (int)((short)dateTime.Hour));
                }

                float weatherMod = SAINPlugin.BotController.WeatherVisibility;

                float visdistcoef = ___botOwner_0.Settings.Current._visibleDistCoef;
                float defaultvision = ___botOwner_0.Settings.Current.CurrentVisibleDistance;

                float MaxVision;
                float currentVisionDistance;
                // User Toggle for if Global Fog is disabled
                if (VisionConfig.OverrideVisionDist.Value)
                {
                    // Uses default settings with a max vision cap for safety if a bot is flashed.
                    if (___botOwner_0.FlashGrenade.IsFlashed)
                    {
                        currentVisionDistance = defaultvision;
                        MaxVision = 30f;
                    }
                    // Unlocked Vision Distance.
                    else
                    {
                        currentVisionDistance = VisionConfig.AbsoluteMaxVisionDistance.Value * visdistcoef;
                        MaxVision = VisionConfig.AbsoluteMaxVisionDistance.Value;
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

                // Sets a minimum cap based on weather conditions to avoid bots having too low of a vision Distance while at peace in bad weather
                float currentVisionDistanceCapped = Mathf.Clamp(currentVisionDistance * weatherMod, 80f, MaxVision);

                // Applies SeenTime Modifier to the final vision Distance results
                float finalVisionDistance = currentVisionDistanceCapped * timeMod;

                _clearVisibleDistProperty.SetValue(__instance, finalVisionDistance);
                _visibleDistProperty.SetValue(__instance, ___botOwner_0.NightVision.UpdateVision(finalVisionDistance));
                _visibleDistProperty.SetValue(__instance, ___botOwner_0.BotLight.UpdateLightEnable(finalVisionDistance));

                // Log Everything!
                if (VisionConfig.DebugWeather.Value && DebugTimer < Time.time)
                {
                    DebugTimer = Time.time + 5f;
                    System.Console.WriteLine($"SAIN Weather: VisibleDist: [{finalVisionDistance}]");
                }

                // Original Logic. Update Frequency - once a minute
                ___float_3 = Time.time + (float)(___botOwner_0.FlashGrenade.IsFlashed ? 3 : 10);
            }
            return false;
        }
    }

    public class NoAIESPPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetMethod("IsEnemyLookingAtMe", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(IAIDetails) }, null);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }

    public class VisionSpeedPatch : ModulePatch
    {
        private static PropertyInfo _GoalEnemyProp;
        protected override MethodBase GetTargetMethod()
        {
            _GoalEnemyProp = AccessTools.Property(typeof(BotMemoryClass), "GoalEnemy");
            return AccessTools.Method(_GoalEnemyProp.PropertyType, "method_7");
        }

        [PatchPostfix]
        public static void PatchPostfix(BifacialTransform BotTransform, BifacialTransform enemy, ref float __result)
        {
            float dist = (BotTransform.position - enemy.position).magnitude;
            float weatherModifier = SAINPlugin.BotController.WeatherVisibility;
            float inverseWeatherModifier = Mathf.Sqrt(2f - weatherModifier);
            __result *= Math.CalcVisSpeed(dist);
            if (dist > 20f)
            {
                __result *= inverseWeatherModifier;
            }
        }
    }
}
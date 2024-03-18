using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Preset;
using SAIN.Components;
using SAIN.Plugin;
using SAIN.Preset.BotSettings.SAINSettings;
using System;
using System.Reflection;
using UnityEngine;
using Comfort.Common;
using SAIN.SAINComponent.Classes;

namespace SAIN.Patches.Vision
{
    public class Math
    {
        public static float CalcVisSpeed(float dist, SAINSettingsClass preset)
        {
            float result = 1f;
            if (dist >= preset.Look.CloseFarThresh)
            {
                result *= preset.Look.FarVisionSpeed;
            }
            else
            {
                result *= preset.Look.CloseVisionSpeed;
            }
            result *= preset.Look.VisionSpeedModifier;

            return Mathf.Round(result * 100f) / 100f;
        }
    }

    public class VisibleDistancePatch : ModulePatch
    {
        private static PropertyInfo _clearVisibleDistProperty;
        private static PropertyInfo _visibleDistProperty;
        private static PropertyInfo _HourServerProperty;

        protected override MethodBase GetTargetMethod()
        {
            _clearVisibleDistProperty = typeof(LookSensor).GetProperty("ClearVisibleDist");
            _visibleDistProperty = typeof(LookSensor).GetProperty("VisibleDist");
            _HourServerProperty = typeof(LookSensor).GetProperty("HourServer");

            return AccessTools.Method(typeof(LookSensor), "method_2");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ____botOwner, ref float ____nextUpdateVisibleDist)
        {
            if (____nextUpdateVisibleDist < Time.time)
            {
                float timeMod = 1f;
                float weatherMod = 1f;

                // Checks to make sure a date and time is present
                if (____botOwner.GameDateTime != null)
                {
                    DateTime dateTime = SAINPlugin.BotController.TimeVision.GameDateTime;
                    timeMod = SAINPlugin.BotController.TimeVision.TimeOfDayVisibility;
                    // Modify the Rounding of the "HourServer" property to the hour from the DateTime object
                    _HourServerProperty.SetValue(____botOwner.LookSensor, (int)((short)dateTime.Hour));
                }
                if (SAINPlugin.BotController != null)
                {
                    weatherMod = SAINPlugin.BotController.WeatherVision.VisibilityNum;
                    weatherMod = Mathf.Clamp(weatherMod, 0.5f, 1f);
                }

                float currentVisionDistance = ____botOwner.Settings.Current.CurrentVisibleDistance;

                // Sets a minimum cap based on weather conditions to avoid bots having too low of a vision Distance while at peace in bad weather
                float currentVisionDistanceCapped = Mathf.Clamp(currentVisionDistance * weatherMod, 80f, currentVisionDistance);

                // Applies SeenTime Modifier to the final vision Distance results
                float finalVisionDistance = currentVisionDistanceCapped * timeMod;

                _clearVisibleDistProperty.SetValue(____botOwner.LookSensor, finalVisionDistance);

                finalVisionDistance = ____botOwner.NightVision.UpdateVision(finalVisionDistance);
                finalVisionDistance = ____botOwner.BotLight.UpdateLightEnable(finalVisionDistance);
                _visibleDistProperty.SetValue(____botOwner.LookSensor, finalVisionDistance);

                ____nextUpdateVisibleDist = Time.time + (____botOwner.FlashGrenade.IsFlashed ? 3 : 20);
            }
            return false;
        }
    }

    public class NoAIESPPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetMethod("IsEnemyLookingAtMe", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(IPlayer) }, null);
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
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EnemyInfo), "method_5");
        }

        [PatchPostfix]
        public static void PatchPostfix(BifacialTransform BotTransform, BifacialTransform enemy, ref float __result, EnemyInfo __instance)
        {
            float dist = (BotTransform.position - enemy.position).magnitude;
            float weatherModifier = SAINPlugin.BotController.WeatherVision.VisibilityNum;
            float inverseWeatherModifier = Mathf.Sqrt(2f - weatherModifier);

            WildSpawnType wildSpawnType = __instance.Owner.Profile.Info.Settings.Role;
            if (PresetHandler.LoadedPreset.BotSettings.SAINSettings.TryGetValue(wildSpawnType, out var botType) )
            {
                BotDifficulty diff = __instance.Owner.Profile.Info.Settings.BotDifficulty;
                __result *= Math.CalcVisSpeed(dist, botType.Settings[diff]);
            }
            if (dist > 20f)
            {
                __result *= inverseWeatherModifier;
            }

            // Not Looking Implementation
            if (__instance?.Person?.IsYourPlayer == true)
            {
                __result *= SAINNotLooking.GetVisionSpeedIncrease(__instance.Owner);
            }

            __result = Mathf.Round(__result * 100f) / 100f; ;
        }
    }

    public class CheckFlashlightPatch : ModulePatch
    {
        private static FieldInfo _tacticalModesField;
        private static MethodInfo _UsingLight;
        protected override MethodBase GetTargetMethod()
        {
            _UsingLight = AccessTools.PropertySetter(typeof(AIData), "UsingLight");

            _tacticalModesField = AccessTools.Field(typeof(TacticalComboVisualController), "list_0");

            return AccessTools.Method(typeof(Player.FirearmController), "SetLightsState");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref Player ____player)
        {
            if (____player.gameObject.TryGetComponent<SAINFlashLightComponent>(out var component))
            {
                component.CheckDevice(____player, _tacticalModesField);
                if (!component.WhiteLight && !component.Laser)
                {
                    _UsingLight.Invoke(____player.AIData, new object[] { false });
                }
            }
        }
    }
}
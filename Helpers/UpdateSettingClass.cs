using EFT;
using SAIN.Preset.BotSettings.SAINSettings;
using SAIN.Preset.GlobalSettings;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static MineDirectional;
using EFTSettingsGroup = GClass566;

namespace SAIN.Helpers
{
    internal class UpdateSettingClass
    {
        public static readonly string[] AimMultiplierNames =
        {
                nameof(EFTSettingsGroup.Aiming.BASE_SHIEF),
                nameof(EFTSettingsGroup.Aiming.BOTTOM_COEF)
        };

        public static readonly string[] ScatterMultiplierNames =
        {
                nameof(EFTSettingsGroup.Aiming.XZ_COEF),
                nameof(EFTSettingsGroup.Aiming.XZ_COEF_STATIONARY_BULLET),
                nameof(EFTSettingsGroup.Aiming.XZ_COEF_STATIONARY_GRENADE),
                nameof(EFTSettingsGroup.Scattering.MinScatter),
                nameof(EFTSettingsGroup.Scattering.MaxScatter),
                nameof(EFTSettingsGroup.Scattering.WorkingScatter)
        };

        public static string VisibleDistance = nameof(EFTSettingsGroup.Core.VisibleDistance);
        public static string GainSightCoef = nameof(EFTSettingsGroup.Core.GainSightCoef);

        private static GlobalSettingsClass GlobalSettings => SAINPlugin.LoadedPreset.GlobalSettings;

        public static void ManualSettingsUpdate(WildSpawnType WildSpawnType, BotDifficulty botDifficulty, EFTSettingsGroup eftSettings, EFTSettingsGroup defaultSettings = null, SAINSettingsClass sainSettings = null)
        {
            if (sainSettings == null)
            {
                sainSettings = SAINPlugin.LoadedPreset.BotSettings.GetSAINSettings(WildSpawnType, botDifficulty);
            }
            if (defaultSettings == null)
            {
                defaultSettings = HelpersGClass.GetEFTSettings(WildSpawnType, botDifficulty);
            }

            float multiplier = ScatterMulti(sainSettings);

            eftSettings.Aiming.BASE_SHIEF = MultiplySetting(
                defaultSettings.Aiming.BASE_SHIEF,
                multiplier,
                "BASE_SHIEF");
            eftSettings.Aiming.BOTTOM_COEF = MultiplySetting(
                defaultSettings.Aiming.BOTTOM_COEF,
                multiplier,
                "BOTTOM_COEF");

            multiplier = AimMulti(sainSettings);

            eftSettings.Aiming.XZ_COEF = MultiplySetting(
                defaultSettings.Aiming.XZ_COEF,
                multiplier,
                "XZ_COEF");
            eftSettings.Aiming.XZ_COEF_STATIONARY_BULLET = MultiplySetting(
                defaultSettings.Aiming.XZ_COEF_STATIONARY_BULLET,
                multiplier,
                "XZ_COEF_STATIONARY_BULLET");
            eftSettings.Aiming.XZ_COEF_STATIONARY_GRENADE = MultiplySetting(
                defaultSettings.Aiming.XZ_COEF_STATIONARY_GRENADE,
                multiplier,
                "XZ_COEF_STATIONARY_GRENADE");

            eftSettings.Scattering.MinScatter = MultiplySetting(
                defaultSettings.Scattering.MinScatter,
                multiplier,
                "MinScatter");
            eftSettings.Scattering.MaxScatter = MultiplySetting(
                defaultSettings.Scattering.MaxScatter,
                multiplier,
                "MaxScatter");
            eftSettings.Scattering.WorkingScatter = MultiplySetting(
                defaultSettings.Scattering.WorkingScatter,
                multiplier,
                "WorkingScatter");

            eftSettings.Core.VisibleDistance = MultiplySetting(
                sainSettings.Core.VisibleDistance,
                VisionDistanceMulti,
                "VisibleDistance");

            eftSettings.Core.GainSightCoef = MultiplySetting(
                sainSettings.Core.GainSightCoef,
                VisionSpeedMulti(sainSettings),
                "GainSightCoef");
        }
        public static void ManualSettingsUpdate(WildSpawnType WildSpawnType, BotDifficulty botDifficulty, BotOwner BotOwner, SAINSettingsClass sainSettings)
        {
            var eftSettings = BotOwner.Settings.FileSettings;

            ManualSettingsUpdate(WildSpawnType, botDifficulty, eftSettings, null, sainSettings);

            if (BotOwner.WeaponManager?.WeaponAIPreset != null)
            {
                BotOwner.WeaponManager.WeaponAIPreset.XZ_COEF = eftSettings.Aiming.XZ_COEF;
                BotOwner.WeaponManager.WeaponAIPreset.BaseShift = eftSettings.Aiming.BASE_SHIEF;
            }
        }

        private static float MultiplySetting(float defaultValue, float multiplier, string name)
        {
            float result = Mathf.Round(defaultValue * multiplier * 100f) / 100f;
            if (SAINPlugin.DebugModeEnabled)
            {
                Logger.LogInfo($"{name} Default {defaultValue} Multiplier: {multiplier} Result: {result}");
            }
            return result;
        }

        public static float AimMulti(SAINSettingsClass SAINSettings) => Round(SAINSettings.Aiming.AccuracySpreadMulti * GlobalSettings.Aiming.AccuracySpreadMultiGlobal / GlobalSettings.General.GlobalDifficultyModifier);
        public static float ScatterMulti(SAINSettingsClass SAINSettings) => Round(SAINSettings.Scattering.ScatterMultiplier * GlobalSettings.Shoot.GlobalScatterMultiplier / GlobalSettings.General.GlobalDifficultyModifier);
        public static float VisionSpeedMulti(SAINSettingsClass SAINSettings) => Round(SAINSettings.Look.VisionSpeedModifier * GlobalSettings.Look.GlobalVisionSpeedModifier / GlobalSettings.General.GlobalDifficultyModifier);
        public static float VisionDistanceMulti => GlobalSettings.Look.GlobalVisionDistanceMultiplier;

        private static float Round(float value)
        {
            return Mathf.Round(value * 100f) / 100f;
        }
    }
}
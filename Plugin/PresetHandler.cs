using SAIN.Editor;
using SAIN.Helpers;
using SAIN.Preset;
using System;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.Plugin
{
    internal class PresetHandler
    {
        public const string DefaultPreset = "3 SAIN Hard - Default";

        private const string Settings = "Settings";

        public static Action PresetsUpdated;

        public static readonly List<SAINPresetDefinition> PresetOptions = new List<SAINPresetDefinition>();
        
        public static SAINPresetClass LoadedPreset;

        public static PresetEditorDefaults EditorDefaults;

        public static void LoadPresetOptions()
        {
            Load.GetPresetOptions(PresetOptions);
        }

        public static void Init()
        {
            ImportEditorDefaults();
            LoadPresetOptions();

            if (!LoadPresetDefinition(EditorDefaults.SelectedPreset, 
                out SAINPresetDefinition presetDefinition))
            {
                if (!LoadPresetDefinition(DefaultPreset, 
                    out presetDefinition))
                {
                    LoadedPreset = CreateDefaultPresets();
                    return;
                }
            }
            InitPresetFromDefinition(presetDefinition);
        }

        public static bool LoadPresetDefinition(string presetKey, out SAINPresetDefinition definition)
        {
            for (int i = 0; i < PresetOptions.Count; i++)
            {
                var preset = PresetOptions[i];
                if (preset.Name == presetKey)
                {
                    definition = preset;
                    return true;
                }
            }
            if (Load.LoadObject(out definition, "Info", PresetsFolder, presetKey))
            {
                PresetOptions.Add(definition);
                return true;
            }
            return false;
        }

        public static void SavePresetDefinition(SAINPresetDefinition definition)
        {
            bool newPreset = true;
            for (int i = 0; i < PresetOptions.Count; i++)
            {
                var preset = PresetOptions[i];
                if (preset.Name == definition.Name)
                {
                    newPreset = false;
                }
            }
            if (newPreset)
            {
                PresetOptions.Add(definition);
            }

            SaveObjectToJson(definition, "Info", PresetsFolder, definition.Name);
        }

        public static void InitPresetFromDefinition(SAINPresetDefinition def)
        {
            try
            {
                LoadedPreset = new SAINPresetClass(def);
            }
            catch (Exception ex)
            {
                Sounds.PlaySound(EFT.UI.EUISoundType.ErrorMessage);
                Logger.LogError(ex);

                LoadPresetDefinition(DefaultPreset, out def);
                LoadedPreset = new SAINPresetClass(def);
            }
            UpdateExistingBots();
            ExportEditorDefaults();
        }

        public static void ExportEditorDefaults()
        {
            EditorDefaults.SelectedPreset = LoadedPreset.Info.Name;
            SaveObjectToJson(EditorDefaults, Settings, PresetsFolder);
        }

        public static void ImportEditorDefaults()
        {
            if (Load.LoadObject(out PresetEditorDefaults editorDefaults, Settings, PresetsFolder))
            {
                EditorDefaults = editorDefaults;
            }
            else
            {
                EditorDefaults = new PresetEditorDefaults(DefaultPreset);
            }
        }

        public static void UpdateExistingBots()
        {
            if (SAINPlugin.BotController?.Bots != null && SAINPlugin.BotController.Bots.Count > 0)
            {
                PresetsUpdated();
                AudioHelpers.ClearCache();
            }
        }

        private static SAINPresetClass CreateDefaultPresets()
        {
            CreateEasyPreset();
            CreateNormalPreset();
            var hard = CreateHardPreset();
            CreateVeryHardPreset();
            CreateImpossiblePreset(); 
            return hard;
        }

        private static SAINPresetClass CreateEasyPreset()
        {
            var preset = SAINPresetDefinition.CreateDefault("Easy");

            var global = preset.GlobalSettings;
            global.Shoot.GlobalRecoilMultiplier = 2.5f;
            global.Shoot.GlobalScatterMultiplier = 1.5f;
            global.Aiming.AccuracySpreadMultiGlobal = 2f;
            global.Aiming.FasterCQBReactionsGlobal = false;
            global.General.GlobalDifficultyModifier = 0.5f;
            global.Look.GlobalVisionDistanceMultiplier = 0.66f;
            global.Look.GlobalVisionSpeedModifier = 1.75f;

            foreach (var bot in preset.BotSettings.SAINSettings)
            {
                bot.Value.DifficultyModifier = Mathf.Clamp(bot.Value.DifficultyModifier * 0.5f, 0.01f, 1f).Round100();
                foreach (var setting in bot.Value.Settings)
                {
                    setting.Value.Core.VisibleAngle = 120f;
                    setting.Value.Shoot.FireratMulti *= 0.75f;
                }
            }

            preset.ExportGlobalSettings();
            return preset;
        }

        private static SAINPresetClass CreateNormalPreset()
        {
            var preset = SAINPresetDefinition.CreateDefault("Normal");

            var global = preset.GlobalSettings;
            global.Shoot.GlobalRecoilMultiplier = 1.6f;
            global.Shoot.GlobalScatterMultiplier = 1.2f;
            global.Aiming.AccuracySpreadMultiGlobal = 1.5f;
            global.Aiming.FasterCQBReactionsGlobal = false;
            global.General.GlobalDifficultyModifier = 0.75f;
            global.Look.GlobalVisionDistanceMultiplier = 0.85f;
            global.Look.GlobalVisionSpeedModifier = 1.25f;

            foreach (var bot in preset.BotSettings.SAINSettings)
            {
                bot.Value.DifficultyModifier = Mathf.Clamp(bot.Value.DifficultyModifier * 0.85f, 0.01f, 1f).Round100();
                foreach (var setting in bot.Value.Settings)
                {
                    setting.Value.Core.VisibleAngle = 150f;
                }
            }

            preset.ExportGlobalSettings();
            return preset;
        }

        private static SAINPresetClass CreateHardPreset()
        {
            var preset = SAINPresetDefinition.CreateDefault("Hard");
            preset.ExportGlobalSettings();
            return preset;
        }

        private static SAINPresetClass CreateVeryHardPreset()
        {
            var preset = SAINPresetDefinition.CreateDefault("Very Hard");

            var global = preset.GlobalSettings;
            global.Shoot.GlobalRecoilMultiplier = 0.66f;
            global.Shoot.GlobalScatterMultiplier = 0.85f;
            global.Aiming.AccuracySpreadMultiGlobal = 0.8f;
            global.General.GlobalDifficultyModifier = 1.35f;
            global.Look.GlobalVisionDistanceMultiplier = 1.33f;
            global.Look.GlobalVisionSpeedModifier = 0.8f;

            foreach (var bot in preset.BotSettings.SAINSettings)
            {
                bot.Value.DifficultyModifier = Mathf.Clamp(bot.Value.DifficultyModifier * 1.33f, 0.01f, 1f).Round100();
                foreach (var setting in bot.Value.Settings)
                {
                    setting.Value.Core.VisibleAngle = 170f;
                    setting.Value.Shoot.FireratMulti *= 1.2f;
                }
            }

            preset.ExportGlobalSettings();
            return preset;
        }

        private static SAINPresetClass CreateImpossiblePreset()
        {
            var preset = SAINPresetDefinition.CreateDefault("Impossible", "Prepare To Die");

            var global = preset.GlobalSettings;
            global.Shoot.GlobalRecoilMultiplier = 0.15f;
            global.Shoot.GlobalScatterMultiplier = 0.05f;
            global.Aiming.AccuracySpreadMultiGlobal = 0.33f;
            global.General.GlobalDifficultyModifier = 3f;
            global.Look.GlobalVisionDistanceMultiplier = 2.5f;
            global.Look.GlobalVisionSpeedModifier = 0.5f;

            foreach (var bot in preset.BotSettings.SAINSettings)
            {
                bot.Value.DifficultyModifier = Mathf.Sqrt(bot.Value.DifficultyModifier).Round100();
                foreach (var setting in bot.Value.Settings)
                {
                    setting.Value.Core.VisibleAngle = 180f;
                    setting.Value.Shoot.FireratMulti *= 2f;
                }
            }

            preset.ExportGlobalSettings();
            return preset;
        }
    }
}
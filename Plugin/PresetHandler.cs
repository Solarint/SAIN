using SAIN.Editor;
using SAIN.Preset;
using System;
using System.Collections.Generic;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.Plugin
{
    internal class PresetHandler
    {
        public static Action PresetsUpdated;
        public static List<SAINPresetDefinition> PresetOptions = new List<SAINPresetDefinition>();
        public static SAINPresetClass LoadedPreset;

        public static void LoadPresetOptions()
        {
            PresetOptions = Load.GetPresetOptions(PresetOptions);
        }

        public static bool PresetVersionMismatch = false;

        private const string Settings = "Settings";

        public static void Init()
        {
            LoadPresetOptions();

            if (!Load.LoadObject(out PresetEditorDefaults presetDefaults, Settings, PresetsFolder))
            {
                presetDefaults = new PresetEditorDefaults
                {
                    SelectedPreset = DefaultPreset
                };
            }
            presetDefaults.DefaultPreset = DefaultPreset;
            SAINPlugin.Editor.AdvancedBotConfigs = presetDefaults.ShowAdvanced;

            if (!LoadPresetDefinition(presetDefaults.SelectedPreset, out SAINPresetDefinition presetDefinition))
            {
                if (!LoadPresetDefinition(DefaultPreset, out presetDefinition))
                {
                    presetDefinition = CreateDefaultPresets();
                }
            }
            InitPresetFromDefinition(presetDefinition);
        }

        private static SAINPresetDefinition CreateDefaultPresets()
        {
            var easy = new SAINPresetDefinition
            {
                Name = "SAIN Easy",
                Description = "The Default SAIN Preset",
                Creator = "Solarint",
                SAINVersion = AssemblyInfo.SAINVersion,
                DateCreated = DateTime.Today.ToString()
            };
            //SavePresetDefinition(easy);
            //new SAINPresetClass(easy);
            var normal = new SAINPresetDefinition
            {
                Name = "SAIN Normal",
                Description = "The Default SAIN Preset",
                Creator = "Solarint",
                SAINVersion = AssemblyInfo.SAINVersion,
                DateCreated = DateTime.Today.ToString()
            };
            //SavePresetDefinition(normal);
            //new SAINPresetClass(normal);
            var hard = new SAINPresetDefinition
            {
                Name = DefaultPreset,
                Description = "The Default SAIN Preset",
                Creator = "Solarint",
                SAINVersion = AssemblyInfo.SAINVersion,
                DateCreated = DateTime.Today.ToString()
            };
            SavePresetDefinition(hard);
            new SAINPresetClass(hard);
            var impossible = new SAINPresetDefinition
            {
                Name = "SAIN Impossible",
                Description = "The Default SAIN Preset",
                Creator = "Solarint",
                SAINVersion = AssemblyInfo.SAINVersion,
                DateCreated = DateTime.Today.ToString()
            };
            //SavePresetDefinition(impossible);
            //new SAINPresetClass(impossible);

            return hard;
        }

        private static readonly string DefaultPreset = "SAIN Default";

        public static bool LoadPresetDefinition(string presetKey, out SAINPresetDefinition definition)
        {
            return Load.LoadObject(out definition, "Info", PresetsFolder, presetKey);
        }

        public static void SavePresetDefinition(SAINPresetDefinition definition)
        {
            SaveObjectToJson(definition, "Info", PresetsFolder, definition.Name);
        }

        public static void InitPresetFromDefinition(SAINPresetDefinition def)
        {
            try
            {
                LoadedPreset = new SAINPresetClass(def);
                var defaults = new PresetEditorDefaults
                {
                    SelectedPreset = def.Name,
                    DefaultPreset = DefaultPreset,
                    ShowAdvanced = SAINPlugin.Editor?.AdvancedBotConfigs == true,
                    DebugGizmos = SAINPlugin.DrawDebugGizmos,
                    GlobalDebugMode = SAINPlugin.DebugModeEnabled
                };
                SaveObjectToJson(defaults, Settings, PresetsFolder);
                UpdateExistingBots();
            }
            catch (Exception ex)
            {
                Sounds.PlaySound(EFT.UI.EUISoundType.ErrorMessage);
                Logger.LogError(ex);
                LoadPresetDefinition(DefaultPreset, out def);
                LoadedPreset = new SAINPresetClass(def);
                SaveEditorDefaults();
                UpdateExistingBots();
            }
        }

        public static void SaveEditorDefaults()
        {
            var defaults = new PresetEditorDefaults
            {
                SelectedPreset = LoadedPreset.Info.Name,
                DefaultPreset = DefaultPreset,
                ShowAdvanced = SAINPlugin.Editor.AdvancedBotConfigs,
                DebugGizmos = SAINPlugin.DrawDebugGizmos,
                GlobalDebugMode = SAINPlugin.DebugModeEnabled
            };
            SaveObjectToJson(defaults, Settings, PresetsFolder);
        }

        public static void UpdateExistingBots()
        {
            LoadedPreset.GlobalSettings.Shoot.AmmoShootability.UpdateValues();
            LoadedPreset.GlobalSettings.Shoot.WeaponShootability.UpdateValues();
            LoadedPreset.GlobalSettings.Hearing.AudibleRanges.UpdateValues();

            if (SAINPlugin.BotController?.Bots != null && SAINPlugin.BotController.Bots.Count > 0)
            {
                PresetsUpdated();
            }
        }
    }
}
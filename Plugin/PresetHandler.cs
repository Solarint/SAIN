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

            if (Load.LoadObject(out PresetEditorDefaults editorDefaults, Settings, PresetsFolder))
            {
                EditorDefaults = editorDefaults;
            }
            editorDefaults.DefaultPreset = DefaultPreset;
            SAINEditor.AdvancedBotConfigs = editorDefaults.ShowAdvanced;
            SAINPlugin.DebugModeEnabled = editorDefaults.GlobalDebugMode;
            SAINPlugin.DrawDebugGizmos = editorDefaults.DebugGizmos;

            if (!LoadPresetDefinition(editorDefaults.SelectedPreset, out SAINPresetDefinition presetDefinition))
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

        public static readonly string DefaultPreset = "SAIN Default";

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
                EditorDefaults.SelectedPreset = def.Name;
                EditorDefaults.DefaultPreset = DefaultPreset;
                EditorDefaults.ShowAdvanced = SAINEditor.AdvancedBotConfigs == true;
                EditorDefaults.DebugGizmos = SAINPlugin.DrawDebugGizmos;
                EditorDefaults.GlobalDebugMode = SAINPlugin.DebugModeEnabled;
                SaveObjectToJson(EditorDefaults, Settings, PresetsFolder);
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

        private static PresetEditorDefaults EditorDefaults = new PresetEditorDefaults();

        public static void SaveEditorDefaults()
        {
            EditorDefaults.SelectedPreset = LoadedPreset.Info.Name;
            EditorDefaults.DefaultPreset = DefaultPreset;
            EditorDefaults.ShowAdvanced = SAINEditor.AdvancedBotConfigs;
            EditorDefaults.DebugGizmos = SAINPlugin.DrawDebugGizmos;
            EditorDefaults.GlobalDebugMode = SAINPlugin.DebugModeEnabled;
            SaveObjectToJson(EditorDefaults, Settings, PresetsFolder);
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
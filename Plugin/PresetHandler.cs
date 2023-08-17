using SAIN.Editor;
using SAIN.Helpers;
using SAIN.Preset;
using System;
using System.Collections.Generic;
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
            var definition = new SAINPresetDefinition
            {
                Name = "1 SAIN Easy",
                Description = "The Default Easy SAIN Preset",
                Creator = "Solarint",
                SAINVersion = AssemblyInfo.SAINVersion,
                DateCreated = DateTime.Now.ToString()
            };

            SavePresetDefinition(definition);
            var preset = new SAINPresetClass(definition);

            var shoot = preset.GlobalSettings.Shoot;
            shoot.GlobalRecoilMultiplier = 2.5f;
            shoot.GlobalScatterMultiplier = 1.5f;

            var aim = preset.GlobalSettings.Aiming;
            aim.AccuracySpreadMultiGlobal = 2f;
            aim.FasterCQBReactionsGlobal = false;

            var general = preset.GlobalSettings.General;
            general.GlobalDifficultyModifier = 0.5f;

            var look = preset.GlobalSettings.Look;
            look.GlobalVisionDistanceMultiplier = 0.66f;
            look.GlobalVisionSpeedModifier = 1.75f;

            preset.ExportGlobalSettings();

            return preset;
        }

        private static SAINPresetClass CreateNormalPreset()
        {
            var definition = new SAINPresetDefinition
            {
                Name = "2 SAIN Normal",
                Description = "The Default Normal SAIN Preset",
                Creator = "Solarint",
                SAINVersion = AssemblyInfo.SAINVersion,
                DateCreated = DateTime.Now.ToString()
            };

            SavePresetDefinition(definition);
            var preset = new SAINPresetClass(definition);

            var shoot = preset.GlobalSettings.Shoot;
            shoot.GlobalRecoilMultiplier = 1.6f;
            shoot.GlobalScatterMultiplier = 1.2f;

            var aim = preset.GlobalSettings.Aiming;
            aim.AccuracySpreadMultiGlobal = 1.5f;
            aim.FasterCQBReactionsGlobal = false;

            var general = preset.GlobalSettings.General;
            general.GlobalDifficultyModifier = 0.75f;

            var look = preset.GlobalSettings.Look;
            look.GlobalVisionDistanceMultiplier = 0.85f;
            look.GlobalVisionSpeedModifier = 1.25f;

            preset.ExportGlobalSettings();

            return preset;
        }

        private static SAINPresetClass CreateHardPreset()
        {
            var definition = new SAINPresetDefinition
            {
                Name = DefaultPreset,
                Description = "3 The Default Hard SAIN Preset. The way it was designed to be played",
                Creator = "Solarint",
                SAINVersion = AssemblyInfo.SAINVersion,
                DateCreated = DateTime.Now.ToString()
            };

            SavePresetDefinition(definition);
            var preset = new SAINPresetClass(definition);
            return preset;
        }

        private static SAINPresetClass CreateVeryHardPreset()
        {
            var definition = new SAINPresetDefinition
            {
                Name = "4 SAIN Very Hard",
                Description = "The Default Very Hard SAIN Preset.",
                Creator = "Solarint",
                SAINVersion = AssemblyInfo.SAINVersion,
                DateCreated = DateTime.Now.ToString()
            };

            SavePresetDefinition(definition);
            var preset = new SAINPresetClass(definition);

            var shoot = preset.GlobalSettings.Shoot;
            shoot.GlobalRecoilMultiplier = 0.66f;
            shoot.GlobalScatterMultiplier = 0.85f;

            var aim = preset.GlobalSettings.Aiming;
            aim.AccuracySpreadMultiGlobal = 0.8f;

            var general = preset.GlobalSettings.General;
            general.GlobalDifficultyModifier = 1.35f;

            var look = preset.GlobalSettings.Look;
            look.GlobalVisionDistanceMultiplier = 1.33f;
            look.GlobalVisionSpeedModifier = 0.8f;

            preset.ExportGlobalSettings();

            return preset;
        }

        private static SAINPresetClass CreateImpossiblePreset()
        {
            var definition = new SAINPresetDefinition
            {
                Name = "5 SAIN Impossible",
                Description = "Prepare to Die",
                Creator = "Solarint",
                SAINVersion = AssemblyInfo.SAINVersion,
                DateCreated = DateTime.Now.ToString()
            };

            SavePresetDefinition(definition);
            var preset = new SAINPresetClass(definition);

            var shoot = preset.GlobalSettings.Shoot;
            shoot.GlobalRecoilMultiplier = 0.15f;
            shoot.GlobalScatterMultiplier = 0.15f;

            var aim = preset.GlobalSettings.Aiming;
            aim.AccuracySpreadMultiGlobal = 0.15f;

            var general = preset.GlobalSettings.General;
            general.GlobalDifficultyModifier = 3f;

            var look = preset.GlobalSettings.Look;
            look.GlobalVisionDistanceMultiplier = 2f;
            look.GlobalVisionSpeedModifier = 0.5f;

            preset.ExportGlobalSettings();

            return preset;
        }
    }
}
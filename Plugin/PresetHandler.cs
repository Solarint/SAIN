using Comfort.Common;
using SAIN.Editor;
using SAIN.Helpers;
using SAIN.SAINPreset;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static EFT.SpeedTree.TreeWind;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.Plugin
{
    internal class PresetHandler
    {
        public static Action PresetsUpdated;
        public static List<SAINPresetDefinition> PresetOptions = new List<SAINPresetDefinition>();
        public static SAINPresetClass LoadedPreset;

        const string Settings = "Settings";
        public static void Init()
        {
            PresetOptions = Load.GetPresetOptions(PresetOptions);
            if (!Load.LoadObject(out PresetEditorDefaults presetDefaults, Settings, PresetsFolder))
            {
                presetDefaults = new PresetEditorDefaults
                {
                    SelectedPreset = DefaultPreset
                };
            }
            presetDefaults.DefaultPreset = DefaultPreset;

            if (!LoadPresetDefinition(presetDefaults.SelectedPreset, out SAINPresetDefinition presetDefinition))
            {
                if (!LoadPresetDefinition(DefaultPreset, out presetDefinition))
                {
                    presetDefinition = new SAINPresetDefinition(DefaultPreset, "SAIN Hard", "The Default SAIN Preset", "Solarint");
                    SavePresetDefinition(presetDefinition);
                }
            }
            InitPresetFromDefinition(presetDefinition);
        }

        static readonly string DefaultPreset = nameof(SAIN) + "_" + PluginInfo.Version + "_hard";

        public static bool LoadPresetDefinition(string presetKey, out SAINPresetDefinition definition)
        {
            return Load.LoadObject(out definition, "Info", PresetsFolder, presetKey);
        }

        public static void SavePresetDefinition(SAINPresetDefinition definition)
        {
            Save.SaveJson(definition, "Info", PresetsFolder, definition.Key);
        }

        public static void InitPresetFromDefinition(SAINPresetDefinition def)
        {
            LoadedPreset = new SAINPresetClass(def);
            var defaults = new PresetEditorDefaults
            {
                SelectedPreset = def.Key,
                DefaultPreset = DefaultPreset
            };
            Save.SaveJson(defaults, Settings, PresetsFolder);
            if (SAINPlugin.BotController?.Bots != null && SAINPlugin.BotController.Bots.Count > 0)
            {
                PresetsUpdated();
            }
        }

        public static void SavePreset(SAINPresetClass preset)
        {
            preset.SavePreset();
        }

        public static void SaveLoadedPreset()
        {
            SavePreset(LoadedPreset);
        }
    }
}

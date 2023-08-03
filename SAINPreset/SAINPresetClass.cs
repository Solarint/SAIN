using Newtonsoft.Json;
using SAIN.Helpers;
using SAIN.SAINPreset.GlobalSettings;
using System;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.SAINPreset
{
    public class SAINPresetClass
    {
        public SAINPresetClass(SAINPresetDefinition preset)
        {
            //HelpersGClass.LoadSettings();
            Definition = preset;
            GlobalSettings = LoadGlobalSettings(preset);
            BotSettings = new BotSettings.BotSettingsClass(preset);
        }

        private const string PresetsFolder = "Presets";

        private static GlobalSettingsClass LoadGlobalSettings(SAINPresetDefinition Preset)
        {
            string fileName = "GlobalSettings"; 
            string presetsFolder = "Presets";
            string presetNameFolder = Preset.Name;
            if (!Load.LoadObject(out GlobalSettingsClass result, fileName, presetsFolder, presetNameFolder))
            {
                result = new GlobalSettingsClass
                {
                    EFTCoreSettings = EFTCoreSettings.GetCore()
                };

                Save.SaveJson(result, fileName, presetsFolder, presetNameFolder);
            }

            return result;
        }

        public void SavePreset()
        {
            string[] folders = new string[] { PresetsFolder, Definition.Name };
            Save.SaveJson(Definition, "Info", folders);
            Save.SaveJson(GlobalSettings, nameof(GlobalSettings), folders);
            BotSettings.SaveSettings(Definition);
        }

        public SAINPresetDefinition Definition;
        public GlobalSettingsClass GlobalSettings;
        public BotSettings.BotSettingsClass BotSettings;
    }

    public sealed class SAINPresetDefinition
    {
        [JsonConstructor]
        public SAINPresetDefinition() { }

        public SAINPresetDefinition(string name, string description, string creator)
        {
            Name = name;
            Description = description;
            Creator = creator;
            SAINVersion = PluginInfo.Version;
            DateCreated = DateTime.UtcNow.Date.ToString();
        }

        public string Name;
        public string Description;
        public string Creator;
        public string SAINVersion;
        public string DateCreated;
    }
}
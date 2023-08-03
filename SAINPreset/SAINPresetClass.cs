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
            HelpersGClass.LoadSettings();
            Definition = preset;
            GlobalSettings = LoadGlobalSettings(preset);
            BotSettings = new BotSettings.BotSettingsClass(preset);
        }

        private const string PresetsFolder = "Presets";

        private static GlobalSettingsClass LoadGlobalSettings(SAINPresetDefinition Preset)
        {
            string fileName = nameof(GlobalSettings);
            string[] folders = new string[] { PresetsFolder, Preset.Key };

            if (!Load.LoadObject(out GlobalSettingsClass result, fileName, folders))
            {
                result = new GlobalSettingsClass
                {
                    EFTCoreSettings = EFTCoreSettings.GetCore()
                };

                Save.SaveJson(result, fileName, folders);
            }

            return result;
        }

        public void SavePreset()
        {
            string[] folders = new string[] { PresetsFolder, Definition.Key };
            Save.SaveJson(Definition, "Info", folders);
            Save.SaveJson(GlobalSettings, nameof(GlobalSettings), folders);
            BotSettings.SaveSettings(Definition);
        }

        public readonly SAINPresetDefinition Definition;
        public readonly GlobalSettingsClass GlobalSettings;
        public readonly BotSettings.BotSettingsClass BotSettings;
    }

    public sealed class SAINPresetDefinition
    {
        [JsonConstructor]
        public SAINPresetDefinition()
        { }

        public SAINPresetDefinition(string key, string displayName, string description, string creator)
        {
            Update(key, displayName, description, creator);
        }

        public void Update(string key = null, string displayname = null, string description = null, string creator = null)
        {
            if (key != null)
            {
                Key = key;
            }
            if (displayname != null)
            {
                DisplayName = displayname;
            }
            if (description != null)
            {
                Description = description;
            }
            if (creator != null)
            {
                Creator = creator;
            }
            SAINVersion = PluginInfo.Version;
            DateCreated = DateTime.Today.ToString();
        }

        public string Key { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public string Creator { get; private set; }
        public string SAINVersion { get; private set; }
        public string DateCreated { get; private set; }
    }
}
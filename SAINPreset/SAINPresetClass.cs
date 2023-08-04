using Newtonsoft.Json;
using SAIN.Helpers;
using SAIN.SAINPreset.GlobalSettings;
using SAIN.SAINPreset.Personalities;
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
            PersonalityManager = new PersonalityManagerClass(preset);
        }

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
            else
            {
                EFTCoreSettings.UpdateCoreSettings(result.EFTCoreSettings);
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
        public PersonalityManagerClass PersonalityManager;
    }

    public sealed class SAINPresetDefinition
    {
        public string Name;
        public string Description;
        public string Creator;
        public readonly string SAINVersion = PluginInfo.Version;
        public readonly string DateCreated = DateTime.Today.ToString();
    }
}
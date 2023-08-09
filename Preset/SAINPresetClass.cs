using SAIN.Preset.GlobalSettings;
using SAIN.Preset.Personalities;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.Preset
{
    public class SAINPresetClass
    {
        public SAINPresetClass(SAINPresetDefinition preset)
        {
            Info = preset;
            GlobalSettings = GlobalSettingsClass.LoadGlobal(preset);
            BotSettings = new BotSettings.BotSettingsClass(preset);
            PersonalityManager = new PersonalityManagerClass(preset);
        }

        public void SavePreset()
        {
            string[] folders = new string[] { PresetsFolder, Info.Name };

            SaveObjectToJson(Info, nameof(Info), folders);
            SaveObjectToJson(GlobalSettings, nameof(GlobalSettings), folders);
            BotSettings.SaveSettings(Info);
        }

        public SAINPresetDefinition Info;
        public GlobalSettingsClass GlobalSettings;
        public BotSettings.BotSettingsClass BotSettings;
        public PersonalityManagerClass PersonalityManager;
    }
}
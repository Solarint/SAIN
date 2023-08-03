using EFT;
using SAIN.BotPresets;
using SAIN.Helpers;
using SAIN.SAINPreset;
using System.Collections.Generic;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.BotSettings
{
    public class BotSettingsClass
    {
        public BotSettingsClass(SAINPresetDefinition Preset)
        {
            BotDifficulty[] Difficulties = EnumValues.Difficulties;
            foreach (var BotType in BotTypeDefinitions.BotTypesList)
            {
                string name = BotType.Name;
                WildSpawnType wildSpawnType = BotType.WildSpawnType;

                string[] sainFolders = Folders(Preset.Name, "SAIN");

                if (!Load.LoadObject(out SAINSettingsGroup settings, name, sainFolders))
                {
                    settings = new SAINSettingsGroup(name, wildSpawnType, Difficulties);
                    Save.SaveJson(settings, name, sainFolders);
                }

                SAINSettings.Add(wildSpawnType, settings);
                /*
                string[] eftFolders = Folders(Preset.Name, "EFT");
                if (!Load.LoadObject(out EFTBotSettings eftSettings, name, eftFolders))
                {
                    eftSettings = new EFTBotSettings(name, wildSpawnType, Difficulties);
                    Save.SaveJson(eftSettings, name, eftFolders);
                }

                SAINSettings.Add(wildSpawnType, settings);
                EFTSettings.Add(wildSpawnType, eftSettings);
                */
            }
        }

        public void SaveSettings(SAINPresetDefinition preset)
        {
            string[] sainFolders = Folders(preset.Name, "SAIN");
            string[] eftFolders = Folders(preset.Name, "EFT");

            foreach (SAINSettingsGroup settings in SAINSettings.Values)
            {
                Save.SaveJson(settings, settings.Name, sainFolders);
            }
            return;

            foreach (EFTBotSettings settings in EFTSettings.Values)
            {
                Save.SaveJson(settings, settings.Name, eftFolders);
            }
        }

        private static string[] Folders(string presetKey, string subfolder) => new string[] { "Presets", presetKey, "BotSettings", subfolder };

        public readonly Dictionary<WildSpawnType, SAINSettingsGroup> SAINSettings = new Dictionary<WildSpawnType, SAINSettingsGroup>();
        public readonly Dictionary<WildSpawnType, EFTBotSettings> EFTSettings = new Dictionary<WildSpawnType, EFTBotSettings>();
    }
}
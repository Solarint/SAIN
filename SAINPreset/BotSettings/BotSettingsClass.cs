using EFT;
using HarmonyLib;
using SAIN.BotPresets;
using SAIN.BotSettings.Categories.Util;
using SAIN.Helpers;
using SAIN.SAINPreset;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms.VisualStyles;
using static HBAO_Core;
using static Mono.Security.X509.X520;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.BotSettings
{
    public class BotSettingsClass
    {
        public BotSettingsClass(SAINPresetDefinition Preset)
        {
            BotDifficulty[] Difficulties = EnumValues.Difficulties;
            BotType[] BotTypes = BotTypeDefinitions.BotTypes;
            foreach (var BotType in BotTypes)
            {
                string name = BotType.Name;
                WildSpawnType wildSpawnType = BotType.WildSpawnType;

                string[] sainFolders = Folders(Preset.Key, "SAIN");
                string[] eftFolders = Folders(Preset.Key, "EFT");

                if (!Load.LoadObject(out SAINSettingsGroup settings, name, sainFolders))
                {
                    settings = new SAINSettingsGroup(name, wildSpawnType, Difficulties);
                    Save.SaveJson(settings, name, sainFolders);
                }

                if (!Load.LoadObject(out EFTBotSettings eftSettings, name, eftFolders))
                {
                    eftSettings = new EFTBotSettings(name, wildSpawnType, Difficulties);
                    Save.SaveJson(eftSettings, name, eftFolders);
                }

                SAINSettings.Add(wildSpawnType, settings);
                EFTSettings.Add(wildSpawnType, eftSettings);
            }
        }

        public void SaveSettings(SAINPresetDefinition preset)
        {
            string[] sainFolders = Folders(preset.Key, "SAIN");
            string[] eftFolders = Folders(preset.Key, "EFT");

            foreach (SAINSettingsGroup settings in SAINSettings.Values)
            {
                Save.SaveJson(settings, settings.Name, sainFolders);
            }

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
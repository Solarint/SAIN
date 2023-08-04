using BepInEx.Logging;
using EFT;
using HarmonyLib;
using SAIN.BotPresets;
using SAIN.Helpers;
using SAIN.SAINPreset;
using SAIN.SAINPreset.Attributes;
using SAIN.SAINPreset.Settings;
using System;
using System.Collections.Generic;
using System.Reflection;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.BotSettings
{
    public class BotSettingsClass
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(BotSettingsClass));
        private readonly SAINPresetDefinition Preset;

        public BotSettingsClass(SAINPresetDefinition preset)
        {
            Preset = preset;
            BotDifficulty[] Difficulties = EnumValues.Difficulties;

            string[] eftFolders = Folders(preset.Name, "EFT");
            string[] sainFolders = Folders(preset.Name, "SAIN");

            foreach (var BotType in BotTypeDefinitions.BotTypesList)
            {
                string name = BotType.Name;
                WildSpawnType wildSpawnType = BotType.WildSpawnType;

                if (!EFTSettings.ContainsKey(wildSpawnType))
                {
                    if (Load.LoadObject(out EFTBotSettings eftSettings, name, eftFolders))
                    {
                        EFTSettings.Add(wildSpawnType, eftSettings);
                    }
                }

                if (!Load.LoadObject(out SAINSettingsGroup settings, name, sainFolders))
                {
                    settings = new SAINSettingsGroup(name, wildSpawnType, Difficulties);
                    UpdateSAINSettingsToEFTDefault(wildSpawnType, settings);
                    Save.SaveJson(settings, name, sainFolders);
                }

                SAINSettings.Add(wildSpawnType, settings);

            }
        }

        private void UpdateSAINSettingsToEFTDefault(WildSpawnType wildSpawnType, SAINSettingsGroup sainSettingsGroup)
        {
            foreach (var keyPair in sainSettingsGroup.Settings)
            {
                SAINSettings sainSettings = keyPair.Value;
                BotDifficulty Difficulty = keyPair.Key;

                // Get SAIN and EFT settings for the given WildSpawnType and difficulties
                object eftSettings = GetEFTSettings(wildSpawnType, Difficulty);
                if (eftSettings != null)
                {
                    CopyValuesAtoB(eftSettings, sainSettings, (field) => ShallUseEFTBotDefault(field));
                }
            }
        }

        private void CopyValuesAtoB(object A, object B, Func<FieldInfo, bool> shouldCopyFieldFunc = null)
        {
            // Get the names of the fields in EFT settings
            List<string> ACatNames = AccessTools.GetFieldNames(A);
            foreach (FieldInfo BCatField in Reflection.GetFieldsInType(B.GetType()))
            {
                // Check if the category inside SAIN Settings has a matching category in EFT settings
                if (ACatNames.Contains(BCatField.Name))
                {
                    // Get the value of the category from SAIN settings
                    object BCatObject = BCatField.GetValue(B);
                    // Get the fields inside that category from SAIN settings
                    FieldInfo[] BVariableFieldArray = Reflection.GetFieldsInType(BCatField.FieldType);

                    // Get the category of the matching sain category from EFT settings
                    FieldInfo ACatField = AccessTools.Field(A.GetType(), BCatField.Name);
                    if (ACatField != null)
                    {
                        // Get the value of the EFT settings Category
                        object ACatObject = ACatField.GetValue(A);
                        // List the field names in that category
                        List<string> AVariableNames = AccessTools.GetFieldNames(ACatObject);

                        foreach (FieldInfo BVariableField in BVariableFieldArray)
                        {
                            // Check if the sain variable is set to grab default EFT numbers and that it exists inside the EFT settings category
                            if (AVariableNames.Contains(BVariableField.Name))
                            {
                                if (shouldCopyFieldFunc != null && !shouldCopyFieldFunc(BVariableField))
                                {
                                    continue;
                                }
                                // Get the Variable from this category that matched
                                FieldInfo AVariableField = AccessTools.Field(ACatObject.GetType(), BVariableField.Name);
                                if (AVariableField != null)
                                {
                                    // Get the final Value of the variable from EFT settings, and set the SAIN Setting variable to that value
                                    object AValue = AVariableField.GetValue(ACatObject);
                                    BVariableField.SetValue(BCatObject, AValue);
                                    Logger.LogWarning($"Set [{BVariableField.Name}] to [{AValue}]");
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool ShallUseEFTBotDefault(FieldInfo field) => field.GetCustomAttribute<UseEFTBotDefaultAttribute>()?.Value == true;

        public void LoadEFTSettings()
        {
            BotDifficulty[] Difficulties = EnumValues.Difficulties;
            foreach (var BotType in BotTypeDefinitions.BotTypesList)
            {
                string name = BotType.Name;
                WildSpawnType wildSpawnType = BotType.WildSpawnType;

                if (!EFTSettings.ContainsKey(wildSpawnType))
                {
                    string[] eftFolders = Folders(Preset.Name, "EFT");

                    if (!Load.LoadObject(out EFTBotSettings eftSettings, name, eftFolders))
                    {
                        eftSettings = new EFTBotSettings(name, wildSpawnType, Difficulties);
                        Save.SaveJson(eftSettings, name, eftFolders);
                    }

                    EFTSettings.Add(wildSpawnType, eftSettings);
                }
            }
        }

        public SAINSettings GetSAINSettings(WildSpawnType type, BotDifficulty difficulty)
        {
            LoadEFTSettings();
            if (SAINSettings.TryGetValue(type, out var settingsGroup))
            {
                if (settingsGroup.Settings.TryGetValue(difficulty, out var settings))
                {
                    return settings;
                }
                else
                {
                    Logger.LogError($"[{difficulty}] does not exist in [{type}] Settings Group!");
                }
            }
            else
            {
                Logger.LogError($"[{type}] does not exist in SAINSettings Dictionary!");
            }
            return null;
        }

        public object GetEFTSettings(WildSpawnType type, BotDifficulty difficulty)
        {
            LoadEFTSettings();
            if (EFTSettings.TryGetValue(type, out var settingsGroup))
            {
                if (settingsGroup.Settings.TryGetValue(difficulty, out var settings))
                {
                    return settings;
                }
                else
                {
                    Logger.LogError($"[{difficulty}] does not exist in [{type}] Settings Group!");
                }
            }
            else
            {
                Logger.LogError($"[{type}] does not exist in EFTSettings Dictionary!");
            }
            return null;
        }

        public void SaveSettings(SAINPresetDefinition preset)
        {
            string[] sainFolders = Folders(preset.Name, "SAIN");
            string[] eftFolders = Folders(preset.Name, "EFT");

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

        public Dictionary<WildSpawnType, SAINSettingsGroup> SAINSettings = new Dictionary<WildSpawnType, SAINSettingsGroup>();
        public Dictionary<WildSpawnType, EFTBotSettings> EFTSettings = new Dictionary<WildSpawnType, EFTBotSettings>();
    }
}
using BepInEx.Logging;
using EFT;
using HarmonyLib;
using SAIN.Attributes;
using SAIN.Helpers;
using SAIN.Preset.BotSettings.SAINSettings;
using System;
using System.Collections.Generic;
using System.Reflection;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.Preset.BotSettings
{
    public class BotSettingsClass
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(BotSettingsClass));
        private readonly SAINPresetDefinition Preset;

        public BotSettingsClass(SAINPresetDefinition preset)
        {
            Preset = preset;
            BotDifficulty[] Difficulties = EnumValues.Difficulties;

            string[] sainFolders = Folders(preset.Name);

            foreach (var BotType in BotTypeDefinitions.BotTypesList)
            {
                string name = BotType.Name;
                WildSpawnType wildSpawnType = BotType.WildSpawnType;

                if (!EFTSettings.ContainsKey(wildSpawnType))
                {
                    if (Load.LoadObject(out EFTBotSettings eftSettings, name, EFTFolderString))
                    {
                        EFTSettings.Add(wildSpawnType, eftSettings);
                    }
                }

                if (!Load.LoadObject(out SAINSettingsGroupClass settings, name, sainFolders))
                {
                    settings = new SAINSettingsGroupClass(Difficulties)
                    {
                        Name = name,
                        WildSpawnType = wildSpawnType,
                        DifficultyModifier = DefaultDifficultyModifier[wildSpawnType]
                    };

                    UpdateSAINSettingsToEFTDefault(wildSpawnType, settings);
                    Save.SaveJson(settings, name, sainFolders);
                }

                SAINSettings.Add(wildSpawnType, settings);
            }
        }

        private void UpdateSAINSettingsToEFTDefault(WildSpawnType wildSpawnType, SAINSettingsGroupClass sainSettingsGroup)
        {
            foreach (var keyPair in sainSettingsGroup.Settings)
            {
                SAINSettingsClass sainSettings = keyPair.Value;
                BotDifficulty Difficulty = keyPair.Key;

                // Get SAIN and EFT group for the given WildSpawnType and difficulties
                object eftSettings = GetEFTSettings(wildSpawnType, Difficulty);
                if (eftSettings != null)
                {
                    CopyValuesAtoB(eftSettings, sainSettings, (field) => ShallUseEFTBotDefault(field));
                }
            }
        }

        private void CopyValuesAtoB(object A, object B, Func<FieldInfo, bool> shouldCopyFieldFunc = null)
        {
            // Get the names of the fields in EFT group
            List<string> ACatNames = AccessTools.GetFieldNames(A);
            foreach (FieldInfo BCatField in Reflection.GetFieldsInType(B.GetType()))
            {
                // Check if the category inside SAIN GlobalSettings has a matching category in EFT group
                if (ACatNames.Contains(BCatField.Name))
                {
                    // Get the multiplier of the category from SAIN group
                    object BCatObject = BCatField.GetValue(B);
                    // Get the fields inside that category from SAIN group
                    FieldInfo[] BVariableFieldArray = Reflection.GetFieldsInType(BCatField.FieldType);

                    // Get the category of the matching sain category from EFT group
                    FieldInfo ACatField = AccessTools.Field(A.GetType(), BCatField.Name);
                    if (ACatField != null)
                    {
                        // Get the multiplier of the EFT group Category
                        object ACatObject = ACatField.GetValue(A);
                        // List the field names in that category
                        List<string> AVariableNames = AccessTools.GetFieldNames(ACatObject);

                        foreach (FieldInfo BVariableField in BVariableFieldArray)
                        {
                            // Check if the sain variable is set to grab default EFT numbers and that it exists inside the EFT group category
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
                                    // Get the final Value of the variable from EFT group, and set the SAIN Setting variable to that multiplier
                                    object AValue = AVariableField.GetValue(ACatObject);
                                    BVariableField.SetValue(BCatObject, AValue);
                                    //Logger.LogWarning($"Set [{BVariableField.LayerName}] to [{AValue}]");
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool ShallUseEFTBotDefault(FieldInfo field) => field.GetCustomAttribute<AdvancedOptionsAttribute>()?.CopyValueFromEFT == true;

        public void LoadEFTSettings()
        {
            BotDifficulty[] Difficulties = EnumValues.Difficulties;
            foreach (var BotType in BotTypeDefinitions.BotTypesList)
            {
                string name = BotType.Name;
                WildSpawnType wildSpawnType = BotType.WildSpawnType;

                if (!EFTSettings.ContainsKey(wildSpawnType))
                {
                    if (!Load.LoadObject(out EFTBotSettings eftSettings, name, EFTFolderString))
                    {
                        eftSettings = new EFTBotSettings(name, wildSpawnType, Difficulties);
                        Save.SaveJson(eftSettings, name, EFTFolderString);
                    }

                    EFTSettings.Add(wildSpawnType, eftSettings);
                }
            }
        }

        private const string EFTFolderString = "EFT Bot Settings - DO NOT TOUCH";

        public SAINSettingsClass GetSAINSettings(WildSpawnType type, BotDifficulty difficulty)
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
            string[] sainFolders = Folders(preset.Name);

            foreach (SAINSettingsGroupClass settings in SAINSettings.Values)
            {
                Save.SaveJson(settings, settings.Name, sainFolders);
            }

            foreach (EFTBotSettings settings in EFTSettings.Values)
            {
                Save.SaveJson(settings, settings.Name, EFTFolderString);
            }
        }

        private static string[] Folders(string presetKey) => new string[] { "Presets", presetKey, "BotSettings" };

        public Dictionary<WildSpawnType, SAINSettingsGroupClass> SAINSettings = new Dictionary<WildSpawnType, SAINSettingsGroupClass>();
        public Dictionary<WildSpawnType, EFTBotSettings> EFTSettings = new Dictionary<WildSpawnType, EFTBotSettings>();

        static BotSettingsClass()
        {
            DefaultDifficultyModifier = new Dictionary<WildSpawnType, float>
            {
                { WildSpawnType.assault, 0.2f },
                { WildSpawnType.marksman, 0.2f },

                { WildSpawnType.crazyAssaultEvent, 0.33f },
                { WildSpawnType.cursedAssault, 0.33f },
                { WildSpawnType.assaultGroup, 0.33f },

                { WildSpawnType.bossBully, 0.75f },
                { WildSpawnType.bossGluhar, 0.75f },
                { WildSpawnType.bossKilla, 0.75f },
                { WildSpawnType.bossSanitar, 0.75f },
                { WildSpawnType.bossKojaniy, 0.75f },
                { WildSpawnType.bossZryachiy, 0.75f },
                { WildSpawnType.sectantPriest, 0.75f },
                { WildSpawnType.bossKnight, 0.75f },

                { WildSpawnType.sectantWarrior, 0.5f },
                { WildSpawnType.followerBully, 0.5f },
                { WildSpawnType.followerGluharAssault, 0.5f },
                { WildSpawnType.followerGluharScout, 0.5f },
                { WildSpawnType.followerGluharSecurity, 0.5f },
                { WildSpawnType.followerGluharSnipe, 0.5f },
                { WildSpawnType.followerKojaniy, 0.5f },
                { WildSpawnType.followerSanitar, 0.5f },
                { WildSpawnType.followerTagilla, 0.5f },
                { WildSpawnType.followerZryachiy, 0.5f },

                { WildSpawnType.followerBigPipe, 0.66f },
                { WildSpawnType.followerBirdEye, 0.66f },
                { WildSpawnType.pmcBot, 0.66f },
                { WildSpawnType.exUsec, 0.66f },
                { WildSpawnType.arenaFighter, 0.66f },
                { WildSpawnType.arenaFighterEvent, 0.66f },

                { EnumValues.WildSpawn.Usec, 1f },
                { EnumValues.WildSpawn.Bear, 1f },
            };

            foreach (WildSpawnType type in BotTypeDefinitions.BotTypes.Keys)
            {
                if (!DefaultDifficultyModifier.ContainsKey(type))
                {
                    DefaultDifficultyModifier.Add(type, 0.5f);
                }
            }
        }

        public static readonly Dictionary<WildSpawnType, float> DefaultDifficultyModifier;
    }
}
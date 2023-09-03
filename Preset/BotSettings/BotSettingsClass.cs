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
    public class BotSettingsClass : BasePreset
    {
        public BotSettingsClass(SAINPresetClass preset) : base(preset)
        {
            ImportBotSettings();
        }

        private void ImportBotSettings()
        {
            if (Preset == null)
            {
                Logger.LogError($"Preset Is Null in {GetType().Name}");
                return;
            }

            BotDifficulty[] Difficulties = EnumValues.Difficulties;
            foreach (var BotType in BotTypeDefinitions.BotTypesList)
            {
                string name = BotType.Name;
                WildSpawnType wildSpawnType = BotType.WildSpawnType;

                if (Load.LoadObject(out EFTBotSettings eftSettingsGroup, name, EFTFolderString))
                {
                    if (!EFTSettings.ContainsKey(wildSpawnType))
                        EFTSettings.Add(wildSpawnType, eftSettingsGroup);
                }
                else
                {
                    Logger.LogError($"Failed to import EFT Bot Settings for {name}");
                }
                if (!Preset.Import(out SAINSettingsGroupClass sainSettingsGroup, name, "BotSettings"))
                {
                    sainSettingsGroup = new SAINSettingsGroupClass(Difficulties)
                    {
                        Name = name,
                        WildSpawnType = wildSpawnType,
                        DifficultyModifier = DefaultDifficultyModifier[wildSpawnType]
                    };

                    UpdateSAINSettingsToEFTDefault(wildSpawnType, sainSettingsGroup);
                    Preset.Export(sainSettingsGroup, name, "BotSettings");
                }
                SAINSettings.Add(wildSpawnType, sainSettingsGroup);
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
                                    // Get the final Rounding of the variable from EFT group, and set the SAIN Setting variable to that multiplier
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

        private bool ShallUseEFTBotDefault(FieldInfo field) => AttributesGUI.GetAttributeInfo(field)?.CopyValue == true;

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
                        SaveObjectToJson(eftSettings, name, EFTFolderString);
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
            return SAINSettings[EnumValues.WildSpawn.Usec].Settings[BotDifficulty.normal];
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
            return EFTSettings[EnumValues.WildSpawn.Usec].Settings[BotDifficulty.normal];
        }

        public void ExportBotSettings()
        {
            if (Preset == null)
            {
                Logger.LogError($"Preset Is Null in {GetType().Name}");
                return;
            }

            foreach (SAINSettingsGroupClass settings in SAINSettings.Values)
            {
                Preset.Export(settings, settings.Name, "BotSettings");
            }
        }

        public Dictionary<WildSpawnType, SAINSettingsGroupClass> SAINSettings = new Dictionary<WildSpawnType, SAINSettingsGroupClass>();
        public Dictionary<WildSpawnType, EFTBotSettings> EFTSettings = new Dictionary<WildSpawnType, EFTBotSettings>();

        static BotSettingsClass()
        {
            DefaultDifficultyModifier = new Dictionary<WildSpawnType, float>
            {
                { WildSpawnType.assault, 0.35f },
                { WildSpawnType.marksman, 0.35f },

                { WildSpawnType.crazyAssaultEvent, 0.35f },
                { WildSpawnType.cursedAssault, 0.35f },
                { WildSpawnType.assaultGroup, 0.35f },

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
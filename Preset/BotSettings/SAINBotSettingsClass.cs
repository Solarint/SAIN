using EFT;
using HarmonyLib;
using SAIN.Attributes;
using SAIN.Components.BotController;
using SAIN.Helpers;
using SAIN.Preset.BotSettings.SAINSettings;
using System;
using System.Collections.Generic;
using System.Reflection;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.Preset.BotSettings
{
    public class SAINBotSettingsClass : BasePreset
    {
        public SAINBotSettingsClass(SAINPresetClass preset) : base(preset)
        {
            LoadEFTSettings();
            LoadSAINSettings();
        }

        public void Update()
        {
            foreach (var settings in SAINSettings) {
                foreach (var group in settings.Value.Settings) {
                    group.Value.Update();
                }
            }
        }

        public void Init()
        {
            foreach (var settings in SAINSettings) {
                foreach (var group in settings.Value.Settings) {
                    group.Value.Init();
                }
            }
        }

        public void UpdateDefaults(SAINBotSettingsClass replacement)
        {
            foreach (var settings in SAINSettings) {
                var replacementSettings = replacement?.SAINSettings[settings.Key];
                foreach (var group in settings.Value.Settings) {
                    var replacementGroup = replacementSettings?.Settings[group.Key];
                    group.Value.UpdateDefaults(replacementGroup);
                }
            }
        }

        private void LoadSAINSettings()
        {
            BotDifficulty[] Difficulties = EnumValues.Difficulties;
            foreach (var BotType in BotTypeDefinitions.BotTypesList) {
                string name = BotType.Name;
                WildSpawnType wildSpawnType = BotType.WildSpawnType;

                if (BotSpawnController.StrictExclusionList.Contains(wildSpawnType)) {
                }

                SAINSettingsGroupClass sainSettingsGroup;
                if (Preset.Info.IsCustom == false || !SAINPresetClass.Import(out sainSettingsGroup, Preset.Info.Name, name, "BotSettings")) {
                    sainSettingsGroup = new SAINSettingsGroupClass(Difficulties) {
                        Name = name,
                        WildSpawnType = wildSpawnType,
                        DifficultyModifier = DefaultDifficultyModifier[wildSpawnType]
                    };

                    UpdateSAINSettingsToEFTDefault(wildSpawnType, sainSettingsGroup);

                    if (Preset.Info.IsCustom == true) {
                        SAINPresetClass.Export(sainSettingsGroup, Preset.Info.Name, name, "BotSettings");
                    }
                }
                SAINSettings.Add(wildSpawnType, sainSettingsGroup);
            }
        }

        private void UpdateSAINSettingsToEFTDefault(WildSpawnType wildSpawnType, SAINSettingsGroupClass sainSettingsGroup)
        {
            foreach (var keyPair in sainSettingsGroup.Settings) {
                SAINSettingsClass sainSettings = keyPair.Value;
                BotDifficulty Difficulty = keyPair.Key;

                // Get SAIN and EFT group for the given WildSpawnType and difficulties
                object eftSettings = GetEFTSettings(wildSpawnType, Difficulty);
                if (eftSettings != null) {
                    CopyValuesAtoB(eftSettings, sainSettings, (field) => ShallUseEFTBotDefault(field));
                }
            }
        }

        private void CopyValuesAtoB(object A, object B, Func<FieldInfo, bool> shouldCopyFieldFunc = null)
        {
            // Get the names of the fields in EFT group
            List<string> ACatNames = AccessTools.GetFieldNames(A);
            foreach (FieldInfo BCatField in Reflection.GetFieldsInType(B.GetType())) {
                // Check if the category inside SAIN GlobalSettings has a matching category in EFT group
                if (ACatNames.Contains(BCatField.Name)) {
                    // Get the multiplier of the category from SAIN group
                    object BCatObject = BCatField.GetValue(B);
                    // Get the fields inside that category from SAIN group
                    FieldInfo[] BVariableFieldArray = Reflection.GetFieldsInType(BCatField.FieldType);

                    // Get the category of the matching sain category from EFT group
                    FieldInfo ACatField = AccessTools.Field(A.GetType(), BCatField.Name);
                    if (ACatField != null) {
                        // Get the value of the EFT group Category
                        object ACatObject = ACatField.GetValue(A);
                        // list the field names in that category
                        List<string> AVariableNames = AccessTools.GetFieldNames(ACatObject);

                        foreach (FieldInfo BVariableField in BVariableFieldArray) {
                            // Check if the sain variable is set to grab default EFT numbers and that it exists inside the EFT group category
                            if (AVariableNames.Contains(BVariableField.Name)) {
                                if (shouldCopyFieldFunc != null && !shouldCopyFieldFunc(BVariableField)) {
                                    continue;
                                }
                                // Get the Variable from this category that matched
                                FieldInfo AVariableField = AccessTools.Field(ACatObject.GetType(), BVariableField.Name);
                                if (AVariableField != null) {
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
            foreach (var BotType in BotTypeDefinitions.BotTypesList) {
                string name = BotType.Name;
                WildSpawnType wildSpawnType = BotType.WildSpawnType;

                if (!EFTSettings.ContainsKey(wildSpawnType)) {
                    if (!Load.LoadObject(out EFTBotSettings eftSettings, name, "Default Bot Config Values")) {
                        Logger.LogError($"Failed to Import EFT Bot Settings for {name}");
                        eftSettings = new EFTBotSettings(name, wildSpawnType, Difficulties);
                        SaveObjectToJson(eftSettings, name, "Default Bot Config Values");
                    }
                    if (wildSpawnType != WildSpawnType.shooterBTR
                        && wildSpawnType != WildSpawnType.bossZryachiy
                        && wildSpawnType != WildSpawnType.followerZryachiy) {
                        foreach (var settings in eftSettings.Settings) {
                            //var enemyTypes = settings.Value.Mind.ENEMY_BOT_TYPES;
                            //if (enemyTypes.Length == 0)
                            //{
                            //    Logger.LogError($"{name} : {settings.Key} has empty enemy types.");
                            //}
                            //for (int i = 0; i < enemyTypes.Length; i++)
                            //{
                            //    _enemyTypeList.Add(enemyTypes[i]);
                            //}
                            //if (!_enemyTypeList.Contains(EnumValues.WildSpawn.Usec))
                            //{
                            //    Logger.LogError($"{name} : {settings.Key} is missing Usec as enemy type");
                            //    _enemyTypeList.Add(EnumValues.WildSpawn.Usec);
                            //}
                            //if (!_enemyTypeList.Contains(EnumValues.WildSpawn.Bear))
                            //{
                            //    Logger.LogError($"{name} : {settings.Key} is missing Bear as enemy type");
                            //    _enemyTypeList.Add(EnumValues.WildSpawn.Bear);
                            //}
                            //settings.Value.Mind.ENEMY_BOT_TYPES = _enemyTypeList.ToArray();
                            //_enemyTypeList.Clear();
                        }
                    }
                    EFTSettings.Add(wildSpawnType, eftSettings);
                }
            }
        }

        private static List<WildSpawnType> _enemyTypeList = new List<WildSpawnType>();

        public SAINSettingsClass GetSAINSettings(WildSpawnType type, BotDifficulty difficulty)
        {
            LoadEFTSettings();
            if (SAINSettings.TryGetValue(type, out var settingsGroup)) {
                if (settingsGroup.Settings.TryGetValue(difficulty, out var settings)) {
                    return settings;
                }
                else {
                    Logger.LogError($"[{difficulty}] does not exist in [{type}] SAIN Settings!");
                }
            }
            else {
                Logger.LogError($"[{type}] does not exist in SAINSettings Dictionary!");
            }
            return SAINSettings[WildSpawnType.pmcUSEC].Settings[BotDifficulty.normal];
        }

        public object GetEFTSettings(WildSpawnType type, BotDifficulty difficulty)
        {
            LoadEFTSettings();
            if (EFTSettings.TryGetValue(type, out var settingsGroup)) {
                if (settingsGroup.Settings.TryGetValue(difficulty, out var settings)) {
                    return settings;
                }
                else {
                    Logger.LogError($"[{difficulty}] does not exist in [{type}] Settings Group!");
                }
            }
            else {
                Logger.LogError($"[{type}] does not exist in EFTSettings Dictionary!");
            }
            return EFTSettings[WildSpawnType.pmcUSEC].Settings[BotDifficulty.normal];
        }

        public Dictionary<WildSpawnType, SAINSettingsGroupClass> SAINSettings = new Dictionary<WildSpawnType, SAINSettingsGroupClass>();
        public Dictionary<WildSpawnType, EFTBotSettings> EFTSettings = new Dictionary<WildSpawnType, EFTBotSettings>();

        static SAINBotSettingsClass()
        {
            DefaultDifficultyModifier = new Dictionary<WildSpawnType, float>
            {
                { WildSpawnType.assault, 0.3f },
                { WildSpawnType.marksman, 0.3f },

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
                { WildSpawnType.bossPartisan, 0.75f },
                { WildSpawnType.bossKnight, 1f },

                { WildSpawnType.sectantWarrior, 0.7f },
                { WildSpawnType.followerBully, 0.55f },
                { WildSpawnType.followerGluharAssault, 0.55f },
                { WildSpawnType.followerGluharScout, 0.55f },
                { WildSpawnType.followerGluharSecurity, 0.55f },
                { WildSpawnType.followerGluharSnipe, 0.55f },
                { WildSpawnType.followerKojaniy, 0.55f },
                { WildSpawnType.followerSanitar, 0.55f },
                { WildSpawnType.followerTagilla, 0.55f },
                { WildSpawnType.followerZryachiy, 0.55f },

                { WildSpawnType.followerBigPipe, 1f },
                { WildSpawnType.followerBirdEye, 1f },
                { WildSpawnType.pmcBot, 0.66f },
                { WildSpawnType.exUsec, 0.66f },
                { WildSpawnType.arenaFighter, 0.66f },
                { WildSpawnType.arenaFighterEvent, 0.66f },

                { WildSpawnType.pmcUSEC, 1f },
                { WildSpawnType.pmcBEAR, 1f },
            };

            foreach (WildSpawnType type in BotTypeDefinitions.BotTypes.Keys) {
                if (!DefaultDifficultyModifier.ContainsKey(type)) {
                    DefaultDifficultyModifier.Add(type, 0.5f);
                }
            }
        }

        public static readonly Dictionary<WildSpawnType, float> DefaultDifficultyModifier;
    }
}
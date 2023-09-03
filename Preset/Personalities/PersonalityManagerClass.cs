using EFT;
using SAIN.Editor;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.Preset.GlobalSettings;
using System.Collections.Generic;
using System.Linq;
using static SAIN.Helpers.EnumValues;

namespace SAIN.Preset.Personalities
{
    public class PersonalityManagerClass : BasePreset
    {
        public PersonalityManagerClass(SAINPresetClass preset) : base(preset)
        {
            ImportPersonalities();
        }


        public bool VerificationPassed = true;

        private void ImportPersonalities()
        {
            foreach (var item in EnumValues.Personalities)
            {
                if (Preset.Import(out PersonalitySettingsClass personality, item.ToString(), nameof(Personalities)))
                {
                    Personalities.Add(item, personality);
                }
            }

            InitDefaults();

            bool hadToFix = false;
            foreach (var item in Personalities)
            {
                if (item.Value.Variables.AllowedTypes.Count == 0)
                {
                    hadToFix = true;
                    if (item.Key == IPersonality.Chad || item.Key == IPersonality.GigaChad)
                    {
                        AddPMCTypes(item.Value.Variables.AllowedTypes);
                    }
                    else
                    {
                        AddAllBotTypes(item.Value.Variables.AllowedTypes);
                    }
                }
            }
            if (hadToFix)
            {
                Logger.LogWarning("The Preset you are using is out of date, and required manual fixing. Its recommended you create a new one.");
            }
        }

        public void ExportPersonalities()
        {
            foreach (var pers in Personalities)
            {
                if (pers.Value != null && Preset?.Export(pers.Value, pers.Key.ToString(), nameof(Personalities)) == true)
                {
                    continue;
                }
                else if (pers.Value == null)
                {
                    Logger.LogError("Personality Settings Are Null");
                }
                else if (Preset == null)
                {
                    Logger.LogError("Preset Is Null");
                }
                else
                {
                    Logger.LogError($"Failed to Export {pers.Key}");
                }
            }
        }

        private void InitDefaults()
        {
            var pers = IPersonality.GigaChad;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "A true alpha threat. Hyper Aggressive and typically wearing high tier equipment.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    Variables =
                    {
                        Enabled = true,
                        CanJumpCorners = true,
                        RandomChanceIfMeetRequirements = 60,
                        RandomlyAssignedChance = 3,
                        CanTaunt = true,
                        CanRespondToVoice = true,
                        TauntFrequency = 8,
                        TauntMaxDistance = 50f,
                        HoldGroundBaseTime = 2f,
                        HoldGroundMaxRandom = 1.5f,
                        HoldGroundMinRandom = 0.65f,
                        SearchBaseTime = 0.65f,
                        PowerLevelMin = 115f,
                        SprintWhileSearch = true,
                        FrequentSprintWhileSearch = true,
                        CanRushEnemyReloadHeal = true,
                        ConstantTaunt = true,
                        AggressionMultiplier = 4f,
                    }
                };

                AddPMCTypes(settings.Variables.AllowedTypes);
                Personalities.Add(pers, settings);
                Preset.Export(settings, pers.ToString(), nameof(Personalities));
            }

            pers = IPersonality.Chad;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "An aggressive player. Typically wearing high tier equipment, and is more aggressive than usual.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    Variables =
                    {
                        Enabled = true,
                        CanJumpCorners = true,
                        RandomChanceIfMeetRequirements = 60,
                        RandomlyAssignedChance = 3,
                        CanTaunt = true,
                        CanRespondToVoice = true,
                        TauntFrequency = 10,
                        TauntMaxDistance = 40f,
                        HoldGroundBaseTime = 1.5f,
                        HoldGroundMaxRandom = 1.5f,
                        HoldGroundMinRandom = 0.65f,
                        SearchBaseTime = 4f,
                        PowerLevelMin = 85f,
                        SprintWhileSearch = true,
                        CanRushEnemyReloadHeal = true,
                        FrequentTaunt = true,
                        AggressionMultiplier = 2f,
                    }
                };

                AddPMCTypes(settings.Variables.AllowedTypes);
                Personalities.Add(pers, settings);
                Preset.Export(settings, pers.ToString(), nameof(Personalities));
            }

            pers = IPersonality.Rat;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "Scum of Tarkov. Rarely Seeks out enemies, and will hide and ambush.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    Variables =
                    {
                        Enabled = true,
                        RandomChanceIfMeetRequirements = 60,
                        RandomlyAssignedChance = 30,
                        HoldGroundBaseTime = 1f,
                        SearchBaseTime = 240f,
                        PowerLevelMax = 50f,
                        AggressionMultiplier = 0.6f,
                        Sneaky = true,
                        BaseSearchMoveSpeed = 0.2f,
                    }
                };

                AddAllBotTypes(settings.Variables.AllowedTypes);
                Personalities.Add(pers, settings);
                Preset.Export(settings, pers.ToString(), nameof(Personalities));
            }

            pers = IPersonality.Timmy;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "A New Player, terrified of everything.";

                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    Variables =
                    {
                        Enabled = true,
                        RandomlyAssignedChance = 50,
                        PowerLevelMax = 40f,
                        MaxLevel = 10,
                        HoldGroundBaseTime = 0.6f,
                        SearchBaseTime = 120f,
                        AggressionMultiplier = 0.75f,
                        BaseSearchMoveSpeed = 0.65f,
                    }
                };

                AddPMCTypes(settings.Variables.AllowedTypes);
                Personalities.Add(pers, settings);
                Preset.Export(settings, pers.ToString(), nameof(Personalities));
            }

            pers = IPersonality.Coward;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "A player who is more passive and afraid than usual.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    Variables =
                    {
                        Enabled = true,
                        RandomlyAssignedChance = 30,
                        HoldGroundBaseTime = 0.33f,
                        SearchBaseTime = 75f,
                        AggressionMultiplier = 0.4f,
                        BaseSearchMoveSpeed = 0.35f,
                    }
                };
                AddAllBotTypes(settings.Variables.AllowedTypes);
                Personalities.Add(pers, settings);
                Preset.Export(settings, pers.ToString(), nameof(Personalities));
            }

            pers = IPersonality.Normal;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "An Average Tarkov Enjoyer";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    Variables =
                    {
                        Enabled = true,
                        HoldGroundBaseTime = 0.75f,
                        SearchBaseTime = 30f,
                        CanRespondToVoice = true,
                        BaseSearchMoveSpeed = 0.8f,
                    }
                };

                AddAllBotTypes(settings.Variables.AllowedTypes);
                Personalities.Add(pers, settings);
                Preset.Export(settings, pers.ToString(), nameof(Personalities));
            }
        }

        private static void AddAllBotTypes(List<string> allowedTypes)
        {
            allowedTypes.Clear();
            allowedTypes.AddRange(BotTypeDefinitions.BotTypesNames);
        }

        private static void AddPMCTypes(List<string> allowedTypes)
        {
            allowedTypes.Add(BotTypeDefinitions.BotTypes[WildSpawn.Usec].Name);
            allowedTypes.Add(BotTypeDefinitions.BotTypes[WildSpawn.Bear].Name);
        }

        public Dictionary<IPersonality, PersonalitySettingsClass> Personalities =
            new Dictionary<IPersonality, PersonalitySettingsClass>();
    }
}
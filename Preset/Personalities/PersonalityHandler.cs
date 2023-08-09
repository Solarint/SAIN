using EFT;
using SAIN.Helpers;
using System.Collections.Generic;

namespace SAIN.Preset.Personalities
{
    public class PersonalityManagerClass
    {
        private static Dictionary<WildSpawnType, BotType> AddAllBotTypes(Dictionary<WildSpawnType, BotType> botTypes)
        {
            botTypes.AddRange(BotTypeDefinitions.BotTypes);
            return botTypes;
        }

        private static Dictionary<WildSpawnType, BotType> AddPMCTypes(Dictionary<WildSpawnType, BotType> botTypes)
        {
            AddWildSpawn(botTypes, EnumValues.WildSpawn.Usec, EnumValues.WildSpawn.Bear);
            return botTypes;
        }

        private static Dictionary<WildSpawnType, BotType> AddWildSpawn(Dictionary<WildSpawnType, BotType> botTypes, params WildSpawnType[] types)
        {
            foreach (var type in types)
            {
                botTypes.Add(type, BotTypeDefinitions.BotTypes[type]);
            }
            return botTypes;
        }

        private static Dictionary<WildSpawnType, BotType> AddBotType(Dictionary<WildSpawnType, BotType> botTypes, params BotType[] types)
        {
            foreach (var type in types)
            {
                botTypes.Add(type.WildSpawnType, type);
            }
            return botTypes;
        }

        public PersonalityManagerClass(SAINPresetDefinition preset)
        {
            Personalities = new Dictionary<SAINPersonality, PersonalitySettingsClass>();

            string[] folders = new string[]
            {
                "Presets", preset.Name, "Personalities"
            };

            foreach (var item in EnumValues.Personalities)
            {
                if (JsonUtility.Load.LoadJsonFile(out string json, item.ToString(), folders))
                {
                    var persClass = JsonUtility.Load.DeserializeObject<PersonalitySettingsClass>(json);
                    Personalities.Add(item, persClass);
                }
            }

            InitDefaults();

            foreach (var pers in Personalities)
            {
                JsonUtility.SaveObjectToJson(pers.Value, pers.Key.ToString(), folders);
            }
        }

        private void InitDefaults()
        {
            var pers = SAINPersonality.Rat;
            if (!Personalities.ContainsKey(SAINPersonality.Rat))
            {
                string name = pers.ToString();
                string description = "Scum of Tarkov. Rarely Seeks out enemies, and will hide and ambush.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    Enabled = true,
                    Variables =
                    {
                        RandomChanceIfMeetRequirements = 60,
                        RandomlyAssignedChance = 30,
                        HoldGroundBaseTime = HoldGroundBaseTime(pers),
                        SearchBaseTime = SearchBaseTime(pers),
                        PowerLevelMax = 50f,
                    }
                };

                AddAllBotTypes(settings.AllowedBotTypes);
                Personalities.Add(pers, settings);
            }

            pers = SAINPersonality.Timmy;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "A New Player, terrified of everything.";

                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    Enabled = true,
                    Variables =
                    {
                        RandomlyAssignedChance = 50,
                        PowerLevelMax = 40f,
                        MaxLevel = 10,
                        HoldGroundBaseTime = HoldGroundBaseTime(pers),
                        SearchBaseTime = SearchBaseTime(pers),
                    }
                };

                AddPMCTypes(settings.AllowedBotTypes);
                Personalities.Add(pers, settings);
            }

            pers = SAINPersonality.Coward;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "A player who is more passive and afraid than usual.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    Enabled = true,
                    Variables =
                    {
                        RandomlyAssignedChance = 30,
                        HoldGroundBaseTime = HoldGroundBaseTime(pers),
                        SearchBaseTime = SearchBaseTime(pers)
                    }
                };
                AddAllBotTypes(settings.AllowedBotTypes);
                Personalities.Add(pers, settings);
            }

            pers = SAINPersonality.Normal;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "An Average Tarkov Enjoyer";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    Enabled = true,
                    Variables =
                    {
                        HoldGroundBaseTime = HoldGroundBaseTime(pers),
                        SearchBaseTime = SearchBaseTime(pers),
                        CanRespondToVoice = true,
                    }
                };

                AddAllBotTypes(settings.AllowedBotTypes);
                Personalities.Add(pers, settings);
            }

            pers = SAINPersonality.GigaChad;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "A true alpha threat. Hyper Aggressive and typically wearing high tier equipment.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    Enabled = true,
                    Variables =
                    {
                        CanJumpCorners = true,
                        RandomChanceIfMeetRequirements = 60,
                        RandomlyAssignedChance = 3,
                        CanTaunt = true,
                        CanRespondToVoice = true,
                        TauntFrequency = 8,
                        TauntMaxDistance = 50f,
                        HoldGroundBaseTime = HoldGroundBaseTime(pers),
                        HoldGroundMaxRandom = 2f,
                        HoldGroundMinRandom = 0.25f,
                        SearchBaseTime = SearchBaseTime(pers),
                        PowerLevelMin = 115f,
                        SprintWhileSearch = true,
                        FrequentSprintWhileSearch = true,
                        CanRushEnemyReloadHeal = true,
                        ConstantTaunt = true,
                    }
                };

                AddPMCTypes(settings.AllowedBotTypes);
                Personalities.Add(pers, settings);
            }

            pers = SAINPersonality.Chad;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "An aggressive player. Typically wearing high tier equipment, and is more aggressive than usual.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    Enabled = true,
                    Variables =
                    {
                        CanJumpCorners = true,
                        RandomChanceIfMeetRequirements = 60,
                        RandomlyAssignedChance = 3,
                        CanTaunt = true,
                        CanRespondToVoice = true,
                        TauntFrequency = 8,
                        TauntMaxDistance = 50f,
                        HoldGroundBaseTime = HoldGroundBaseTime(pers),
                        HoldGroundMaxRandom = 2f,
                        HoldGroundMinRandom = 0.5f,
                        SearchBaseTime = SearchBaseTime(pers),
                        PowerLevelMin = 85f,
                        SprintWhileSearch = true,
                        CanRushEnemyReloadHeal = true,
                        FrequentTaunt = true,
                    }
                };

                AddPMCTypes(settings.AllowedBotTypes);
                Personalities.Add(pers, settings);
            }
        }

        private static float SearchBaseTime(SAINPersonality pers)
        {
            switch (pers)
            {
                case SAINPersonality.GigaChad:
                    return 1.5f;

                case SAINPersonality.Chad:
                    return 5f;

                case SAINPersonality.Timmy:
                    return 60f;

                case SAINPersonality.Rat:
                    return 240f;

                case SAINPersonality.Coward:
                    return 60f;

                default:
                    return 24f;
            }
        }

        private static float HoldGroundBaseTime(SAINPersonality pers)
        {
            if (pers == SAINPersonality.Rat || pers == SAINPersonality.Coward || pers == SAINPersonality.Timmy)
            {
                return 0.25f;
            }
            else if (pers == SAINPersonality.GigaChad)
            {
                return 2f;
            }
            else if (pers == SAINPersonality.Chad)
            {
                return 1.5f;
            }
            else
            {
                return 1f;
            }
        }

        public Dictionary<SAINPersonality, PersonalitySettingsClass> Personalities;
    }
}
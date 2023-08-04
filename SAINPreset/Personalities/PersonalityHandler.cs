using EFT;
using SAIN.BotPresets;
using SAIN.Classes;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.SAINPreset.Personalities
{
    public class PersonalityManagerClass
    {
        static PersonalityManagerClass()
        {
            AllTypes = new List<WildSpawnType>();
            foreach (BotType type in BotTypeDefinitions.BotTypesList)
            {
                PMCTypes.Add(type.WildSpawnType);
            }
            PMCTypes = new List<WildSpawnType>
            {
                EnumValues.WildSpawn.Usec,
                EnumValues.WildSpawn.Bear
            };
        }
        public static List<WildSpawnType> AllTypes;
        public static List<WildSpawnType> PMCTypes;

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
                JsonUtility.Save.SaveJson(pers.Value, pers.Key.ToString(), folders);
            }
        }

        void InitDefaults()
        {
            var pers = SAINPersonality.Rat;
            if (!Personalities.ContainsKey(SAINPersonality.Rat))
            {
                string name = pers.ToString();
                string description = "Scum of Tarkov. Rarely Seeks out enemies, and will hide and ambush.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    RandomChanceIfMeetRequirements = 60,
                    TrueRandomChance = 30,
                    HoldGroundBaseTime = HoldGroundBaseTime(pers),
                    SearchBaseTime = SearchBaseTime(pers),
                    PowerLevelMax = 50f,
                    AllowedBotTypes = AllTypes
                };
                Personalities.Add(pers, settings);
            }

            pers = SAINPersonality.Timmy;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "A New Player, terrified of everything.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    TrueRandomChance = 50,
                    HoldGroundBaseTime = HoldGroundBaseTime(pers),
                    SearchBaseTime = SearchBaseTime(pers),
                    PowerLevelMax = 40f,
                    MaxLevel = 10,
                    AllowedBotTypes = AllTypes
                };
                Personalities.Add(pers, settings);
            }

            pers = SAINPersonality.Coward;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "A player who is more passive and afraid than usual.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    TrueRandomChance = 30,
                    HoldGroundBaseTime = HoldGroundBaseTime(pers),
                    SearchBaseTime = SearchBaseTime(pers),
                    AllowedBotTypes = AllTypes
                };
                Personalities.Add(pers, settings);
            }

            pers = SAINPersonality.Normal;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "An Average Tarkov Enjoyer";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    HoldGroundBaseTime = HoldGroundBaseTime(pers),
                    SearchBaseTime = SearchBaseTime(pers),
                    CanRespondToVoice = true,
                    AllowedBotTypes = AllTypes
                };
                Personalities.Add(pers, settings);
            }

            pers = SAINPersonality.GigaChad;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "A true alpha threat. Hyper Aggressive and typically wearing high tier equipment.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    CanJumpCorners = true,
                    RandomChanceIfMeetRequirements = 60,
                    TrueRandomChance = 3,
                    AllowedBotTypes = PMCTypes,
                    CanTaunt = true,
                    CanRespondToVoice = true,
                    TauntFrequency = 8,
                    TauntMaxDistance = 50f,
                    HoldGroundBaseTime = HoldGroundBaseTime(pers),
                    HoldGroundMaxRandom = 2f,
                    HoldGroundMinRandom = 0.25f,
                    SearchBaseTime = SearchBaseTime(pers),
                    PowerLevelMin = 115f
                };
                Personalities.Add(pers, settings);
            }

            pers = SAINPersonality.Chad;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "An aggressive player. Typically wearing high tier equipment, and is more aggressive than usual.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    CanJumpCorners = true,
                    RandomChanceIfMeetRequirements = 60,
                    TrueRandomChance = 3,
                    AllowedBotTypes = PMCTypes,
                    CanTaunt = true,
                    CanRespondToVoice = true,
                    TauntFrequency = 8,
                    TauntMaxDistance = 50f,
                    HoldGroundBaseTime = HoldGroundBaseTime(pers),
                    HoldGroundMaxRandom = 2f,
                    HoldGroundMinRandom = 0.5f,
                    SearchBaseTime = SearchBaseTime(pers),
                    PowerLevelMin = 85f
                };
                Personalities.Add(pers, settings);
            }
        }

        static float SearchBaseTime(SAINPersonality pers)
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

        static float HoldGroundBaseTime(SAINPersonality pers)
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

        public readonly Dictionary<SAINPersonality, PersonalitySettingsClass> Personalities;
    }
}

using EFT;
using Newtonsoft.Json;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EFT.UI.CharacterSelectionStartScreen;
using static SAIN.Editor.EditorSettings;

namespace SAIN.Classes
{
    public class PersonalityClass : SAINInfoAbstract
    {
        public PersonalityClass(BotOwner owner) : base(owner) { }

        public void Update()
        {
            Personality = GetPersonality();
            PersonalitySettings = PersonalityManager.Personalities[Personality];
        }

        public SAINPersonality GetPersonality()
        {
            if (AllGigaChads.Value || CanBePersonality(SAINPersonality.GigaChad))
            {
                return SAINPersonality.GigaChad;
            }
            if (AllChads.Value || CanBePersonality(SAINPersonality.Chad))
            {
                return SAINPersonality.Chad;
            }
            if (AllRats.Value || CanBePersonality(SAINPersonality.Rat))
            {
                return SAINPersonality.Rat;
            }
            if (CanBePersonality(SAINPersonality.Timmy))
            {
                return SAINPersonality.Timmy;
            }
            if (CanBePersonality(SAINPersonality.Coward))
            {
                return SAINPersonality.Coward;
            }
            return SAINPersonality.Normal;
        }

        bool CanBePersonality(SAINPersonality personality)
        {
            var Personalities = PersonalityManager.Personalities;
            if (Personalities == null || !Personalities.ContainsKey(personality))
            {
                return false;
            }
            return Personalities[personality].CanBePersonality(BotType, PowerLevel, PlayerLevel);
        }

        public SAINPersonality Personality { get; private set; }
        public PersonalitySettingsClass PersonalitySettings { get; private set; }
    }

    public class PersonalityManager
    {
        static PersonalityManager()
        {
            Personalities = JsonUtility.Load.LoadPersonalityClasses();

            InitDefaults();

            JsonUtility.Save.SavePersonalities(Personalities);
        }

        static readonly BotTypeEnum[] PMCOnly = { BotTypeEnum.PMC };

        static void InitDefaults()
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
                    PowerLevelMax = 50f
                };
                Personalities.Add(pers, settings);
            }

            pers = SAINPersonality.Timmy;
            if (!Personalities.ContainsKey(pers))
            {
                string  name = pers.ToString();
                string description = "A New Player, terrified of everything.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    TrueRandomChance = 50,
                    HoldGroundBaseTime = HoldGroundBaseTime(pers),
                    SearchBaseTime = SearchBaseTime(pers),
                    PowerLevelMax = 40f,
                    MaxLevel = 10
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
                    AllowedBotTypes = PMCOnly,
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
                    AllowedBotTypes = PMCOnly,
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

        public static readonly Dictionary<SAINPersonality, PersonalitySettingsClass> Personalities;
    }

    public class PersonalitySettingsClass
    {
        [JsonConstructor]
        public PersonalitySettingsClass() { }

        public PersonalitySettingsClass(SAINPersonality personality, string name, string description)
        {
            SAINPersonality = personality;
            Name = name;
            Description = description;
        }

        [JsonProperty]
        public SAINPersonality SAINPersonality { get; private set; }
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public string Description { get; private set; }

        [JsonProperty]
        public int MaxLevel { get; set; } = 100;
        [JsonProperty]
        public float TrueRandomChance { get; set; } = 3;
        [JsonProperty]
        public float RandomChanceIfMeetRequirements { get; set; } = 60;
        [JsonProperty]
        public float PowerLevelMin { get; set; } = 0;
        [JsonProperty]
        public float PowerLevelMax { get; set; } = 500;

        [JsonProperty]
        public float HoldGroundBaseTime { get; set; } = 1f;
        [JsonProperty]
        public float HoldGroundMinRandom { get; set; } = 0.66f;
        [JsonProperty]
        public float HoldGroundMaxRandom { get; set; } = 1.5f;

        [JsonProperty]
        public float SearchBaseTime { get; set; } = 45;
        [JsonProperty]
        public bool CanJumpCorners { get; set; } = false;
        [JsonProperty]
        public bool CanTaunt { get; set; } = false;
        [JsonProperty]
        public bool CanRespondToVoice { get; set; } = false;
        [JsonProperty]
        public float TauntFrequency { get; set; } = 20f;
        [JsonProperty]
        public float TauntMaxDistance { get; set; } = 20f;
        [JsonProperty]
        public BotTypeEnum[] AllowedBotTypes { get; set; } =
        {
            BotTypeEnum.None,
            BotTypeEnum.Boss,
            BotTypeEnum.Follower,
            BotTypeEnum.PMC,
            BotTypeEnum.Scav,
        };

        public bool CanBePersonality(BotTypeEnum botType, float PowerLevel, int PlayerLevel)
        {
            if (EFTMath.RandomBool(TrueRandomChance))
            {
                return true;
            }
            if (!AllowedBotTypes.Contains(botType))
            {
                return false;
            }
            if (PowerLevel > PowerLevelMax || PowerLevel < PowerLevelMin)
            {
                return false;
            }
            if (PlayerLevel > MaxLevel)
            {
                return false;
            }
            return EFTMath.RandomBool(RandomChanceIfMeetRequirements);
        }
    }
}
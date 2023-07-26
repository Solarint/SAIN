using EFT;
using Newtonsoft.Json;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using static SAIN.Editor.EditorSettings;

namespace SAIN.Classes
{
    public class PersonalityClass : SAINInfoAbstract
    {
        public PersonalityClass(BotOwner owner) : base(owner) { }

        public void Update()
        {
            SetPersonality();
            PersonalityManager.Personalities[SAINPersonality.Timmy].CanTaunt = false;
        }

        public void SetPersonality()
        {
            if (CanBeGigaChad)
            {
                Personality = SAINPersonality.GigaChad;
            }
            else if (CanBeChad)
            {
                Personality = SAINPersonality.Chad;
            }
            else if (CanBeRat)
            {
                Personality = SAINPersonality.Rat;
            }
            else if (CanBeCoward)
            {
                Personality = SAINPersonality.Coward;
            }
            else if (CanBeTimmy)
            {
                Personality = SAINPersonality.Timmy;
            }
            else
            {
                Personality = SAINPersonality.Normal;
            }
        }

        private bool CanBeGigaChad
        {
            get
            {
                if (AllGigaChads.Value)
                {
                    return true;
                }
                if (PowerLevel > 110f && IsPMC && EFTMath.RandomBool(75))
                {
                    return true;
                }
                if (EFTMath.RandomBool(RandomGigaChadChance.Value))
                {
                    return true;
                }
                return false;
            }
        }

        private bool CanBeChad
        {
            get
            {
                if (AllChads.Value)
                {
                    return true;
                }
                if (PowerLevel > 85f && IsPMC && EFTMath.RandomBool(65))
                {
                    return true;
                }
                if (EFTMath.RandomBool(RandomChadChance.Value))
                {
                    return true;
                }
                return false;
            }
        }

        private bool CanBeTimmy
        {
            get
            {
                if (BotOwner.Profile.Info.Level <= 10 && PowerLevel < 25f)
                {
                    return true;
                }
                return false;
            }
        }

        private bool CanBeRat
        {
            get
            {
                if (AllRats.Value)
                {
                    return true;
                }
                if (PowerLevel < 40 && EFTMath.RandomBool(RandomRatChance.Value))
                {
                    return true;
                }
                return false;
            }
        }

        private bool CanBeCoward
        {
            get
            {
                if (EFTMath.RandomBool(RandomCowardChance.Value))
                {
                    return true;
                }
                return false;
            }
        }

        public SAINPersonality Personality { get; private set; }
    }

    public class PersonalityManager
    {
        static PersonalityManager()
        {
            Personalities = new Dictionary<SAINPersonality, PersonalitySettingsClass>();

            var array = (SAINPersonality[])Enum.GetValues(typeof(SAINPersonality));
            foreach ( var item in array)
            {
                if (JsonUtility.Load.LoadJsonFile(out string json, item.ToString(), "Personalities"))
                {
                    var persClass = JsonUtility.Load.DeserializeObject<PersonalitySettingsClass>(json);
                    Personalities.Add(item, persClass);
                }
            }

            InitDefaults();

            foreach (var pers in Personalities)
            {
                JsonUtility.Save.SaveJson(pers.Value, pers.Key.ToString(), "Personalities");
            }
        }

        static void InitDefaults()
        {
            var pers = SAINPersonality.Rat;
            if (!Personalities.ContainsKey(pers))
            {
                string name = pers.ToString();
                string description = "Scum of Tarkov. Rarely Seeks out enemies, and will hide and ambush.";
                var settings = new PersonalitySettingsClass(pers, name, description)
                {
                    RandomChancePowerLevel = 60,
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
                    RandomChancePowerLevel = 60,
                    TrueRandomChance = 3,
                    MustBePMC = true,
                    CanTaunt = true,
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
                    RandomChancePowerLevel = 60,
                    TrueRandomChance = 3,
                    MustBePMC = true,
                    CanTaunt = true,
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
        public float RandomChancePowerLevel { get; set; } = 60;
        [JsonProperty]
        public float PowerLevelMin { get; set; } = 0;
        [JsonProperty]
        public float PowerLevelMax { get; set; } = 1000;
        [JsonProperty]
        public bool MustBePMC { get; set; } = false;

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
        public float TauntFrequency { get; set; } = 20f;
        [JsonProperty]
        public float TauntMaxDistance { get; set; } = 20f;
    }
}
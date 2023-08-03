using EFT;
using Newtonsoft.Json;
using SAIN.Classes;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.SAINPreset.Personalities
{
    public sealed class PersonalitySettingsClass
    {
        [JsonConstructor]
        public PersonalitySettingsClass() { }

        public PersonalitySettingsClass(SAINPersonality personality, string name, string description)
        {
            SAINPersonality = personality;
            Name = name;
            Description = description;
        }

        public SAINPersonality SAINPersonality;
        public string Name;
        public string Description;

        public int MaxLevel = 100;
        public float TrueRandomChance = 3;
        public float RandomChanceIfMeetRequirements = 60;
        public float PowerLevelMin = 0;
        public float PowerLevelMax = 500;
        public float HoldGroundBaseTime = 1f;
        public float HoldGroundMinRandom = 0.66f;
        public float HoldGroundMaxRandom = 1.5f;
        public float SearchBaseTime = 45;
        public bool CanJumpCorners = false;
        public bool CanTaunt = false;
        public bool CanRespondToVoice = false;
        public float TauntFrequency = 20f;
        public float TauntMaxDistance = 20f;

        public List<WildSpawnType> AllowedBotTypes = new List<WildSpawnType>();

        public bool CanBePersonality(WildSpawnType botType, float PowerLevel, int PlayerLevel)
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

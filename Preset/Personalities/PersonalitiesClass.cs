using EFT;
using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes.Info;
using System.Collections.Generic;

namespace SAIN.Preset.Personalities
{
    public sealed class PersonalitySettingsClass
    {
        [JsonConstructor]
        public PersonalitySettingsClass()
        { }

        public PersonalitySettingsClass(SAINPersonality personality, string name, string description)
        {
            SAINPersonality = personality;
            Name = name;
            Description = description;
        }

        public SAINPersonality SAINPersonality;
        public string Name;
        public string Description;

        public PersonalityVariablesClass Variables = new PersonalityVariablesClass();

        public Dictionary<WildSpawnType, BotType> AllowedBotTypes = new Dictionary<WildSpawnType, BotType>();

        public bool CanBePersonality(SAINBotInfoClass infoClass)
        {
            return CanBePersonality(infoClass.WildSpawnType, infoClass.PowerLevel, infoClass.PlayerLevel);
        }
        public bool CanBePersonality(WildSpawnType wildSpawnType, float PowerLevel, int PlayerLevel)
        {
            if (Variables.CanBeRandomlyAssigned && EFTMath.RandomBool(Variables.TrueRandomChance))
            {
                return true;
            }
            if (!AllowedBotTypes.ContainsKey(wildSpawnType))
            {
                return false;
            }
            if (PowerLevel > Variables.PowerLevelMax || PowerLevel < Variables.PowerLevelMin)
            {
                return false;
            }
            if (PlayerLevel > Variables.MaxLevel)
            {
                return false;
            }
            return EFTMath.RandomBool(Variables.RandomChanceIfMeetRequirements);
        }

        public class PersonalityVariablesClass
        {
            public bool CanBeRandomlyAssigned = true;
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
            public bool FrequentTaunt = false;
            public bool ConstantTaunt = false;
            public bool CanRespondToVoice = false;
            public float TauntFrequency = 20f;
            public float TauntMaxDistance = 20f;
            public bool SprintWhileSearch = false;
            public bool FrequentSprintWhileSearch = false;
            public bool CanRushEnemyReloadHeal = false;
            public bool CanFakeDeathRare = false;
        }
    }
}
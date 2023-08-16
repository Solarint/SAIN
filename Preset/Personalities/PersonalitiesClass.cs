using EFT;
using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes.Info;
using System.Collections.Generic;
using System.ComponentModel;

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
            if (Variables.Enabled == false)
            {
                return false;
            }
            if (Variables.CanBeRandomlyAssigned && EFTMath.RandomBool(Variables.RandomlyAssignedChance))
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
            [Advanced(AdvancedEnum.Hidden)]
            const string PowerLevelDescription = " Power level is a combined number that takes into account armor, the class of that armor, and the weapon class that is currently used by a bot." +
                " Power Level usually falls within 30 to 120 on average, and almost never goes above 150";

            [NameAndDescription("Personality Enabled", "Enables or Disables this Personality, if a All Chads, All GigaChads, or AllRats is enabled in global settings, this value is ignored")]
            [DefaultValue(true)]
            public bool Enabled = true;

            [NameAndDescription("Can Be Randomly Assigned", "A percentage chance that this personality can be applied to any bot, regardless of bot stats, power, player level, or anything else.")]
            [DefaultValue(true)]
            public bool CanBeRandomlyAssigned = true;

            [NameAndDescription("Randomly Assigned Chance", "If personality can be randomly assigned, this is the chance that will happen")]
            [DefaultValue(3)]
            [MinMaxRound(0, 100)]
            public float RandomlyAssignedChance = 3;

            [NameAndDescription("Max Level", "The max level that a bot can be to be eligible for this personality.")]
            [DefaultValue(100)]
            [MinMaxRound(1, 100)]
            public float MaxLevel = 100;

            [NameAndDescription("Random Chance If Meets Requirements", "If the bot meets all conditions for this personality, this is the chance the personality will actually be assigned.")]
            [DefaultValue(60)]
            [MinMaxRound(0, 100, 1)]
            public float RandomChanceIfMeetRequirements = 60;

            [NameAndDescription("Power Level Minimum", "Minimum Power level for a bot to use this personality." + PowerLevelDescription)]
            [DefaultValue(0)]
            [MinMaxRound(0, 250, 1)]
            public float PowerLevelMin = 0;

            [NameAndDescription("Power Level Maximum", "Maximum Power level for a bot to use this personality." + PowerLevelDescription)]
            [DefaultValue(250)]
            [MinMaxRound(0, 250, 1)]
            public float PowerLevelMax = 250;

            [Advanced(AdvancedEnum.IsAdvanced)]
            public float HoldGroundBaseTime = 1f;

            [Advanced(AdvancedEnum.IsAdvanced)]
            public float HoldGroundMinRandom = 0.66f;

            [Advanced(AdvancedEnum.IsAdvanced)]
            public float HoldGroundMaxRandom = 1.5f;

            public float SearchBaseTime = 45;

            public float SearchAggressionModifier = 1f;

            public bool CanJumpCorners = false;

            public bool CanTaunt = false;

            [Advanced(AdvancedEnum.IsAdvanced)]
            public bool FrequentTaunt = false;

            [Advanced(AdvancedEnum.IsAdvanced)]
            public bool ConstantTaunt = false;

            [Advanced(AdvancedEnum.IsAdvanced)]
            public bool CanRespondToVoice = false;

            [Advanced(AdvancedEnum.IsAdvanced)]
            public float TauntFrequency = 20f;

            [Advanced(AdvancedEnum.IsAdvanced)]
            public float TauntMaxDistance = 20f;

            public bool SprintWhileSearch = false;

            [Advanced(AdvancedEnum.IsAdvanced)]
            public bool FrequentSprintWhileSearch = false;

            public bool CanRushEnemyReloadHeal = false;

            [Advanced(AdvancedEnum.IsAdvanced)]
            public bool CanFakeDeathRare = false;
        }
    }
}
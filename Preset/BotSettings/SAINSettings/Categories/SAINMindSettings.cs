using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINMindSettings
    {
        [NameAndDescription("Aggression", 
            "How quickly this bot will move to search for enemies after losing sight, " +
            "and how carefully they will search. Higher number equals higher aggression.")]
        [DefaultValue(0.5f)]
        [MinMax(0.01f, 3f, 10f)]
        public float Aggression = 1f;

        [NameAndDescription("Weapon Proficiency", 
            "How Well this bot can fire any weapon type, affects recoil, fire-rate, and burst length. " +
            "Higher number equals harder bots.")]
        [DefaultValue(0.5f)]
        [MinMax(0.01f, 1f)]
        public float WeaponProficiency = 0.5f;

        [NameAndDescription("Talk Frequency", 
            "How often this bot can say voicelines.")]
        [DefaultValue(2f)]
        [MinMax(0f, 30f)]
        public float TalkFrequency = 2f;

        [DefaultValue(true)] public bool CanTalk = true;
        [DefaultValue(true)] public bool BotTaunts = true;
        [DefaultValue(true)] public bool SquadTalk = true;

        [NameAndDescription("Squad Talk Frequency")]
        [DefaultValue(3f)]
        [MinMax(0f, 60f)]
        public float SquadMemberTalkFreq = 3f;

        [NameAndDescription("Squad Leader Talk Frequency")]
        [DefaultValue(3f)]
        [MinMax(0f, 60f)]
        public float SquadLeadTalkFreq = 3f;

        [NameAndDescription("Max Raid Percentage before Extract", 
            "The longest possible time before this bot can decide to move to extract. " +
                "Based on total raid timer and time remaining. " +
            "60 min total raid time with 6 minutes remaining would be 10 percent")]
        [DefaultValue(30f)]
        [MinMax(0f, 100f)]
        public float MaxExtractPercentage = 30f;

        [NameAndDescription("Min Raid Percentage before Extract", 
            "The longest possible time before this bot can decide to move to extract. " +
                "Based on total raid timer and time remaining. " +
            "60 min total raid time with 6 minutes remaining would be 10 percent")]
        [DefaultValue(5f)]
        [MinMax(0f, 100f)]
        public float MinExtractPercentage = 5f;

        [NameAndDescription("Enable Extracts")]
        [DefaultValue(true)]
        public bool EnableExtracts = true;

        [NameAndDescription("Middle Finger Chance", 
            "Chance this bot will flick you off when spotted")]
        [DefaultValue(0f)]
        [MinMax(0f, 100f)]
        [Advanced(IAdvancedOption.IsAdvanced)]
        public float CHANCE_FUCK_YOU_ON_CONTACT_100 = 0f;

        [Advanced(IAdvancedOption.Hidden)] public bool SURGE_KIT_ONLY_SAFE_CONTAINER = false;
        [Advanced(IAdvancedOption.Hidden)] public float SEC_TO_MORE_DIST_TO_RUN = 0f;
        [Advanced(IAdvancedOption.Hidden)] public float DIST_TO_STOP_RUN_ENEMY = 0f;
        [Advanced(IAdvancedOption.Hidden)] public bool NO_RUN_AWAY_FOR_SAFE = true;
        [Advanced(IAdvancedOption.Hidden)] public bool CAN_USE_MEDS = true;
        [Advanced(IAdvancedOption.Hidden)] public bool CAN_USE_FOOD_DRINK = true;
        [Advanced(IAdvancedOption.Hidden)] public float GROUP_ANY_PHRASE_DELAY = 5f;
        [Advanced(IAdvancedOption.Hidden)] public float GROUP_EXACTLY_PHRASE_DELAY = 5f;
    }
}

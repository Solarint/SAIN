using EFT;
using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINMindSettings
    {
        [Name("Talk Frequency")]
        [Description("How often this bot can say voicelines.")]
        [DefaultValue(2f)]
        [Minimum(0f)]
        [Maximum(30f)]
        [Rounding(1)]
        public float TalkFrequency = 2f;

        [DefaultValue(true)]
        public bool CanTalk = true;
        [DefaultValue(true)]
        public bool BotTaunts = true;
        [DefaultValue(true)]
        public bool SquadTalk = true;

        [Name("Squad Talk Frequency")]
        [DefaultValue(3f)]
        [Minimum(0f)]
        [Maximum(60f)]
        [Rounding(1)]
        public float SquadMemberTalkFreq = 3f;

        [Name("Squad Leader Talk Frequency")]
        [DefaultValue(3f)]
        [Minimum(0f)]
        [Maximum(60f)]
        [Rounding(1)]
        public float SquadLeadTalkFreq = 3f;

        [Name("Max Raid Percentage before Extract")]
        [Description("The longest possible time before this bot can decide to move to extract. " +
                "Based on total raid timer and time remaining. 60 min total raid time with 6 minutes remaining would be 10 percent")]
        [DefaultValue(30f)]
        [Minimum(0f)]
        [Maximum(100f)]
        [Rounding(1)]
        public float MaxExtractPercentage = 30f;

        [Name("Min Raid Percentage before Extract")]
        [Description("The longest possible time before this bot can decide to move to extract. " +
                "Based on total raid timer and time remaining. 60 min total raid time with 6 minutes remaining would be 10 percent")]
        [DefaultValue(5f)]
        [Minimum(0f)]
        [Maximum(100f)]
        [Rounding(1)]
        public float MinExtractPercentage = 5f;

        [Name("Enable Extracts")]
        [DefaultValue(true)]
        public bool EnableExtracts = true;

        [Name("Time To Forget About Enemy")]
        [Description("If a bot hasn't seen or heard their enemy after this amount of time, they will return to patrol")]
        [DefaultValue(240f)]
        [Minimum(30f)]
        [Maximum(1200f)]
        [Rounding(1)]
        public float TIME_TO_FORGOR_ABOUT_ENEMY_SEC = 240f;

        [Name("Midlle Finger Chance")]
        [Description("Chance this bot will flick you off when spotted")]
        [DefaultValue(0f)]
        [Minimum(0f)]
        [Maximum(100f)]
        [Rounding(1)]
        [IsAdvanced(true)]
        public float CHANCE_FUCK_YOU_ON_CONTACT_100 = 0f;

        [IsHidden(true)]
        [DefaultValue(false)]
        [IsAdvanced(true)]
        public bool SURGE_KIT_ONLY_SAFE_CONTAINER = false;

        [IsHidden(true)]
        [DefaultValue(0f)]
        [IsAdvanced(true)]
        public float SEC_TO_MORE_DIST_TO_RUN = 0f;

        [IsHidden(true)]
        [DefaultValue(0f)]
        [IsAdvanced(true)]
        public float DIST_TO_STOP_RUN_ENEMY = 0f;

        [IsHidden(true)]
        [DefaultValue(true)]
        [IsAdvanced(true)]
        public bool NO_RUN_AWAY_FOR_SAFE = true;

        [IsHidden(true)]
        [DefaultValue(true)]
        [IsAdvanced(true)]
        public bool CAN_USE_MEDS = true;

        [IsHidden(true)]
        [DefaultValue(true)]
        [IsAdvanced(true)]
        public bool CAN_USE_FOOD_DRINK = true;

        [IsHidden(true)]
        [DefaultValue(5f)]
        [IsAdvanced(true)]
        public float GROUP_ANY_PHRASE_DELAY = 5f;

        [IsHidden(true)]
        [DefaultValue(5f)]
        [IsAdvanced(true)]
        public float GROUP_EXACTLY_PHRASE_DELAY = 5f;
    }
}

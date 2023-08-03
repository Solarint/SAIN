using EFT;
using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINMindSettings
    {
        static void InitTalk()
        {
            string name = "Talk Frequency";
            string desc = "Multiplies how often this bot can say voicelines.";
            string section = "Talk";
            float def = 1f;
            float min = 0.25f;
            float max = 3f;
            float round = 100f;

            name = "Can Talk";
            desc = "Sets whether this bot can talk or not";
            section = "Talk";

            name = "Taunts";
            desc = "Enables bots yelling nasty words at you.";
            section = "Talk";

            name = "Squad Talk";
            desc = "Enables bots talking to each other in a squad";
            section = "Talk";

            name = "Squad Talk Multiplier";
            desc = "Multiplies the time between squad voice communication";
            section = "Talk";
            def = 1f;
            min = 0.1f;
            max = 5f;
            round = 100f;

            name = "Squad Leader Talk Multiplier";
            desc = "Multiplies the time between squad Leader commands and callouts";
            section = "Talk";
            def = 1f;
            min = 0.1f;
            max = 5f;
            round = 100f;
        }

        static void initExtract()
        {
            string name = "Extracts";
            string desc = "Can This Bot Use Extracts?";
            string section = "Extract";

            name = "Extract Max Percentage";
            desc = "The shortest possible time before this bot can decide to move to extract. " +
                "Based on total raid timer and time remaining. 60 min total raid time with 6 minutes remaining would be 10 percent";
            section = "Extract";
            float def = 35f;
            float min = 1f;
            float max = 99f;
            float round = 1f;

            name = "Extract Min Percentage";
            desc = "The longest possible time before this bot can decide to move to extract. " +
                "Based on total raid timer and time remaining. 60 min total raid time with 6 minutes remaining would be 10 percent";
            section = "Extract";
            def = 5f;
            min = 1f;
            max = 99f;
            round = 1f;
        }
        public float TalkFrequency = 2f;
        public bool CanTalk = true;
        public bool BotTaunts = true;
        public bool SquadTalk = true;
        public float SquadMemberTalkFreq = 3f;
        public float SquadLeadTalkFreq = 3f;

        [Name("Max Raid Percentage before Extract")]
        [Description()]
        [DefaultValue(30f)]
        [Minimum(0f)]
        [Maximum(100f)]
        [Rounding(1)]
        public float MaxExtractPercentage = 30f;

        [Name("Min Raid Percentage before Extract")]
        [Description()]
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
        public float CHANCE_FUCK_YOU_ON_CONTACT_100 = 0f;

        [IsHidden(true)]
        [DefaultValue(false)]
        public bool SURGE_KIT_ONLY_SAFE_CONTAINER = false;

        [IsHidden(true)]
        [DefaultValue(0f)]
        public float SEC_TO_MORE_DIST_TO_RUN = 0f;

        [IsHidden(true)]
        [DefaultValue(0f)]
        public float DIST_TO_STOP_RUN_ENEMY = 0f;

        [IsHidden(true)]
        [DefaultValue(true)]
        public bool NO_RUN_AWAY_FOR_SAFE = true;

        [IsHidden(true)]
        [DefaultValue(true)]
        public bool CAN_USE_MEDS = true;

        [IsHidden(true)]
        [DefaultValue(true)]
        public bool CAN_USE_FOOD_DRINK = true;

        [IsHidden(true)]
        [DefaultValue(5f)]
        public float GROUP_ANY_PHRASE_DELAY = 5f;

        [IsHidden(true)]
        [DefaultValue(5f)]
        public float GROUP_EXACTLY_PHRASE_DELAY = 5f;
    }
}

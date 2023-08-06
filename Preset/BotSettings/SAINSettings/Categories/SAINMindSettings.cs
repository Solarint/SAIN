using EFT;
using Newtonsoft.Json;
using SAIN.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINMindSettings
    {
        [NameAndDescription("Aggression", "How quickly this bot will move to search for enemies after losing sight, and how carefully they will search.")]
        [DefaultValue(0.5f)]
        [MinMaxRound(0.01f, 1f)]
        public float Aggression = 0.5f;

        [NameAndDescription("Weapon Proficiency", "How Well this bot can fire any weapon type, affects recoil, fire-rate, and burst length.")]
        [DefaultValue(0.5f)]
        [MinMaxRound(0.01f, 1f)]
        public float WeaponProficiency = 0.5f;

        [NameAndDescription("Talk Frequency", "How often this bot can say voicelines.")]
        [DefaultValue(2f)]
        [MinMaxRound(0f, 30f)]
        public float TalkFrequency = 2f;

        [DefaultValue(true)]
        public bool CanTalk = true;
        [DefaultValue(true)]
        public bool BotTaunts = true;
        [DefaultValue(true)]
        public bool SquadTalk = true;

        [NameAndDescription("Squad Talk Frequency")]
        [DefaultValue(3f)]
        [MinMaxRound(0f, 60f)]
        public float SquadMemberTalkFreq = 3f;

        [NameAndDescription("Squad Leader Talk Frequency")]
        [DefaultValue(3f)]
        [MinMaxRound(0f, 60f)]
        public float SquadLeadTalkFreq = 3f;

        [NameAndDescription("Max Raid Percentage before Extract", "The longest possible time before this bot can decide to move to extract. " +
                "Based on total raid timer and time remaining. 60 min total raid time with 6 minutes remaining would be 10 percent")]
        [DefaultValue(30f)]
        [MinMaxRound(0f, 100f)]
        public float MaxExtractPercentage = 30f;

        [NameAndDescription("Min Raid Percentage before Extract", "The longest possible time before this bot can decide to move to extract. " +
                "Based on total raid timer and time remaining. 60 min total raid time with 6 minutes remaining would be 10 percent")]
        [DefaultValue(5f)]
        [MinMaxRound(0f, 100f)]
        public float MinExtractPercentage = 5f;

        [NameAndDescription("Enable Extracts")]
        [DefaultValue(true)]
        public bool EnableExtracts = true;

        [NameAndDescription("Time To Forget About Enemy", "If a bot hasn't seen or heard their enemy after this amount of time, they will return to patrol")]
        [DefaultValue(240f)]
        [MinMaxRound(30f, 1200f)]
        public float TIME_TO_FORGOR_ABOUT_ENEMY_SEC = 240f;

        [NameAndDescription("Middle Finger Chance", "Chance this bot will flick you off when spotted")]
        [DefaultValue(0f)]
        [MinMaxRound(0f, 100f)]
        [AdvancedOptions(true)]
        public float CHANCE_FUCK_YOU_ON_CONTACT_100 = 0f;

        [DefaultValue(false)]
        [AdvancedOptions(true, true)]
        public bool SURGE_KIT_ONLY_SAFE_CONTAINER = false;

        [DefaultValue(0f)]
        [AdvancedOptions(true, true)]
        public float SEC_TO_MORE_DIST_TO_RUN = 0f;

        [DefaultValue(0f)]
        [AdvancedOptions(true, true)]
        public float DIST_TO_STOP_RUN_ENEMY = 0f;

        [DefaultValue(true)]
        [AdvancedOptions(true, true)]
        public bool NO_RUN_AWAY_FOR_SAFE = true;

        [DefaultValue(true)]
        [AdvancedOptions(true, true)]
        public bool CAN_USE_MEDS = true;

        [DefaultValue(true)]
        [AdvancedOptions(true, true)]
        public bool CAN_USE_FOOD_DRINK = true;

        [DefaultValue(5f)]
        [AdvancedOptions(true, true)]
        public float GROUP_ANY_PHRASE_DELAY = 5f;

        [DefaultValue(5f)]
        [AdvancedOptions(true, true)]
        public float GROUP_EXACTLY_PHRASE_DELAY = 5f;
    }
}

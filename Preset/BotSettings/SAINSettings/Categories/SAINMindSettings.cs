using SAIN.Attributes;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public partial class SAINMindSettings
    {
        [NameAndDescription("Aggression",
            "How quickly this bot will move to search for enemies after losing sight, " +
            "and how carefully they will search. Higher number equals higher aggression.")]
        [Default(0.5f)]
        [MinMax(0.01f, 3f, 10f)]
        public float Aggression = 1f;

        [NameAndDescription("Weapon Proficiency",
            "How Well this bot can fire any weapon type, affects recoil, fire-rate, and burst length. " +
            "Higher number equals harder bots.")]
        [Default(0.5f)]
        [Percentage01to99]
        public float WeaponProficiency = 0.5f;

        [NameAndDescription("Talk Frequency",
            "How often this bot can say voicelines.")]
        [Default(2f)]
        [MinMax(0f, 30f)]
        public float TalkFrequency = 2f;

        [Default(true)]
        public bool CanTalk = true;
        [Default(true)]
        public bool BotTaunts = true;
        [Default(true)]
        public bool SquadTalk = true;

        [Name("Squad Talk Frequency")]
        [Default(3f)]
        [MinMax(0f, 60f)]
        public float SquadMemberTalkFreq = 3f;

        [Name("Squad Leader Talk Frequency")]
        [Default(3f)]
        [MinMax(0f, 60f)]
        public float SquadLeadTalkFreq = 3f;

        [NameAndDescription("Max Raid Percentage before Extract",
            "The longest possible time before this bot can decide to move to extract. " +
                "Based on total raid timer and time remaining. " +
            "60 min total raid time with 6 minutes remaining would be 10 percent")]
        [Default(30f)]
        [MinMax(0f, 100f)]
        public float MaxExtractPercentage = 30f;

        [NameAndDescription("Min Raid Percentage before Extract",
            "The longest possible time before this bot can decide to move to extract. " +
                "Based on total raid timer and time remaining. " +
            "60 min total raid time with 6 minutes remaining would be 10 percent")]
        [Default(5f)]
        [MinMax(0f, 100f)]
        public float MinExtractPercentage = 5f;

        [Name("Enable Extracts")]
        [Default(true)]
        public bool EnableExtracts = true;

        [NameAndDescription("Middle Finger Chance",
            "Chance this bot will flick you off when spotted")]
        [Default(0f)]
        [MinMax(0f, 100f)]
        [Advanced]
        public float CHANCE_FUCK_YOU_ON_CONTACT_100 = 0f;
    }

    // Hidden Settings
    public partial class SAINMindSettings
    {
        [Hidden]
        public readonly bool SURGE_KIT_ONLY_SAFE_CONTAINER = false;

        [Hidden]
        public readonly float SEC_TO_MORE_DIST_TO_RUN = 0f;

        [Hidden]
        public readonly float DIST_TO_STOP_RUN_ENEMY = 0f;

        [Hidden]
        public readonly bool NO_RUN_AWAY_FOR_SAFE = true;

        [Hidden]
        public readonly bool CAN_USE_MEDS = true;

        [Hidden]
        public readonly bool CAN_USE_FOOD_DRINK = true;
    }
}
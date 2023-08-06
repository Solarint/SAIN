using Newtonsoft.Json;
using SAIN.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINAimingSettings
    {
        [NameAndDescription(
            "Faster CQB Reactions",
            "Sets whether this bot reacts faster at close ranges")]
        [DefaultValue(true)]
        public bool FasterCQBReactions = true;

        [NameAndDescription(
            "Faster CQB Reactions Max Distance",
            "Max distance a bot can react faster for Faster CQB Reactions. Scales with distance.")]
        [DefaultValue(30f)]
        [MinMaxRound(5f, 100f)]
        public float FasterCQBReactionsDistance = 30f;

        [NameAndDescription(
            "Faster CQB Reactions Minimum Speed",
            "Absolute minimum speed (in seconds) that bot can react and shoot")]
        [DefaultValue(0.15f)]
        [MinMaxRound(0.05f, 0.5f, 100f)]
        public float FasterCQBReactionsMinimum = 0.15f;

        [NameAndDescription(
            "Accuracy Spread Multiplier",
            "Higher = less accurate. Modifies a bot's base accuracy and spread. 1.5 = 1.5x higher accuracy spread")]
        [DefaultValue(1f)]
        [MinMaxRound(0.1f, 5f, 10f)]
        public float AccuracySpreadMulti = 1f;

        [NameAndDescription("Aiming Upgrade By Time")]
        [DefaultValue(0.8f)]
        [MinMaxRound(0.1f, 0.95f, 100f)]
        [AdvancedOptions(true, false, true)]
        public float MAX_AIMING_UPGRADE_BY_TIME = 0.8f;

        [NameAndDescription("Max Aim Time")]
        [Description(null)]
        [DefaultValue(2f)]
        [MinMaxRound(0.1f, 5f, 10f)]
        [AdvancedOptions(true, false, true)]
        public float MAX_AIM_TIME = 2f;

        [NameAndDescription("Aim Type")]
        [Description(null)]
        [DefaultValue(4)]
        [MinMaxRound(1, 6)]
        public int AIMING_TYPE = 4;

        [NameAndDescription("Friendly Fire Spherecast Size")]
        [Description(null)]
        [DefaultValue(0.15f)]
        [MinMaxRound(0f, 0.5f, 100f)]
        [AdvancedOptions(true)]
        public float SHPERE_FRIENDY_FIRE_SIZE = 0.15f;

        [DefaultValue(1)]
        [AdvancedOptions(true, true)]
        public int RECALC_MUST_TIME = 1;

        [DefaultValue(1)]
        [AdvancedOptions(true, true)]
        public int RECALC_MUST_TIME_MIN = 1;

        [DefaultValue(2)]
        [AdvancedOptions(true, true)]
        public int RECALC_MUST_TIME_MAX = 2;

        [NameAndDescription(
            "Hit Reaction Recovery Time",
            "How much time it takes to recover a bot's aim when they get hit by a bullet")]
        [DefaultValue(0.5f)]
        [MinMaxRound(0.1f, 0.99f, 100f)]
        [AdvancedOptions(true)]
        public float BASE_HIT_AFFECTION_DELAY_SEC = 0.5f;

        [NameAndDescription(
            "Minimum Hit Reaction Angle",
            "How much to kick a bot's aim when they get hit by a bullet")]
        [DefaultValue(3f)]
        [MinMaxRound(0f, 25f, 10f)]
        [AdvancedOptions(true)]
        public float BASE_HIT_AFFECTION_MIN_ANG = 3f;

        [NameAndDescription(
            "Maximum Hit Reaction Angle",
            "How much to kick a bot's aim when they get hit by a bullet")]
        [DefaultValue(5f)]
        [MinMaxRound(0f, 25f, 10f)]
        [AdvancedOptions(true)]
        public float BASE_HIT_AFFECTION_MAX_ANG = 5f;
    }
}
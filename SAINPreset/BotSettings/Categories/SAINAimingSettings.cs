using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINAimingSettings
    {
        [Name("Faster CQB Reactions")]
        [Description("Sets whether this bot reacts faster at close ranges")]
        [DefaultValue(true)]
        public bool FasterCQBReactions = true;

        [Name("Faster CQB Reactions Max Distance")]
        [Description("Max distance a bot can react faster for Faster CQB Reactions. Scales with distance.")]
        [DefaultValue(30f)]
        [Minimum(5)]
        [Maximum(100)]
        [Rounding(1)]
        public float FasterCQBReactionsDistance = 30f;

        [Name("Faster CQB Reactions Minimum Speed")]
        [Description("Absolute minimum speed (in seconds) that bot can react and shoot")]
        [DefaultValue(0.15f)]
        [Minimum(0.05f)]
        [Maximum(0.5f)]
        [Rounding(100f)]
        public float FasterCQBReactionsMinimum = 0.15f;

        [Name("Accuracy Spread Multiplier")]
        [Description("Higher = less accurate. Modifies a bot's base accuracy and spread. 1.5 = 1.5x higher accuracy spread")]
        [DefaultValue(1f)]
        [Minimum(0.1f)]
        [Maximum(5f)]
        [Rounding(10f)]
        public float AccuracySpreadMulti = 1f;

        [Name("Aiming Upgrade By Time")]
        [Description(null)]
        [DefaultValue(0.8f)]
        [Minimum(0.1f)]
        [Maximum(0.95f)]
        [Rounding(100)]
        [IsAdvanced(true)]
        public float MAX_AIMING_UPGRADE_BY_TIME = 0.8f;

        [Name("Max Aim Time")]
        [Description(null)]
        [DefaultValue(2f)]
        [Minimum(0.1f)]
        [Maximum(5f)]
        [Rounding(10)]
        [IsAdvanced(true)]
        public float MAX_AIM_TIME = 2f;

        [Name("Aim Type")]
        [Description(null)]
        [DefaultValue(4)]
        [Minimum(1)]
        [Maximum(6)]
        public int AIMING_TYPE = 4;

        [Name("Frieldly Fire Spherecast Size")]
        [Description(null)]
        [DefaultValue(0.15f)]
        [Minimum(0f)]
        [Maximum(0.5f)]
        [Rounding(100)]
        [IsAdvanced(true)]
        public float SHPERE_FRIENDY_FIRE_SIZE = 0.15f;

        [DefaultValue(1)]
        [IsHidden(true)]
        [IsAdvanced(true)]
        public int RECALC_MUST_TIME = 1;

        [DefaultValue(1)]
        [IsHidden(true)]
        [IsAdvanced(true)]
        public int RECALC_MUST_TIME_MIN = 1;

        [DefaultValue(2)]
        [IsHidden(true)]
        [IsAdvanced(true)]
        public int RECALC_MUST_TIME_MAX = 2;

        [Name("Hit Reaction Recovery Time")]
        [Description("How much time it takes to recover a bot's aim when they get hit by a bullet")]
        [DefaultValue(0.5f)]
        [Minimum(0.1f)]
        [Maximum(0.95f)]
        [Rounding(100)]
        [IsAdvanced(true)]
        public float BASE_HIT_AFFECTION_DELAY_SEC = 0.5f;

        [Name("Minimum Hit Reaction Angle")]
        [Description("How much to kick a bot's aim when they get hit by a bullet")]
        [DefaultValue(3f)]
        [Minimum(0f)]
        [Maximum(25f)]
        [Rounding(10)]
        [IsAdvanced(true)]
        public float BASE_HIT_AFFECTION_MIN_ANG = 3f;

        [Name("Maximum Hit Reaction Angle")]
        [Description("How much to kick a bot's aim when they get hit by a bullet")]
        [DefaultValue(5f)]
        [Minimum(0f)]
        [Maximum(25f)]
        [Rounding(10)]
        [IsAdvanced(true)]
        public float BASE_HIT_AFFECTION_MAX_ANG = 5f;
    }
}
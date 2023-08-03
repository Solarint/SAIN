using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINAimingSettings
    {
        static void InitGeneral()
        {
            string name = "Faster CQB Reactions";
            string desc = "Sets whether this bot reacts faster at close ranges";
            string section = "General";

            name = "Faster CQB Reactions Max Distance";
            desc = "Max distance a bot can react faster for Faster CQB Reactions. Scales with distance.";
            section = "General";
            float def = 30f;
            float min = 1f;
            float max = 100f;
            float round = 1f;

            name = "Faster CQB Reactions Minimum Speed";
            desc = "Absolute minimum speed (in seconds) that bot can react and shoot";
            section = "General";
            def = 0.1f;
            min = 0.0f;
            max = 1f;
            round = 100f;
        }
        static void InitShoot()
        {
            string name = "Accuracy Spread Multiplier";
            string desc = "Modifies a bot's base accuracy and spread. Higher = less accurate. 1.5 = 1.5x higher accuracy spread";
            string section = "Shoot/Aim";
            float def = 1f;
            float min = 0.1f;
            float max = 5f;
            float round = 100f;

            name = "Accuracy Speed Multiplier";
            desc = "Modifies a bot's Accuracy Speed, or how fast their accuracy improves over time when shooting. " +
                "Higher = longer to gain accuracy. 1.5 = 1.5x longer to aim";
            section = "Shoot/Aim";
            def = 1f;
            min = 0.1f;
            max = 5f;
            round = 100f;

            name = "Max Aim Time Multiplier";
            desc = "Modifies the maximum time a bot can aim, or how long in seconds a bot takes to finish aiming. " +
                "Higher = longer to full aim. 1.5 = 1.5x longer to aim";
            section = "Shoot/Aim";
            def = 1f;
            min = 0.1f;
            max = 5f;
            round = 100f;
        }
        public bool FasterCQBReactions = true;
        public float FasterCQBReactionsDistance = 30f;
        public float FasterCQBReactionsMinimum = 0.15f;
        public float AccuracySpreadMulti = 1f;

        [Name("Aiming Upgrade By Time")]
        [Description(null)]
        [DefaultValue(0.8f)]
        [Minimum(0.1f)]
        [Maximum(0.95f)]
        [Rounding(100)]
        public float MAX_AIMING_UPGRADE_BY_TIME = 0.8f;

        [Name("Max Aim Time")]
        [Description(null)]
        [DefaultValue(2f)]
        [Minimum(0.1f)]
        [Maximum(5f)]
        [Rounding(10)]
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
        public float SHPERE_FRIENDY_FIRE_SIZE = 0.15f;

        [DefaultValue(1)]
        [IsHidden(true)]
        public int RECALC_MUST_TIME = 1;

        [DefaultValue(1)]
        [IsHidden(true)]
        public int RECALC_MUST_TIME_MIN = 1;

        [DefaultValue(2)]
        [IsHidden(true)]
        public int RECALC_MUST_TIME_MAX = 2;

        [Name("Hit Reaction Recovery Time")]
        [Description("How much time it takes to recover a bot's aim when they get hit by a bullet")]
        [DefaultValue(0.5f)]
        [Minimum(0.1f)]
        [Maximum(0.95f)]
        [Rounding(100)]
        public float BASE_HIT_AFFECTION_DELAY_SEC = 0.5f;

        [Name("Minimum Hit Reaction Angle")]
        [Description("How much to kick a bot's aim when they get hit by a bullet")]
        [DefaultValue(3f)]
        [Minimum(0f)]
        [Maximum(25f)]
        [Rounding(10)]
        public float BASE_HIT_AFFECTION_MIN_ANG = 3f;

        [Name("Maximum Hit Reaction Angle")]
        [Description("How much to kick a bot's aim when they get hit by a bullet")]
        [DefaultValue(5f)]
        [Minimum(0f)]
        [Maximum(25f)]
        [Rounding(10)]
        public float BASE_HIT_AFFECTION_MAX_ANG = 5f;
    }
}
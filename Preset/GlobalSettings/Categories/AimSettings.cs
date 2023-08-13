using SAIN.Attributes;
using System.ComponentModel;
using Description = SAIN.Attributes.DescriptionAttribute;

namespace SAIN.Preset.GlobalSettings
{
    public class AimSettings
    {
        [Name("Global Accuracy Spread Multiplier")]
        [Description("Higher = less accurate. Modifies all bots base accuracy and spread. 1.5 = 1.5x higher accuracy spread")]
        [DefaultValue(1f)]
        [MinMaxRound(0.1f, 10f, 100f)]
        public float AccuracySpreadMultiGlobal = 1f;

        [Name("Global Faster CQB Reactions")]
        [Description("if this toggle is disabled, all bots will have Faster CQB Reactions turned OFF, so their individual settings will be ignored.")]
        [DefaultValue(true)]
        public bool FasterCQBReactionsGlobal = true;
    }
}
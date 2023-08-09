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
        [MinMaxRound(0.1f, 5f, 10f)]
        public float AccuracySpreadMultiGlobal = 1f;
    }
}
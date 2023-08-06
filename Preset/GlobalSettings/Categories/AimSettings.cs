using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class AimSettings
    {
        [NameAndDescription("Global Accuracy Spread Multiplier", "Higher = less accurate. Modifies all bots base accuracy and spread. 1.5 = 1.5x higher accuracy spread")]
        [Description("Higher = less accurate. Modifies all bots base accuracy and spread. 1.5 = 1.5x higher accuracy spread")]
        [DefaultValue(1f)]
        [MinMaxRound(0.1f, 5f, 10f)]
        [AdvancedOptions(false, false, false)]
        public float AccuracySpreadMultiGlobal = 1f;
    }
}
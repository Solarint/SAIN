using SAIN.SAINPreset.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.SAINPreset.Settings
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

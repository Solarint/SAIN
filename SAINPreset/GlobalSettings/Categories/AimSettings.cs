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
        [Name("Global Accuracy Spread Multiplier")]
        [Description("Higher = less accurate. Modifies all bots base accuracy and spread. 1.5 = 1.5x higher accuracy spread")]
        [DefaultValue(1f)]
        [Minimum(0.1f)]
        [Maximum(5f)]
        [Rounding(10f)]
        public float AccuracySpreadMulti = 1f;
    }
}

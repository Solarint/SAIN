using BepInEx.Configuration;
using SAIN.SAINPreset.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.SAINPreset.Settings
{
    public class CoverSettings
    {
        [DefaultValue(0.75f)]
        [Minimum(0.5f)]
        [Maximum(1.5f)]
        [Rounding(100f)]
        [IsAdvanced(true)]
        public float CoverMinHeight = 0.75f;

        [DefaultValue(8f)]
        [Minimum(0f)]
        [Maximum(30f)]
        [Rounding(1f)]
        [IsAdvanced(true)]
        public float CoverMinEnemyDistance = 8f;

        [DefaultValue(0.33f)]
        [Minimum(0.01f)]
        [Maximum(1f)]
        [Rounding(100f)]
        [IsAdvanced(true)]
        public float CoverUpdateFrequency = 0.33f;

        [DefaultValue(false)]
        [IsAdvanced(true)]
        public bool DebugCoverFinder = false;
    }
}

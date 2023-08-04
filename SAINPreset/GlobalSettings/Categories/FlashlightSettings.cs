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
    public class FlashlightSettings
    {
        [DefaultValue(3f)]
        [Minimum(0.25f)]
        [Maximum(10f)]
        [Rounding(100f)]
        public float DazzleEffectiveness = 3f;

        [DefaultValue(30f)]
        [Minimum(0f)]
        [Maximum(60f)]
        [Rounding(1f)]
        public float MaxDazzleRange = 30f;

        [DefaultValue(false)]
        [IsAdvanced(true)]
        public bool DebugFlash = false;

        [DefaultValue(false)]
        public bool SillyMode = false;
    }
}

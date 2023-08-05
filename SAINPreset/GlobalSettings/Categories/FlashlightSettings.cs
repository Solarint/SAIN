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
        [MinMaxRound(0.25f, 10f, 100f)]
        public float DazzleEffectiveness = 3f;

        [DefaultValue(30f)]
        [MinMaxRound(0f, 60f)]
        [Rounding(1f)]
        public float MaxDazzleRange = 30f;

        [DefaultValue(false)]
        [AdvancedOptions(true)]
        public bool DebugFlash = false;

        [DefaultValue(false)]
        public bool SillyMode = false;
    }
}

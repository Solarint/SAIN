using SAIN.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class HearingSettings
    {
        [NameAndDescription(
            "Global Gunshot Audible Range Multiplier")]
        [DefaultValue(1f)]
        [MinMax(0.1f, 2, 100f)]
        public float GlobalGunshotAudioMulti = 1f;

        [NameAndDescription(
            "Global Footstep Audible Range Multiplier")]
        [DefaultValue(1f)]
        [MinMax(0.1f, 2f, 100f)]
        public float GlobalFootstepAudioMulti = 1f;

        [NameAndDescription(
            "Suppressed Sound Modifier",
            "Audible Gun Range is multiplied by this number when using a suppressor")]
        [DefaultValue(0.6f)]
        [MinMax(0.1f, 0.95f, 100f)]
        public float SuppressorModifier = 0.6f;

        [NameAndDescription(
            "Subsonic Sound Modifier",
            "Audible Gun Range is multiplied by this number when using a suppressor and subsonic ammo")]
        [DefaultValue(0.25f)]
        [MinMax(0.1f, 0.95f, 100f)]
        public float SubsonicModifier = 0.25f;

        [Dictionary(typeof(Caliber), typeof(float))]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public AudibleRangesClass AudibleRanges = new AudibleRangesClass();
    }
}
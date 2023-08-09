using SAIN.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class HearingSettings
    {
        [NameAndDescription(
            "Suppressed Sound Modifier",
            "Audible Gun Range is multiplied by this number when using a suppressor")]
        [DefaultValue(0.6f)]
        [MinMaxRound(0.1f, 0.95f, 100f)]
        public float SuppressorModifier = 0.6f;

        [NameAndDescription(
            "Subsonic Sound Modifier",
            "Audible Gun Range is multiplied by this number when using a suppressor and subsonic ammo")]
        [DefaultValue(0.25f)]
        [MinMaxRound(0.1f, 0.95f, 100f)]
        public float SubsonicModifier = 0.25f;

        [Dictionary(typeof(Caliber), typeof(float))]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public AudibleRangesClass AudibleRanges = new AudibleRangesClass();
    }
}
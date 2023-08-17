using Newtonsoft.Json;
using SAIN.Attributes;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings
{
    public class HearingSettings
    {
        [Name("Global Gunshot Audible Range Multiplier")]
        [Default(1f)]
        [MinMax(0.1f, 2f, 100f)]
        public float GunshotAudioMultiplier = 1f;

        [Name("Global Footstep Audible Range Multiplier")]
        [Default(1f)]
        [MinMax(0.1f, 2f, 100f)]
        public float FootstepAudioMultiplier = 1f;

        [Name("Suppressed Sound Modifier")]
        [Description("Audible Gun Range is multiplied by this number when using a suppressor")]
        [Default(0.6f)]
        [MinMax(0.1f, 0.95f, 100f)]
        public float SuppressorModifier = 0.6f;

        [Name("Subsonic Sound Modifier")]
        [Description("Audible Gun Range is multiplied by this number when using a suppressor and subsonic ammo")]
        [Default(0.25f)]
        [MinMax(0.1f, 0.95f, 100f)]
        public float SubsonicModifier = 0.33f;

        [Name("Hearing Distances by Ammo Type")]
        [Description("How far a bot can hear a gunshot when fired from each specific caliber listed here.")]
        [MinMax(30f, 400f, 1f)]
        [Advanced]
        [Dictionary(typeof(ICaliber), typeof(float))]
        public Dictionary<ICaliber, float> AudibleRanges = new Dictionary<ICaliber, float>(AudibleRangesDefaults);

        [JsonIgnore]
        [Advanced(IAdvancedOption.Hidden)]
        public static readonly Dictionary<ICaliber, float> AudibleRangesDefaults = new Dictionary<ICaliber, float>()
        {
            { ICaliber.Caliber9x18PM, 110f },
            { ICaliber.Caliber9x19PARA, 110f },
            { ICaliber.Caliber46x30, 120f },
            { ICaliber.Caliber9x21, 120f },
            { ICaliber.Caliber57x28, 120f },
            { ICaliber.Caliber762x25TT, 120f },
            { ICaliber.Caliber1143x23ACP, 115f },
            { ICaliber.Caliber9x33R, 125 },
            { ICaliber.Caliber545x39, 160 },
            { ICaliber.Caliber556x45NATO, 160 },
            { ICaliber.Caliber9x39, 160 },
            { ICaliber.Caliber762x35, 175 },
            { ICaliber.Caliber762x39, 175 },
            { ICaliber.Caliber366TKM, 175 },
            { ICaliber.Caliber762x51, 200f },
            { ICaliber.Caliber127x55, 200f },
            { ICaliber.Caliber762x54R, 225f },
            { ICaliber.Caliber86x70, 250f },
            { ICaliber.Caliber20g, 185 },
            { ICaliber.Caliber12g, 185 },
            { ICaliber.Caliber23x75, 210 },
            { ICaliber.Caliber26x75, 50 },
            { ICaliber.Caliber30x29, 50 },
            { ICaliber.Caliber40x46, 50 },
            { ICaliber.Caliber40mmRU, 50 },
            { ICaliber.Caliber127x108, 300 },
            { ICaliber.Default, 125 },
        };
    }
}
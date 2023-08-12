using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class VisionSettings
    {
        [NameAndDescription(
            "Global Vision Speed Multiplier",
            "The Base vision speed multiplier, applies to all bots equally, affects all ranges to enemy. " +
            "Bots will see this much faster, or slower, at any range. " +
            "Higher is slower speed, so 1.5 would result in bots taking 1.5 times longer to spot an enemy")]
        [DefaultValue(1f)]
        [MinMaxRound(0.1f, 3f, 10f)]
        public float VisionSpeedModifierGlobal = 1;

        [NameAndDescription(
            "Nighttime Vision Modifier",
            "By how much to lower visible distance at nighttime. " +
            "at the default value of 0.2, bots will see 0.2 times as far, or 20% of their base vision distance at night-time.")]
        [DefaultValue(0.2f)]
        [MinMaxRound(0.01f, 1f, 100f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float NightTimeVisionModifier = 0.2f;

        [Name("Dawn Start Hour")]
        [DefaultValue(6f)]
        [MinMaxRound(5f, 8f, 1f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float HourDawnStart = 6f;

        [Name("Dawn End Hour")]
        [DefaultValue(8f)]
        [MinMaxRound(6f, 9f, 1f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float HourDawnEnd = 8f;

        [Name("Dusk Start Hour")]
        [DefaultValue(20f)]
        [MinMaxRound(19f, 22f, 1f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float HourDuskStart = 20f;

        [Name("Dusk End Hour")]
        [DefaultValue(22f)]
        [MinMaxRound(20f, 23f, 1f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float HourDuskEnd = 22f;
    }
}
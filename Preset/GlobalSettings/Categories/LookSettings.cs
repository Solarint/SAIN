using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings
{
    public class LookSettings
    {
        [Name("Global Vision Distance Multiplier")]
        [Description(
            "Multiplies whatever a bot's visible distance is set to. " +
            "Higher is further visible distance, so 1.5 would result in bots seeing 1.5 times further. " +
            "Or if their visible distance is set to 100 meters, they will see at 150 meters instead.")]
        [Default(1f)]
        [MinMax(0.1f, 5f, 10f)]
        public float GlobalVisionDistanceMultiplier = 1;

        [Name("Global Vision Speed Multiplier")]
        [Description(
            "The Base vision speed multiplier, applies to all bots equally, affects all ranges to enemy. " +
            "Bots will see this much faster, or slower, at any range. " +
            "Higher is slower speed, so 1.5 would result in bots taking 1.5 times longer to spot an enemy")]
        [Default(1f)]
        [MinMax(0.01f, 5f, 100f)]
        public float GlobalVisionSpeedModifier = 1;

        [Name("Nighttime Vision Modifier")]
        [Description(
            "By how much to lower visible distance at nighttime. " +
            "at the default value of 0.2, bots will see 0.2 times as far, or 20% of " +
            "their base vision distance at night-time.")]
        [Default(0.2f)]
        [MinMax(0.01f, 1f, 100f)]
        [Advanced]
        public float NightTimeVisionModifier = 0.2f;

        [Name("Dawn Start Hour")]
        [Default(6f)]
        [MinMax(5f, 8f, 1f)]
        [Advanced]
        public float HourDawnStart = 6f;

        [Name("Dawn End Hour")]
        [Default(8f)]
        [MinMax(6f, 9f, 1f)]
        [Advanced]
        public float HourDawnEnd = 8f;

        [Name("Dusk Start Hour")]
        [Default(20f)]
        [MinMax(19f, 22f, 1f)]
        [Advanced]
        public float HourDuskStart = 20f;

        [Name("Dusk End Hour")]
        [Default(22f)]
        [MinMax(20f, 23f, 1f)]
        [Advanced]
        public float HourDuskEnd = 22f;
    }
}
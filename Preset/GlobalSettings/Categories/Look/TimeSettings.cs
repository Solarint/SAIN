using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings
{
    public class TimeSettings : SAINSettingsBase<TimeSettings>, ISAINSettings
    {
        [Name("Vision Distance Multiplier")]
        [Category("Night-time Vision Effects")]
        [Description("By how much to lower visible distance at nighttime. " +
            "at the default value of 0.2, bots will see 0.2 times as far, or 20% of " +
            "their base vision distance at night-time.")]
        [MinMax(0.01f, 1f, 1000f)]
        public float NightTimeVisionModifier = 0.2f;

        [Name("Snow Vision Distance Multiplier")]
        [Category("Night-time Vision Effects")]
        [Description("By how much to lower visible distance at nighttime in the snow. " +
            "at the default value of 0.2, bots will see 0.2 times as far, or 20% of " +
            "their base vision distance at night-time.")]
        [MinMax(0.01f, 1f, 1000f)]
        public float NightTimeVisionModifierSnow = 0.35f;

        [Name("Vision Speed Max Multiplier")]
        [Category("Night-time Vision Effects")]
        [Description("Bot Vision Speed will be reduced up to X times longer to spot enemies, depending on visibility and time of day. Higher = Worse Vision.")]
        [MinMax(1f, 10f, 1000f)]
        public float TIME_GAIN_SIGHT_SCALE_MAX = 3f;

        [Name("Minimum Multiplier")]
        [Category("Weather Vision Effects")]
        [Description("Weather will not multiply vision distance any lower than this number.")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_MIN_COEF = 0.33f;

        [Name("Minimum Weather Vision Distance")]
        [Category("Weather Vision Effects")]
        [Description("Base Vision distance - after weather calculation and before time effects - will not go lower than this.")]
        [MinMax(0f, 100f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_MIN_DIST_METERS = 30f;

        [Name("Minimum Fog Effect")]
        [Category("Weather Vision Effects - Fog")]
        [Description("Fog will scale between X and 1 depending on fog intensity. Reducing vision distance by X amount. 0.4 = up to 40% vision distance from fog alone.")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_FOG_MAXCOEF = 0.4f;

        [Name("Very Light Rain Threshold")]
        [Category("Weather Vision Effects - Rain")]
        [Description("Rain scales between 0 and 1. If the value is equal to or less than this number. It will use the multiplier below.")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_RAIN_SRINKLE_THRESH = 0.1f;

        [Name("Very Light Rain Scaling Coef")]
        [Category("Weather Vision Effects - Rain")]
        [Description("")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_RAIN_SRINKLE_COEF = 0.9f;

        [Name("Light Rain Threshold")]
        [Category("Weather Vision Effects - Rain")]
        [Description("Rain scales between 0 and 1. If the value is equal to or less than this number. It will use the multiplier below.")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_RAIN_LIGHT_THRESH = 0.35f;

        [Name("Light Rain Scaling Coef")]
        [Category("Weather Vision Effects - Rain")]
        [Description("")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_RAIN_LIGHT_COEF = 0.65f;

        [Name("Normal Rain Threshold")]
        [Category("Weather Vision Effects - Rain")]
        [Description("Rain scales between 0 and 1. If the value is equal to or less than this number. It will use the multiplier below.")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_RAIN_NORMAL_THRESH = 0.5f;

        [Name("Normal Rain Scaling Coef")]
        [Category("Weather Vision Effects - Rain")]
        [Description("")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_RAIN_NORMAL_COEF = 0.5f;

        [Name("Heavy Rain Threshold")]
        [Category("Weather Vision Effects - Rain")]
        [Description("Rain scales between 0 and 1. If the value is equal to or less than this number. It will use the multiplier below.")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_RAIN_HEAVY_THRESH = 0.75f;

        [Name("Heavy Rain Scaling Coef")]
        [Category("Weather Vision Effects - Rain")]
        [Description("")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_RAIN_HEAVY_COEF = 0.45f;

        [Name("Very Heavy Rain Scaling Coef")]
        [Category("Weather Vision Effects - Rain")]
        [Description("If rain is above Heavy Rain Threshold use this scaling coef.")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_RAIN_DOWNPOUR_COEF = 0.4f;

        [Name("Clear Skies Threshold")]
        [Category("Weather Vision Effects - Clouds")]
        [Description("Cloudiness scales 0 to 1. If cloudiness is below X value, have no effect on vision.")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_NOCLOUDS_THRESH = 0.33f;

        [Name("Cloudy Threshold")]
        [Category("Weather Vision Effects - Clouds")]
        [Description("Cloudiness scales 0 to 1. If cloudiness is below X value, it is considered cloudy, if above, it is considered overcast or heavily clouded.")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_CLOUDY_THRESH = 0.7f;

        [Name("Cloudy Scaling Coef")]
        [Category("Weather Vision Effects - Clouds")]
        [Description("How much to reduce vision by if it is cloudy.")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_CLOUDY_COEF = 0.8f;

        [Name("Very Cloudy Scaling Coef")]
        [Category("Weather Vision Effects - Clouds")]
        [Description("How much to reduce vision by if it is very cloudy.")]
        [MinMax(0.01f, 1f, 1000f)]
        [Advanced]
        public float VISION_WEATHER_OVERCAST_COEF = 0.7f;

        [Name("Dawn Start Hour")]
        [MinMax(5f, 8f, 10f)]
        [Advanced]
        public float HourDawnStart = 6f;

        [Name("Dawn End Hour")]
        [MinMax(6f, 9f, 10f)]
        [Advanced]
        public float HourDawnEnd = 8f;

        [Name("Dusk Start Hour")]
        [MinMax(19f, 22f, 10f)]
        [Advanced]
        public float HourDuskStart = 20f;

        [Name("Dusk End Hour")]
        [MinMax(20f, 23f, 10f)]
        [Advanced]
        public float HourDuskEnd = 22f;
    }
}
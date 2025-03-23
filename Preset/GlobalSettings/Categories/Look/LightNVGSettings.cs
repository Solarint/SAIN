using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings
{
    public class LightNVGSettings : SAINSettingsBase<LightNVGSettings>, ISAINSettings
    {
        [Name("Flashlight Enable Visibility Ratio")]
        [Description("If visibility is lower than or equal to this ratio, bots will want to turn on their flashlight to see.")]
        [Advanced]
        [MinMax(0.01f, 0.99f, 100f)]
        public float LightOnRatio = 0.33f;

        [Name("Flashlight Disable Visibility Ratio")]
        [Description("If a bot has a flashlight enabled to see at night, and visibility is higher than or equal to this ratio, bots will want to turn off their flashlight.")]
        [Advanced]
        [MinMax(0.01f, 0.99f, 100f)]
        public float LightOffRatio = 0.66f;

        [Name("Nightvision Enable Visibility Ratio")]
        [Description("If visibility is lower than or equal to this ratio, bots will want to turn on their night vision goggles to see.")]
        [Advanced]
        [MinMax(0.01f, 0.99f, 100f)]
        public float NightVisionOnRatio = 0.33f;

        [Name("Nightvision Disable Visibility Ratio")]
        [Description("If a bot has night vision goggles enabled to see at night, and visibility is higher than or equal to this ratio, bots will want to turn off their NVGs.")]
        [Advanced]
        [MinMax(0.01f, 0.99f, 100f)]
        public float NightVisionOffRatio = 0.66f;
    }
}
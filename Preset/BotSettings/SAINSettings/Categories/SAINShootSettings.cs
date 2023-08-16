using EFT;
using Newtonsoft.Json;
using SAIN.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINShootSettings
    {
        [NameAndDescription("Recoil Scatter Multiplier",
            "Modifies how long bots shoot a burst during full auto fire. " +
            "Higher = longer full auto time. 1.5 = 1.5x longer bursts")]
        [DefaultValue(1f)]
        [MinMax(0.25f, 3f, 100f)]
        public float RecoilMultiplier = 1f;

        [NameAndDescription("Burst Length Multiplier",
            "Modifies how long bots shoot a burst during full auto fire. " +
            "Higher = longer full auto time. 1.5 = 1.5x longer bursts")]
        [DefaultValue(1.25f)]
        [MinMax(0.25f, 3f, 100f)]
        public float BurstMulti = 1.25f;

        [NameAndDescription("Semiauto Firerate Multiplier",
            "Modifies the time a bot waits between semiauto fire. " +
            "Higher = faster firerate. 1.5 = 1.5x more shots per second")]
        [DefaultValue(1.35f)]
        [MinMax(0.25f, 3f, 100f)]
        public float FireratMulti = 1.35f;

        [DefaultValue(true)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public bool CAN_STOP_SHOOT_CAUSE_ANIMATOR = true;

        [DefaultValue(100f)]
        [MinMax(0f, 100f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float CHANCE_TO_CHANGE_TO_AUTOMATIC_FIRE_100 = 100f;

        [DefaultValue(1.5f)]
        [MinMax(1f, 5f, 10f)]
        [Advanced(AdvancedEnum.IsAdvanced, AdvancedEnum.CopyValueFromEFT)]
        public float AUTOMATIC_FIRE_SCATTERING_COEF = 1.5f;

        [DefaultValue(0.5f)]
        [MinMax(0.1f, 2f, 10f)]
        [Advanced(AdvancedEnum.IsAdvanced, AdvancedEnum.CopyValueFromEFT)]
        public float BASE_AUTOMATIC_TIME = 0.5f;

        [DefaultValue(0.0f)]
        [Advanced(AdvancedEnum.Hidden)]
        public float RECOIL_DELTA_PRESS = 0.0f;
    }
}

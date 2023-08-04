using EFT;
using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINShootSettings
    {
        [Name("Recoil Scatter Multiplier")]
        [Description("Modifies how long bots shoot a burst during full auto fire. Higher = longer full auto time. 1.5 = 1.5x longer bursts")]
        [DefaultValue(1f)]
        [Minimum(0.25f)]
        [Maximum(3f)]
        [Rounding(100f)]
        public float RecoilMultiplier = 1f;

        [Name("Burst Length Multiplier")]
        [Description("Modifies how long bots shoot a burst during full auto fire. Higher = longer full auto time. 1.5 = 1.5x longer bursts")]
        [DefaultValue(1.25f)]
        [Minimum(0.25f)]
        [Maximum(3f)]
        [Rounding(100f)]
        public float BurstMulti = 1.25f;

        [Name("Semiauto Firerate Multiplier")]
        [Description("Modifies the time a bot waits between semiauto fire. Higher = faster firerate. 1.5 = 1.5x more shots per second")]
        [DefaultValue(1.35f)]
        [Minimum(0.25f)]
        [Maximum(3f)]
        [Rounding(100f)]
        public float FireratMulti = 1.35f;

        [IsHidden(true)]
        [DefaultValue(true)]
        [IsAdvanced(true)]
        public bool CAN_STOP_SHOOT_CAUSE_ANIMATOR = true;

        [DefaultValue(100f)]
        [Minimum(0f)]
        [Maximum(100f)]
        [Rounding(1)]
        [IsAdvanced(true)]
        public float CHANCE_TO_CHANGE_TO_AUTOMATIC_FIRE_100 = 100f;

        [DefaultValue(1.5f)]
        [Minimum(1f)]
        [Maximum(5f)]
        [Rounding(10)]
        public float AUTOMATIC_FIRE_SCATTERING_COEF = 1.5f;

        [DefaultValue(0.5f)]
        [Minimum(0.1f)]
        [Maximum(2f)]
        [Rounding(10)]
        [IsAdvanced(true)]
        public float BASE_AUTOMATIC_TIME = 0.5f;

        [IsHidden(true)]
        [DefaultValue(0.0f)]
        [IsAdvanced(true)]
        public float RECOIL_DELTA_PRESS = 0.0f;
    }
}

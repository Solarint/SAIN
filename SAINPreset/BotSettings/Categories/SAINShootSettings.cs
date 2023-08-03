using EFT;
using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINShootSettings
    {
        public float RecoilMultiplier;
        public float BurstMulti;
        public float FireratMulti;


        [IsHidden(true)]
        [DefaultValue(true)]
        public bool CAN_STOP_SHOOT_CAUSE_ANIMATOR = true;

        [DefaultValue(100f)]
        [Minimum(0f)]
        [Maximum(100f)]
        [Rounding(1)]
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
        public float BASE_AUTOMATIC_TIME = 0.5f;

        [IsHidden(true)]
        [DefaultValue(0.0f)]
        public float RECOIL_DELTA_PRESS = 0.0f;
    }
}

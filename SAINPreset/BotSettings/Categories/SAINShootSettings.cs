using EFT;
using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINShootSettings
    {
        static void InitShoot()
        {
            string name = "Accuracy Spread Multiplier";
            string desc = "Modifies a bot's base accuracy and spread. Higher = less accurate. 1.5 = 1.5x higher accuracy spread";
            string section = "Shoot/Aim";
            float def = 1f;
            float min = 0.1f;
            float max = 5f;
            float round = 100f;

            name = "Accuracy Speed Multiplier";
            desc = "Modifies a bot's Accuracy Speed, or how fast their accuracy improves over time when shooting. " +
                "Higher = longer to gain accuracy. 1.5 = 1.5x longer to aim";
            section = "Shoot/Aim";
            def = 1f;
            min = 0.1f;
            max = 5f;
            round = 100f;

            name = "Max Aim Time Multiplier";
            desc = "Modifies the maximum time a bot can aim, or how long in seconds a bot takes to finish aiming. " +
                "Higher = longer to full aim. 1.5 = 1.5x longer to aim";
            section = "Shoot/Aim";
            def = 1f;
            min = 0.1f;
            max = 5f;
            round = 100f;

            name = "Recoil Scatter Multiplier";
            desc = "Modifies a bot's recoil impulse from a shot. Higher = less accurate. 1.5 = 1.5x more recoil and scatter per shot";
            section = "Shoot/Aim";
            def = 1f;
            min = 0.25f;
            max = 3f;
            round = 100f;

            name = "Burst Length Multiplier";
            desc = "Modifies how long bots shoot a burst during full auto fire. Higher = longer full auto time. 1.5 = 1.5x longer bursts";
            section = "Shoot/Aim";
            def = 1.25f;
            min = 0.25f;
            max = 3f;
            round = 100f;

            name = "Semiauto Firerate Multiplier";
            desc = "Modifies the time a bot waits between semiauto fire. Higher = faster firerate. 1.5 = 1.5x more shots per second";
            section = "Shoot/Aim";
            def = 1.35f;
            min = 0.25f;
            max = 3f;
            round = 100f;
        }
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

using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class BotShootConfig
    {
        public static ConfigEntry<float> ScatterMultiplier { get; private set; }
        public static ConfigEntry<float> MaxScatter { get; private set; }
        public static ConfigEntry<float> AddRecoil { get; private set; }
        public static ConfigEntry<float> LerpRecoil { get; private set; }
        public static ConfigEntry<float> BurstLengthModifier { get; private set; }
        public static ConfigEntry<float> RateofFire { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string shoot = "Bot Shoot Settings";

            RateofFire = Config.Bind(shoot, "SemiAuto Rate of Fire Modifier", 1.45f,
                new ConfigDescription("Adjusts bot's base rate of fire value. Higher is faster overall rate of fire.",
                new AcceptableValueRange<float>(0.01f, 5.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 3 }));

            BurstLengthModifier = Config.Bind(shoot, "Burst Length Modifier", 1.25f,
                new ConfigDescription("Adjusts the length of full auto bursts. A higher number means that they will shoot longer bursts.",
                new AcceptableValueRange<float>(0.1f, 3.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 7 }));

            ScatterMultiplier = Config.Bind(shoot, "Bot Recoil Scatter Multiplier", 1.5f,
                new ConfigDescription("Increases or decreases scatter from recoil",
                new AcceptableValueRange<float>(0.01f, 10.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4 }));

            MaxScatter = Config.Bind(shoot, "Bot Recoil Max Scatter", 2f,
                new ConfigDescription("Upper Limit for how far from a target the recoil scatter can go",
                new AcceptableValueRange<float>(0.5f, 10.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));

            AddRecoil = Config.Bind(shoot, "Bot Recoil Add Recoil Scatter", 0.35f,
                new ConfigDescription("Adds or subtracts from the recoil felt per shot",
                new AcceptableValueRange<float>(-1.0f, 5.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));

            LerpRecoil = Config.Bind(shoot, "Bot Recoil Recoil Reduction", 0.925f,
                new ConfigDescription("How much to reduce recoil per frame when not shooting",
                new AcceptableValueRange<float>(0.1f, 0.99f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));
        }
    }
}
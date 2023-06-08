using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class RecoilScatterConfig
    {
        public static ConfigEntry<float> ScatterMultiplier { get; private set; }
        public static ConfigEntry<float> MaxScatter { get; private set; }
        public static ConfigEntry<float> AddRecoil { get; private set; }
        public static ConfigEntry<float> LerpRecoil { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string recoil = "1. Recoil";

            ScatterMultiplier = Config.Bind(recoil, "Scatter Multiplier", 1f,
                new ConfigDescription("Increases or decreases scatter from recoil",
                new AcceptableValueRange<float>(0.01f, 5.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4 }));

            MaxScatter = Config.Bind(recoil, "Max Scatter", 2f,
                new ConfigDescription("Upper Limit for how far from a target the recoil scatter can go",
                new AcceptableValueRange<float>(0.5f, 10.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));

            AddRecoil = Config.Bind(recoil, "Add Recoil Scatter", 0.35f,
                new ConfigDescription("Adds or subtracts from the recoil felt per shot",
                new AcceptableValueRange<float>(-1.0f, 5.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));

            LerpRecoil = Config.Bind(recoil, "Recoil Reduction", 0.9f,
                new ConfigDescription("How much to reduce recoil per frame when not shooting",
                new AcceptableValueRange<float>(0.1f, 0.99f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));
        }
    }
}
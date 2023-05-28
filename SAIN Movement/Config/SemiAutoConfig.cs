using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class SemiAutoConfig
    {
        public static ConfigEntry<float> RateofFire { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string scaling = "3. Distance Scaling Settings";

            RateofFire = Config.Bind(scaling, "Rate of Fire", 1f,
                new ConfigDescription("Adjusts bot's base rate of fire value. Higher is faster overall rate of fire.",
                new AcceptableValueRange<float>(0.01f, 2.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));
        }
    }
}
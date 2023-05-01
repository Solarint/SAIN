using BepInEx.Configuration;

namespace Combat.UserSettings
{
    internal class SemiAutoConfig
    {
        public static ConfigEntry<bool> SemiROFToggle { get; private set; }
        public static ConfigEntry<float> RateofFire { get; private set; }
        public static ConfigEntry<bool> SimpleModeROF { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string scaling = "3. Distance Scaling Settings";

            SemiROFToggle = Config.Bind(scaling, "Distance Scaling", true,
                new ConfigDescription("Bots shoot slower the further away their target is. SemiAutoConfig depends on weapon type, bot type, and weapon build.", null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 14 }));

            RateofFire = Config.Bind(scaling, "Rate of Fire", 1f,
                new ConfigDescription("Adjusts bot's base rate of fire value. Higher is faster overall rate of fire.",
                new AcceptableValueRange<float>(0.01f, 2.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));

            SimpleModeROF = Config.Bind(scaling, "Simple Mode", false,
                new ConfigDescription("For those who HATE nuance. Bots will not take into consideration weapon build, bot type, ammo stats, recoil, or ergo when adjusting their burst length.", null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1 }));
        }
    }
}
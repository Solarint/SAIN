using BepInEx.Configuration;


namespace Combat.UserSettings
{
    internal class FullAutoConfig
    {
        public static ConfigEntry<bool> BurstLengthToggle { get; private set; }
        public static ConfigEntry<float> BurstLengthModifier { get; private set; }
        public static ConfigEntry<bool> SimpleModeBurst { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string modeswap = "2. Fullauto Settings";

            BurstLengthToggle = Config.Bind(modeswap, "Burst Length Scaling", true,
                new ConfigDescription("Bots will slow down their burst length depending on distance, eventually reaching single-fire on targets at range.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 92 }));

            BurstLengthModifier = Config.Bind(modeswap, "Burst Length Modifier", 1.0f,
                new ConfigDescription("Adjusts the length of full auto bursts. A higher number means that they will shoot longer bursts.",
                new AcceptableValueRange<float>(0.1f, 3.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 7 }));

            SimpleModeBurst = Config.Bind(modeswap, "Simple Mode", false,
                new ConfigDescription("For those who HATE nuance. Bots will not take into consideration weapon build, bot type, ammo stats, recoil, or ergo when adjusting their burst length.", null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 5 }));
        }
    }
}
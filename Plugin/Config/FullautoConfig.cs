using BepInEx.Configuration;


namespace SAIN.UserSettings
{
    internal class FullAutoConfig
    {
        public static ConfigEntry<float> BurstLengthModifier { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string modeswap = "2. Fullauto Settings";

            BurstLengthModifier = Config.Bind(modeswap, "Burst Length Modifier", 1.0f,
                new ConfigDescription("Adjusts the length of full auto bursts. A higher number means that they will shoot longer bursts.",
                new AcceptableValueRange<float>(0.1f, 3.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 7 }));
        }
    }
}
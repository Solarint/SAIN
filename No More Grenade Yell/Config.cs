using BepInEx.Configuration;

namespace SAIN_Grenades.Configs
{
    internal class GrenadeYell
    {
        public static ConfigEntry<float> GrenadeYellChance { get; private set; }
        public static ConfigEntry<bool> SuperAngryMode { get; private set; }
        public static ConfigEntry<bool> DebugLogs { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string debugmode = "Settings";

            GrenadeYellChance = Config.Bind(debugmode, "Chance to Yell about Grenade", 50f,
                new ConfigDescription("Percentage Chance to allow a bot to yell about a grenade if they try to",
                new AcceptableValueRange<float>(0f, 100f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 999 }));
        }
    }
}
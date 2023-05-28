using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class TalkConfig
    {
        public static ConfigEntry<bool> EnableMod { get; private set; }
        public static ConfigEntry<bool> SuperAngryMode { get; private set; }
        public static ConfigEntry<bool> DebugLogs { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string debugmode = "Settings";

            EnableMod = Config.Bind(debugmode, "Mod Enabled", true,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 999 }));

            SuperAngryMode = Config.Bind(debugmode, "VERY ANGERY MODE", false,
                new ConfigDescription("Bots will ONLY say angry phrases. All of the time.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1 }));

            DebugLogs = Config.Bind(debugmode, "Debug Logs", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = -999 }));
        }
    }
}
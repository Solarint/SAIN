using BepInEx.Configuration;

namespace SAIN.Config
{
    internal class Template
    {
        public static ConfigEntry<bool> EnableMod { get; private set; }
        public static ConfigEntry<float> ConfigValue { get; private set; }
        public static ConfigEntry<bool> DebugLogs { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string debugmode = "Settings";

            EnableMod = Config.Bind(debugmode, "Mod Enabled", true,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 999 }));

            ConfigValue = Config.Bind(debugmode, "configoption", 1f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(0.0f, 1.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1 }));

            DebugLogs = Config.Bind(debugmode, "Debug Logs", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = -999 }));
        }
    }
}
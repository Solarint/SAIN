using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class TalkConfig
    {
        public static ConfigEntry<bool> DebugLogs { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string debugmode = "Bot Talk Settings";

            DebugLogs = Config.Bind(debugmode, "Debug Logs", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = -999 }));
        }
    }
}
using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class DebugConfig
    {
        public static ConfigEntry<bool> DebugLayers { get; private set; }
        public static ConfigEntry<bool> DebugBotDecisions { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string debugmode = "7. Debug Mode";

            DebugLayers = Config.Bind(debugmode, "Debug Layers", false, 
                new ConfigDescription("", 
                null, 
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 7 }));

            DebugBotDecisions = Config.Bind(debugmode, "Debug Bot Decisions", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 5 }));
        }
    }
}
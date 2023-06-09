﻿using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class DebugConfig
    {
        public static ConfigEntry<bool> DebugLayers { get; private set; }
        public static ConfigEntry<bool> DebugBotDecisions { get; private set; }
        public static ConfigEntry<bool> DebugBotInfo { get; private set; }
        public static ConfigEntry<bool> ToggleDebugGizmos { get; private set; }

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

            DebugBotInfo = Config.Bind(debugmode, "Debug Bot Info", false,
                new ConfigDescription("Shows Logs of the results of Created Bot Information",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 5 }));

            ToggleDebugGizmos = Config.Bind(debugmode, "Toggle Debug Gizmos", false,
                new ConfigDescription("Global Toggle for all Debug Gizmos",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 5 }));
        }
    }
}
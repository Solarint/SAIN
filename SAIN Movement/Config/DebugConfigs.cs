using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class DebugConfig
    {
        public static ConfigEntry<bool> DebugLayers { get; private set; }
        public static ConfigEntry<bool> DebugBotDecisions { get; private set; }
        public static ConfigEntry<float> VisionRaycast { get; private set; }
        public static ConfigEntry<float> ShootRaycast { get; private set; }
        public static ConfigEntry<float> CheckPath { get; private set; }

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

            VisionRaycast = Config.Bind(debugmode, "Raycast Vision Frequency", 0.15f,
                new ConfigDescription("How often to update the bool value of Bot's ability to see their enemy",
                new AcceptableValueRange<float>(0f, 0.5f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 11 }));

            ShootRaycast = Config.Bind(debugmode, "Raycast Shoot Frequency", 0.15f,
                new ConfigDescription("How often to update the bool value of Bot's ability to shoot their enemy",
                new AcceptableValueRange<float>(0f, 0.5f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 10 }));

            CheckPath = Config.Bind(debugmode, "Update NavMesh Path Frequency", 0.5f,
                new ConfigDescription("How often to calculate path between enemy and Bot",
                new AcceptableValueRange<float>(0f, 3f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 9 }));
        }
    }
}
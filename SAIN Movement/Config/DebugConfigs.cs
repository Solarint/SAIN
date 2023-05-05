using BepInEx.Configuration;

namespace Movement.UserSettings
{
    internal class Debug
    {
        public static ConfigEntry<bool> DebugDynamicLean { get; private set; }
        public static ConfigEntry<bool> DebugDogFightLayer { get; private set; }
        public static ConfigEntry<bool> DebugDogFightLayerDraw { get; private set; }
        public static ConfigEntry<bool> DebugBotDecisions { get; private set; }
        public static ConfigEntry<bool> DebugUpdateMove { get; private set; }
        public static ConfigEntry<bool> DebugUpdateShoot { get; private set; }
        public static ConfigEntry<bool> DebugUpdateSteering { get; private set; }
        public static ConfigEntry<bool> DebugUpdateTargetting { get; private set; }
        public static ConfigEntry<bool> DebugCoverSystem { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string debugmode = "7. Debug Mode";

            DebugDynamicLean = Config.Bind(debugmode, "Dynamic Lean", false,
                new ConfigDescription("Draws debug Lines and logs events",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 8 }));

            DebugDogFightLayer = Config.Bind(debugmode, "DogFight Layer", false, 
                new ConfigDescription("Draws debug Lines and logs events", 
                null, 
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 7 }));

            DebugDogFightLayerDraw = Config.Bind(debugmode, "DogFight Layer Draw Generated Points", false,
                new ConfigDescription("Draws points everywhere",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 6 }));

            DebugBotDecisions = Config.Bind(debugmode, "DebugBotDecisions", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 5 }));

            DebugUpdateMove = Config.Bind(debugmode, "DebugUpdateMove", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));

            DebugUpdateShoot = Config.Bind(debugmode, "DebugUpdateShoot", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));

            DebugUpdateSteering = Config.Bind(debugmode, "DebugUpdateSteering", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));

            DebugUpdateTargetting = Config.Bind(debugmode, "DebugUpdateTargetting", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));

            DebugCoverSystem = Config.Bind(debugmode, "DebugCoverSystem", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 0 }));
        }
    }
}
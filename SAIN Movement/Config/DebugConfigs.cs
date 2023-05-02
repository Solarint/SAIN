using BepInEx.Configuration;

namespace Movement.UserSettings
{
    internal class Debug
    {
        public static ConfigEntry<bool> DebugDogFightLayer { get; private set; }
        public static ConfigEntry<bool> DebugDogFightLayerDraw { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string debugmode = "7. Debug Mode";

            DebugDogFightLayer = Config.Bind(debugmode, "Debug DogFight Layer", false, 
                new ConfigDescription("Draws debug Lines and logs events", 
                null, 
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 5 }));

            DebugDogFightLayerDraw = Config.Bind(debugmode, "Draw Generated Points", false,
                new ConfigDescription("Draws points everywhere",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 5 }));
        }
    }
}
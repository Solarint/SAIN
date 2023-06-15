using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class DazzleConfig
    {
        public static ConfigEntry<float> Effectiveness { get; private set; }
        public static ConfigEntry<float> MaxDazzleRange { get; private set; }
        public static ConfigEntry<bool> DebugFlash { get; private set; }
        public static ConfigEntry<bool> SillyMode { get; private set; }
        public static void Init(ConfigFile Config)
        {
            string debugmode = "Flashlight Dazzle Settings";

            Effectiveness = Config.Bind(debugmode, "Dazzle Intensity", 3f,
                new ConfigDescription("Intensifies the dazzle effect",
                new AcceptableValueRange<float>(0.1f, 10.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 6 }));

            MaxDazzleRange = Config.Bind(debugmode, "Max Dazzle Range", 25f,
                new ConfigDescription("Maximum possible distance to affect AI aim",
                new AcceptableValueRange<float>(5.0f, 100.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));

            SillyMode = Config.Bind(debugmode, "Silly Mode", false,
                new ConfigDescription("Enables lol so funny random mode. Bots are flashbanged by flashlights and are very upset about it.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 2 }));

            DebugFlash = Config.Bind(debugmode, "Debug Mode", false,
                new ConfigDescription("Draws Lines and Spheres to visualize flashlight detection. Logs information about dazzle.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));
        }
    }
}
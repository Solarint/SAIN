using BepInEx.Configuration;

namespace SAIN.Flashlights.Config
{
    internal class DazzleConfig
    {
        public static ConfigEntry<bool> EnableMod { get; private set; }
        public static ConfigEntry<float> Angle { get; private set; }
        public static ConfigEntry<float> Effectiveness { get; private set; }
        public static ConfigEntry<float> MaxDazzleRange { get; private set; }
        public static ConfigEntry<bool> DebugFlash { get; private set; }
        public static ConfigEntry<bool> AIHatesFlashlights { get; private set; }
        public static ConfigEntry<bool> SillyMode { get; private set; }
        public static void Init(ConfigFile Config)
        {
            string debugmode = "Settings";

            Effectiveness = Config.Bind(debugmode, "Dazzle Intensity", 1f,
                new ConfigDescription("Intensifies the dazzle effect",
                new AcceptableValueRange<float>(0.1f, 3.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 6 }));

            Angle = Config.Bind(debugmode, "Angle Modifier", 1f,
                new ConfigDescription("Lower value is equal to wider flashlight angle. The wider the angle, the easier it is for bots to be dazzled.",
                new AcceptableValueRange<float>(0.8f, 1.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 5 }));

            MaxDazzleRange = Config.Bind(debugmode, "Max Dazzle Range", 25f,
                new ConfigDescription("Maximum possible distance to affect AI aim",
                new AcceptableValueRange<float>(5.0f, 100.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));

            AIHatesFlashlights = Config.Bind(debugmode, "Random Voicelines", true,
                new ConfigDescription("Small Chance to play a random voiceline when being dazzled",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 3 }));

            SillyMode = Config.Bind(debugmode, "Silly Mode", false,
                new ConfigDescription("Enables lol so funny random mode. Bots are flashbanged by flashlights and are very upset about it.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 2 }));


            DebugFlash = Config.Bind(debugmode, "Debug Logs", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));
        }
    }
}
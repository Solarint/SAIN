using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class VisionConfig
    {
        public static ConfigEntry<bool> DebugWeather { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string debugmode = "Vision Settings";

            DebugWeather = Config.Bind(debugmode, "Debug Weather", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));
        }
    }
}
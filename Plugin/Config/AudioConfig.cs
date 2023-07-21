using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class SoundConfig
    {
        public static ConfigEntry<bool> DebugSound { get; private set; }
        public static ConfigEntry<bool> DebugSolarintSound { get; private set; }
        public static ConfigEntry<bool> DebugOcclusion { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string audio = "Bot Audio Settings";

            DebugSound = Config.Bind(audio,
                "Gunshot Audible Distance Logs",
                false, new ConfigDescription(
                "", null, new ConfigurationManagerAttributes
                { IsAdvanced = true, Order = -10 }));

            DebugSolarintSound = Config.Bind(audio,
                "Solarint Audio Logs",
                false, new ConfigDescription(
                "its not steam or oculus audio, its far worse", null, new ConfigurationManagerAttributes
                { IsAdvanced = true, Order = -11 }));

            DebugOcclusion = Config.Bind(audio,
                "Sound Occlusion Logs",
                false, new ConfigDescription(
                "", null, new ConfigurationManagerAttributes
                { IsAdvanced = true, Order = -12 }));
        }
    }
}
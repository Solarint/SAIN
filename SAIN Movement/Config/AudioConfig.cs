using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class SoundConfig
    {
        public static ConfigEntry<float> AudibleRange { get; private set; }
        public static ConfigEntry<float> SuppressorModifier { get; private set; }
        public static ConfigEntry<float> SubsonicModifier { get; private set; }
        public static ConfigEntry<bool> RaycastOcclusion { get; private set; }

        public static ConfigEntry<bool> DebugSound { get; private set; }
        public static ConfigEntry<bool> DebugSolarintSound { get; private set; }
        public static ConfigEntry<bool> DebugOcclusion { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string modeswap = "1. Bot Audio Settings";

            AudibleRange = Config.Bind(modeswap, "Audible Range Modifier", 1.0f,
                new ConfigDescription("Modifier for how far bots will hear gunshots",
                new AcceptableValueRange<float>(0.1f, 2.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));

            SuppressorModifier = Config.Bind(modeswap, "Supersonic Modifier", 0.6f,
                new ConfigDescription("Modifier for how much less audible suppressed shots are when the round is super-sonic",
                new AcceptableValueRange<float>(0.1f, 0.99f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));

            SubsonicModifier = Config.Bind(modeswap, "Subsonic Modifier", 0.3f,
                new ConfigDescription("Modifier for how much less audible suppressed shots are when the round is sub-sonic",
                new AcceptableValueRange<float>(0.1f, 0.99f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));

            RaycastOcclusion = Config.Bind(modeswap, "Raycast Sound Occlusion", true,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1 }));


            string debugmode = "2. Debug Logs";

            DebugSound = Config.Bind(debugmode,
                "Gunshot Audible Distance Logs",
                false, new ConfigDescription(
                "", null, new ConfigurationManagerAttributes
                { IsAdvanced = true, Order = 5 }));

            DebugSolarintSound = Config.Bind(debugmode,
                "Solarint Audio Logs",
                false, new ConfigDescription(
                "its not steam or oculus audio, its far worse", null, new ConfigurationManagerAttributes
                { IsAdvanced = true, Order = 3 }));

            DebugOcclusion = Config.Bind(debugmode,
                "Sound Occlusion Logs",
                false, new ConfigDescription(
                "", null, new ConfigurationManagerAttributes
                { IsAdvanced = true, Order = 2 }));
        }
    }
}
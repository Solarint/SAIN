using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class SoundConfig
    {
        public static ConfigEntry<float> SuppressorModifier { get; private set; }
        public static ConfigEntry<float> SubsonicModifier { get; private set; }
        public static ConfigEntry<bool> RaycastOcclusion { get; private set; }

        public static ConfigEntry<bool> DebugSound { get; private set; }
        public static ConfigEntry<bool> DebugSolarintSound { get; private set; }
        public static ConfigEntry<bool> DebugOcclusion { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string audio = "Bot Audio Settings";

            SuppressorModifier = Config.Bind(audio, "Supersonic Modifier", 0.6f,
                new ConfigDescription("Modifier for how much less audible suppressed shots are when the round is super-sonic",
                new AcceptableValueRange<float>(0.1f, 0.99f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));

            SubsonicModifier = Config.Bind(audio, "Subsonic Modifier", 0.3f,
                new ConfigDescription("Modifier for how much less audible suppressed shots are when the round is sub-sonic",
                new AcceptableValueRange<float>(0.1f, 0.99f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));

            RaycastOcclusion = Config.Bind(audio, "Raycast Sound Occlusion", true,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));


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
using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class CoverConfig
    {
        public static ConfigEntry<float> CoverColliderRadius { get; private set; }
        public static ConfigEntry<float> CoverMinHeight { get; private set; }
        public static ConfigEntry<float> CoverMinEnemyDistance { get; private set; }
        public static ConfigEntry<float> CoverHideSensitivity { get; private set; }
        public static ConfigEntry<float> CoverUpdateFrequency { get; private set; }
        public static ConfigEntry<float> CoverNavSampleDistance { get; private set; }
        public static ConfigEntry<bool> AllBotsMoveToPlayer { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string cover = "1. Cover System";

            AllBotsMoveToPlayer = Config.Bind(cover, "All Bots Move To Player", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 26 }));

            CoverColliderRadius = Config.Bind(cover, "Collider Radius Base", 15f, 
                new ConfigDescription("", 
                new AcceptableValueRange<float>(3f, 50f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 6 }));

            CoverMinHeight = Config.Bind(cover, "Min Height", 0.75f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(0.1f, 2f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 5 }));

            CoverMinEnemyDistance = Config.Bind(cover, "Min Player Distance", 12f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4 }));

            CoverHideSensitivity = Config.Bind(cover, "Hide Sensitivity", 0f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(-1, 1f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 3 }));

            CoverUpdateFrequency = Config.Bind(cover, "Update Frequency", 0.25f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(0.01f, 1f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 2 }));

            CoverNavSampleDistance = Config.Bind(cover, "Nav Sample Distance", 3f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(1f, 5f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 2 }));

        }
    }
}
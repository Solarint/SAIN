using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class CoverConfig
    {
        public static ConfigEntry<float> CoverColliderRadius { get; private set; }
        public static ConfigEntry<int> ColliderArrayCount { get; private set; }

        public static ConfigEntry<float> CoverMinHeight { get; private set; }
        public static ConfigEntry<float> CoverMinEnemyDistance { get; private set; }
        public static ConfigEntry<float> CoverUpdateFrequency { get; private set; }

        public static ConfigEntry<bool> DebugCoverFinder { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string cover = "1. Cover System";

            CoverColliderRadius = Config.Bind(cover, "Collider Radius Base", 50f, 
                new ConfigDescription("", 
                new AcceptableValueRange<float>(5f, 100f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 6 }));

            ColliderArrayCount = Config.Bind(cover, "Collider Array Count", 1500,
                new ConfigDescription("",
                new AcceptableValueRange<int>(75, 3000),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 6 }));

            CoverMinHeight = Config.Bind(cover, "Min Height", 0.75f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(0.5f, 1.6f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 5 }));

            CoverMinEnemyDistance = Config.Bind(cover, "Min Player Distance", 5f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(0f, 20f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4 }));

            CoverUpdateFrequency = Config.Bind(cover, "Update Frequency", 0.25f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(0.05f, 1f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 2 }));

            DebugCoverFinder = Config.Bind(cover, "Debug CoverFinder", false,
                new ConfigDescription("Draws debug gizmos at points and colliders",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = -2 }));
        }
    }
}
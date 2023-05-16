using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class CoverConfig
    {
        public static ConfigEntry<float> CoverColliderRadius { get; private set; }
        public static ConfigEntry<float> CoverMinHeight { get; private set; }
        public static ConfigEntry<float> CoverMinPlayerDistance { get; private set; }
        public static ConfigEntry<float> CoverHideSensitivity { get; private set; }
        public static ConfigEntry<float> CoverUpdateFrequency { get; private set; }
        public static ConfigEntry<float> CoverNavSampleDistance { get; private set; }
        public static ConfigEntry<int> CoverColliderCount { get; private set; }

        public static ConfigEntry<bool> LeanToggle { get; private set; }
        public static ConfigEntry<bool> ScavLeanToggle { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string cover = "1. Cover System";

            CoverColliderRadius = Config.Bind(cover, "Collider Radius Base", 5f, 
                new ConfigDescription("Adds 5m to this value if cover isn't found until it is found, then resets to 0", 
                new AcceptableValueRange<float>(3f, 30f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 6 }));

            CoverMinHeight = Config.Bind(cover, "Min Height", 0.75f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(0.1f, 2f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 5 }));

            CoverMinPlayerDistance = Config.Bind(cover, "Min Player Distance", 12f,
                new ConfigDescription("1 is subtracted from this number each time cover is not found",
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
                new AcceptableValueRange<float>(2f, 50f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 2 }));

            CoverColliderCount = Config.Bind(cover, "Collider Count", 50,
                new ConfigDescription("",
                new AcceptableValueRange<int>(10, 200),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1 }));


            string lean = "2. Dynamic Lean";

            LeanToggle = Config.Bind(lean, "Dynamic Lean", true,
                new ConfigDescription("Enables Fully Dynamic Lean based on corner angles between a bot and their enemy.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 8 }));

            ScavLeanToggle = Config.Bind(lean, "Scav Dynamic Lean", false,
                new ConfigDescription("Enables Scavs using Dynamic Lean",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 7 }));

        }
    }
}
using BepInEx.Configuration;

namespace Movement.UserSettings
{
    internal class CoverSystemConfig
    {
        public static ConfigEntry<bool> SaveCoverPoints { get; private set; }
        public static ConfigEntry<int> RayCastAccuracy { get; private set; }
        public static ConfigEntry<int> CoverPointBatchSize { get; private set; }
        public static ConfigEntry<int> MaxPaths { get; private set; }
        public static ConfigEntry<int> MaxCorners { get; private set; }
        public static ConfigEntry<int> MaxRandomGenIterations { get; private set; }
        public static ConfigEntry<float> PointGeneratorTimer { get; private set; }
        public static ConfigEntry<float> FilterDistance { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string debugmode = "1. Cover System";

            SaveCoverPoints = Config.Bind(debugmode, "Save Points", true,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = -1 }));

            RayCastAccuracy = Config.Bind(debugmode, "RayCastAccuracy", 2,
                new ConfigDescription("",
                new AcceptableValueRange<int>(0, 5),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = -1 }));

            CoverPointBatchSize = Config.Bind(debugmode, "CoverPointBatchSize", 10,
                new ConfigDescription("",
                new AcceptableValueRange<int>(3, 100),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = -1 }));

            MaxPaths = Config.Bind(debugmode, "MaxPaths", 5,
                new ConfigDescription("",
                new AcceptableValueRange<int>(1, 30),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = -1 }));

            MaxCorners = Config.Bind(debugmode, "MaxCorners", 20,
                new ConfigDescription("",
                new AcceptableValueRange<int>(5, 200),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = -1 }));

            MaxRandomGenIterations = Config.Bind(debugmode, "MaxRandomGenIterations", 10,
                new ConfigDescription("",
                new AcceptableValueRange<int>(3, 100),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = -1 }));

            PointGeneratorTimer = Config.Bind(debugmode, "PointGeneratorTimer", 1.0f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(0.1f, 2f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = -1 }));

            FilterDistance = Config.Bind(debugmode, "FilterDistance", 0.5f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(0.1f, 3f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = -1 }));
        }
    }
}
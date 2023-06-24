using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class ExtractConfig
    {
        public static ConfigEntry<float> MaxPercentage { get; private set; }
        public static ConfigEntry<float> MinPercentage { get; private set; }
        public static ConfigEntry<bool> EnableExtracts { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string audio = "Bot Extract Settings";

            MaxPercentage = Config.Bind(audio, "Max Raid Percentage", 35f,
                new ConfigDescription("Assigned on bot spawn. Time Remaining divided by total time of raid. Bots will decide to start moving to extract at no lower than this.",
                new AcceptableValueRange<float>(1f, 99f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));

            MinPercentage = Config.Bind(audio, "Min Raid Percentage", 5f,
                new ConfigDescription("Assigned on bot spawn. Time Remaining divided by total time of raid. All Scavs and PMCs will be moving to extact by this time remaining",
                new AcceptableValueRange<float>(1f, 99f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));

            EnableExtracts = Config.Bind(audio, "Bot Extracts", true,
                new ConfigDescription("Turn off if you hate new features.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));
        }
    }
}
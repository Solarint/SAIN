using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class DifficultyConfig
    {
        public static ConfigEntry<float> GlobalDifficulty { get; private set; }
        public static ConfigEntry<float> PMCDifficulty { get; private set; }
        public static ConfigEntry<float> ScavDifficulty { get; private set; }
        public static ConfigEntry<float> OtherDifficulty { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string shoot = "Difficulty Settings";

            GlobalDifficulty = Config.Bind(shoot, "Global Difficulty", 1.0f,
                new ConfigDescription("Affects all bots. Requires raid restart to update. Only modifies Recoil/scatter as of Beta 2",
                new AcceptableValueRange<float>(0.1f, 2.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 3 }));

            PMCDifficulty = Config.Bind(shoot, "PMC Difficulty", 1.0f,
                new ConfigDescription("Requires raid restart to update. Only modifies Recoil/scatter as of Beta 2",
                new AcceptableValueRange<float>(0.1f, 2.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 3 }));

            ScavDifficulty = Config.Bind(shoot, "Scav Difficulty", 1.0f,
                new ConfigDescription("Requires raid restart to update. Only modifies Recoil/scatter as of Beta 2",
                new AcceptableValueRange<float>(0.1f, 2.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 3 }));

            OtherDifficulty = Config.Bind(shoot, "Other Difficulty", 1.0f,
                new ConfigDescription("Everything except scavs and pmcs. Requires raid restart to update. Only modifies Recoil/scatter as of Beta 2",
                new AcceptableValueRange<float>(0.1f, 2.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 3 }));
        }
    }
}
using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class DifficultyConfig
    {
        public static ConfigEntry<float> GlobalDifficulty { get; private set; }
        public static ConfigEntry<float> PMCDifficulty { get; private set; }
        public static ConfigEntry<float> ScavDifficulty { get; private set; }
        public static ConfigEntry<float> OtherDifficulty { get; private set; }
        public static ConfigEntry<bool> FasterCQBReactions { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string shoot = "Difficulty Settings";

            FasterCQBReactions = Config.Bind(shoot, "Faster CQB Reaction Time", true,
                new ConfigDescription("Bots will start shooting much faster when enemies are close",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 10 }));

            GlobalDifficulty = Config.Bind(shoot, "Global Difficulty", 1.0f,
                new ConfigDescription("Affects all bots. Only modifies Recoil/scatter. Higher Number equals harder bots",
                new AcceptableValueRange<float>(0.5f, 2.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 5 }));

            PMCDifficulty = Config.Bind(shoot, "PMC Difficulty", 1.0f,
                new ConfigDescription("Only modifies Recoil/scatter. Higher Number equals harder bots",
                new AcceptableValueRange<float>(0.5f, 2.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4 }));

            ScavDifficulty = Config.Bind(shoot, "Scav Difficulty", 1.0f,
                new ConfigDescription("Only modifies Recoil/scatter. Higher Number equals harder bots",
                new AcceptableValueRange<float>(0.5f, 2.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 3 }));

            OtherDifficulty = Config.Bind(shoot, "Other Difficulty", 1.0f,
                new ConfigDescription("Everything except scavs and pmcs. Only modifies Recoil/scatter. Higher Number equals harder bots",
                new AcceptableValueRange<float>(0.5f, 2.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 2 }));
        }
    }
}
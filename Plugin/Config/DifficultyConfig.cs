using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class DifficultyConfig
    {
        public static ConfigEntry<float> BotRecoilGlobal { get; private set; }
        public static ConfigEntry<float> PMCRecoil { get; private set; }
        public static ConfigEntry<float> ScavRecoil { get; private set; }
        public static ConfigEntry<float> OtherRecoil { get; private set; }

        public static ConfigEntry<float> BaseAccuracy { get; private set; }

        public static ConfigEntry<bool> FasterCQBReactions { get; private set; }
        public static ConfigEntry<bool> AllChads { get; private set; }
        public static ConfigEntry<bool> AllGigaChads { get; private set; }
        public static ConfigEntry<bool> AllRats { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string shoot = "Difficulty Settings";

            FasterCQBReactions = Config.Bind(shoot, "Faster CQB Reaction Time", true,
                new ConfigDescription("Bots will start shooting much faster when enemies are close",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 10 }));

            BaseAccuracy = Config.Bind(shoot, "Base Accuracy Modifier", 1.0f,
                new ConfigDescription("Affects all bots. Modifies base accuracy for aiming. Higher Number equals easier and less accurate bots",
                new AcceptableValueRange<float>(0.5f, 1.5f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 5 }));

            BotRecoilGlobal = Config.Bind(shoot, "Bot Global Recoil Multiplier", 1.0f,
                new ConfigDescription("Affects all bots. Only modifies Recoil/scatter. Higher Number equals easier and less accurate bots",
                new AcceptableValueRange<float>(0.5f, 3.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 5 }));

            PMCRecoil = Config.Bind(shoot, "PMC Recoil Multiplier", 1.0f,
                new ConfigDescription("Only modifies Recoil/scatter. Higher Number equals easier and less accurate bots",
                new AcceptableValueRange<float>(0.5f, 3.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4 }));

            ScavRecoil = Config.Bind(shoot, "Scav Recoil Multiplier", 1.0f,
                new ConfigDescription("Only modifies Recoil/scatter. Higher Number equals easier and less accurate bots",
                new AcceptableValueRange<float>(0.5f, 3.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 3 }));

            OtherRecoil = Config.Bind(shoot, "Other Bots Recoil Multiplier", 1.0f,
                new ConfigDescription("Everything except scavs and pmcs. Only modifies Recoil/scatter. Higher Number equals easier and less accurate bots",
                new AcceptableValueRange<float>(0.5f, 3.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 2 }));

            AllGigaChads = Config.Bind(shoot, "All Bots are GigaChads", false,
                new ConfigDescription("WARNING not recommended to actually play with this. Bots will be too predictable. For the memes",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = -10 }));

            AllChads = Config.Bind(shoot, "All Bots are Chads", false,
                new ConfigDescription("WARNING not recommended to actually play with this. Bots will be too predictable. For the memes",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = -11 }));

            AllRats = Config.Bind(shoot, "All Bots are Rats", false,
                new ConfigDescription("WARNING not recommended to actually play with this. Bots will be too predictable. For the memes",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = -12 }));
        }
    }
}
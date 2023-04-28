using BepInEx.Configuration;

namespace SAIN_Grenades.Configs
{
    internal class GrenadeConfigs
    {
        public static ConfigEntry<float> BaseReactionTime { get; private set; }
        public static ConfigEntry<float> MaximumReactionTime { get; private set; }
        public static ConfigEntry<float> MinimumReactionTime { get; private set; }
        public static ConfigEntry<bool> DebugLogs { get; private set; }
        public static ConfigEntry<bool> DebugDrawTools { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string grenade = "AI Grenade Settings";

            BaseReactionTime = Config.Bind(grenade, "Base Reaction Time", 0.5f,
                new ConfigDescription("A Base Reaction Time for all bots once a grenade is actually visible. This number is modified by bot difficult and bot type.",
                new AcceptableValueRange<float>(0.01f, 1.5f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4 }));

            MaximumReactionTime = Config.Bind(grenade, "Max Reaction Time", 1.5f,
                new ConfigDescription("The Absolute MAXIMUM Time a bot will take to react to a grenade they have seen after all modifiers are applied",
                new AcceptableValueRange<float>(0.1f, 5.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));

            MinimumReactionTime = Config.Bind(grenade, "Minimum Reaction Time", 0.2f,
                new ConfigDescription("The Absolute MINIMUM Time a bot will take to react to a grenade they have seen after all modifiers are applied",
                new AcceptableValueRange<float>(0.01f, 1.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));

            DebugLogs = Config.Bind(grenade,
                "Debug Logs", false, 
                new ConfigDescription("Log Bot Reaction Times and Grenade Seen Status", 
                null, 
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));

            DebugDrawTools = Config.Bind(grenade,
                "Debug Draw Tools", false,
                new ConfigDescription("Draws lines and spheres to visualize bot reaction and seen status",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));
        }
    }
}
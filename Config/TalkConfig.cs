using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class TalkConfig
    {
        public static ConfigEntry<bool> NoTalkGlobal { get; private set; }
        public static ConfigEntry<bool> NoScavTalk { get; private set; }
        public static ConfigEntry<bool> NoPMCTalk { get; private set; }
        public static ConfigEntry<bool> NoBossTalk { get; private set; }
        public static ConfigEntry<bool> NoFollowerTalk { get; private set; }

        public static ConfigEntry<bool> BotTaunts { get; private set; }
        public static ConfigEntry<float> GlobalTalkLimit { get; private set; }
        public static ConfigEntry<float> TalkDelayModifier { get; private set; }
        public static ConfigEntry<bool> DebugLogs { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string debugmode = "Bot Talk Settings";


            NoTalkGlobal = Config.Bind(debugmode, "No Talk Global", false,
                new ConfigDescription("Disables bots talking across the board. Pls no turn on, I spent a lot of time remaking their talk behavior",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 5 }));

            NoScavTalk = Config.Bind(debugmode, "No Scav Talk", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4 }));

            NoPMCTalk = Config.Bind(debugmode, "No PMC Talk", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 3 }));

            NoBossTalk = Config.Bind(debugmode, "No Boss Talk", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 2 }));

            NoFollowerTalk = Config.Bind(debugmode, "No Follower Talk", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1 }));

            ////

            BotTaunts = Config.Bind(debugmode, "Bot Taunts", true,
                new ConfigDescription("Bots have a percentage chance to yell taunts depending on their personality type.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 0 }));

            GlobalTalkLimit = Config.Bind(debugmode, "Global Talk Limit", 1.0f,
                new ConfigDescription("Global Value. The hard limit on how often any voiceline can be said by a single bot. Recommend leaving this alone and adjusting the modifier instead.",
                new AcceptableValueRange<float>(0.01f, 5.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = -1 }));

            TalkDelayModifier = Config.Bind(debugmode, "Talk Delay Modifier", 1.0f,
                new ConfigDescription("Multiplies individual voiceline delay timers to increase delay for specific voicelines.",
                new AcceptableValueRange<float>(0.25f, 5.0f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = -2 }));

            ////
            ///
            DebugLogs = Config.Bind(debugmode, "Debug Logs", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = -999 }));
        }
    }
}
using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class EditorSettings
    {
        // General
        public static ConfigEntry<bool> NoBushESPToggle { get; set; }
        public static ConfigEntry<bool> HeadShotProtection { get; set; }
        public static ConfigEntry<bool> FasterCQBReactions { get; private set; }
        public static ConfigEntry<float> AccuracyMulti { get; set; }

        // Vision
        public static ConfigEntry<float> VisionSpeed { get; set; }
        public static ConfigEntry<float> CloseVisionSpeed { get; set; }
        public static ConfigEntry<float> FarVisionSpeed { get; set; }
        public static ConfigEntry<float> CloseFarThresh { get; set; }

        // Recoil
        public static ConfigEntry<float> BotRecoilGlobal { get; set; }
        public static ConfigEntry<float> PMCRecoil { get; set; }
        public static ConfigEntry<float> ScavRecoil { get; set; }
        public static ConfigEntry<float> OtherRecoil { get; set; }

        // Advanced Recoil
        public static ConfigEntry<float> MaxScatter { get; set; }
        public static ConfigEntry<float> AddRecoil { get; set; }
        public static ConfigEntry<float> RecoilDecay { get; set; }

        // Shoot
        public static ConfigEntry<float> BurstMulti { get; set; }
        public static ConfigEntry<float> FireratMulti { get; set; }

        // Personality
        public static ConfigEntry<bool> AllChads { get; set; }
        public static ConfigEntry<bool> AllGigaChads { get; set; }
        public static ConfigEntry<bool> AllRats { get; set; }

        public static ConfigEntry<int> RandomGigaChadChance { get; set; }
        public static ConfigEntry<int> RandomChadChance { get; set; }
        public static ConfigEntry<int> RandomRatChance { get; set; }
        public static ConfigEntry<int> RandomCowardChance { get; set; }

        public static void Init(ConfigFile config)
        {
            Config = config;
            // General
            NoBushESPToggle = BindBool(nameof(NoBushESPToggle));
            HeadShotProtection = BindBool(nameof(HeadShotProtection));
            FasterCQBReactions = BindBool(nameof(FasterCQBReactions));
            AccuracyMulti = BindFloat(nameof(AccuracyMulti));

            // Recoil
            BotRecoilGlobal = BindFloat(nameof(BotRecoilGlobal));
            PMCRecoil = BindFloat(nameof(PMCRecoil));
            ScavRecoil = BindFloat(nameof(ScavRecoil));
            OtherRecoil = BindFloat(nameof(OtherRecoil));

            // Advanced Recoil
            MaxScatter = BindFloat(nameof(MaxScatter), null, 2f);
            AddRecoil = BindFloat(nameof(AddRecoil), null, 0f);
            RecoilDecay = BindFloat(nameof(RecoilDecay), null, 0.85f);

            // Vision
            VisionSpeed = BindFloat(nameof(VisionSpeed));
            CloseVisionSpeed = BindFloat(nameof(CloseVisionSpeed));
            FarVisionSpeed = BindFloat(nameof(FarVisionSpeed));
            CloseFarThresh = BindFloat(nameof(CloseFarThresh), null, 50f);

            // Shoot
            BurstMulti = BindFloat(nameof(BurstMulti));
            FireratMulti = BindFloat(nameof(FireratMulti));

            // Personality
            AllChads = BindBool(nameof(AllChads), null, false);
            AllGigaChads = BindBool(nameof(AllGigaChads), null, false);
            AllRats = BindBool(nameof(AllRats), null, false);

            RandomGigaChadChance = BindInt(nameof(RandomGigaChadChance), null, 3);
            RandomChadChance = BindInt(nameof(RandomChadChance), null, 3);
            RandomRatChance = BindInt(nameof(RandomRatChance), null, 30);
            RandomCowardChance = BindInt(nameof(RandomCowardChance), null, 30);

            // NOTE: This is to resolve an issue where previous versions could result in "All*" all being true
            //       If more than 1 "All" option is true, disable them all
            int allPersonalityValuesEnabled = 0;
            allPersonalityValuesEnabled += AllChads.Value ? 1 : 0;
            allPersonalityValuesEnabled += AllGigaChads.Value ? 1 : 0;
            allPersonalityValuesEnabled += AllRats.Value ? 1 : 0;
            if (allPersonalityValuesEnabled > 1)
            {
                AllChads.Value = false;
                AllGigaChads.Value = false;
                AllRats.Value = false;
            }
        }

        public static ConfigEntry<float> BindFloat(string name, string section = null, float defaultValue = 1f, ConfigDescription description = null)
        {
            return Config.Bind(section ?? DefaultSection, name, defaultValue, description ?? DefaultDescription);
        }

        public static ConfigEntry<bool> BindBool(string name, string section = null, bool defaultValue = true, ConfigDescription description = null)
        {
            return Config.Bind(section ?? DefaultSection, name, defaultValue, description ?? DefaultDescription);
        }

        public static ConfigEntry<int> BindInt(string name, string section = null, int defaultValue = 0, ConfigDescription description = null)
        {
            return Config.Bind(section ?? DefaultSection, name, defaultValue, description ?? DefaultDescription);
        }

        public static ConfigurationManagerAttributes Attributes(bool browsable = true, bool advanced = false)
        {
            return new ConfigurationManagerAttributes { Browsable = browsable , IsAdvanced = advanced };
        }

        private static readonly ConfigDescription DefaultDescription = new ConfigDescription("", null, Attributes(false));
        private static readonly string DefaultSection = "Editor Settings";
        private static ConfigFile Config;
    }
}
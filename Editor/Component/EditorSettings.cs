using BepInEx.Configuration;
using SAIN.Plugin.Config;
using static SAIN.Editor.BindConfigs;

namespace SAIN.Editor
{
    internal class EditorSettings : ConfigAbstract
    {
        // General
        public static ConfigEntry<bool> NoBushESPToggle { get; set; }
        public static ConfigEntry<bool> HeadShotProtection { get; set; }

        // Advanced
        public static ConfigEntry<float> MaxRecoil { get; set; }
        public static ConfigEntry<float> AddRecoil { get; set; }
        public static ConfigEntry<float> RecoilDecay { get; set; }

        // Personality
        public static ConfigEntry<bool> AllChads { get; set; }
        public static ConfigEntry<bool> AllGigaChads { get; set; }
        public static ConfigEntry<bool> AllRats { get; set; }

        public static ConfigEntry<float> RandomGigaChadChance { get; set; }
        public static ConfigEntry<float> RandomChadChance { get; set; }
        public static ConfigEntry<float> RandomRatChance { get; set; }
        public static ConfigEntry<float> RandomCowardChance { get; set; }

        // Hearing
        public static ConfigEntry<float> SuppressorModifier { get; set; }
        public static ConfigEntry<float> SubsonicModifier { get; set; }

        public static ConfigEntry<float> AudioRangePistol { get; set; }
        public static ConfigEntry<float> AudioRangeRifle { get; set; }
        public static ConfigEntry<float> AudioRangeMidRifle { get; set; }
        public static ConfigEntry<float> AudioRangeLargeCal { get; set; }
        public static ConfigEntry<float> AudioRangeShotgun { get; set; }
        public static ConfigEntry<float> AudioRangeOther { get; set; }

        public static void Init()
        {
            EditorParameters.Init();

            string section = "Settings";

            string name = "No Bush ESP";
            string description = "Adds extra vision check for bots to help prevent bots seeing or shooting through foliage.";

            NoBushESPToggle = SAINConfig.Bind(section, name, true,
                new ConfigDescription(description,
                null,
                new ConfigurationManagerAttributes { Browsable = false }));

            name = "HeadShot Protection";
            description = "Experimental, will kick bot's aiming target if it ends up on the player's head.";

            HeadShotProtection = SAINConfig.Bind(section, name, true,
                new ConfigDescription(description,
                null,
                new ConfigurationManagerAttributes { Browsable = false }));


            // Audio
            name = "Suppressed Sound Modifier";
            description = "Audible Gun Range is multiplied by this number when using a suppressor";
            SuppressorModifier = SAINConfig.Bind(section, name, 0.6f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(0.05f, 1f),
                new ConfigurationManagerAttributes { Browsable = false }));

            name = "Suppressed Subsonic Sound Modifier";
            description = "Audible Gun Range is multiplied by this number when using a suppressor and subsonic ammo";
            SubsonicModifier = SAINConfig.Bind(section, name, 0.6f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(0.05f, 1f),
                new ConfigurationManagerAttributes { Browsable = false }));


            name = "Pistol Calibers";
            description = "How far bots can hear specific ammo calibers. In Meters";
            AudioRangePistol = SAINConfig.Bind(section, name, 135f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(50, 500),
                new ConfigurationManagerAttributes { Browsable = false }));

            name = "556 and 545 Rifles";
            AudioRangeRifle = SAINConfig.Bind(section, name, 180f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(50, 500),
                new ConfigurationManagerAttributes { Browsable = false }));

            name = "762 Rifles";
            AudioRangeMidRifle = SAINConfig.Bind(section, name, 215f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(50, 500),
                new ConfigurationManagerAttributes { Browsable = false }));

            name = "338";
            AudioRangeLargeCal = SAINConfig.Bind(section, name, 325f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(50, 500),
                new ConfigurationManagerAttributes { Browsable = false }));

            name = "Shotguns";
            AudioRangeShotgun = SAINConfig.Bind(section, name, 225f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(50, 500),
                new ConfigurationManagerAttributes { Browsable = false }));

            name = "GrenadeLaunchers and Other";
            AudioRangeOther = SAINConfig.Bind(section, name, 75f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(20, 500),
                new ConfigurationManagerAttributes { Browsable = false }));

            // Advanced Recoil
            MaxRecoil = SAINConfig.Bind(section, name, 2f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(0.1f, 10f),
                new ConfigurationManagerAttributes { Browsable = false }));

            AddRecoil = SAINConfig.Bind(section, name, 0f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(-5f, 5f),
                new ConfigurationManagerAttributes { Browsable = false }));

            RecoilDecay = SAINConfig.Bind(section, name, 0.8f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(0.1f, 0.99f),
                new ConfigurationManagerAttributes { Browsable = false }));

            description = "Force All Bots To This Personality";

            // Personality
            AllChads = SAINConfig.Bind(section, "All Chads", true,
                new ConfigDescription(description,
                null,
                new ConfigurationManagerAttributes { Browsable = false }));

            AllGigaChads = SAINConfig.Bind(section, "All GigaChads", true,
                new ConfigDescription(description,
                null,
                new ConfigurationManagerAttributes { Browsable = false }));

            AllRats = SAINConfig.Bind(section, "All Rats", true,
                new ConfigDescription(description,
                null,
                new ConfigurationManagerAttributes { Browsable = false }));

            description = "The Chance that any random bot will get assigned this personality, regardless of Gear and Level";

            name = "Random Coward Chance";
            RandomGigaChadChance = SAINConfig.Bind(section, name, 3f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(0f, 100f),
                new ConfigurationManagerAttributes { Browsable = false }));

            name = "Random Chad Chance";
            RandomChadChance = SAINConfig.Bind(section, name, 3f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(0f, 100f),
                new ConfigurationManagerAttributes { Browsable = false }));

            name = "Random Rat Chance";
            RandomCowardChance = SAINConfig.Bind(section, name, 30f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(0f, 100f),
                new ConfigurationManagerAttributes { Browsable = false }));

            name = "Random Coward Chance";
            RandomRatChance = SAINConfig.Bind(section, name, 30f,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(0f, 100f),
                new ConfigurationManagerAttributes { Browsable = false }));

        }
    }

    internal class EditorParameters : ConfigAbstract
    {
        public static ConfigEntry<float> InfoIconWidth { get; set; }
        public static ConfigEntry<float> SliderLabelWidth { get; set; }
        public static ConfigEntry<float> SliderSpaceWidth { get; set; }
        public static ConfigEntry<float> SliderMinMaxWidth { get; set; }
        public static ConfigEntry<float> SliderWidth { get; set; }
        public static ConfigEntry<float> SliderResultWidth { get; set; }

        public static void Init()
        {
            InfoIconWidth = Bind(nameof(InfoIconWidth), 15f, Layout);
            SliderLabelWidth = Bind(nameof(SliderLabelWidth), 220f, Layout);
            SliderSpaceWidth = Bind(nameof(SliderSpaceWidth), 10f, Layout);
            SliderMinMaxWidth = Bind(nameof(SliderMinMaxWidth), 45f, Layout);
            SliderWidth = Bind(nameof(SliderWidth), 275f, Layout);
            SliderResultWidth = Bind(nameof(SliderResultWidth), 55f, Layout);
        }

        private static readonly string Layout = "GUILayout. DO NOT TOUCH";
    }

    internal class BindConfigs : ConfigAbstract
    {
        public static ConfigEntry<T> Bind<T>(string name, T defaultValue, string section = null)
        {
            return SAINConfig.Bind(section ?? DefaultSection, name, defaultValue, DefaultDescription);
        }

        private static readonly ConfigDescription DefaultDescription = new ConfigDescription("", null, Attributes(false));
        private static readonly string DefaultSection = "Editor Settings";

        public static ConfigurationManagerAttributes Attributes(bool browsable = true, bool advanced = false, int? order = null)
        {
            var attributes = new ConfigurationManagerAttributes { Browsable = browsable, IsAdvanced = advanced };
            if (order != null)
            {
                attributes.Order = order;
            }
            return attributes;
        }
    }
}
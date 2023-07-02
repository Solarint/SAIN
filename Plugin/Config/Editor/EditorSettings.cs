using BepInEx.Configuration;
using EFT.UI;
using System;
using UnityEngine;

namespace SAIN.Editor
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
        public static ConfigEntry<float> TestDistance { get; set; }

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
            EditorParameters.Init(config);

            ConsoleScreen.Processor.RegisterCommand("saineditor", new Action(EditorGUI.OpenPanel));

            EditorGUI.TogglePanel = config.Bind(
                "SAIN Settings Editor",
                "",
                new KeyboardShortcut(KeyCode.F5),
                "The keyboard shortcut that toggles editor");

            // General
            NoBushESPToggle = BindConfigs.BindBool(config, nameof(NoBushESPToggle));
            HeadShotProtection = BindConfigs.BindBool(config, nameof(HeadShotProtection));
            FasterCQBReactions = BindConfigs.BindBool(config, nameof(FasterCQBReactions));
            AccuracyMulti = BindConfigs.BindFloat(config, nameof(AccuracyMulti));

            // Recoil
            BotRecoilGlobal = BindConfigs.BindFloat(config, nameof(BotRecoilGlobal));
            PMCRecoil = BindConfigs.BindFloat(config, nameof(PMCRecoil));
            ScavRecoil = BindConfigs.BindFloat(config, nameof(ScavRecoil));
            OtherRecoil = BindConfigs.BindFloat(config, nameof(OtherRecoil));

            // Advanced Recoil
            MaxScatter = BindConfigs.BindFloat(config, nameof(MaxScatter), null, 2f);
            AddRecoil = BindConfigs.BindFloat(config, nameof(AddRecoil), null, 0f);
            RecoilDecay = BindConfigs.BindFloat(config, nameof(RecoilDecay), null, 0.85f);

            // Vision
            VisionSpeed = BindConfigs.BindFloat(config, nameof(VisionSpeed));
            CloseVisionSpeed = BindConfigs.BindFloat(config, nameof(CloseVisionSpeed));
            FarVisionSpeed = BindConfigs.BindFloat(config, nameof(FarVisionSpeed));
            CloseFarThresh = BindConfigs.BindFloat(config, nameof(CloseFarThresh), null, 50f);
            TestDistance = BindConfigs.BindFloat(config, nameof(TestDistance), null, 50f);

            // Shoot
            BurstMulti = BindConfigs.BindFloat(config, nameof(BurstMulti));
            FireratMulti = BindConfigs.BindFloat(config, nameof(FireratMulti));

            // Personality
            AllChads = BindConfigs.BindBool(config, nameof(AllChads));
            AllGigaChads = BindConfigs.BindBool(config, nameof(AllGigaChads));
            AllRats = BindConfigs.BindBool(config, nameof(AllRats));

            RandomGigaChadChance = BindConfigs.BindInt(config, nameof(RandomGigaChadChance), null, 3);
            RandomChadChance = BindConfigs.BindInt(config, nameof(RandomChadChance), null, 3);
            RandomRatChance = BindConfigs.BindInt(config, nameof(RandomRatChance), null, 30);
            RandomCowardChance = BindConfigs.BindInt(config, nameof(RandomCowardChance), null, 30);
        }
    }

    internal class EditorParameters
    {
        public static ConfigEntry<float> SliderLabelWidth { get; set; }
        public static ConfigEntry<float> SliderSpaceWidth { get; set; }
        public static ConfigEntry<float> SliderMinMaxWidth { get; set; }
        public static ConfigEntry<float> SliderWidth { get; set; }
        public static ConfigEntry<float> SliderResultWidth { get; set; }


        public static void Init(ConfigFile config)
        {
            SliderLabelWidth = BindConfigs.BindFloat(config, nameof(SliderLabelWidth), Layout, 200f);
            SliderSpaceWidth = BindConfigs.BindFloat(config, nameof(SliderSpaceWidth), Layout, 22f);
            SliderMinMaxWidth = BindConfigs.BindFloat(config, nameof(SliderMinMaxWidth), Layout, 30f);
            SliderWidth = BindConfigs.BindFloat(config, nameof(SliderWidth), Layout, 200f);
            SliderResultWidth = BindConfigs.BindFloat(config, nameof(SliderResultWidth), Layout, 40f);
        }

        private static readonly string Layout = "GUILayout. DO NOT TOUCH";
    }

    internal class BindConfigs
    {
        public static ConfigEntry<float> BindFloat(ConfigFile Config, string name, string section = null, float defaultValue = 1f, ConfigDescription description = null)
        {
            return Config.Bind(section ?? DefaultSection, name, defaultValue, description ?? DefaultDescription);
        }

        public static ConfigEntry<bool> BindBool(ConfigFile Config, string name, string section = null, bool defaultValue = true, ConfigDescription description = null)
        {
            return Config.Bind(section ?? DefaultSection, name, defaultValue, description ?? DefaultDescription);
        }

        public static ConfigEntry<int> BindInt(ConfigFile Config, string name, string section = null, int defaultValue = 0, ConfigDescription description = null)
        {
            return Config.Bind(section ?? DefaultSection, name, defaultValue, description ?? DefaultDescription);
        }

        private static readonly ConfigDescription DefaultDescription = new ConfigDescription("", null, Attributes(false));
        private static readonly string DefaultSection = "Editor Settings";

        public static ConfigurationManagerAttributes Attributes(bool browsable = true, bool advanced = false)
        {
            return new ConfigurationManagerAttributes { Browsable = browsable, IsAdvanced = advanced };
        }
    }
}
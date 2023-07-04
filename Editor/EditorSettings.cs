using BepInEx.Configuration;
using EFT.UI;
using SAIN.Plugin.Config;
using System;
using UnityEngine;
using static SAIN.Editor.BindConfigs;

namespace SAIN.Editor
{
    internal class EditorSettings : ConfigAbstract
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
        // Advanced
        public static ConfigEntry<float> MaxRecoil { get; set; }
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
        // Talk
        public static ConfigEntry<bool> TalkGlobal { get; set; }
        public static ConfigEntry<float> TalkGlobalFreq { get; set; }
        public static ConfigEntry<float> GlobalTalkLimit { get; set; }
        public static ConfigEntry<bool> BotTaunts { get; set; }

        public static ConfigEntry<bool> ScavTalk { get; set; }
        public static ConfigEntry<float> ScavTalkFreq { get; set; }

        public static ConfigEntry<bool> PMCTalk { get; set; }
        public static ConfigEntry<float> PMCTalkFreq { get; set; }

        public static ConfigEntry<bool> BossTalk { get; set; }
        public static ConfigEntry<float> BossTalkFreq { get; set; }

        public static ConfigEntry<bool> FollowerTalk { get; set; }
        public static ConfigEntry<float> FollowerTalkFreq { get; set; }

        public static ConfigEntry<bool> OtherTalk { get; set; }
        public static ConfigEntry<float> OtherTalkFreq { get; set; }

        public static ConfigEntry<bool> SquadTalk { get; set; }
        public static ConfigEntry<float> SquadMemberTalkFreq { get; set; }
        public static ConfigEntry<float> SquadLeadTalkFreq { get; set; }

        // Extracts
        public static ConfigEntry<float> MaxPercentage { get; set; }
        public static ConfigEntry<float> MinPercentage { get; set; }
        public static ConfigEntry<bool> EnableExtracts { get; set; }

        // Hearing
        public static ConfigEntry<float> SuppressorModifier { get; set; }
        public static ConfigEntry<float> SubsonicModifier { get; set; }

        public static ConfigEntry<float> AudioRangePistol { get; set; }
        public static ConfigEntry<float> AudioRangeRifle { get; set; }
        public static ConfigEntry<float> AudioRangeMidRifle { get; set; }
        public static ConfigEntry<float> AudioRangeLargeCal { get; set; }
        public static ConfigEntry<float> AudioRangeShotgun { get; set; }

        public static void Init()
        {
            EditorParameters.Init();

            ConsoleScreen.Processor.RegisterCommand("saineditor", new Action(EditorGUI.OpenPanel));

            EditorGUI.TogglePanel = SAINConfig.Bind(
                "SAIN Settings Editor",
                "",
                new KeyboardShortcut(KeyCode.F5),
                "The keyboard shortcut that toggles editor");

            // General
            NoBushESPToggle = Bind(nameof(NoBushESPToggle), true);
            HeadShotProtection = Bind(nameof(HeadShotProtection), true);
            FasterCQBReactions = Bind(nameof(FasterCQBReactions), true);
            AccuracyMulti = Bind(nameof(AccuracyMulti), 1f);

            // Extracts
            EnableExtracts = Bind(nameof(EnableExtracts), true);
            MaxPercentage = Bind(nameof(MaxPercentage), 35f);
            MinPercentage = Bind(nameof(MinPercentage), 5f);

            // Audio
            SuppressorModifier = Bind(nameof(SuppressorModifier), 0.6f);
            SubsonicModifier = Bind(nameof(SubsonicModifier), 0.3f);

            AudioRangePistol = Bind(nameof(AudioRangePistol), 135f);
            AudioRangeRifle = Bind(nameof(AudioRangeRifle), 180f);
            AudioRangeMidRifle = Bind(nameof(AudioRangeMidRifle), 215f);
            AudioRangeLargeCal = Bind(nameof(AudioRangeLargeCal), 325f);
            AudioRangeShotgun = Bind(nameof(AudioRangeShotgun), 225f);

            // Recoil
            BotRecoilGlobal = Bind(nameof(BotRecoilGlobal), 1f);
            PMCRecoil = Bind(nameof(PMCRecoil), 1f);
            ScavRecoil = Bind(nameof(ScavRecoil), 1f);
            OtherRecoil = Bind(nameof(OtherRecoil), 1f);

            // Advanced Recoil
            MaxRecoil = Bind(nameof(MaxRecoil), 2f);
            AddRecoil = Bind(nameof(AddRecoil), 0f);
            RecoilDecay = Bind(nameof(RecoilDecay), 0.85f);

            // Vision
            VisionSpeed = Bind(nameof(VisionSpeed), 1f);
            CloseVisionSpeed = Bind(nameof(CloseVisionSpeed), 1f);
            FarVisionSpeed = Bind(nameof(FarVisionSpeed), 1f);
            CloseFarThresh = Bind(nameof(CloseFarThresh), 50f);
            TestDistance = Bind(nameof(TestDistance), 75f);

            // Shoot
            BurstMulti = Bind(nameof(BurstMulti), 1f);
            FireratMulti = Bind(nameof(FireratMulti), 1f);

            // Personality
            AllChads = Bind(nameof(AllChads), false);
            AllGigaChads = Bind(nameof(AllGigaChads), false);
            AllRats = Bind(nameof(AllRats), false);

            RandomGigaChadChance = Bind(nameof(RandomGigaChadChance), 3);
            RandomChadChance = Bind(nameof(RandomChadChance), 3);
            RandomCowardChance = Bind(nameof(RandomCowardChance), 30);
            RandomRatChance = Bind(nameof(RandomRatChance), 30);

            TalkGlobal = Bind(nameof(TalkGlobal), true);

            ScavTalk = Bind(nameof(ScavTalk), true);
            ScavTalkFreq = Bind(nameof(ScavTalkFreq), 1f);

            PMCTalk = Bind(nameof(PMCTalk), true);
            PMCTalkFreq = Bind(nameof(PMCTalkFreq), 1f);

            BossTalk = Bind(nameof(BossTalk), true);
            BossTalkFreq = Bind(nameof(BossTalkFreq), 1f);

            FollowerTalk = Bind(nameof(FollowerTalk), true);
            FollowerTalkFreq = Bind(nameof(FollowerTalkFreq), 1f);

            OtherTalk = Bind(nameof(OtherTalk), true);
            OtherTalkFreq = Bind(nameof(OtherTalkFreq), 1f);

            SquadTalk = Bind(nameof(SquadTalk), true);
            SquadMemberTalkFreq = Bind(nameof(SquadMemberTalkFreq), 1f);
            SquadLeadTalkFreq = Bind(nameof(SquadLeadTalkFreq), 1f);

            BotTaunts = Bind(nameof(BotTaunts), true);
            GlobalTalkLimit = Bind(nameof(GlobalTalkLimit), 3f);
            TalkGlobalFreq = Bind(nameof(TalkGlobalFreq), 1f);
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
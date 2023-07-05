using BepInEx.Configuration;
using Comfort.Common;
using EFT.Console.Core;
using EFT.UI;
using UnityEngine;
using static SAIN.Editor.Buttons;
using static SAIN.Editor.ConfigValues;
using static SAIN.Editor.EditorSettings;
using static SAIN.Editor.RectLayout;
using static SAIN.Editor.Sounds;
using static SAIN.Editor.Styles;
using static SAIN.Editor.ToolTips;
using static SAIN.Editor.UITextures;

namespace SAIN.Editor
{
    public class EditorGUI
    {
        public EditorGUI()
        {
            int i;
            for (i = 0; i < PresetEditor.BotTypeOptions.Length; i++)
            {
                if (PresetEditor.BotTypeOptions[i] == "assault")
                {
                    break;
                }
            }
            PresetEditor.SelectedType = i;

            for (i = 0; i < PresetEditor.BotDifficultyOptions.Length; i++)
            {
                if (PresetEditor.BotDifficultyOptions[i] == "normal")
                {
                    break;
                }
            }
            PresetEditor.SelectedDifficulty = i;
        }

        internal static ConfigEntry<KeyboardShortcut> TogglePanel;

        private static GameObject input;

        private static bool guiStatus = false;

        [ConsoleCommand("Open SAIN Difficulty Editor")]
        public static void OpenPanel()
        {
            if (input == null)
            {
                input = GameObject.Find("___Input");
            }

            guiStatus = !guiStatus;
            Cursor.visible = guiStatus;
            if (guiStatus)
            {
                CursorSettings.SetCursor(ECursorType.Idle);
                Cursor.lockState = CursorLockMode.None;
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuContextMenu);
            }
            else
            {
                CursorSettings.SetCursor(ECursorType.Invisible);
                Cursor.lockState = CursorLockMode.Locked;
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuDropdown);
            }
            input.SetActive(!guiStatus);
            ConsoleScreen.Log("[SAIN]: Opening Editor...");
        }

        private bool CloseEditor;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(TogglePanel.Value.MainKey) || CloseEditor)
            {
                SelectedTab = 0;
                CloseEditor = false;
                //Caching input manager GameObject which script is responsible for reading the player inputs
                if (input == null)
                {
                    input = GameObject.Find("___Input");
                }

                guiStatus = !guiStatus;
                Cursor.visible = guiStatus;
                if (guiStatus)
                {
                    //Changing the default windows cursor to an EFT-style one and playing a sound when the menu appears
                    CursorSettings.SetCursor(ECursorType.Idle);
                    Cursor.lockState = CursorLockMode.None;
                    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuContextMenu);
                }
                else
                {
                    //Hiding cursor and playing a sound when the menu disappears
                    CursorSettings.SetCursor(ECursorType.Invisible);
                    Cursor.lockState = CursorLockMode.Locked;
                    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuDropdown);
                }
                //Disabling the input manager so the player won't move
                input.SetActive(!guiStatus);
            }
        }

        public void OnGUI()
        {
            if (guiStatus)
            {
                InitStyles();
                DrawShadow();
                MainWindow = GUI.Window(0, MainWindow, MainWindowFunc, "SAIN AI Settings Editor", WindowStyle);
            }
        }

        public static void CreateToolbar()
        {
            GUILayout.BeginArea(ToolBarRectangle);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            float optionWidth = ToolBarRectangle.width / 4f;

            ToolBarButton("Home", HomeTab, optionWidth);
            ToolBarButton("Vision", VisionTab, optionWidth);
            ToolBarButton("Shoot", ShootTab, optionWidth);
            ToolBarButton("Extracts", ExtractsTab, optionWidth);

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            ToolBarButton("Hearing", HearingTab, optionWidth);
            ToolBarButton("Personalties", PersonalityTab, optionWidth);
            ToolBarButton("Talk", TalkTab, optionWidth);
            ToolBarButton("Advanced", AdvancedTab, optionWidth);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private static void ToolBarButton(string name, int tab, float width)
        {
            SelectedTab = Button(name, SelectedTab, tab, width);
        }

        private void MainWindowFunc(int TWCWindowID)
        {
            DrawWindowBackground(SelectedTab);
            GUI.DragWindow(DragRectangle);
            if (GUI.Button(ExitButton, "X"))
            {
                ResetClickSound();
                CloseEditor = true;
            }

            CreateToolbar();

            string name;
            string description;
            if (SelectedTab == HomeTab)
            {
                GUILayout.BeginArea(TabRectangle); GUILayout.BeginVertical();
                if (ExpandGeneral = BuilderUtil.ExpandableMenu("General Settings", ExpandGeneral))
                {
                    name = "No Bush ESP";
                    description = "Adds extra vision check for bots to help prevent bots seeing or shooting through foliage.";
                    BuilderUtil.CreateButtonOption(NoBushESPToggle, name, description);

                    name = "HeadShot Protection";
                    description = "Experimental, will kick bot's aiming target if it ends up on the player's head.";
                    BuilderUtil.CreateButtonOption(NoBushESPToggle, name, description);

                    name = "Faster CQB Reactions";
                    description = "Bots will react faster to enemies closer than 30 meters away.";
                    BuilderUtil.CreateButtonOption(NoBushESPToggle, name, description);

                    GUILayout.Box("Mod Detection");
                    GUILayout.BeginHorizontal();
                    SingleTextBool("Looting Bots", SAINPlugin.LootingBotsLoaded);
                    SingleTextBool("Realism Mod", SAINPlugin.RealismLoaded);
                    GUILayout.EndHorizontal();
                }

                PresetEditor.PresetSelectionMenu();

                GUILayout.EndVertical(); GUILayout.EndArea();
            }
            else if (SelectedTab == VisionTab)
            {
                GUILayout.BeginArea(TabRectangle); GUILayout.BeginVertical();

                if (ExpandVisionSpeed = BuilderUtil.ExpandableMenu("Vision Speed", ExpandVisionSpeed, "1.5 = 1.5x faster vision speed. Result will vary between bot types."))
                {
                    name = "Base Speed";
                    description = "The Base vision speed multiplier. Bots will see this much faster, or slower, at any range.";
                    BuilderUtil.HorizSlider(name, VisionSpeed, 0.25f, 3f, 100f, description);

                    name = "Close Speed";
                    description = "Vision speed multiplier at close range. Bots will see this much faster, or slower, at close range.";
                    BuilderUtil.HorizSlider(name, CloseVisionSpeed, 0.25f, 3f, 100f, description);

                    name = "Far Speed";
                    description = "Vision speed multiplier at Far range. Bots will see this much faster, or slower, at Far range.";
                    BuilderUtil.HorizSlider(name, FarVisionSpeed, 0.25f, 3f, 100f, description);

                    name = "Close/Far Threshold";
                    description = "The Distance that defines what is Close Or Far for the above options.";
                    BuilderUtil.HorizSlider(name, CloseFarThresh, 10f, 150f, 1f, description);

                    GUILayout.Box("Vision Speed Test");
                    BuilderUtil.HorizSlider("Test Distance", TestDistance, 0f, 500f, 1f, "How much faster or slower bot vision will be. In Meters.");
                    GUILayout.Box("Test Result Final Vision Speed Multiplier = " + Patches.Math.CalcVisSpeed(TestDistance.Value) + " at " + TestDistance.Value + " meters away from the player");
                }

                GUILayout.EndVertical(); GUILayout.EndArea();
            }
            else if (SelectedTab == ShootTab)
            {
                GUILayout.BeginArea(TabRectangle); GUILayout.BeginVertical();

                if (ExpandShoot = BuilderUtil.ExpandableMenu("Bot Shoot Settings", ExpandShoot))
                {
                    description = "1.5 = 1.5x Faster Firerate per second";
                    BuilderUtil.HorizSlider("Semiauto Firerate Multiplier", FireratMulti, 0.25f, 3f, 10f, description);
                    description = "1.5 = 1.5x Longer Bursts when firing full auto";
                    BuilderUtil.HorizSlider("Fullauto Burst Multiplier", BurstMulti, 0.25f, 3f, 10f, description);
                    description = "1.5 = 1.5x Worse Accuracy";
                    BuilderUtil.HorizSlider("Bot Accuracy Multiplier", AccuracyMulti, 0.25f, 3f, 10f, description);
                }
                if (ExpandRecoil = BuilderUtil.ExpandableMenu("Bot Recoil Settings", ExpandRecoil))
                {
                    description = "1.5 = 1.5x more recoil and scatter per shot";
                    BuilderUtil.HorizSlider("All Bots", BotRecoilGlobal, 0.25f, 3f, 10f, description);
                    BuilderUtil.HorizSlider("PMCs", PMCRecoil, 0.25f, 3f, 10f, description);
                    BuilderUtil.HorizSlider("Scavs", ScavRecoil, 0.25f, 3f, 10f, description);
                    BuilderUtil.HorizSlider("Other Bots", OtherRecoil, 0.25f, 3f, 10f, description);
                }

                GUILayout.EndVertical(); GUILayout.EndArea();
            }
            else if (SelectedTab == PersonalityTab)
            {
                GUILayout.BeginArea(TabRectangle); GUILayout.BeginVertical();
                GUILayout.Box("Personality Settings");
                GUILayout.Box("For The Memes. Recommended not to use these during normal gameplay!");
                GUILayout.Box("Bots will be more predictable and exploitable.");
                if (ExpandForcedPers = BuilderUtil.ExpandableMenu("Force Bot Personalities", ExpandForcedPers))
                {
                    if (BuilderUtil.CreateButtonOption(AllGigaChads, "All GigaChads"))
                    {
                        AllChads.Value = false;
                        AllRats.Value = false;
                    }
                    if (BuilderUtil.CreateButtonOption(AllChads, "All Chads"))
                    {
                        AllGigaChads.Value = false;
                        AllRats.Value = false;
                    }
                    if (BuilderUtil.CreateButtonOption(AllRats, "All Rats"))
                    {
                        AllGigaChads.Value = false;
                        AllChads.Value = false;
                    }
                }
                if (ExpandPersChances = BuilderUtil.ExpandableMenu("Set Personality Chances", ExpandPersChances))
                {
                    description = "The Chance that any random bot will get assigned this personality, regardless of Gear and Level";

                    name = "Random GigaChad Chance";
                    BuilderUtil.HorizSlider(name, RandomGigaChadChance, 0f, 100f, 1, description);

                    name = "Random Chad Chance";
                    BuilderUtil.HorizSlider(name, RandomChadChance, 0f, 100f, 1, description);

                    name = "Random Rat Chance";
                    BuilderUtil.HorizSlider(name, RandomRatChance, 0f, 100f, 1, description);

                    name = "Random Coward Chance";
                    BuilderUtil.HorizSlider(name, RandomCowardChance, 0f, 100f, 1, description);
                }

                GUILayout.EndVertical(); GUILayout.EndArea();
            }
            else if (SelectedTab == TalkTab)
            {
                GUILayout.BeginArea(TabRectangle); GUILayout.BeginVertical();
                if (ExpandTalkSettings = BuilderUtil.ExpandableMenu("Global Talk Settings", ExpandTalkSettings))
                {
                    BuilderUtil.CreateButtonOption(TalkGlobal, "All Bots Can Talk");

                    name = "Global Talk Limit";
                    description = "The Absolute Minimum Time between any phrases that a bot says.";
                    BuilderUtil.HorizSlider(name, GlobalTalkLimit, 1f, 10f, 100f, description);

                    name = "Talk Delay Modifier";
                    description = "Multiplies the time delay that a bot can say specific phrases.";
                    BuilderUtil.HorizSlider(name, TalkGlobalFreq, 0.25f, 5f, 100f, description);

                    BuilderUtil.CreateButtonOption(BotTaunts, "Taunting");
                }
                description = "Multiplies the time delay that a bot can say specific phrases.";
                if (ExpandTalkSquad = BuilderUtil.ExpandableMenu("Squads", ExpandTalkSquad))
                {
                    BuilderUtil.CreateButtonOption(SquadTalk, "Squad Talking");
                    BuilderUtil.HorizSlider("Squad Member Talk Frequency", SquadMemberTalkFreq, 0.25f, 5f, 100f, description);
                    BuilderUtil.HorizSlider("Squad Leader Talk Frequency", SquadLeadTalkFreq, 0.25f, 5f, 100f, description);
                }
                if (ExpandTalkBotType = BuilderUtil.ExpandableMenu("Bot Types", ExpandTalkBotType))
                {
                    string cantalk = "Can Talk";
                    string talkfreqname = "Talk Frequency Multiplier";
                    description = "Multiplies the time delay that a bot can say specific phrases.";
                    if (ExpandTalkPMC = BuilderUtil.ExpandableMenu("PMCs", ExpandTalkPMC, null, 25f))
                    {
                        GUILayout.ExpandHeight(true);
                        BuilderUtil.CreateButtonOption(PMCTalk, cantalk);
                        BuilderUtil.HorizSlider(talkfreqname, PMCTalkFreq, 0.25f, 5f, 100f, description);
                    }
                    if (ExpandTalkScav = BuilderUtil.ExpandableMenu("Scavs", ExpandTalkScav, null, 25f))
                    {
                        GUILayout.ExpandHeight(true);
                        BuilderUtil.CreateButtonOption(ScavTalk, cantalk);
                        BuilderUtil.HorizSlider(talkfreqname, ScavTalkFreq, 0.25f, 5f, 100f, description);
                    }
                    if (ExpandTalkBosses = BuilderUtil.ExpandableMenu("Bosses", ExpandTalkBosses, null, 25f))
                    {
                        GUILayout.ExpandHeight(true);
                        BuilderUtil.CreateButtonOption(BossTalk, cantalk);
                        BuilderUtil.HorizSlider(talkfreqname, BossTalkFreq, 0.25f, 5f, 100f, description);
                    }
                    if (ExpandTalkFollowers = BuilderUtil.ExpandableMenu("Followers", ExpandTalkFollowers, null, 25f))
                    {
                        GUILayout.ExpandHeight(true);
                        BuilderUtil.CreateButtonOption(FollowerTalk, cantalk);
                        BuilderUtil.HorizSlider(talkfreqname, FollowerTalkFreq, 0.25f, 5f, 100f, description);
                    }
                    if (ExpandTalkOther = BuilderUtil.ExpandableMenu("Other", ExpandTalkOther, null, 25f))
                    {
                        GUILayout.ExpandHeight(true);
                        BuilderUtil.CreateButtonOption(OtherTalk, cantalk);
                        BuilderUtil.HorizSlider(talkfreqname, OtherTalkFreq, 0.25f, 5f, 100f, description);
                    }
                }
                GUILayout.EndVertical(); GUILayout.EndArea();
            }
            else if (SelectedTab == ExtractsTab)
            {
                GUILayout.BeginArea(TabRectangle); GUILayout.BeginVertical();
                if (ExpandExtracts = BuilderUtil.ExpandableMenu("Extract Settings", ExpandExtracts))
                {
                    BuilderUtil.CreateButtonOption(EnableExtracts, "Bot Extracts");

                    description = "The Percentage of the raid remaining before All bots will begin moving to extract. (How much time has passed divided by the total raid time times 100)";
                    BuilderUtil.HorizSlider("Raid Time Min Percentage", MinPercentage, 0.1f, 99.9f, 100f, description);

                    description = "The Percentage of the raid remaining before some bots begin deciding to move to extract. (How much time has passed divided by the total raid time times 100)";
                    BuilderUtil.HorizSlider("Raid Time Max Percentage", MaxPercentage, 0.1f, 99.9f, 100f, description);
                }
                GUILayout.EndVertical(); GUILayout.EndArea();
            }
            else if (SelectedTab == HearingTab)
            {
                GUILayout.BeginArea(TabRectangle); GUILayout.BeginVertical();
                if (ExpandAudioSuppressor = BuilderUtil.ExpandableMenu("Suppressor Settings", ExpandAudioSuppressor))
                {
                    description = "Audible Gun Range is multiplied by this number when using a suppressor";
                    BuilderUtil.HorizSlider("Suppressor Modifier", SuppressorModifier, 0.1f, 0.75f, 100f, description);

                    description = "Audible Gun Range is multiplied by this number when using a suppressor and subsonic ammo";
                    BuilderUtil.HorizSlider("Subsonic Ammo Modifier", SubsonicModifier, 0.1f, 0.75f, 100f, description);
                }
                if (ExpandAudioRanges = BuilderUtil.ExpandableMenu("Weapon Audible Ranges", ExpandAudioRanges))
                {
                    description = "How far bots can hear specific ammo calibers. In Meters";
                    BuilderUtil.HorizSlider("Pistol Caliber", AudioRangePistol, 75f, 500f, 1f, description);
                    BuilderUtil.HorizSlider("556 or 545 Caliber", AudioRangeRifle, 75f, 500f, 1f, description);
                    BuilderUtil.HorizSlider("762 Caliber", AudioRangeMidRifle, 75f, 500f, 1f, description);
                    BuilderUtil.HorizSlider("Large Caliber", AudioRangeLargeCal, 75f, 500f, 1f, description);
                    BuilderUtil.HorizSlider("Shotguns", AudioRangeShotgun, 75f, 500f, 1f, description);
                }
                GUILayout.EndVertical(); GUILayout.EndArea();
            }
            else if (SelectedTab == AdvancedTab)
            {
                GUILayout.BeginArea(TabRectangle); GUILayout.BeginVertical();

                GUILayout.Box("Advanced Settings. Edit at your own risk.");

                if (ExpandRecoilAdvanced = BuilderUtil.ExpandableMenu("Advanced Recoil", ExpandRecoilAdvanced))
                {
                    name = "Max Recoil";
                    description = "The Max Recoil that can be applied from a single gunshot.";
                    BuilderUtil.HorizSlider(name, MaxRecoil, 0.5f, 10f, 100f, description);

                    name = "Add Recoil";
                    description = "Linearly Adds or Subtracts Recoil from each gunshot";
                    BuilderUtil.HorizSlider(name, AddRecoil, -1f, 1f, 100f, description);

                    name = "Recoil Decay";
                    description = "How much to Decay recoil per frame. 0.9 = 10 percent recoil decay per frame";
                    BuilderUtil.HorizSlider(name, RecoilDecay, 0.5f, 0.99f, 1000f, description);
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset Advanced to Default"))
                {
                    MenuClickSound();
                    ResetAdvanced();
                }
                if (GUILayout.Button("Reset ALL Settings"))
                {
                    ResetClickSound();
                    ResetAll();
                }
                if (GUILayout.Button("Mysterious Button"))
                {
                    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.PlayerIsDead);
                    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.QuestFailed);
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical(); GUILayout.EndArea();
            }

            DrawToolTip();
            ToolTip = null;
            ToolTipContent = null;
            GUILayout.FlexibleSpace();
        }

        private static bool ExpandTalkSettings = false;
        private static bool ExpandTalkBotType = false;
        private static bool ExpandTalkPMC;
        private static bool ExpandTalkScav;
        private static bool ExpandTalkBosses;
        private static bool ExpandTalkFollowers;
        private static bool ExpandTalkOther;
        private static bool ExpandTalkSquad;

        private static bool ExpandGeneral = false;
        private static bool ExpandVisionSpeed = false;
        private static bool ExpandShoot = false;
        private static bool ExpandRecoil = false;
        private static bool ExpandRecoilAdvanced = false;
        private static bool ExpandForcedPers = false;
        private static bool ExpandPersChances = false;

        private static bool ExpandExtracts;
        private static bool ExpandAudioSuppressor;
        private static bool ExpandAudioRanges;

        private static void ResetPersonality()
        {
            DefaultValue(RandomGigaChadChance);
            DefaultValue(RandomChadChance);
            DefaultValue(RandomRatChance);
            DefaultValue(RandomCowardChance);
            DefaultValue(AllGigaChads);
            DefaultValue(AllChads);
            DefaultValue(AllRats);
        }

        private static void ResetGeneral()
        {
            DefaultValue(NoBushESPToggle);
            DefaultValue(HeadShotProtection);
            DefaultValue(FasterCQBReactions);
        }

        private static void ResetVision()
        {
            DefaultValue(VisionSpeed);
            DefaultValue(CloseVisionSpeed);
            DefaultValue(FarVisionSpeed);
            DefaultValue(CloseFarThresh);
            DefaultValue(TestDistance);
        }

        private static void ResetShoot()
        {
            DefaultValue(FireratMulti);
            DefaultValue(BurstMulti);
            DefaultValue(AccuracyMulti);
            DefaultValue(BotRecoilGlobal);
            DefaultValue(PMCRecoil);
            DefaultValue(ScavRecoil);
            DefaultValue(OtherRecoil);
        }

        private static void ResetAdvanced()
        {
            DefaultValue(MaxRecoil);
            DefaultValue(AddRecoil);
            DefaultValue(RecoilDecay);
        }

        private static void ResetAll()
        {
            ResetGeneral();
            ResetVision();
            ResetShoot();
            ResetPersonality();
            ResetAdvanced();
        }
    }
}
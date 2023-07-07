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
using static SAIN.Editor.GUISections.Toolbar;
using SAIN.Editor.GUISections;
using SAIN.Editor.Util;
using SAIN.Helpers;
using Font = UnityEngine.Font;

namespace SAIN.Editor
{
    public class EditorGUI
    {
        public EditorGUI()
        {
        }

        public static EditorCustomization Settings { get; private set; }

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
            if ((Input.GetKeyDown(KeyCode.Escape) && guiStatus) || Input.GetKeyDown(TogglePanel.Value.MainKey) || CloseEditor)
            {
                JsonUtility.SaveToJson.EditorSettings(Settings);
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

        private static Vector2 ScaledPivot;
        public void OnGUI()
        {
            if (!Inited)
            {
                Inited = true;

                Settings = JsonUtility.LoadFromJson.EditorSettings();
                if (Settings != null && Settings.FontName != null)
                {
                    GUI.skin.font = Font.CreateDynamicFontFromOSFont(Settings.FontName, Settings.FontSize);
                }

                Fonts.Init();
                RectLayout.Init();
                InitStyles();
                ScaledPivot = new Vector2(ScalingFactor, ScalingFactor);
            }
            if (guiStatus)
            {
                GUIUtility.ScaleAroundPivot(ScaledPivot, Vector2.zero);
                ApplyTextures(GUI.skin.window, Backgrounds[SelectedTab]);
                MainWindow = GUI.Window(0, MainWindow, MainWindowFunc, "SAIN AI Settings Editor");
                DrawToolTip();
            }
        }

        private static bool Inited = false;

        private static Vector2 ScrollPos = Vector2.zero;
        public static Vector2 MousePos = Vector2.zero;

        private static bool OpenFontMenu = false;
        private void MainWindowFunc(int TWCWindowID)
        {
            GUI.DragWindow(DragRectangle);
            if (GUI.Button(ExitButton, "X"))
            {
                ResetClickSound();
                CloseEditor = true;
            }

            GUILayout.BeginVertical();

            CreateTabSelection();

            string name;
            string description;

            if (IsTabSelected(Home))
            {
                if (ExpandGeneral = BuilderUtil.ExpandableMenu("General Settings", ExpandGeneral, "General Global SAIN Settings, No Bush ESP, Headshot Protection, Mod Detection"))
                {
                    name = "No Bush ESP";
                    description = "Adds extra vision check for bots to help prevent bots seeing or shooting through foliage.";
                    BuilderUtil.CreateButtonOption(NoBushESPToggle);

                    name = "HeadShot Protection";
                    description = "Experimental, will kick bot's aiming target if it ends up on the player's head.";
                    BuilderUtil.CreateButtonOption(HeadShotProtection);

                    GUILayout.Box("Mod Detection");
                    GUILayout.BeginHorizontal();
                    SingleTextBool("Looting Bots", SAINPlugin.LootingBotsLoaded);
                    SingleTextBool("Realism Mod", SAINPlugin.RealismLoaded);
                    GUILayout.EndHorizontal();
                }

                PresetEditor.PresetMenu();
            }
            if (IsTabSelected(Personalities))
            {
                GUILayout.Box("Personality Settings");
                GUILayout.Box("For The Memes. Recommended not to use these during normal gameplay!");
                GUILayout.Box("Bots will be more predictable and exploitable.");

                if (BuilderUtil.CreateButtonOption(AllGigaChads))
                {
                    AllChads.Value = false;
                    AllRats.Value = false;
                }
                if (BuilderUtil.CreateButtonOption(AllChads))
                {
                    AllGigaChads.Value = false;
                    AllRats.Value = false;
                }
                if (BuilderUtil.CreateButtonOption(AllRats))
                {
                    AllGigaChads.Value = false;
                    AllChads.Value = false;
                }

                description = "The Chance that any random bot will get assigned this personality, regardless of Gear and Level";

                name = "Random GigaChad Chance";
                BuilderUtil.HorizSlider(RandomGigaChadChance, 1);

                name = "Random Chad Chance";
                BuilderUtil.HorizSlider(RandomChadChance, 1);

                name = "Random Rat Chance";
                BuilderUtil.HorizSlider(RandomRatChance, 1);

                name = "Random Coward Chance";
                BuilderUtil.HorizSlider(RandomCowardChance, 1);
            }
            if (IsTabSelected(Hearing))
            {
                description = "Audible Gun Range is multiplied by this number when using a suppressor";
                BuilderUtil.HorizSlider(SuppressorModifier, 100f);
                //BuilderUtil.HorizSlider("Suppressor Modifier", SuppressorModifier, 0.1f, 0.75f, 100f, description);

                description = "Audible Gun Range is multiplied by this number when using a suppressor and subsonic ammo";
                BuilderUtil.HorizSlider(SubsonicModifier, 100f);
                //BuilderUtil.HorizSlider("Subsonic Ammo Modifier", SubsonicModifier, 0.1f, 0.75f, 100f, description);

                description = "How far bots can hear specific ammo calibers. In Meters";
                BuilderUtil.HorizSlider(AudioRangePistol, 1f);
                //BuilderUtil.HorizSlider("Pistol Caliber", AudioRangePistol, 75f, 500f, 1f, description);
                BuilderUtil.HorizSlider(AudioRangeRifle, 1f);
                //BuilderUtil.HorizSlider("556 or 545 Caliber", AudioRangeRifle, 75f, 500f, 1f, description);
                BuilderUtil.HorizSlider(AudioRangeMidRifle, 1f);
                //BuilderUtil.HorizSlider("762 Caliber", AudioRangeMidRifle, 75f, 500f, 1f, description);
                BuilderUtil.HorizSlider(AudioRangeLargeCal, 1f);
                //BuilderUtil.HorizSlider("Large Caliber", AudioRangeLargeCal, 75f, 500f, 1f, description);
                BuilderUtil.HorizSlider(AudioRangeShotgun, 1f);
                //BuilderUtil.HorizSlider("Shotguns", AudioRangeShotgun, 75f, 500f, 1f, description);
            }
            if (IsTabSelected(Advanced))
            {
                GUILayout.Box("Advanced Settings. Edit at your own risk.");

                name = "Max Recoil";
                description = "The Max Recoil that can be applied from a single gunshot.";
                BuilderUtil.HorizSlider(MaxRecoil, 100f);

                name = "Add Recoil";
                description = "Linearly Adds or Subtracts Recoil from each gunshot";
                BuilderUtil.HorizSlider(AddRecoil, 100f);

                name = "Recoil Decay";
                description = "How much to Decay recoil per frame. 0.9 = 10 percent recoil decay per frame";
                BuilderUtil.HorizSlider(RecoilDecay, 1000f);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Font"))
                {
                    OpenFontMenu = !OpenFontMenu;
                }
                if (GUILayout.Button("Reset Advanced to Default"))
                {
                    MenuClickSound();
                }
                if (GUILayout.Button("Reset ALL Settings"))
                {
                    ResetClickSound();
                }
                if (GUILayout.Button("Mysterious Button"))
                {
                    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.PlayerIsDead);
                    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.QuestFailed);
                }
                GUILayout.EndHorizontal();

                if (OpenFontMenu)
                {
                    FontEditMenu.OpenMenu();
                }

                ColorEditorMenu();
            }

            GUILayout.EndVertical();
        }

        private static bool ExpandGeneral = false;
    }
}
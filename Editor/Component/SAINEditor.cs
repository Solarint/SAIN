using BepInEx.Configuration;
using Comfort.Common;
using EFT.Console.Core;
using EFT.UI;
using UnityEngine;
using System;
using static SAIN.Editor.EditorSettings;
using static SAIN.Editor.RectLayout;
using static SAIN.Editor.Sounds;
using static SAIN.Editor.UITextures;
using SAIN.Editor.GUISections;
using SAIN.Editor.Util;
using ColorsClass = SAIN.Editor.Util.ColorsClass;
using static SAIN.Editor.Names.StyleNames;
using EFT;

namespace SAIN.Editor
{
    public class SAINEditor : MonoBehaviour
    {
        public Action EditorToggled { get; set; }

        void Awake()
        {
            ConsoleScreen.Processor.RegisterCommand("saineditor", new Action(OpenEditor));
            ConsoleScreen.Processor.RegisterCommand("pausegame", new Action(TogglePause));

            OpenEditorButton = SAINPlugin.SAINConfig.Bind("SAIN Editor", "Open Editor", false, "Opens the Editor on press");
            OpenEditorConfigEntry = SAINPlugin.SAINConfig.Bind("SAIN Editor", "Open Editor Shortcut", new KeyboardShortcut(KeyCode.F6), "The keyboard shortcut that toggles editor");
            PauseConfigEntry = SAINPlugin.SAINConfig.Bind("SAIN Editor", "PauseButton", new KeyboardShortcut(KeyCode.Pause), "Pause The Game");


            TexturesClass = new TexturesClass(gameObject);
            Colors = new ColorsClass(gameObject);
            StyleOptions = new StyleOptions(gameObject);
            PresetEditor = new PresetEditor(gameObject);
            Builder = new BuilderClass(gameObject);
            Buttons = new ButtonsClass(gameObject);
            ToolTips = new ToolTips(gameObject);
            MouseFunctions = new MouseFunctions(gameObject);
        }

        public MouseFunctions MouseFunctions { get; private set; }
        public ToolTips ToolTips { get; private set; }
        public ButtonsClass Buttons { get; private set; }
        public BuilderClass Builder { get; private set; }
        public PresetEditor PresetEditor { get; private set; }
        public StyleOptions StyleOptions { get; private set; }
        public ColorsClass Colors { get; private set; }
        public TexturesClass TexturesClass { get; private set; }

        internal static ConfigEntry<bool> OpenEditorButton;
        internal static ConfigEntry<KeyboardShortcut> OpenEditorConfigEntry;
        internal static ConfigEntry<KeyboardShortcut> PauseConfigEntry;

        public bool GameIsPaused { get; private set; }

        private GameObject EFTInput;

        public bool MainWindowOpen { get; private set; } = false;

        [ConsoleCommand("Open SAIN GUI Editor")]
        private void OpenEditor()
        {
            if (!MainWindowOpen)
            {
                GUIToggle();
                ConsoleScreen.Log("[SAIN]: Opening Editor...");
            }
        }
        private void CloseEditor()
        {
            if (MainWindowOpen)
            {
                GUIToggle();
            }
        }

        private void TogglePause()
        {
            Player mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
            if (mainPlayer?.HandsAnimator != null)
            {
                GameIsPaused = !GameIsPaused;
                if (GameIsPaused)
                {
                    ConsoleScreen.Log("Pausing Game...");

                    Time.timeScale = 0f;
                    mainPlayer.HandsAnimator.SetAnimationSpeed(0f);
                }
                else
                {
                    Time.timeScale = 1f;
                    mainPlayer.HandsAnimator.SetAnimationSpeed(1f);
                }
            }
            else
            {
                GameIsPaused = false;
            }
        }

        public bool ShiftKeyPressed { get; private set; }
        public bool CtrlKeyPressed { get; private set; }

        void Update()
        {
            ShiftKeyPressed = Input.GetKey(KeyCode.LeftShift);
            CtrlKeyPressed = Input.GetKey(KeyCode.LeftControl);

            if (EFTInput == null)
            {
                EFTInput = GameObject.Find("___Input");
            }
            if (MainWindowOpen && EFTInput != null && EFTInput.activeSelf)
            {
                EFTInput?.SetActive(false);
            }
            if (PauseConfigEntry.Value.IsDown())
            {
                TogglePause();
            }
            if (OpenEditorConfigEntry.Value.IsDown() || 
                (Input.GetKeyDown(KeyCode.Escape) && MainWindowOpen) || 
                OpenEditorButton.Value)
            {
                if (OpenEditorButton.Value)
                {
                    OpenEditorButton.Value = false;
                }
                GUIToggle();
            }
        }

        private void GUIToggle()
        {
            MainWindowOpen = !MainWindowOpen;
            Cursor.visible = MainWindowOpen;
            if (MainWindowOpen)
            {
                CursorSettings.SetCursor(ECursorType.Idle);
                Cursor.lockState = CursorLockMode.None;
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuContextMenu);
            }
            else
            {
                PresetEditor.OpenAdjustmentWindow = false;
                CursorSettings.SetCursor(ECursorType.Invisible);
                Cursor.lockState = CursorLockMode.Locked;
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuDropdown);
            }
            EFTInput.SetActive(!MainWindowOpen);
        }

        void OnGUI()
        {
            if (MainWindowOpen)
            {
                if (!Inited)
                {
                    TexturesClass.Init();
                    StyleOptions.Init();
                    Fonts.Init();
                    Inited = true;
                }

                GUIUtility.ScaleAroundPivot(ScaledPivot, Vector2.zero);

                MainWindow = GUI.Window(0, MainWindow, MainWindowFunc, "SAIN AI Settings Editor", StyleOptions.GetStyle(window));

                ToolTips.DrawToolTip();

                if (PresetEditor.OpenAdjustmentWindow)
                {
                    AdjustmentRect = Builder.NewWindow(1, AdjustmentRect, PresetEditor.GUIAdjustment, "GUI Adjustment");
                }
            }
        }

        public Rect AdjustmentRect = new Rect(500, 50, 600f, 500f);

        public bool IsTabSelected(string tab)
        {
            return Tabs[SelectedTab] == tab;
        }

        public static readonly string Home = nameof(Home);
        public static readonly string Hearing = nameof(Hearing);
        public static readonly string Personalities = nameof(Personalities);
        public static readonly string Advanced = nameof(Advanced);
        public static readonly string[] Tabs = { Home, Hearing, Personalities, Advanced };

        private static bool Inited = false;
        float CurrentTabGridHeight = 15f;
        float[] TabGridHeights = new float[Tabs.Length];
        Rect[] TabGridOptions = new Rect[Tabs.Length];
        Rect[] TabGridOptionsDetections = new Rect[Tabs.Length];

        const float TabMenuHeight = 45f;
        const float TabMenuVerticalMargin = 5f;

        private void TabSelectMenu()
        {
            float startY = ExitButton.height + TabMenuVerticalMargin;
            float optionWidth = MainWindow.width / Tabs.Length;
            float optionX = 0f;

            var gridStyle = StyleOptions.GetStyle(toggle);

            for (int i = 0; i < Tabs.Length; i++)
            {
                float optionDrawHeight = TabGridHeights[i];

                Rect rect = new Rect(i * optionWidth, startY, optionWidth, TabMenuHeight);
                optionDrawHeight = AdjustHeight(optionDrawHeight, rect, TabMenuHeight);
                rect.height = optionDrawHeight;

                if (optionDrawHeight < TabMenuHeight)
                {
                    gridStyle.fontSize = 0;
                }

                if (GUI.Toggle(rect, SelectedTab == i, Tabs[i], gridStyle)) 
                { 
                    SelectedTab = i;
                }

                TabGridHeights[i] = optionDrawHeight;
                optionX += optionWidth;
            }
        }

        private float AdjustHeight( float current , Rect rect , float clamp , float incPerFrame = 3f, float closeMulti = 0.66f)
        {
            if (MouseFunctions.IsMouseInside(rect))
            {
                current += incPerFrame;
            }
            else
            {
                current -= incPerFrame * closeMulti;
            }
            current = Mathf.Clamp(current, 0, clamp);
            return current;
        }

        private static bool OpenFontMenu = false;
        private void MainWindowFunc(int TWCWindowID)
        {
            GUI.DrawTexture(MainWindow, Backgrounds[SelectedTab], ScaleMode.ScaleAndCrop);
            GUI.DragWindow(DragBarMainWindow);
            if (GUI.Button(ExitButton, "X", StyleOptions.GetStyle(button)))
            {
                ResetClickSound();
                CloseEditor();
            }

            TabSelectMenu();

            GUILayout.BeginVertical();
            GUILayout.Space(MainWindow.height * 0.05f + TabMenuHeight + TabMenuVerticalMargin);


            if (IsTabSelected(Home))
            {
                if (ExpandGeneral = Builder.ExpandableMenu("General Settings", ExpandGeneral, "General Global SAIN Settings, No Bush ESP, Headshot Protection, Mod Detection"))
                {
                    Builder.CreateButtonOption(NoBushESPToggle);
                    Builder.CreateButtonOption(HeadShotProtection);

                    GUILayout.Box("Mod Detection");
                    GUILayout.BeginHorizontal();
                    Buttons.SingleTextBool("Looting Bots", SAINPlugin.LootingBotsLoaded);
                    Buttons.SingleTextBool("Realism Mod", SAINPlugin.RealismLoaded);
                    GUILayout.EndHorizontal();
                }

                PresetEditor.PresetMenu();
            }
            if (IsTabSelected(Personalities))
            {
                GUILayout.Box("Personality Settings");
                GUILayout.Box("For The Memes. Recommended not to use these during normal gameplay!");
                GUILayout.Box("Bots will be more predictable and exploitable.");

                if (Builder.CreateButtonOption(AllGigaChads))
                {
                    AllChads.Value = false;
                    AllRats.Value = false;
                }
                if (Builder.CreateButtonOption(AllChads))
                {
                    AllGigaChads.Value = false;
                    AllRats.Value = false;
                }
                if (Builder.CreateButtonOption(AllRats))
                {
                    AllGigaChads.Value = false;
                    AllChads.Value = false;
                }

                Builder.HorizSlider(RandomGigaChadChance, 1);

                Builder.HorizSlider(RandomChadChance, 1);

                Builder.HorizSlider(RandomRatChance, 1);

                Builder.HorizSlider(RandomCowardChance, 1);
            }
            if (IsTabSelected(Hearing))
            {
                Builder.HorizSlider(SuppressorModifier, 100f);
                Builder.HorizSlider(SubsonicModifier, 100f);
                Builder.HorizSlider(AudioRangePistol, 1f);
                Builder.HorizSlider(AudioRangeRifle, 1f);
                Builder.HorizSlider(AudioRangeMidRifle, 1f);
                Builder.HorizSlider(AudioRangeLargeCal, 1f);
                Builder.HorizSlider(AudioRangeShotgun, 1f);
                Builder.HorizSlider(AudioRangeOther, 1f);
            }
            if (IsTabSelected(Advanced))
            {
                GUILayout.Box("Advanced Settings. Edit at your own risk.");

                Builder.HorizSlider(MaxRecoil, 100f);
                Builder.HorizSlider(AddRecoil, 100f);
                Builder.HorizSlider(RecoilDecay, 1000f);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Font"))
                {
                    OpenFontMenu = !OpenFontMenu;
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

                //ColorEditorMenu();
            }

            GUILayout.EndVertical();
        }

        public bool ExpandGeneral = false;
    }
}

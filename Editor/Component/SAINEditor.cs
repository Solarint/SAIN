using BepInEx.Configuration;
using Comfort.Common;
using EFT.Console.Core;
using EFT.UI;
using UnityEngine;
using System;
using static SAIN.Editor.EditorSettings;
using static SAIN.Editor.RectLayout;
using static SAIN.Editor.Sounds;
using SAIN.Editor.GUISections;
using SAIN.Editor.Util;
using ColorsClass = SAIN.Editor.Util.ColorsClass;
using static SAIN.Editor.Names.StyleNames;
using EFT;
using UnityEngine.EventSystems;

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
            WindowLayoutCreator = new WindowLayoutCreator(gameObject);
        }

        public WindowLayoutCreator WindowLayoutCreator { get; private set; }
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

        float baseHeight;

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
                    baseHeight = MainWindow.height;
                    DragBackgroundTexture = TexturesClass.GetColor(Names.ColorNames.MidGray);
                    OpenTabRect = new Rect(0, 85, MainWindow.width, 1000f);
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

        public static readonly string None = nameof(None);
        public static readonly string Home = nameof(Home);
        public static readonly string BotPresets = nameof(BotPresets);
        public static readonly string Hearing = nameof(Hearing);
        public static readonly string Personalities = nameof(Personalities);
        public static readonly string Advanced = nameof(Advanced);
        public static readonly string[] Tabs = { Home, BotPresets, Hearing, Personalities, Advanced };

        private static bool Inited = false;

        float TabMenuHeight = 50f;
        float TabMenuVerticalMargin = 5f;

        private void CreateNewTabMenuRects()
        {
            TabMenuRect = new Rect(0, ExitRect.height + TabMenuVerticalMargin, MainWindow.width, TabMenuHeight);
            TabRects = Builder.HorizontalGridRects(TabMenuRect, Tabs.Length, 15f);
        }

        private void TabSelectMenu(float minHeight = 15, float speed = 3, float closeSpeedMulti = 0.66f)
        {
            if (TabMenuRect == null || TabRects == null)
            {
                CreateNewTabMenuRects();
            }

            OpenTab = Builder.SelectionGridExpandHeight(TabMenuRect, Tabs, OpenTab, TabRects, minHeight, speed, closeSpeedMulti);
        }

        Rect[] TabRects;
        Rect TabMenuRect;
        Rect OpenTabRect;

        private bool TabSelected(string tab, out Rect tabRect, float targetHeight)
        {
            bool selected = OpenTab == tab;
            //OpenTabRect.height += 45;
            //OpenTabRect.height = Mathf.Clamp(OpenTabRect.height, 0f, targetHeight);
            //MainWindow.height = baseHeight + OpenTabRect.height;
            tabRect = OpenTabRect;
            return selected;
        }

        private int FindSelected()
        {
            for (int i = 0; i < Tabs.Length; i++)
            {
                if (Tabs[i] == OpenTab)
                {
                    return i;
                }
            }
            return 0;
        }

        Texture2D DragBackgroundTexture;

        string OpenTab = None;

        private static bool OpenFontMenu = false;

        private void MainWindowFunc(int TWCWindowID)
        {
            var dragStyle = Builder.GetStyle(blankbox);
            dragStyle.alignment = TextAnchor.MiddleLeft;
            dragStyle.padding = new RectOffset(10, 10, 0, 0);
            GUI.DrawTexture(DragRect, DragBackgroundTexture, ScaleMode.StretchToFill, true, 0);
            GUI.Box(DragRect, "SAIN GUI Editor", dragStyle);
            GUI.DragWindow(DragRect);
            if (GUI.Toggle(PauseRect, GameIsPaused, "Pause Game", StyleOptions.GetStyle(button)))
            {
                MenuClickSound();
                TogglePause();
            }
            if (GUI.Button(ExitRect, "X", StyleOptions.GetStyle(button)))
            {
                ResetClickSound();
                CloseEditor();
            }

            TabSelectMenu(25f, 3f, 0.5f);

            if (NoTabSelected())
            {
                return;
            }
            float spacing = DragRect.height + TabMenuRect.height + 5;
            Builder.Space(spacing);
            if (TabSelected(BotPresets, out Rect tabRect, 1000f))
            {
                Builder.BeginArea(tabRect);
                PresetEditor.OpenPresetWindow(tabRect);
                Builder.EndArea();
                return;
            }

            Builder.BeginVertical();

            if (TabSelected(Home, out tabRect, 1000f))
            {
                Builder.CreateButtonOption(NoBushESPToggle);
                Builder.CreateButtonOption(HeadShotProtection);

                Builder.Box("Mod Detection");

                Builder.BeginHorizontal();

                Buttons.SingleTextBool("Looting Bots", SAINPlugin.LootingBotsLoaded);
                Buttons.SingleTextBool("Realism Mod", SAINPlugin.RealismLoaded);

                Builder.EndHorizontal();
            }
            else if (TabSelected(Personalities, out tabRect, 1000f))
            {
                Builder.Box("Personality Settings");
                Builder.Box("For The Memes. Recommended not to use these during normal gameplay!");
                Builder.Box("Bots will be more predictable and exploitable.");

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
            else if (TabSelected(Hearing, out tabRect, 1000f))
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
            else if (TabSelected(Advanced, out tabRect, 1000f))
            {
                Builder.Box("Advanced Settings. Edit at your own risk.");

                Builder.HorizSlider(MaxRecoil, 100f);
                Builder.HorizSlider(AddRecoil, 100f);
                Builder.HorizSlider(RecoilDecay, 1000f);

                Builder.BeginHorizontal();
                if (Builder.Button("Font"))
                {
                    OpenFontMenu = !OpenFontMenu;
                }
                if (Builder.Button("Mysterious Button"))
                {
                    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.PlayerIsDead);
                    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.QuestFailed);
                }
                Builder.EndHorizontal();

                if (OpenFontMenu)
                {
                    FontEditMenu.OpenMenu();
                }

                WindowLayoutCreator.CreateGUIAdjustmentSliders();
            }
            Builder.EndVertical();
        }

        bool NoTabSelected()
        {
            bool noTab = OpenTab == None;
            if (noTab)
            {
                //OpenTabRect.height -= 30;
                //OpenTabRect.height = Mathf.Clamp(OpenTabRect.height, baseHeight, 1000);
                //MainWindow.height -= 30f;
                //MainWindow.height = Mathf.Clamp(MainWindow.height, baseHeight, 1000);
            }
            return noTab;
        }

        public bool ExpandGeneral = false;
    }
}

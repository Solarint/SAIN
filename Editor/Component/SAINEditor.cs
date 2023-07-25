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
using System.Reflection;
using BepInEx;
using EFT.Visual;

namespace SAIN.Editor
{
    public class SAINEditor
    {
        public SAINEditor() { }

        public void Init()
        {
            ConsoleScreen.Processor.RegisterCommand("saineditor", new Action(OpenEditor));
            ConsoleScreen.Processor.RegisterCommand("pausegame", new Action(TogglePause));

            OpenEditorButton = SAINPlugin.SAINConfig.Bind("SAIN Editor", "Open Editor", false, "Opens the Editor on press");
            OpenEditorConfigEntry = SAINPlugin.SAINConfig.Bind("SAIN Editor", "Open Editor Shortcut", new KeyboardShortcut(KeyCode.F6), "The keyboard shortcut that toggles editor");
            PauseConfigEntry = SAINPlugin.SAINConfig.Bind("SAIN Editor", "PauseButton", new KeyboardShortcut(KeyCode.Pause), "Pause The Game");

            // Use reflection to keep compatibility with unity 4.x since it doesn't have Cursor
            var tCursor = typeof(Cursor);
            _curLockState = tCursor.GetProperty("lockState", BindingFlags.Static | BindingFlags.Public);
            _curVisible = tCursor.GetProperty("visible", BindingFlags.Static | BindingFlags.Public);

            if (_curLockState == null && _curVisible == null)
            {
                _obsoleteCursor = true;

                _curLockState = typeof(Screen).GetProperty("lockCursor", BindingFlags.Static | BindingFlags.Public);
                _curVisible = typeof(Screen).GetProperty("showCursor", BindingFlags.Static | BindingFlags.Public);
            }

            TexturesClass = new TexturesClass(this);
            Colors = new ColorsClass(this);
            StyleOptions = new StyleOptions(this);
            PresetEditor = new PresetEditor(this);
            Builder = new BuilderClass(this);
            Buttons = new ButtonsClass(this);
            ToolTips = new ToolTips(this);
            MouseFunctions = new MouseFunctions(this);
            WindowLayoutCreator = new WindowLayoutCreator(this);
        }

        internal static Texture2D TooltipBg { get; private set; }

        private PropertyInfo _curLockState;
        private PropertyInfo _curVisible;
        private int _previousCursorLockState;
        private bool _previousCursorVisible;
        private bool _obsoleteCursor;

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

        [ConsoleCommand("Open SAIN GUI Editor")]

        private void OpenEditor()
        {
            if (!DisplayingWindow)
            {
                GUIToggle();
                ConsoleScreen.Log("[SAIN]: Opening Editor...");
            }
        }

        private void CloseEditor()
        {
            if (DisplayingWindow)
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

        public void Update()
        {
            ShiftKeyPressed = Input.GetKey(KeyCode.LeftShift);
            CtrlKeyPressed = Input.GetKey(KeyCode.LeftControl);

            if (DisplayingWindow) SetUnlockCursor(0, true);

            if (PauseConfigEntry.Value.IsDown())
            {
                TogglePause();
            }
            if (OpenEditorConfigEntry.Value.IsDown() || (Input.GetKeyDown(KeyCode.Escape) && DisplayingWindow) || OpenEditorButton.Value)
            {
                if (OpenEditorButton.Value)
                {
                    OpenEditorButton.Value = false;
                }
                GUIToggle();
            }
        }

        private void SetUnlockCursor(int lockState, bool cursorVisible)
        {
            if (_curLockState != null)
            {
                // Do through reflection for unity 4 compat
                //Cursor.lockState = CursorLockMode.None;
                //Cursor.visible = true;
                if (_obsoleteCursor)
                    _curLockState.SetValue(null, Convert.ToBoolean(lockState), null);
                else
                    _curLockState.SetValue(null, lockState, null);

                _curVisible.SetValue(null, cursorVisible, null);
            }
        }

        public void LateUpdate()
        {
            if (DisplayingWindow) SetUnlockCursor(0, true);
        }

        private void GUIToggle()
        {
            DisplayingWindow = !DisplayingWindow;
        }

        public void OnGUI()
        {
            if (DisplayingWindow)
            {
                if (!Inited)
                {
                    Inited = true;

                    TexturesClass.Init();
                    StyleOptions.Init();
                    Fonts.Init();
                    DragBackgroundTexture = TexturesClass.GetColor(Names.ColorNames.MidGray);
                    TooltipBg = TexturesClass.GetColor(Names.ColorNames.VeryDarkGray);
                    OpenTabRect = new Rect(0, 85, MainWindow.width, 1000f);
                }

                SetUnlockCursor(0, true);

                GUIUtility.ScaleAroundPivot(ScaledPivot, Vector2.zero);

                MainWindow = GUI.Window(0, MainWindow, MainWindowFunc, "SAIN AI Settings Editor", StyleOptions.GetStyle(window));

                if (PresetEditor.OpenAdjustmentWindow)
                {
                    AdjustmentRect = Builder.NewWindow(1, AdjustmentRect, PresetEditor.GUIAdjustment, "GUI Adjustment");
                }

                UnityInput.Current.ResetInputAxes();
            }
        }

        private bool _displayingWindow;

        public bool DisplayingWindow
        {
            get => _displayingWindow;
            set
            {
                if (_displayingWindow == value) return;
                _displayingWindow = value;

                if (_displayingWindow)
                {
                    // Do through reflection for unity 4 compat
                    if (_curLockState != null)
                    {
                        _previousCursorLockState = _obsoleteCursor ? Convert.ToInt32((bool)_curLockState.GetValue(null, null)) : (int)_curLockState.GetValue(null, null);
                        _previousCursorVisible = (bool)_curVisible.GetValue(null, null);
                    }
                }
                else
                {
                    if (!_previousCursorVisible || _previousCursorLockState != 0) // 0 = CursorLockMode.None
                    {
                        SetUnlockCursor(_previousCursorLockState, _previousCursorVisible);
                    }

                    PresetEditor.OpenAdjustmentWindow = false;
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
            tabRect = OpenTabRect;
            return OpenTab == tab;
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
            }
            else
            {
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
                    //if (Builder.Button("Font"))
                    //{
                    //    OpenFontMenu = !OpenFontMenu;
                    //}
                    if (Builder.Button("Mysterious Button"))
                    {
                        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.PlayerIsDead);
                        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.QuestFailed);
                    }
                    Builder.EndHorizontal();

                    //if (OpenFontMenu)
                    //{
                    //    FontEditMenu.OpenMenu();
                    //}

                    //WindowLayoutCreator.CreateGUIAdjustmentSliders();
                }

                Builder.EndVertical();
            }

            DrawTooltip();
        }

        public Rect ToolTipRect = Rect.zero;
        public GUIContent ToolTipContent;

        private void DrawTooltip()
        {
            string tooltip = GUI.tooltip;
            if (!string.IsNullOrEmpty(tooltip))
            {
                var currentEvent = Event.current;

                const int width = 200;

                var height = ToolTipStyle.CalcHeight(new GUIContent(tooltip), width) + 10;

                var x = currentEvent.mousePosition.x;
                var y = currentEvent.mousePosition.y + 15;

                GUI.Box(new Rect(x, y, width, height), tooltip, ToolTipStyle);
            }
        }

        private GUIStyle ToolTipStyle => StyleOptions.GetStyle(tooltip);

        bool NoTabSelected()
        {
            return OpenTab == None;
        }

        public bool ExpandGeneral = false;
    }
}

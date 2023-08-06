using BepInEx;
using Comfort.Common;
using EFT;
using EFT.Console.Core;
using EFT.UI;
using SAIN.Editor.GUISections;
using SAIN.Editor.Util;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.BotSettings.SAINSettings;
using SAIN.Preset.BotSettings.SAINSettings.Categories;
using SAIN.Preset.GlobalSettings;
using System;
using UnityEngine;
using static SAIN.Editor.RectLayout;
using static SAIN.Editor.Sounds;
using ColorsClass = SAIN.Editor.Util.ColorsClass;

namespace SAIN.Editor
{
    public class SAINEditor
    {
        public SAINEditor()
        {
            ConsoleScreen.Processor.RegisterCommand("saineditor", new Action(OpenEditor));
            ConsoleScreen.Processor.RegisterCommand("pausegame", new Action(TogglePause));
        }

        public void Init()
        {
            CursorSettings.InitCursor();
            TexturesClass = new TexturesClass(this);
            Colors = new ColorsClass(this);
            StyleOptions = new StyleOptions(this);
            BotSelectionClass = new PresetEditor(this);
            Builder = new BuilderClass(this);
            Buttons = new ButtonsClass(this);
            MouseFunctions = new MouseFunctions(this);
            WindowLayoutCreator = new WindowLayoutCreator(this);
            BotSettingsEditor = new BotSettingsEditor(this);
            BotPersonalityEditor = new BotPersonalityEditor(this);
            PresetSelection = new PresetSelection(this);
        }

        internal static Texture2D TooltipBg { get; private set; }

        public PresetSelection PresetSelection { get; private set; }
        public BotPersonalityEditor BotPersonalityEditor { get; private set; }
        public BotSettingsEditor BotSettingsEditor { get; private set; }
        public WindowLayoutCreator WindowLayoutCreator { get; private set; }
        public MouseFunctions MouseFunctions { get; private set; }
        public ButtonsClass Buttons { get; private set; }
        public BuilderClass Builder { get; private set; }
        public PresetEditor BotSelectionClass { get; private set; }
        public StyleOptions StyleOptions { get; private set; }
        public ColorsClass Colors { get; private set; }
        public TexturesClass TexturesClass { get; private set; }

        public bool GameIsPaused { get; private set; }
        public bool AdvancedOptionsEnabled { get; private set; }

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

        public readonly FieldsCache SAINBotSettingsCache = new FieldsCache(typeof(SAINSettingsClass));
        public readonly FieldsCache GlobalSettingsCache = new FieldsCache(typeof(GlobalSettingsClass));

        public void Update()
        {
            ShiftKeyPressed = Input.GetKey(KeyCode.LeftShift);
            CtrlKeyPressed = Input.GetKey(KeyCode.LeftControl);

            if (DisplayingWindow) CursorSettings.SetUnlockCursor(0, true);

            if (SAINPlugin.PauseConfigEntry.Value.IsDown())
            {
                TogglePause();
            }
            if (SAINPlugin.OpenEditorConfigEntry.Value.IsDown() || (Input.GetKeyDown(KeyCode.Escape) && DisplayingWindow) || SAINPlugin.OpenEditorButton.Value)
            {
                if (SAINPlugin.OpenEditorButton.Value)
                {
                    SAINPlugin.OpenEditorButton.BoxedValue = false;
                    SAINPlugin.OpenEditorButton.Value = false;
                }
                GUIToggle();
            }

            SAINBotSettingsCache.CacheHandler(DisplayingWindow);
        }

        public void LateUpdate()
        {
            if (DisplayingWindow) CursorSettings.SetUnlockCursor(0, true);
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
                    DragBackgroundTexture = TexturesClass.GetColor(ColorNames.MidGray);
                    TooltipBg = TexturesClass.GetColor(ColorNames.VeryDarkGray);
                    OpenTabRect = new Rect(0, 85, MainWindow.width, 1000f);
                }

                StyleOptions.CustomStyle.Cache(true);

                CursorSettings.SetUnlockCursor(0, true);

                GUIUtility.ScaleAroundPivot(ScaledPivot, Vector2.zero);

                MainWindow = GUI.Window(0, MainWindow, MainWindowFunc, "SAIN AI Settings Editor", StyleOptions.GetStyle(Style.window));

                if (BotSelectionClass.OpenAdjustmentWindow)
                {
                    AdjustmentRect = Builder.NewWindow(1, AdjustmentRect, BotSelectionClass.GUIAdjustment, "GUI Adjustment");
                }

                UnityInput.Current.ResetInputAxes();
            }
            else
            {
                StyleOptions.CustomStyle.Cache(false);
            }
        }

        public bool DisplayingWindow
        {
            get => CursorSettings.DisplayingWindow;
            set { CursorSettings.DisplayingWindow = value; }
        }

        public Rect AdjustmentRect = new Rect(500, 50, 600f, 500f);

        public static readonly string None = nameof(None);
        public static readonly string Home = nameof(Home);
        public static readonly string Global = "Global Settings";
        public static readonly string BotSettings = "Bot Settings";
        public static readonly string Personalities = "Personality Settings";
        public static readonly string Advanced = nameof(Advanced);
        public static readonly string[] Tabs = { Home, Global, BotSettings, Personalities, Advanced };

        private static bool Inited = false;

        private float TabMenuHeight = 50f;
        private float TabMenuVerticalMargin = 5f;

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

        private Rect[] TabRects;
        private Rect TabMenuRect;
        private Rect OpenTabRect;

        private bool TabSelected(string tab, out Rect tabRect, float targetHeight)
        {
            tabRect = OpenTabRect;
            return OpenTab == tab;
        }

        private Texture2D DragBackgroundTexture;

        private string OpenTab = None;

        private void MainWindowFunc(int TWCWindowID)
        {
            var dragStyle = Builder.GetStyle(Style.blankbox);
            dragStyle.alignment = TextAnchor.MiddleLeft;
            dragStyle.padding = new RectOffset(10, 10, 0, 0);
            GUI.DrawTexture(DragRect, DragBackgroundTexture, ScaleMode.StretchToFill, true, 0);
            GUI.Box(DragRect, "SAIN GUI Editor", dragStyle);
            GUI.DragWindow(DragRect);
            if (GUI.Toggle(PauseRect, GameIsPaused, "Pause Game", StyleOptions.GetStyle(Style.button)))
            {
                MenuClickSound();
                TogglePause();
            }
            if (GUI.Button(ExitRect, "X", StyleOptions.GetStyle(Style.button)))
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
            if (TabSelected(BotSettings, out Rect tabRect, 1000f))
            {
                Builder.BeginArea(tabRect);
                BotSelectionClass.Menu(tabRect);
                BotSettingsEditor.EditMenu(new SAINSettingsClass());
                Builder.EndArea();
            }
            else
            {
                Builder.BeginVertical();

                if (TabSelected(Home, out tabRect, 1000f))
                {
                    ModDetection.ModDetectionGUI();

                    Builder.Space(10f);

                    PresetSelection.Menu();
                }
                else if (TabSelected(Personalities, out tabRect, 1000f))
                {
                    BotPersonalityEditor.PersonalityMenu();
                }
                else if (TabSelected(Advanced, out tabRect, 1000f))
                {
                    AdvancedOptionsEnabled = Builder.Toggle(AdvancedOptionsEnabled, "Advanced Bot Configs", "Edit at your own risk.", Builder.Height(40f));
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

        private GUIStyle ToolTipStyle => StyleOptions.GetStyle(Style.tooltip);

        private bool NoTabSelected()
        {
            return OpenTab == None;
        }

        public bool ExpandGeneral = false;
    }
}
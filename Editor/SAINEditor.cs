using BepInEx;
using Comfort.Common;
using EFT;
using EFT.Console.Core;
using EFT.UI;
using SAIN.Attributes;
using SAIN.Plugin;
using System;
using UnityEngine;
using static SAIN.Editor.RectLayout;
using static SAIN.Editor.SAINLayout;
using static SAIN.Editor.Sounds;
using ColorsClass = SAIN.Editor.Util.ColorsClass;

namespace SAIN.Editor
{
    public static class SAINEditor
    {
        static SAINEditor()
        {
            ConsoleScreen.Processor.RegisterCommand("saineditor", new Action(ToggleGUI));
        }

        public static void Init()
        {
            CursorSettings.InitCursor();
        }

        public static bool AdvancedBotConfigs => PresetHandler.EditorDefaults.AdvancedBotConfigs;

        [ConsoleCommand("Toggle SAIN GUI Editor")]
        private static void ToggleGUI()
        {
            DisplayingWindow = !DisplayingWindow;
        }

        private static float CheckKeyLimiter;
        public static bool ShiftKeyPressed;
        public static bool CtrlKeyPressed;
        private static bool ToggleKeyPressed;
        private static bool EscapeKeyPressed;

        private static void CheckKeys()
        {
            if (CheckKeyLimiter < Time.time)
            {
                CheckKeyLimiter = Time.time + 0.1f;
                ShiftKeyPressed = Input.GetKey(KeyCode.LeftShift);
                CtrlKeyPressed = Input.GetKey(KeyCode.LeftControl);
                ToggleKeyPressed = Input.GetKeyDown(SAINPlugin.OpenEditorConfigEntry.Value.MainKey);
                EscapeKeyPressed = Input.GetKeyDown(KeyCode.Escape);
            }
        }

        public static void Update()
        {
            if (DisplayingWindow)
            {
                CursorSettings.SetUnlockCursor(0, true);
            }
            else
            {
                CheckKeys();
            }

            if ((SAINPlugin.OpenEditorConfigEntry.Value.IsDown() && !DisplayingWindow) || SAINPlugin.OpenEditorButton.Value)
            {
                if (SAINPlugin.OpenEditorButton.Value)
                {
                    SAINPlugin.OpenEditorButton.BoxedValue = false;
                    SAINPlugin.OpenEditorButton.Value = false;
                }
                ToggleGUI();
            }
        }

        public static void LateUpdate()
        {
            if (DisplayingWindow) CursorSettings.SetUnlockCursor(0, true);
        }

        public static void OnGUI()
        {
            if (DisplayingWindow)
            {
                if (!CacheCreated)
                {
                    CacheCreated = true;
                    ColorsClass.CreateCache();
                    TexturesClass.CreateCache();
                    StylesClass.CreateCache();
                }

                CursorSettings.SetUnlockCursor(0, true);
                GUIUtility.ScaleAroundPivot(ScaledPivot, Vector2.zero);
                MainWindow = GUI.Window(0, MainWindow, MainWindowFunc, "SAIN AI Settings Editor", GetStyle(Style.window));
                UnityInput.Current.ResetInputAxes();
            }
        }

        private static bool CacheCreated;

        private static void MainWindowFunc(int TWCWindowID)
        {
            GUI.FocusWindow(TWCWindowID);

            CheckKeys();

            if (ToggleKeyPressed || EscapeKeyPressed)
            {
                ToggleGUI();
                return;
            }

            CreateDragBar();

            CreateTopBarOptions();

            EEditorTab selectedTab = EditTabsClass.TabSelectMenu(35f, 3f, 0.5f);

            float space = DragRect.height + EditTabsClass.TabMenuRect.height;
            Space(space);

            if (!string.IsNullOrEmpty(ExceptionString))
            {
                BeginHorizontal();
                BuilderClass.Alert(ExceptionString, "EXPORT ERROR",
                    40f, 1000, ColorNames.MidRed);
                BuilderClass.Alert(
                    "LogOutput is located in Bepinex folder in SPT install. GitHub Link is available on the SAIN mod Page, right hand side.",
                    "Please Report Issue to SAIN Github along with your LogOutput File",
                    40f, 1000, ColorNames.MidRed);
                EndHorizontal();
            }

            GUITabs.CreateTabs(selectedTab);

            DrawTooltip();
        }

        private static void CreateDragBar()
        {
            GUI.DrawTexture(DragRect, DragBackgroundTexture, ScaleMode.StretchToFill, true, 0);
            GUI.Box(DragRect, $"SAIN GUI Editor | Preset: {SAINPlugin.LoadedPreset.Info.Name}", GetStyle(Style.dragBar));
            GUI.DragWindow(DragRect);
        }

        public static string ExceptionString = string.Empty;

        private static readonly GUIContent SaveContent = new GUIContent
                ("Save All Changes", $"Export All Changes to SAIN/Presets/{SAINPlugin.LoadedPreset.Info.Name}");

        private static void CreateTopBarOptions()
        {
            var style = GetStyle(Style.toggle);
            SaveContent.tooltip = $"Export All Changes to SAIN/Presets/{SAINPlugin.LoadedPreset.Info.Name}";
            if (GUI.Button(SaveAllRect, SaveContent, GetStyle(Style.button)))
            {
                PlaySound(EUISoundType.InsuranceInsured);
                SAINPlugin.LoadedPreset.ExportAll();
            }

            if (GUI.Button(ExitRect, "X", style))
            {
                PlaySound(EUISoundType.MenuEscape);
                ToggleGUI();
            }
        }

        private static void DrawTooltip()
        {
            if (!string.IsNullOrEmpty(GUI.tooltip))
            {
                const int width = 250;
                var x = Event.current.mousePosition.x;
                var y = Event.current.mousePosition.y + 15;
                if (x > Screen.width / 3)
                {
                    x -= width;
                }

                var ToolTipStyle = GetStyle(Style.tooltip);
                var height = ToolTipStyle.CalcHeight(new GUIContent(GUI.tooltip), width) + 10;
                GUI.Box(new Rect(x, y, width, height), GUI.tooltip, ToolTipStyle);
            }
        }

        public static bool DisplayingWindow
        {
            get => CursorSettings.DisplayingWindow;
            set { CursorSettings.DisplayingWindow = value; }
        }

        public static Rect OpenTabRect = new Rect(0, 0, MainWindow.width, 1000f);

        private static Texture2D DragBackgroundTexture => TexturesClass.GetTexture(ColorNames.MidGray);
    }
}
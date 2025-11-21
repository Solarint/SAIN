using BepInEx;
using EFT.Console.Core;
using EFT.UI;
using SAIN.Editor.Util;
using SAIN.Plugin;
using SAIN.Preset;
using System;
using UnityEngine;
using static SAIN.Editor.RectLayout;
using static SAIN.Editor.SAINLayout;
using static SAIN.Editor.Sounds;
using ColorsClass = SAIN.Editor.Util.ColorsClass;

namespace SAIN.Editor;

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

    public static void ManualUpdate()
    {
        if (DisplayingWindow)
        {
            CursorSettings.SetUnlockCursor(0, true);
            MouseFunctions.Update();
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

            MouseFunctions.OnGUI();
            CursorSettings.SetUnlockCursor(0, true);
            GUIUtility.ScaleAroundPivot(ScaledPivot, Vector2.zero);
            MainWindow = GUI.Window(0, MainWindow, MainWindowFunc, "SAIN AI Settings Editor", GetStyle(Style.window));
            UnityInput.Current.ResetInputAxes();
            ConfigEditingTracker.Update();
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
        GUITabs.CreateTabs(selectedTab);
        MouseFunctions.OnGUI();
        DrawTooltip();
    }

    private static void CreateDragBar()
    {
        GUI.DrawTexture(DragRect, DragBackgroundTexture, ScaleMode.StretchToFill, true, 0);
        GUI.Box(DragRect, $"SAIN {AssemblyInfoClass.SAINVersion} GUI Editor | Preset: {SAINPlugin.LoadedPreset.Info.Name}", GetStyle(Style.dragBar));
        GUI.DragWindow(DragRect);
    }

    public static string ExceptionString = string.Empty;

    private static readonly GUIContent SaveContent = new("Save All Changes", $"Export All Changes to SAIN/Presets/{SAINPlugin.LoadedPreset.Info.Name}");

    private static void CreateTopBarOptions()
    {
        SaveContent.tooltip = ConfigEditingTracker.GetUnsavedValuesString();

        var style = GetStyle(Style.botTypeGrid);
        var oldAlignment = style.alignment;
        style.alignment = TextAnchor.MiddleCenter;

        bool advancedEnabled = PresetHandler.EditorDefaults.AdvancedBotConfigs;
        string status = advancedEnabled ? "ON" : "OFF";
        bool newValue = GUI.Toggle(AdvRect, advancedEnabled, $"Advanced Settings: [{status}]", GetStyle(Style.botTypeGrid));
        if (advancedEnabled != newValue)
        {
            PlaySound(EUISoundType.MenuEscape);
            PresetHandler.EditorDefaults.AdvancedBotConfigs = newValue;
            PresetHandler.ExportEditorDefaults();
        }

        if (GUI.Button(SaveAllRect, SaveContent, GetStyle(Style.botTypeGrid)))
        {
            PlaySound(EUISoundType.InsuranceInsured);
            SAINPresetClass.ExportAll(SAINPlugin.LoadedPreset);
        }

        if (GUI.Button(ExitRect, "X", GetStyle(Style.botTypeGrid)))
        {
            PlaySound(EUISoundType.MenuEscape);
            ToggleGUI();
        }
        style.alignment = oldAlignment;
    }

    private static void DrawTooltip()
    {
        if (string.IsNullOrEmpty(GUI.tooltip))
        {
            //var sb = new StringBuilder();
            //
            //sb.AppendLine(Event.current.rawType.ToString());
            //sb.AppendLine(Event.current.type.ToString());
            //sb.AppendLine(Event.current.control.ToString());
            //sb.AppendLine(Event.current.commandName.ToString());
            //GUI.tooltip = sb.ToString();
            return;
        }

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

    public static bool DisplayingWindow
    {
        get => CursorSettings.DisplayingWindow;
        set { CursorSettings.DisplayingWindow = value; }
    }

    public static Rect OpenTabRect = new(0, 0, MainWindow.width, 1000f);

    private static Texture2D DragBackgroundTexture => TexturesClass.GetTexture(EGraynessLevel.Mid);
}
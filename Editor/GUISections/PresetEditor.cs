using SAIN.Preset;
using SAIN.Editor.Abstract;
using SAIN.Plugin;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using SAIN.Preset.BotSettings.SAINSettings;
using EFT;

namespace SAIN.Editor
{
    public class PresetEditorDefaults
    {
        public string SelectedPreset;
        public string DefaultPreset;
        public bool Advanced;
    }

    public class PresetEditor : EditorAbstract
    {
        public PresetEditorDefaults PresetEditorSettings { get; private set; }

        public PresetEditor(SAINEditor editor) : base(editor)
        {
            List<string> sections = new List<string>();
            foreach (var type in BotTypeDefinitions.BotTypes.Values)
            {
                if (!sections.Contains(type.Section))
                {
                    sections.Add(type.Section);
                }
            }

            Sections = sections.ToArray();
        }

        public static string GetLastFolderName(string filePath)
        {
            string directoryName = Path.GetDirectoryName(filePath);
            string[] folders = directoryName.Split(Path.DirectorySeparatorChar);
            return folders[folders.Length - 1];
        }

        private float RecheckOptionsTimer;

        private void LoadPresetOptions(bool refresh = false)
        {
            if (RecheckOptionsTimer < Time.time || refresh)
            {
                RecheckOptionsTimer = Time.time + 30f;
            }
        }

        public void Menu(Rect windowRect)
        {
            Rect12Height = windowRect.height;

            OpenFirstMenu = Builder.ExpandableMenu("Bots", OpenFirstMenu, "Select the bots you wish to edit the settings for");
            if (OpenFirstMenu)
            {
                float startHeight = 25;
                SectionRectangle = new Rect(0, startHeight, SectionRectWidth, FinalWindowHeight);
                BeginArea(SectionRectangle);
                SelectSection(SectionRectangle);
                EndArea();

                TypeRectangle = new Rect(TypeRectX, startHeight, TypeRectWidth, FinalWindowHeight);

                BeginArea(TypeRectangle);

                FirstMenuScroll = BeginScrollView(FirstMenuScroll);
                SelectType();
                EndScrollView();

                EndArea();

                Space(SectionRectangle.height);

                if (Button("Clear", Width(150)))
                {
                    SelectedSections.Clear();
                    SelectedWildSpawnTypes.Clear();
                }
            }

            BeginHorizontal();

            ButtonsClass.InfoBox("Select which difficulties you wish to modify.");
            Label("Difficulties", Width(150f));
            int spacing = 0;
            for (int i = 0; i < BotDifficultyOptions.Length; i++)
            {
                var diff = BotDifficultyOptions[i];

                spacing = SelectionGridOption(
                    spacing,
                    diff.ToString(),
                    SelectedDifficulties,
                    diff,
                    5,
                    25);
            }

            EndHorizontal();

            OpenPropEdit = Builder.ExpandableMenu("Edit Settings", OpenPropEdit, "Modify settings for bots here");
            if (OpenPropEdit)
            {
                PropEditMenu();
            }
        }

        private bool OpenFirstMenu = false;
        private bool OpenPropEdit = false;

        private float FinalWindowWidth => Editor.WindowLayoutCreator.FinalWindowWidth;

        private float FinalWindowHeight => 300f;

        private float Rect1OptionSpacing = 2f;
        private float Rect2OptionSpacing = 2f;
        private float SectionSelOptionHeight => (Rect12Height / Sections.Length) - (Rect1OptionSpacing);
        private float SectionRectX => BothRectGap;
        private float SectionRectWidth = 150f;
        private float BothRectGap = 6f;
        private float TypeRectWidth => RectLayout.MainWindow.width - SectionRectWidth - BothRectGap * 2f;
        private float TypeRectX => SectionRectX + SectionRectWidth + BothRectGap;
        private float Rect12Height = 285f;
        private float Rect12HeightMinusMargin => Rect12Height - ScrollMargin;

        private float ScrollMargin = 0f;

        private float TypeOptLabelHeight = 18f;
        private float TypeOptOptionHeight = 19f;
        private float SectOptFontSizeInc = 15f;

        private float YWithGenClose = 160f;
        private float YWithGenOpen = 270f;
        private float MenuStartHeight => Editor.ExpandGeneral ? YWithGenOpen : YWithGenClose;

        private Rect SectionRectangle;
        private Rect TypeRectangle;

        private Vector2 FirstMenuScroll = Vector2.zero;

        private Rect RelativeRect(Rect mainRect, Rect insideRect, Rect lastRect)
        {
            float X = lastRect.x + insideRect.x + mainRect.x;
            float Y = lastRect.y + insideRect.y + mainRect.y;
            return new Rect(X, Y, lastRect.width, lastRect.height);
        }

        private Rect RelativeRectMainWindow(Rect insideRect, Rect lastRect)
        {
            return RelativeRect(RectLayout.MainWindow, insideRect, lastRect);
        }

        private Rect RelativeRectMainWindow(Rect lastRect)
        {
            float X = lastRect.x + RectLayout.MainWindow.x;
            float Y = lastRect.y + RectLayout.MainWindow.y;
            return new Rect(X, Y, lastRect.width, lastRect.height);
        }

        private Rect? RelativeRectLastRectMainWindow(Rect insideRect)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return null;
            }
            return RelativeRectMainWindow(insideRect, GUILayoutUtility.GetLastRect());
        }

        public bool OpenAdjustmentWindow = false;
        private Rect AdjustmentRect => Editor.AdjustmentRect;

        public void GUIAdjustment(int wini)
        {
            GUI.DragWindow(new Rect(0, 0, AdjustmentRect.width, 20f));

            Space(25f);

            BeginVertical();

            YWithGenClose = HorizontalSliderNoStyle(nameof(YWithGenClose), YWithGenClose, 100f, 500f);
            YWithGenClose = Round(YWithGenClose);
            YWithGenOpen = HorizontalSliderNoStyle(nameof(YWithGenOpen), YWithGenOpen, 100f, 500f);
            YWithGenOpen = Round(YWithGenOpen);

            Space(10f);

            SectionRectWidth = HorizontalSliderNoStyle(nameof(SectionRectWidth), SectionRectWidth, 100f, 500f);
            SectionRectWidth = Round(SectionRectWidth);
            BothRectGap = HorizontalSliderNoStyle(nameof(BothRectGap), BothRectGap, 0f, 50f);
            BothRectGap = Round(BothRectGap);
            Rect12Height = HorizontalSliderNoStyle(nameof(Rect12Height), Rect12Height, 100f, 500f);
            Rect12Height = Round(Rect12Height);

            Space(10f);

            ScrollMargin = HorizontalSliderNoStyle(nameof(ScrollMargin), ScrollMargin, 0, 50f);
            ScrollMargin = Round(ScrollMargin);
            Rect1OptionSpacing = HorizontalSliderNoStyle(nameof(Rect1OptionSpacing), Rect1OptionSpacing, 0f, 50f);
            Rect1OptionSpacing = Round(Rect1OptionSpacing);
            Rect2OptionSpacing = HorizontalSliderNoStyle(nameof(Rect2OptionSpacing), Rect2OptionSpacing, 0, 50f);
            Rect2OptionSpacing = Round(Rect2OptionSpacing, 1);

            Space(10f);

            TypeOptLabelHeight = HorizontalSliderNoStyle(nameof(TypeOptLabelHeight), TypeOptLabelHeight, 5, 25f);
            TypeOptLabelHeight = Round(TypeOptLabelHeight, 1);
            TypeOptOptionHeight = HorizontalSliderNoStyle(nameof(TypeOptOptionHeight), TypeOptOptionHeight, 5, 50f);
            TypeOptOptionHeight = Round(TypeOptOptionHeight);
            SectOptFontSizeInc = HorizontalSliderNoStyle(nameof(SectOptFontSizeInc), SectOptFontSizeInc, 0, 50f);
            SectOptFontSizeInc = Round(SectOptFontSizeInc);

            Space(10f);

            CreateGUIAdjustmentSliders();

            EndVertical();
        }

        public float Round(float value, float dec = 10f)
        {
            return Mathf.Round(value * dec) / dec;
        }

        private Rect[] SectionRects;

        private void SelectSection(Rect window)
        {
            if (SectionRects == null)
            {
                SectionRects = Builder.VerticalGridRects(window, Sections.Length, 80f);
            }

            Builder.SelectionGridExpandWidth(window, Sections, SelectedSections, SectionRects, 80f, 5);
        }

        private void SelectType()
        {
            for (int i = 0; i < SelectedSections.Count; i++)
            {
                string section = SelectedSections[i];
                GUIStyle style = new GUIStyle(GetStyle(Style.label))
                {
                    fontStyle = FontStyle.Normal,
                    alignment = TextAnchor.MiddleLeft
                };

                BeginHorizontal();
                Space(Rect2OptionSpacing);

                Label(section, style, Height(TypeOptLabelHeight), Width(200f));

                FlexibleSpace();
                EndHorizontal();

                Space(Rect2OptionSpacing);

                var botTypes = BotTypeDefinitions.BotTypes.Values.ToArray();
                for (int j = 0; j < botTypes.Length; j++)
                {
                    BotType type = botTypes[j];
                    if (type.Section == section)
                    {
                        GUIStyle style2 = new GUIStyle(GetStyle(Style.toggle))
                        {
                            fontStyle = FontStyle.Normal,
                            alignment = TextAnchor.LowerCenter
                        };
                        bool typeSelected = SelectedWildSpawnTypes.Contains(type);

                        BeginHorizontal();
                        Space(Rect2OptionSpacing);

                        bool AddToList = Toggle(typeSelected, new GUIContent(type.Name, type.Description), style2, Height(TypeOptOptionHeight));

                        Rect? lastRect = RelativeRectLastRectMainWindow(TypeRectangle);
                        if (lastRect != null && CheckDrag(lastRect.Value))
                        {
                            AddToList = !Editor.ShiftKeyPressed;
                        }

                        if (AddToList && !SelectedWildSpawnTypes.Contains(type))
                        {
                            SelectedWildSpawnTypes.Add(type);
                        }
                        else if (!AddToList && SelectedWildSpawnTypes.Contains(type))
                        {
                            SelectedWildSpawnTypes.Remove(type);
                        }

                        Space(Rect2OptionSpacing);
                        EndHorizontal();
                    }
                }
                Space(Rect2OptionSpacing);
            }
        }

        public readonly string[] Sections;
        private readonly List<string> SelectedSections = new List<string>();

        private readonly List<BotType> SelectedWildSpawnTypes = new List<BotType>();

        public readonly BotDifficulty[] BotDifficultyOptions = { BotDifficulty.easy, BotDifficulty.normal, BotDifficulty.hard, BotDifficulty.impossible };
        public readonly List<BotDifficulty> SelectedDifficulties = new List<BotDifficulty>();

        private void PropSelectionMenu(int optionsPerLine = 3, float optionHeight = 25f, float scrollHeight = 200f)
        {
            PropertyScroll = BeginScrollView(PropertyScroll, Height(scrollHeight));

            BeginHorizontal();

            EndHorizontal();
            EndScrollView();
        }

        private readonly List<PropertyInfo> SelectedProperties = new List<PropertyInfo>();
        private static Vector2 PropertyScroll = Vector2.zero;

        private void PropEditMenu()
        {
            bool returnToStart = false;

            BeginHorizontal();

            if (SelectedWildSpawnTypes.Count == 0)
            {
                Box("No Bots Selected");
                returnToStart = true;
            }
            if (SelectedDifficulties.Count == 0)
            {
                Box("No Difficulties Selected");
                returnToStart = true;
            }

            EndHorizontal();

            if (returnToStart)
            {
                Label("Please Select Options to Edit", Height(35f), Width(200f));
                return;
            }

            BeginHorizontal();

            BotType typeInEdit = SelectedWildSpawnTypes[0];

            var count = SelectedWildSpawnTypes.Count;
            var diffCount = SelectedDifficulties.Count;

            Label($"Bots Selected: [{count}]", ConvertBotTypeListToString(), Height(35f), Width(200f));

            Label($"Difficulties Selected: [{diffCount}]", ConvertListToString(SelectedDifficulties), Height(35f), Width(200f));

            FlexibleSpace();

            if (Button("Save", "Apply Values set below to all selected bot types for all selected difficulties. Saves edited values to SAIN/Presets folder", Height(35f), Width(200f)))
            {
                SAINPlugin.LoadedPreset.SavePreset();
                PresetHandler.UpdateExistingBots();
            }
            if (Button("Discard", "Clear all selected bots, difficulties", Height(35f), Width(200f)))
            {
                Reset();
            }

            EndHorizontal();

            BeginHorizontal();
            FlexibleSpace();

            Label($"Currently Viewing: [{typeInEdit.Name}] at difficulty [{EditDifficulty}]", Height(35f));
            Label($"Shown Values will be copied to all [{count}] selected types and [{diffCount}] difficulties", Height(35f));

            FlexibleSpace();
            EndHorizontal();

            Scroll = BeginScrollView(Scroll);

            if (EditingSettings == null || EditingType != typeInEdit.WildSpawnType || EditingDifficulty != EditDifficulty)
            {
                EditingType = typeInEdit.WildSpawnType;
                EditingDifficulty = EditDifficulty;
                EditingSettings = SAINPlugin.LoadedPreset.BotSettings.GetSAINSettings(EditingType, EditingDifficulty);
            }

            Editor.BotSettingsEditor.SettingsMenu(EditingSettings, Editor.SAINBotSettingsCache);

            EndScrollView();
        }

        private WildSpawnType EditingType;
        private BotDifficulty EditingDifficulty;
        private SAINSettingsClass EditingSettings;
        private Vector2 Scroll = Vector2.zero;

        private string ConvertBotTypeListToString()
        {
            string result = string.Empty;
            for (int i = 0; i < SelectedWildSpawnTypes.Count; i++)
            {
                result += SelectedWildSpawnTypes[i].Name;
                if (i != SelectedWildSpawnTypes.Count - 1)
                {
                    result += ", ";
                }
            }
            return result;
        }

        private string ConvertListToString<T>(List<T> list)
        {
            string result = string.Empty;
            for (int i = 0; i < list.Count; i++)
            {
                result += list[i].ToString();
                if (i != list.Count - 1)
                {
                    result += ", ";
                }
            }
            return result;
        }

        private BotDifficulty EditDifficulty => SelectedDifficulties[SelectedDifficulties.Count - 1];

        private void Reset()
        {
            SelectedDifficulties.Clear();
            SelectedSections.Clear();
            SelectedWildSpawnTypes.Clear();
            SelectedProperties.Clear();
        }

        private void CreatePropertyOption(PropertyInfo property, BotType botType)
        {
        }

        private int SelectionGridOption<T>(int spacing, string optionName, List<T> list, T item, float optionPerLine = 3f, float optionHeight = 25f, string tooltip = null)
        {
            spacing = CheckSpacing(spacing, Mathf.RoundToInt(optionPerLine));

            tooltip = tooltip ?? string.Empty;

            bool selected = list.Contains(item);
            if (Toggle(selected, optionName, tooltip, Height(optionHeight), Width(RectLayout.MainWindow.width / optionPerLine - 20f)))
            {
                if (!list.Contains(item))
                {
                    list.Add(item);
                }
            }
            else
            {
                if (list.Contains(item))
                {
                    list.Remove(item);
                }
            }
            if (CheckDragLayout())
            {
                if (!list.Contains(item))
                {
                    list.Add(item);
                }
            }
            return spacing;
        }

        private int CheckSpacing(int spacing, int check)
        {
            if (spacing == check)
            {
                spacing = 0;
                EndHorizontal();
                BeginHorizontal();
            }
            spacing++;
            return spacing;
        }
    }
}
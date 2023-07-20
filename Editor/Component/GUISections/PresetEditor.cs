using EFT.Visual;
using SAIN.BotPresets;
using SAIN.Editor.Abstract;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static SAIN.Editor.Names;
using SAIN.Editor.GUISections;
using UnityEngine.UIElements;
using BepInEx.Configuration;
using static UnityEngine.EventSystems.EventTrigger;
using System.Windows.Forms.VisualStyles;

namespace SAIN.Editor
{
    public class PresetEditor : EditorAbstract
    {
        public PresetEditor(SAINEditor editor) : base(editor)
        {
            List<string> sections = new List<string>();
            WildSpawnTypes = PresetManager.BotTypes;
            foreach (var type in WildSpawnTypes)
            {
                if (!sections.Contains(type.Section))
                {
                    sections.Add(type.Section);
                }
            }

            Sections = sections.ToArray();
        }

        public void OpenPresetWindow(Rect windowRect)
        {
            PresetWindow = windowRect;
            Rect12Height = windowRect.height;

            OpenFirstMenu = BuilderClass.ExpandableMenu("Bots", OpenFirstMenu, "Select the bots you wish to edit the settings for");
            if (OpenFirstMenu)
            {
                SectionRectangle = new Rect(0, 30, SectionRectWidth, FinalWindowHeight);
                BeginArea(SectionRectangle);
                SelectSection(SectionRectangle);
                EndArea();

                TypeRectangle = new Rect(TypeRectX, 30, TypeRectWidth, FinalWindowHeight);

                BeginArea(TypeRectangle);

                FirstMenuScroll = BeginScrollView(FirstMenuScroll);
                SelectType();
                EndScrollView();

                EndArea();

                Space(SectionRectangle.height + SectionRectangle.y);
                BeginHorizontal();

                float width = PresetWindow.width / 3;
                if (Button("Confirm", width, true))
                {
                    OpenFirstMenu = false;
                }
                if (Button("Clear", width, true))
                {
                    SelectedSections.Clear();
                    SelectedWildSpawnTypes.Clear();
                }
                OpenAdjustmentWindow = Toggle(OpenAdjustmentWindow, "Open GUI Adjustment", width, true);

                EndHorizontal();
                DifficultyTable();
            }

            OpenPropertySelection = BuilderClass.ExpandableMenu("Properties", OpenPropertySelection, "Select which properties you wish to modify.");
            if (OpenPropertySelection)
            {
                PropertyMenu();
            }

            OpenPresetEditMenu = BuilderClass.ExpandableMenu("Presets", OpenPresetEditMenu, "Modify selected properties here");
            if (OpenPresetEditMenu)
            {
                PresetEditWindow();
            }
        }

        bool OpenFirstMenu = true;
        bool OpenPropertyMenu = false;
        bool OpenPresetEditMenu = false;
        Rect PresetWindow;

        float FinalWindowWidth => Editor.WindowLayoutCreator.FinalWindowWidth;

        float FinalWindowHeight => 300f;

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

        private Rect RelativeRect( Rect mainRect , Rect insideRect, Rect lastRect)
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

        Rect[] SectionRects;

        private void SelectSection(Rect window)
        {
            if (SectionRects == null)
            {
                SectionRects = BuilderClass.VerticalGridRects(window, Sections.Length, 80f);
            }

            BuilderClass.SelectionGridExpandWidth(window, Sections, SelectedSections, SectionRects, 70f, 5);
        }

        private void SelectType()
        {
            for (int i = 0; i < SelectedSections.Count;i++) 
            {
                string section = SelectedSections[i];
                GUIStyle style = new GUIStyle(GetStyle(StyleNames.label))
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

                for (int j = 0; j < WildSpawnTypes.Length; j++)
                {
                    BotType type = WildSpawnTypes[j];
                    if (type.Section == section)
                    {
                        GUIStyle style2 = new GUIStyle(GetStyle(StyleNames.toggle))
                        {
                            fontStyle = FontStyle.Normal,
                            alignment = TextAnchor.LowerCenter
                        };
                        bool typeSelected = SelectedWildSpawnTypes.Contains(type);

                        BeginHorizontal();
                        Space(Rect2OptionSpacing);

                        bool AddToList = Toggle(typeSelected, type.Name, style2, Height(TypeOptOptionHeight));

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
        private List<string> SelectedSections = new List<string>();

        private readonly BotType[] WildSpawnTypes;
        private readonly List<BotType> SelectedWildSpawnTypes = new List<BotType>();

        private void DifficultyTable(int optionsPerLine = 2, float labelHeight = 35f, float labelWidth = 200f, float optionHeight = 25f)
        {
            OpenDifficultySelection = BuilderClass.ExpandableMenu("Difficulties", OpenDifficultySelection, "Select which difficulties you wish to modify.");
            if (!OpenDifficultySelection)
            {
                return;
            }

            BeginHorizontal();

            int spacing = 0;
            for (int i = 0; i < BotDifficultyOptions.Length; i++)
            {
                var diff = BotDifficultyOptions[i];

                spacing = SelectionGridOption(
                    spacing,
                    diff.ToString(),
                    SelectedDifficulties,
                    diff,
                    optionsPerLine,
                    optionHeight);
            }

            EndHorizontal();
        }

        private bool OpenDifficultySelection = false;
        public readonly BotDifficulty[] BotDifficultyOptions = { BotDifficulty.easy, BotDifficulty.normal, BotDifficulty.hard, BotDifficulty.impossible };
        private readonly List<BotDifficulty> SelectedDifficulties = new List<BotDifficulty>();
        private bool SelectAllDifficulties = false;

        private void PropertyMenu(int optionsPerLine = 3, float labelHeight = 35f, float labelWidth = 200f, float optionHeight = 25f, float scrollHeight = 200f)
        {
            PropertyScroll = BeginScrollView(PropertyScroll, Height(scrollHeight));

            BeginHorizontal();

            var properties = PresetManager.Properties;
            int spacing = 0;
            for (int i = 0; i < properties.Count; i++)
            {
                spacing = SelectionGridOption(
                    spacing,
                    properties[i].Name,
                    SelectedProperties,
                    properties[i],
                    optionsPerLine,
                    optionHeight);
            }

            EndHorizontal();
            EndScrollView();
        }

        private bool OpenPropertySelection = false;
        private readonly List<PropertyInfo> SelectedProperties = new List<PropertyInfo>();
        private bool SelectAllProperties = false;
        private static Vector2 PropertyScroll = Vector2.zero;
        private static bool ExpandPresetEditor = false;

        private void PresetEditWindow()
        {
            if (SelectedProperties.Count > 0 && SelectedWildSpawnTypes.Count > 0 && SelectedDifficulties.Count > 0)
            {
                BeginHorizontal();

                BotType typeInEdit = SelectedWildSpawnTypes[0];
                Box("Editing: " + SelectedWildSpawnTypes.Count + " Bots for difficulties: " + SelectedDifficulties, Height(35f));

                if (Button("Save", Height(35f), Width(200f)))
                {
                    SaveValues(typeInEdit);
                    return;
                }
                if (Button("Discard", Height(35f)))
                {
                    Reset();
                    return;
                }

                FlexibleSpace();
                EndHorizontal();

                for (int i = 0; i < SelectedProperties.Count; i++)
                {
                    CreatePropertyOption(SelectedProperties[i], typeInEdit);
                }
            }
        }

        private void SaveValues(BotType editingType)
        {
            return;
            foreach (var diff in SelectedDifficulties)
            {
                foreach (var Property in SelectedProperties)
                {
                    try
                    {
                        object value = PresetManager.GetPresetValue(editingType, Property, diff);
                        PresetManager.SetPresetValue(value, SelectedWildSpawnTypes, Property, SelectedDifficulties);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(Property.Name, GetType(), true);
                        Logger.LogError(ex.Message, GetType(), true);
                    }
                }
            }

            Reset();
        }

        private void Reset()
        {
            SelectedDifficulties.Clear();
            SelectedSections.Clear();
            SelectedWildSpawnTypes.Clear();
            SelectedProperties.Clear();
        }

        private void CreatePropertyOption(PropertyInfo property, BotType botType)
        {
            BotDifficulty diff = SelectedDifficulties[SelectedDifficulties.Count - 1];

            if (property.PropertyType == typeof(SAINProperty<float>))
            {
                SAINProperty<float> floatProperty = PresetManager.GetSainProp<float>(botType, property);
                BuilderClass.HorizSlider(floatProperty, diff);
            }
            else if (property.PropertyType == typeof(SAINProperty<bool>))
            {
                SAINProperty<bool> boolProperty = PresetManager.GetSainProp<bool>(botType, property);
                ButtonsClass.ButtonProperty(boolProperty, diff);
            }
            else
            {
                Logger.LogError("Value is not float or bool!", GetType(), true);
            }
        }

        private int SelectionGridOption<T>(int spacing, string optionName, List<T> list, T item, float optionPerLine = 3f, float optionHeight = 25f)
        {
            spacing = CheckSpacing(spacing, Mathf.RoundToInt(optionPerLine));

            bool selected = list.Contains(item);
            if (Toggle(selected, optionName, Height(optionHeight), Width(RectLayout.MainWindow.width / optionPerLine - 20f)))
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

        private bool LabelAndAll(string labelName, bool value, float height = 35, float width = 200f)
        {
            BeginHorizontal();

            Box(labelName, true);
            value = Toggle(value, "All", true);

            FlexibleSpace();

            EndHorizontal();

            return value;
        }
    }
}
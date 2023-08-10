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
    public class BotSelectionClass : EditorAbstract
    {
        public BotSelectionClass(SAINEditor editor) : base(editor)
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

        public void Menu()
        {
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

                if (Button("Clear", null, Width(150)))
                {
                    SelectedSections.Clear();
                    SelectedWildSpawnTypes.Clear();
                }
            }

            BeginHorizontal();
            ButtonsClass.InfoBox("Select which difficulties you wish to modify.");
            Label("Difficulties", Height(25));
            EndHorizontal();

            BeginHorizontal();
            for (int i = 0; i < BotDifficultyOptions.Length; i++)
            {
                var diff = BotDifficultyOptions[i];
                if (Toggle(diff == EditDifficulty, diff.ToString(), EFT.UI.EUISoundType.MenuCheckBox, Height(25)))
                {
                    EditDifficulty = diff;
                }
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

        private float FinalWindowHeight => 300f;

        private float Rect2OptionSpacing = 2f;
        private float SectionRectX => BothRectGap;
        private float SectionRectWidth = 150f;
        private float BothRectGap = 6f;
        private float TypeRectWidth => RectLayout.MainWindow.width - SectionRectWidth - BothRectGap * 2f;
        private float TypeRectX => SectionRectX + SectionRectWidth + BothRectGap;

        private float TypeOptLabelHeight = 18f;
        private float TypeOptOptionHeight = 19f;

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

        private Rect? RelativeRectLastRectMainWindow(Rect insideRect)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return null;
            }
            return RelativeRectMainWindow(insideRect, GUILayoutUtility.GetLastRect());
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
                        bool typeSelected = SelectedWildSpawnTypes.Contains(type);

                        BeginHorizontal();
                        Space(Rect2OptionSpacing);

                        bool AddToList = Toggle(typeSelected, new GUIContent(type.Name, type.Description), GetStyle(Style.botTypeGrid), null, Height(TypeOptOptionHeight));

                        // Rect? lastRect = RelativeRectLastRectMainWindow(TypeRectangle);
                        // if (lastRect != null && CheckDrag(lastRect.Value))
                        // {
                        //     AddToList = !Editor.ShiftKeyPressed;
                        // }

                        if (AddToList && !SelectedWildSpawnTypes.Contains(type))
                        {
                            SelectedWildSpawnTypes.Clear();
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

        private void PropEditMenu()
        {
            if (SelectedWildSpawnTypes.Count == 0)
            {
                Box("No Bots Selected");
                return;
            }

            BeginHorizontal();

            BotType typeInEdit = SelectedWildSpawnTypes[0];

            if (Button("Save", "Apply Values set below to all selected bot types for all selected difficulties. Saves edited values to SAIN/Presets folder", null, Height(35f), Width(200f)))
            {
                SAINPlugin.LoadedPreset.SavePreset();
                PresetHandler.UpdateExistingBots();
            }
            if (Button("Clear", "Clear all selected bots", null, Height(35f), Width(200f)))
            {
                Reset();
            }

            EndHorizontal();

            Label($"Currently Editing: [{typeInEdit.Name}] at difficulty [{EditDifficulty}]", Height(35f));

            if (EditingSettings == null || EditingType != typeInEdit.WildSpawnType || EditingDifficulty != EditDifficulty)
            {
                EditingType = typeInEdit.WildSpawnType;
                EditingDifficulty = EditDifficulty;
                EditingSettings = SAINPlugin.LoadedPreset.BotSettings.GetSAINSettings(EditingType, EditingDifficulty);
            }
            Scroll = BeginScrollView(Scroll);
            Editor.GUITabs.SettingsEditor.SettingsMenu(EditingSettings);
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

        private BotDifficulty EditDifficulty;

        private void Reset()
        {
            SelectedDifficulties.Clear();
            SelectedSections.Clear();
            SelectedWildSpawnTypes.Clear();
        }
    }
}
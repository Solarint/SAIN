using EFT;
using EFT.UI;
using SAIN.Attributes;
using SAIN.Editor.Abstract;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.BotSettings.SAINSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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

        public void ClearCache()
        {
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
            FlexibleSpace();

            ButtonsClass.InfoBox("Select which difficulties you wish to modify.", Height(25), Width(30));
            Label("Difficulties", Height(25), Width(150));

            FlexibleSpace();
            EndHorizontal();

            Builder.ModifyLists.AddOrRemove(SelectedDifficulties, out bool newEdit);

            SelectProperties();
        }

        public bool WasEdited;

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

            string toolTip = $"Apply Values set below to selected Bot Type. " +
                $"Exports edited values to SAIN/Presets/{SAINPlugin.LoadedPreset.Info.Name}/BotSettings folder";
;
            if (Builder.SaveChanges(BotSettingsWereEdited, toolTip, 35))
            {
                SAINPlugin.LoadedPreset.ExportBotSettings();
            }
            if (Button("Clear All", "Clear all selected bot options", null, Height(35f), Width(200f)))
            {
                SelectedDifficulties.Clear();
                SelectedSections.Clear();
                SelectedWildSpawnTypes.Clear();
            }

            EndHorizontal();
        }

        public bool BotSettingsWereEdited;

        private void SelectProperties()
        {
            if (SelectedWildSpawnTypes.Count == 0)
            {
                Box("No Bots Selected");
                return;
            }

            BeginHorizontal();

            string toolTip = $"Apply Values set below to selected Bot Type. " +
                $"Exports edited values to SAIN/Presets/{SAINPlugin.LoadedPreset.Info.Name}/BotSettings folder";
            ;
            if (Builder.SaveChanges(BotSettingsWereEdited, toolTip, 35))
            {
                SAINPlugin.LoadedPreset.ExportBotSettings();
            }
            if (Button("Clear All", "Clear all selected bot options", null, Height(35f), Width(200f)))
            {
                SelectedDifficulties.Clear();
                SelectedSections.Clear();
                SelectedWildSpawnTypes.Clear();
            }

            EndHorizontal();

            GUIEntryConfig entryConfig = new GUIEntryConfig
            {
                MinMaxWidth = 0,
                ResultWidth = 0.065f,
                ResetWidth = 0.03f
            };

            var container = Editor.GUITabs.SettingsEditor.
                SelectSettingsGUI(
                typeof(SAINSettingsClass), 
                "Select Options to Edit", out bool newEdit);
            if (newEdit)
            {
                BotSettingsWereEdited = true;
            }

            container.SecondOpen = Builder.ExpandableMenu("Edit Selected Options", container.SecondOpen);
            if (!container.SecondOpen)
            {
                return;
            }
            container.SecondScroll = BeginScrollView(container.SecondScroll, Height(400));
            foreach (var category in container.SelectedCategories)
            {
                FieldInfo CategoryField = category.Field;
                List<AttributesInfoClass> attributes = category.SelectedList;
                foreach (var fieldAttribute in category.SelectedList)
                {
                    foreach (var bot in SelectedWildSpawnTypes)
                    {
                        if (SAINPlugin.LoadedPreset.BotSettings.SAINSettings.TryGetValue(bot.WildSpawnType, out var settings))
                        {
                            foreach (var difficulty in SelectedDifficulties)
                            {
                                if (settings.Settings.TryGetValue(difficulty, out var SAINSettings))
                                {
                                    try
                                    {
                                        object categoryValue = CategoryField.GetValue(SAINSettings);
                                        object value = fieldAttribute.Field.GetValue(categoryValue);

                                        BeginHorizontal();

                                        Label(
                                            $"{bot.Name} {difficulty}", 
                                            Height(entryConfig.EntryHeight), Width(150));

                                        value = AttributesGUI.EditValue(value, fieldAttribute, out newEdit, entryConfig);
                                        if (newEdit)
                                        {
                                            WasEdited = true;
                                        }

                                        EndHorizontal();

                                        fieldAttribute.Field.SetValue(categoryValue, value);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.LogError(ex);
                                    }
                                }
                                else
                                {
                                    Logger.LogError(difficulty);
                                }
                            }
                        }
                        else
                        {
                            Logger.LogError(bot.WildSpawnType);
                        }
                    }
                }
            }
            EndScrollView();
        }
    }
}
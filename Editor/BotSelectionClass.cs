using SAIN.Attributes;
using SAIN.Editor.Abstract;
using SAIN.Editor.Util;
using SAIN.Preset;
using SAIN.Preset.BotSettings.SAINSettings;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static HarmonyLib.AccessTools;

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
            OpenFirstMenu = Builder.ExpandableMenu("Bots", OpenFirstMenu, "Select the bots you wish to edit the settings for", 35);
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


            Label("Difficulties", "Select which difficulties you wish to modify.", Height(35));
            Builder.ModifyLists.AddOrRemove(SelectedDifficulties, out bool newEdit);

            SelectProperties();
        }

        public bool WasEdited;

        private bool OpenFirstMenu = false;

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

        public bool BotSettingsWereEdited;

        int offsetWidth = 3;
        int offsetHeight = 3;

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
            if (Builder.SaveChanges(BotSettingsWereEdited, toolTip, 30f))
            {
                SAINPlugin.LoadedPreset.ExportBotSettings();
            }
            if (Button("Clear All", "Clear all selected bot options", null, Height(30f), Width(200f)))
            {
                SelectedDifficulties.Clear();
                SelectedSections.Clear();
                SelectedWildSpawnTypes.Clear();
            }

            EndHorizontal();

            GUIEntryConfig entryConfig = new GUIEntryConfig
            {
                EntryHeight = 30,
                SliderWidth = 0.45f,
                MinMaxWidth = 0f,
                ResultWidth = 0.065f,
                ResetWidth = 0.05f
            };

            var container = Editor.GUITabs.SettingsEditor.
                SelectSettingsGUI(
                typeof(SAINSettingsClass),
                "Select Options to Edit", out bool newEdit);
            if (newEdit)
            {
                BotSettingsWereEdited = true;
            }

            container.SecondOpen = Builder.ExpandableMenu("Edit Selected Options", container.SecondOpen, null, 30f);
            if (!container.SecondOpen)
            {
                return;
            }
            const float ScreenWidth = 1920;
            float LineHeight = entryConfig.EntryHeight + 6;

            const float FieldLabelWidth = 275;
            const float BotLabelWidth = 225;

            const float Remaining = ScreenWidth - FieldLabelWidth - BotLabelWidth;
            float TotalHeight = LineHeight * SelectedDifficulties.Count * SelectedWildSpawnTypes.Count;

            var noOffset = new RectOffset(0,0,0,0);
            var boxStyle = new GUIStyle( GetStyle(Style.box) )
            {
                padding = noOffset,
                margin = new RectOffset(5, 10, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
            };

            var blankStyle = new GUIStyle( GetStyle(Style.blankbox) )
            {
                padding = noOffset,
                margin = new RectOffset(5, 10, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
            };

            Rect totalRect = GUILayoutUtility.GetRect(
                ScreenWidth, TotalHeight * 5,
                blankStyle);

            GUI.BeginGroup(totalRect, blankStyle);

            int count = 0;
            for (int i = 0; i < container.SelectedCategories.Count; i++)
            {
                var category = container.SelectedCategories[i];

                FieldInfo CategoryField = category.Field;
                for (int j = 0; j < category.SelectedList.Count; j++)
                {
                    var fieldAttribute = category.SelectedList[j];

                    Rect fieldRect = new Rect(0, TotalHeight * count, ScreenWidth - 15, TotalHeight);
                    count++;
                    GUI.BeginGroup(fieldRect, blankStyle);

                    Rect fieldGroupLabelRect = new Rect(0, 0, FieldLabelWidth, TotalHeight);
                    Rect UsedfieldGroupLabelRect = new Rect(offsetWidth, offsetHeight, FieldLabelWidth - offsetWidth * 2, TotalHeight - offsetHeight * 2);
                    ApplyColor(boxStyle, fieldAttribute.Name);
                    GUI.Box(UsedfieldGroupLabelRect, new GUIContent(fieldAttribute.Name, fieldAttribute.Description), boxStyle);

                    for (int k = 0; k < SelectedWildSpawnTypes.Count; k++)
                    {
                        var bot = SelectedWildSpawnTypes[k];

                        if (SAINPlugin.LoadedPreset.BotSettings.SAINSettings.TryGetValue(bot.WildSpawnType, out var settings))
                        {
                            float botGroupHeight = LineHeight * SelectedDifficulties.Count;
                            Rect botGroupRect = new Rect(fieldGroupLabelRect.width, botGroupHeight * k, ScreenWidth - 15 - FieldLabelWidth, botGroupHeight);
                            GUI.BeginGroup(botGroupRect, blankStyle);

                            ApplyColor(boxStyle, bot.Name);
                            Rect botGroupLabelRect = new Rect(0, 0, BotLabelWidth, botGroupHeight);
                            Rect UsedbotGroupLabelRect = new Rect(offsetWidth, offsetHeight, BotLabelWidth - offsetWidth * 2, botGroupHeight - offsetHeight * 2);

                            GUI.Box(UsedbotGroupLabelRect, new GUIContent(bot.Name), boxStyle);

                            Rect ValuesRect = new Rect(botGroupLabelRect.width, 0, Remaining, botGroupHeight);
                            GUILayout.BeginArea(ValuesRect, blankStyle);

                            for (int t = 0; t < SelectedDifficulties.Count; t++)
                            {
                                var difficulty = SelectedDifficulties[t];
                                if (settings.Settings.TryGetValue(difficulty, out var SAINSettings))
                                {
                                    Space(3f);

                                    BeginHorizontal();

                                    object categoryValue = CategoryField.GetValue(SAINSettings);
                                    object value = fieldAttribute.Field.GetValue(categoryValue);

                                    ApplyColor(boxStyle, difficulty.ToString());
                                    Label($"{difficulty}", boxStyle,
                                        Height(entryConfig.EntryHeight), Width(200));

                                    value = AttributesGUI.EditFloatBoolInt(value, fieldAttribute, entryConfig, out newEdit, false, false);
                                    if (newEdit)
                                    {
                                        WasEdited = true;
                                    }

                                    fieldAttribute.Field.SetValue(categoryValue, value);

                                    EndHorizontal();

                                    Space(3f);
                                }
                                else
                                {
                                    Logger.LogError(difficulty);
                                }
                            }
                            EndArea();
                            EndGroup();
                        }
                        else
                        {
                            Logger.LogError(bot.WildSpawnType);
                        }
                    }
                    EndGroup();
                }
            }

            EndGroup();
        }

        private void ApplyColor(GUIStyle style, string key)
        {
            var texture = TexturesClass.GetRandomGray(key);
            ApplyToStyle.BGAllStates(style, texture);
        }
    }
}
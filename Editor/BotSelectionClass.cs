using EFT.UI;
using SAIN.Attributes;
using SAIN.Editor.GUISections;
using SAIN.Editor.Util;
using SAIN.Preset;
using SAIN.Preset.BotSettings.SAINSettings;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static SAIN.Editor.SAINLayout;

namespace SAIN.Editor
{
    public static class BotSelectionClass
    {
        static BotSelectionClass()
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
            SectionOpens = new bool[Sections.Length];
        }

        private static bool[] SectionOpens;

        public static void Menu()
        {
            BeginHorizontal();
            FlexibleSpace();

            float sectionWidth = 1850f / Sections.Length;
            for (int i = 0; i < Sections.Length; i++)
            {
                BeginVertical();

                var botTypeSectionStyle = new GUIStyle(GetStyle(Style.toggle))
                {
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset(5, 5, 0, 0),
                    border = new RectOffset(5, 5, 0, 0),
                    fontStyle = FontStyle.Bold
                };

                string section = Sections[i];
                SectionOpens[i] = Toggle(SectionOpens[i], new GUIContent(section), botTypeSectionStyle, EUISoundType.MenuDropdown, Height(35), Width(sectionWidth));
                if (SectionOpens[i])
                {
                    ModifyLists.AddOrRemove(SelectedBotTypes, section, 27.5f, sectionWidth);
                }

                EndVertical();
            }

            FlexibleSpace();
            EndHorizontal();

            Space(3f);

            if (Button("Clear", EUISoundType.ButtonBottomBarClick))
            {
                SelectedBotTypes.Clear();
            }

            Label("Difficulties", "Select which difficulties you wish to modify.", Height(35));
            ModifyLists.AddOrRemove(SelectedDifficulties, out bool newEdit);

            SelectProperties();
        }

        public static readonly string[] Sections;

        private static readonly List<BotType> SelectedBotTypes = new List<BotType>();

        public static readonly BotDifficulty[] BotDifficultyOptions = { BotDifficulty.easy, BotDifficulty.normal, BotDifficulty.hard, BotDifficulty.impossible };
        public static readonly List<BotDifficulty> SelectedDifficulties = new List<BotDifficulty>();

        public static bool BotSettingsWereEdited;

        private static int offsetWidth = 3;
        private static int offsetHeight = 3;

        private static void SelectProperties()
        {
            if (SelectedBotTypes.Count == 0)
            {
                Box("No Bots Selected");
                return;
            }

            BeginHorizontal();

            string toolTip = $"Apply Values set below to selected Bot Type. " +
                $"Exports edited values to SAIN/Presets/{SAINPlugin.LoadedPreset.Info.Name}/BotSettings folder";
            ;
            if (BuilderClass.SaveChanges(BotSettingsWereEdited, toolTip, 35f))
            {
                SAINPlugin.LoadedPreset.ExportBotSettings();
            }
            if (Button("Clear All", "Clear all selected bot options", null, Height(35f), Width(200f)))
            {
                SelectedDifficulties.Clear();
                SelectedBotTypes.Clear();
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

            var container = BotSettingsEditor.
                SelectSettingsGUI(
                typeof(SAINSettingsClass),
                "Select Options to Edit", out bool newEdit);
            if (newEdit)
            {
                BotSettingsWereEdited = true;
            }

            container.SecondOpen = BuilderClass.ExpandableMenu("Edit Selected Options", container.SecondOpen, null);
            if (!container.SecondOpen)
            {
                return;
            }
            const float ScreenWidth = 1920;
            float LineHeight = entryConfig.EntryHeight + 6;

            const float FieldLabelWidth = 275;
            const float BotLabelWidth = 225;

            const float Remaining = ScreenWidth - FieldLabelWidth - BotLabelWidth;
            float TotalHeight = LineHeight * SelectedDifficulties.Count * SelectedBotTypes.Count;

            var noOffset = new RectOffset(0, 0, 0, 0);
            var boxStyle = new GUIStyle(GetStyle(Style.box))
            {
                padding = noOffset,
                margin = new RectOffset(5, 10, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
            };

            var blankStyle = new GUIStyle(GetStyle(Style.blankbox))
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

                    for (int k = 0; k < SelectedBotTypes.Count; k++)
                    {
                        var bot = SelectedBotTypes[k];

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

                                    object categoryValue = category.GetValue(SAINSettings);
                                    object value = fieldAttribute.GetValue(categoryValue);

                                    ApplyColor(boxStyle, difficulty.ToString());
                                    Label($"{difficulty}", boxStyle,
                                        Height(entryConfig.EntryHeight), Width(200));

                                    value = AttributesGUI.EditFloatBoolInt(value, fieldAttribute, entryConfig, out newEdit, false, false);
                                    if (newEdit)
                                    {
                                        BotSettingsWereEdited = true;
                                    }

                                    fieldAttribute.SetValue(categoryValue, value);

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

        private static void ApplyColor(GUIStyle style, string key)
        {
            var texture = TexturesClass.GetRandomGray(key);
            ApplyToStyle.BackgroundAllStates(texture, style);
        }
    }
}
using EFT;
using System;
using System.Reflection;
using System.Windows.Forms.VisualStyles;
using UnityEngine;
using static HBAO_Core;

namespace SAIN.Editor
{
    internal class PresetEditor
    {
        private static bool SelectTypeOpen = false;
        private static bool SelectDiffOpen = false;
        public static string[] BotTypeOptions => SAINBotPresetManager.WildSpawnTypes;
        public static string[] BotDifficultyOptions => SAINBotPresetManager.Difficulties;

        public static int SelectedType = 0;
        public static int SelectedDifficulty = 0;

        private static SAINBotPreset PresetInEdit;

        private static Vector2 scrollPosition = Vector2.zero;
        public static void PresetSelectionMenu()
        {
            float width = RectLayout.TabRectangle.width / 2f;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load", GUILayout.Height(50), GUILayout.Width(150)))
            {
                PresetInEdit = LoadPreset();
            }
            if (GUILayout.Button("Save", GUILayout.Height(50), GUILayout.Width(150)))
            {
                if (PresetInEdit != null)
                {
                    SAINBotPresetManager.SavePreset(PresetInEdit);
                    PresetInEdit = null;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            if (GUILayout.Button(BotTypeOptions[SelectedType], GUILayout.Width(width)))
            {
                SelectTypeOpen = !SelectTypeOpen;
            }
            int selType;
            if (SelectTypeOpen)
            {
                float buttonHeight = 20f;
                float dropdownHeight = Mathf.Min(BotTypeOptions.Length * buttonHeight, 6 * buttonHeight); // Limit dropdown height to show 8 entries at a time

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(dropdownHeight), GUILayout.Width(width - 20));

                selType = GUILayout.SelectionGrid(SelectedType, BotTypeOptions, 2, GUILayout.Width(width - 40));

                GUILayout.EndScrollView();

                // Close the dropdown when an option is selType
                if (selType != SelectedType)
                {
                    SelectedType = selType;
                    SelectTypeOpen = false;
                }
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            int selDiff;
            if (GUILayout.Button(BotDifficultyOptions[SelectedDifficulty], GUILayout.Width(width)))
            {
                SelectDiffOpen = !SelectDiffOpen;
            }

            if (SelectDiffOpen)
            {
                selDiff = GUILayout.SelectionGrid(SelectedDifficulty, BotDifficultyOptions, 1, GUILayout.Width(width));

                // Close the dropdown when an option is selType
                if (selDiff != SelectedDifficulty)
                {
                    SelectedDifficulty = selDiff;
                    SelectDiffOpen = false;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (PresetInEdit != null)
            {
                GUILayout.BeginVertical();
                CreateOptions(PresetInEdit);
                GUILayout.EndVertical();
            }
        }

        private static void CreateOptions(SAINBotPreset preset)
        {
            GUILayout.Label("Editing: " + preset.BotType.ToString() + " " + preset.Difficulty.ToString());
            PropertyInfo[] properties = preset.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                Type propertyType = property.PropertyType;
                if (propertyType == typeof(SAINProperty<float>))
                {
                    var floatProperty = (SAINProperty<float>)property.GetValue(preset);
                    CreatePropertySlider(floatProperty);
                }
                else if (propertyType == typeof(SAINProperty<int>))
                {
                    var intProperty = (SAINProperty<int>)property.GetValue(preset);
                    CreatePropertySlider(intProperty);
                }
                else if (propertyType == typeof(SAINProperty<bool>))
                {
                    var boolProperty = (SAINProperty<bool>)property.GetValue(preset);
                    BuilderUtil.CreateButtonOption(boolProperty);
                }
            }
        }

        private static void CreatePropertySlider(SAINProperty<float> property)
        {
            BuilderUtil.HorizSlider(property);
        }

        private static void CreatePropertySlider(SAINProperty<int> property)
        {
            BuilderUtil.HorizSlider(property.Name, property.Value, property.Min, property.Max, 1f, property.Description);
        }

        private static SAINBotPreset LoadPreset()
        {
            WildSpawnType convertedType = (WildSpawnType)Enum.Parse(typeof(WildSpawnType), BotTypeOptions[SelectedType]);
            BotDifficulty convertedDiff = (BotDifficulty)Enum.Parse(typeof(BotDifficulty), BotDifficultyOptions[SelectedDifficulty]);
            // Perform any desired actions with the selType option
            Console.WriteLine(convertedType.ToString() + " and " + convertedDiff.ToString());
            return SAINBotPresetManager.LoadPreset(convertedType, convertedDiff);
        }
    }
}

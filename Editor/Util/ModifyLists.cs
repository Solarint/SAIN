using EFT;
using EFT.UI;
using SAIN.Helpers;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings.Categories;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.Editor.SAINLayout;

namespace SAIN.Editor.Util
{
    public static class ModifyLists
    {
        public static void AddOrRemove(List<BigBrainConfigClass> list, out bool wasEdited, int optionsPerLine = 4)
        {
            wasEdited = false;
            if (list != null)
            {
                int i = StartListEdit(optionsPerLine, out var options);
                foreach (BigBrainConfigClass brain in BigBrainSettings.DefaultBrains)
                {
                    AddOrRemove(brain, list, out bool newEdit, null, null, options);
                    if (newEdit)
                    {
                        wasEdited = true;
                    }
                    i = ListSpacing(i, optionsPerLine);
                }
                EndListEdit();
            }
        }

        public static void AddOrRemove(List<WildSpawnType> list, out bool wasEdited, int optionsPerLine = 4)
        {
            wasEdited = false;
            if (list != null)
            {
                int i = StartListEdit(optionsPerLine, out var options);
                foreach (var botType in BotTypeDefinitions.BotTypes.Values)
                {
                    AddOrRemove(botType.WildSpawnType, list, out bool newEdit, botType.Name, botType.Description, options);
                    if (newEdit)
                    {
                        wasEdited = true;
                    }
                    i = ListSpacing(i, optionsPerLine);
                }
                EndListEdit();
            }
        }

        public static void AddOrRemove(SettingsContainer container, out bool wasEdited, string search = null, int optionsPerLine = 5)
        {
            wasEdited = false;
            foreach (var category in container.Categories)
            {
                string categoryName = category.CategoryAttributes.Name;
                string categoryDesciption = category.CategoryAttributes.Description;

                // Display the value of the category. And make it a openable dropdown menu
                if (string.IsNullOrEmpty(search))
                {
                    category.Open = BuilderClass.ExpandableMenu(categoryName, category.Open, categoryDesciption);
                    if (!category.Open)
                    {
                        continue;
                    }
                }
                else
                {
                    Label(categoryName, categoryDesciption);
                }
                bool newEdit;
                int i = StartListEdit(optionsPerLine, out var options);
                // Get the fields in this category
                foreach (var fieldAtt in category.FieldAttributes)
                {
                    // Check if the user is searching
                    if (!string.IsNullOrEmpty(search) && !fieldAtt.Name.ToLower().Contains(search))
                    {
                        continue;
                    }

                    // Add or remove this field from the list
                    AddOrRemove(fieldAtt, category.SelectedList, out newEdit, fieldAtt.Name, fieldAtt.Description, options);
                    if (newEdit)
                    {
                        wasEdited = true;
                    }
                    // Send interation count to our spacing function
                    i = ListSpacing(i++, optionsPerLine);
                }
                // PeekEnd this list edit.
                EndListEdit();

                AddOrRemove(category, container.SelectedCategories, category.SelectedList.Count > 0, out newEdit);
                if (newEdit)
                {
                    wasEdited = true;
                }
            }
        }

        private static void AddOrRemove<T>(T item, List<T> list, bool value, out bool wasEdited)
        {
            wasEdited = false;
            if (value)
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
        }

        public static void AddOrRemove(List<BotDifficulty> list, out bool wasEdited, int optionsPerLine = 4)
        {
            wasEdited = false;
            int i = StartListEdit(optionsPerLine, out var options);
            foreach (var dificulty in EnumValues.Difficulties)
            {
                AddOrRemove(dificulty, list, out bool newEdit, null, null, options);
                if (newEdit)
                {
                    wasEdited = true;
                }
                i = ListSpacing(i, optionsPerLine);
            }
            EndListEdit();
        }

        public static void AddOrRemove(List<BotType> list, out bool wasEdited, int optionsPerLine = 5)
        {
            wasEdited = false;
            int i = StartListEdit(optionsPerLine, out var options);
            foreach (var botType in BotTypeDefinitions.BotTypes.Values)
            {
                AddOrRemove(botType, list, out bool newEdit, botType.Name, botType.Description, options);
                if (newEdit)
                {
                    wasEdited = true;
                }
                i = ListSpacing(i, optionsPerLine);
            }
            EndListEdit();
        }

        public static void AddOrRemove(List<BotType> list, string section, float height, float width)
        {
            foreach (var botType in BotTypeDefinitions.BotTypes.Values)
            {
                if (botType.Section == section)
                {
                    AddOrRemove(botType, list, out bool newEdit, botType.Name, botType.Description, Height(height), Width(width));
                }
            }
        }

        private static void AddOrRemove<T>(T value, List<T> list, out bool wasEdited, string name = null, string description = null, params GUILayoutOption[] options)
        {
            wasEdited = false;
            if (list != null)
            {
                bool toggleValue = Toggle(
                    list.Contains(value),
                    new GUIContent(name ?? value.ToString(), description),
                    GetStyle(Style.selectionList),
                    EUISoundType.MenuCheckBox,
                    options);

                AddOrRemove(value, list, toggleValue, out bool newEdit);
                if (newEdit)
                {
                    wasEdited = true;
                }
            }
        }

        private static int StartListEdit(int optionsPerLine, out GUILayoutOption[] dimensions, float gridWidth = 1875f)
        {
            BeginVertical();
            BeginHorizontal();
            Space(5);

            float width = (gridWidth / optionsPerLine).Round10();
            dimensions = new GUILayoutOption[]
            {
                Height(27.5f), Width(width),
            };
            return 0;
        }

        private static int ListSpacing(int i, int max)
        {
            i++;
            if (i >= max)
            {
                i = 0;
                Space(5);
                EndHorizontal();
                BeginHorizontal();
            }
            return i;
        }

        private static void EndListEdit()
        {
            EndHorizontal();
            EndVertical();
        }
    }
}
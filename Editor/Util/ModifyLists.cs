using EFT;
using EFT.UI;
using SAIN.Attributes;
using SAIN.Editor.Abstract;
using SAIN.Helpers;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings.Categories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using UnityEngine;
using static Mono.Security.X509.X520;

namespace SAIN.Editor.Util
{
    public class ModifyLists : EditorAbstract
    {
        public ModifyLists(SAINEditor editor) : base(editor)
        {
        }

        public void ClearCache()
        {
        }

        public void AddOrRemove(List<BigBrainConfigClass> list, out bool wasEdited, int optionsPerLine = 4)
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

        public void AddOrRemove(List<WildSpawnType> list, out bool wasEdited, int optionsPerLine = 4)
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

        public void AddOrRemove(SettingsContainer container, out bool wasEdited, string search = null, int optionsPerLine = 5)
        {
            wasEdited = false;
            foreach (var category in container.Categories)
            {
                string categoryName = category.CategoryAttributes.Name;
                string categoryDesciption = category.CategoryAttributes.Description;

                // Display the name of the category. And make it a openable dropdown menu
                if (string.IsNullOrEmpty(search))
                {
                    category.Open = Builder.ExpandableMenu(categoryName, category.Open, categoryDesciption);
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
                // End this list edit.
                EndListEdit();

                AddOrRemove(category, container.SelectedCategories, category.SelectedList.Count > 0, out newEdit);
                if (newEdit)
                {
                    wasEdited = true;
                }
            }
        }

        private void AddOrRemove<T>(T item, List<T> list, bool value, out bool wasEdited)
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

        public void AddOrRemove(List<BotDifficulty> list, out bool wasEdited, int optionsPerLine = 4)
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

        public void AddOrRemove(List<BotType> list, out bool wasEdited, int optionsPerLine = 5)
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

        private void AddOrRemove<T>(T value, List<T> list, out bool wasEdited, string name = null, string description = null, params GUILayoutOption[] options)
        {
            wasEdited = false;
            if (list != null)
            {
                bool toggleValue = Builder.
                    Toggle(
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

        private int StartListEdit(int optionsPerLine, out GUILayoutOption[] dimensions)
        {
            BeginVertical();
            BeginHorizontal();
            FlexibleSpace();

            float width = Mathf.Round(1800 / optionsPerLine * 10) / 10;
            float height = 22.5f;
            dimensions = new GUILayoutOption[]
            {
                Height(height), Width(width),
            };
            return 0;
        }

        private int ListSpacing(int i, int max)
        {
            i++;
            if (i >= max)
            {
                i = 0;
                FlexibleSpace();
                EndHorizontal();
                BeginHorizontal();
                FlexibleSpace();
            }
            return i;
        }

        private void EndListEdit()
        {
            FlexibleSpace();
            EndHorizontal();
            EndVertical();
        }
    }
}
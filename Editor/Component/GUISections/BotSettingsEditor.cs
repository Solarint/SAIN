using EFT;
using SAIN.BotPresets;
using SAIN.Classes;
using SAIN.BotSettings;
using SAIN.Editor.Abstract;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SAIN.BotSettings.Categories;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;

namespace SAIN.Editor.GUISections
{
    public class BotSettingsEditor : EditorAbstract
    {
        public BotSettingsEditor(SAINEditor editor) : base(editor)
        {
        }

        public void EditMenu(bool open = true)
        {
            if (open)
            {
                BeginVertical();

                SAINSettings testSettings = new SAINSettings();
                List<object> categories = GetCategories(testSettings);
                CategoryOpenable(categories);

                EndVertical();
            }
            else
            {

            }
        }

        static List<object> GetCategories(object settingsObject)
        {
            List<object> categories = new List<object>();
            foreach (FieldInfo field in settingsObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                categories.Add(field.GetValue(settingsObject));
            }
            return categories;
        }

        void CategoryOpenable(List<object> categories)
        {
            if (OpenCategories == null)
            {
                OpenCategories = new bool[categories.Count];
            }

            for (int i = 0; i < categories.Count; i++)
            {
                object category = categories[i];
                bool open = OpenCategories[i];

                Type categoryType = category.GetType();
                FieldInfo[] categoryFields = GetFields(categoryType);

                if (categoryFields.Length > 0)
                {
                    string name = categoryType.GetCustomAttribute<NameAttribute>()?.Name ?? categoryType.Name;
                    string description = categoryType.GetCustomAttribute<DescriptionAttribute>()?.Description;

                    open = Builder.ExpandableMenu(name, open, description, EntryConfig.EntryHeight, EntryConfig.InfoWidth);
                    if (open)
                    {
                        foreach (FieldInfo field in categoryFields)
                        {
                            object value = field.GetValue(category);
                            value = GetAttributeValue.AttributesGUI.EditValue(value, field, EntryConfig);
                            field.SetValue(category, value);
                        }
                    }
                }

                OpenCategories[i] = open;
            }
        }

        readonly GetAttributeValue.GUIEntryConfig EntryConfig = new GetAttributeValue.GUIEntryConfig();
        bool[] OpenCategories;

        FieldInfo[] GetFields(Type type) => Editor.SAINSettingsCache.GetFields(type);
    }
}

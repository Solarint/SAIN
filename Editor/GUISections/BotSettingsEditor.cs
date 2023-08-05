using SAIN.BotSettings;
using SAIN.Editor.Abstract;
using SAIN.SAINPreset.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.Editor.GUISections
{
    public class BotSettingsEditor : EditorAbstract
    {
        public BotSettingsEditor(SAINEditor editor) : base(editor)
        {
        }

        public void EditMenu(SAINSettings settings)
        {
            BeginVertical();

            List<object> categories = GetCategories(settings);
            CategoryOpenable(categories);

            EndVertical();
        }

        private static List<object> GetCategories(object settingsObject)
        {
            List<object> categories = new List<object>();
            foreach (FieldInfo field in settingsObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                categories.Add(field.GetValue(settingsObject));
            }
            return categories;
        }

        private void CategoryOpenable(List<object> categories)
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
                    foreach (FieldInfo field in categoryFields)
                    {
                        var nameDesc = field.GetCustomAttribute<NameAndDescriptionAttribute>();
                        string name = nameDesc?.Name ?? field.Name;
                        string description = nameDesc?.Description;

                        open = Builder.ExpandableMenu(name, open, description, EntryConfig.EntryHeight, EntryConfig.InfoWidth);
                        if (open)
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

        private readonly GetAttributeValue.GUIEntryConfig EntryConfig = new GetAttributeValue.GUIEntryConfig();
        private bool[] OpenCategories;

        private FieldInfo[] GetFields(Type type) => Editor.SAINBotSettingsCache.GetFields(type);
    }
}
using SAIN.Attributes;
using SAIN.Editor.Abstract;
using SAIN.Preset.BotSettings.SAINSettings.Categories;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAIN.Editor.GUISections
{
    public class BotSettingsEditor : EditorAbstract
    {
        public BotSettingsEditor(SAINEditor editor) : base(editor)
        {
        }

        public void SettingsMenu(object settings, FieldsCache fieldCache = null)
        {
            BeginVertical();

            Type type = settings.GetType();
            if (!Categories.ContainsKey(type))
            {
                Categories.Add(type, GetCategories(settings));
            }

            if (fieldCache == null)
            {
                fieldCache = new FieldsCache(type);
            }

            CategoryOpenable(Categories[type], fieldCache, settings);

            EndVertical();
        }

        private static List<Category> GetCategories(object settingsObject)
        {
            List<Category> result = new List<Category>();
            var fields = settingsObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                result.Add(new Category
                {
                    Field = field,
                });
            }
            return result;
        }

        private readonly Dictionary<Type, List<Category>> Categories = new Dictionary<Type, List<Category>>();

        public sealed class Category
        {
            public FieldInfo Field;
            public bool Open = false;
            public int OptionsCount = 0;

            public object Value(object settingsObject) => Field.GetValue(settingsObject);

            public void SetValue(object categoryObject, object settingsObject) => Field.SetValue(settingsObject, categoryObject);

        }

        private void CategoryOpenable(List<Category> categories, FieldsCache fieldCache, object settingsObject)
        {
            foreach (var categoryClass in categories)
            {
                FieldInfo categoryField = categoryClass.Field;
                object categoryObject = categoryClass.Value(settingsObject);

                var nameDesc = categoryField.GetCustomAttribute<NameAndDescriptionAttribute>();
                string name = nameDesc?.Name ?? categoryField.Name;
                string description = nameDesc?.Description;

                FieldInfo[] variableFields = fieldCache.GetFields(categoryObject.GetType());
                if (variableFields.Length > 0)
                {
                    BeginHorizontal();

                    categoryClass.Open = Builder.ExpandableMenu(name, categoryClass.Open, description, EntryConfig.EntryHeight, EntryConfig.InfoWidth, false);
                    string labelText = $"Options Count: [{categoryClass.OptionsCount}]";
                    string advanced = Editor.AdvancedOptionsEnabled ?
                        " Advanced Options are on, so the reason for this section being empty is that the values are hidden because they SHOULD NOT be changed under any circumstance."
                        : " Advanced Options in the Advanced Tab may show more options.";
                    string toolTip = $"The Number of Options available in this section. {advanced}";
                    Label(labelText, toolTip, Height(EntryConfig.EntryHeight), Width(150));

                    EndHorizontal();

                    AttributesGUI.EditAllValuesInObj(categoryObject, out int count, categoryClass.Open);
                    categoryClass.OptionsCount = count;
                }
            }
        }

        private readonly GUIEntryConfig EntryConfig = new GUIEntryConfig();
    }
}
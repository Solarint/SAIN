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

        public object SettingsMenu(object settings, FieldsCache fieldCache)
        {
            BeginVertical();

            Type type = settings.GetType();
            if (!Categories.ContainsKey(type))
            {
                Categories.Add(type, GetCategories(settings));
            }

            Categories[type] = CategoryOpenable(Categories[type], fieldCache, settings, out object modifiedObject);

            EndVertical();

            return modifiedObject;
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
            public object Value(object settingsObject) => Field.GetValue(settingsObject);
            public void SetValue(object categoryObject, object settingsObject) => Field.SetValue(settingsObject, categoryObject);
            public bool Open = false;
        }

        private Dictionary<Type, bool[]> CategoriesOpenableValues = new Dictionary<Type, bool[]>();

        private List<Category> CategoryOpenable(List<Category> categories, FieldsCache fieldCache, object settingsObject, out object modifiedSettingsObject)
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
                    categoryClass.Open = Builder.ExpandableMenu(name, categoryClass.Open, description, EntryConfig.EntryHeight, EntryConfig.InfoWidth);
                    if (categoryClass.Open)
                    {
                        foreach (FieldInfo field in variableFields)
                        {
                            object value = field.GetValue(categoryObject);
                            value = AttributesGUI.EditValue(value, field, EntryConfig);
                            field.SetValue(categoryObject, value);
                        }
                        categoryClass.SetValue(categoryObject, settingsObject);
                    }
                }
            }
            modifiedSettingsObject = settingsObject;
            return categories;
        }

        private readonly GUIEntryConfig EntryConfig = new GUIEntryConfig();
    }
}
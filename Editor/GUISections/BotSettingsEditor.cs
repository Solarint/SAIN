using SAIN.Attributes;
using SAIN.Editor.Abstract;
using SAIN.Helpers;
using SAIN.Preset.BotSettings.SAINSettings.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAIN.Editor.GUISections
{
    public class BotSettingsEditor : EditorAbstract
    {
        public BotSettingsEditor(SAINEditor editor) : base(editor)
        {
        }

        public void ClearCache()
        {
            ListHelpers.ClearCache(Categories);
        }

        public void SettingsMenu(object settings)
        {
            BeginVertical();

            Type type = settings.GetType();
            if (!Categories.ContainsKey(type))
            {
                Categories.Add(type, GetCategories(settings));
            }

            CategoryOpenable(Categories[type], settings);

            EndVertical();
        }

        private static List<Category> GetCategories(object settingsObject)
        {
            List<Category> result = new List<Category>();
            var fields = settingsObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                var attInfo = AttributesGUI.GetAttributeInfo(field);
                if (attInfo.AdvancedOptions.Contains(AdvancedEnum.Hidden))
                {
                    continue;
                }
                result.Add(new Category(field));
            }
            return result;
        }

        private readonly Dictionary<Type, List<Category>> Categories = new Dictionary<Type, List<Category>>();

        public sealed class Category
        {
            public Category(FieldInfo field)
            {
                Field = field;
                foreach (FieldInfo subFields in field.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    var attInfo = AttributesGUI.GetAttributeInfo(subFields);
                    if (attInfo.AdvancedOptions.Contains(AdvancedEnum.Hidden))
                    {
                        continue;
                    }
                    OptionsCount++;
                }
            }

            public readonly FieldInfo Field;
            public readonly int OptionsCount = 0;

            public bool Open = false;

            public object Value(object settingsObject) => Field.GetValue(settingsObject);

            public void SetValue(object categoryObject, object settingsObject) => Field.SetValue(settingsObject, categoryObject);

        }

        private void CategoryOpenable(List<Category> categories, object settingsObject)
        {
            foreach (var categoryClass in categories)
            {
                FieldInfo categoryField = categoryClass.Field;
                var attInfo = AttributesGUI.GetAttributeInfo(categoryField);
                if (attInfo.DoNotShowGUI)
                {
                    continue;
                }

                object categoryObject = categoryClass.Value(settingsObject);

                FieldInfo[] variableFields = categoryField.FieldType.GetFields(BindingFlags.Instance | BindingFlags.Public);
                if (variableFields.Length > 0)
                {
                    BeginHorizontal();

                    categoryClass.Open = Builder.ExpandableMenu(attInfo.Name, categoryClass.Open, attInfo.Description, EntryConfig.EntryHeight, EntryConfig.InfoWidth, false);
                    string labelText = $"Options Count: [{categoryClass.OptionsCount}]";
                    string advanced = Editor.AdvancedOptionsEnabled ?
                        " Advanced Options are on, so the reason for this section being empty is that the values are hidden because they SHOULD NOT be changed under any circumstance."
                        : " Advanced Options in the Advanced Tab may show more options.";
                    string toolTip = $"The Number of Options available in this section. {advanced}";
                    Label(labelText, toolTip, Height(EntryConfig.EntryHeight), Width(150));

                    EndHorizontal();

                    if (categoryClass.Open)
                    {
                        AttributesGUI.EditAllValuesInObj(categoryObject);
                    }
                }
            }
        }

        private readonly GUIEntryConfig EntryConfig = new GUIEntryConfig();
    }
}
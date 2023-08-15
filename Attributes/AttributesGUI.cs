using EFT;
using EFT.UI;
using SAIN.Editor;
using SAIN.Helpers;
using SAIN.Preset;
using SAIN.Preset.BotSettings.SAINSettings;
using SAIN.Preset.GlobalSettings.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SAIN.Attributes
{
    public class AttributesGUI
    {
        public static AttributesInfoClass GetAttributeInfo(FieldInfo field)
        {
            string name = field.Name + field.DeclaringType.Name;
            if (!AttributesClasses.ContainsKey(name))
            {
                AttributesClasses.Add(name, new AttributesInfoClass(field));
            }
            return AttributesClasses[name];
        }

        public static AttributesInfoClass GetAttributeInfo(PropertyInfo property)
        {
            string name = property.Name + property.DeclaringType.Name;
            if (!AttributesClasses.ContainsKey(name))
            {
                AttributesClasses.Add(name, new AttributesInfoClass(property));
            }
            return AttributesClasses[name];
        }

        public static void ClearCache()
        {
            if (ListHelpers.ClearCache(AttributesClasses))
            {
                LabelStyle = null;
            }
        }

        private static readonly Type[] FloatBoolInt =
        {
            typeof(bool),
            typeof(float),
            typeof(int),
        };

        private static readonly GUIEntryConfig DefaultEntryConfig = new GUIEntryConfig();

        private static GUIStyle LabelStyle;
        private static readonly Dictionary<string, AttributesInfoClass> AttributesClasses = new Dictionary<string, AttributesInfoClass>();

        public static object EditValue(object value, AttributesInfoClass attributes, out bool wasEdited, GUIEntryConfig entryConfig = null)
        {
            wasEdited = false;
            if (value != null && attributes != null && !attributes.DoNotShowGUI)
            {
                entryConfig = entryConfig ?? DefaultEntryConfig;

                if (FloatBoolInt.Contains(attributes.ValueType))
                {
                    value = EditFloatBoolInt(value, attributes, entryConfig, out wasEdited);
                }
                else
                {
                    bool isDictionary = attributes.EListType == EListType.Dictionary;
                    bool isList = attributes.EListType == EListType.List;

                    if ((isDictionary || isList) && ExpandableList(attributes, entryConfig.EntryHeight))
                    {
                        if (isDictionary)
                        {
                            EditAllValuesInObj(value, out wasEdited);
                        }
                        else if (value is List<WildSpawnType> wildList)
                        {
                            Builder.ModifyLists.AddOrRemove(wildList, out wasEdited);
                            value = wildList;
                        }
                        else if (value is List<BotDifficulty> diffList)
                        {
                            Builder.ModifyLists.AddOrRemove(diffList, out wasEdited);
                            value = diffList;
                        }
                        else if (value is List<BotType> botList)
                        {
                            Builder.ModifyLists.AddOrRemove(botList, out wasEdited);
                            value = botList;
                        }
                        else if (value is List<BigBrainConfigClass> brainList)
                        {
                            Builder.ModifyLists.AddOrRemove(brainList, out wasEdited);
                            value = brainList;
                        }
                    }
                }
            }
            return value;
        }

        public static object EditFloatBoolInt(object value, AttributesInfoClass attributes, GUIEntryConfig entryConfig, out bool wasEdited, bool showLabel = true, bool beginHoriz = true)
        {
            if (beginHoriz)
            {
                Builder.BeginHorizontal();
                Builder.Space(15);
            }

            if (showLabel)
            {
                var labelHeight = Builder.Height(entryConfig.EntryHeight);
                Buttons.InfoBox(attributes.Description, entryConfig.Info);

                if (LabelStyle == null)
                {
                    GUIStyle boxstyle = Buttons.GetStyle(Style.box);
                    LabelStyle = new GUIStyle(Buttons.GetStyle(Style.label))
                    {
                        alignment = TextAnchor.MiddleLeft,
                        margin = boxstyle.margin,
                        padding = boxstyle.padding
                    };
                }

                Builder.Box(new GUIContent(attributes.Name), LabelStyle, labelHeight);
            }

            float min = default;
            float max = default;
            bool showResult = false;
            object originalValue = value;

            if (attributes.ValueType == typeof(bool))
            {
                showResult = true;

                value = Builder.Toggle((bool)value, (bool)value ? "On" : "Off", EUISoundType.MenuCheckBox, entryConfig.Toggle);
            }
            else if (attributes.ValueType == typeof(float))
            {
                showResult = true;

                if (!MinMaxFloat(out min, out max, attributes))
                {
                    min = Mathf.Round(((float)value / 10f) * 10f) / 10f;
                    max = Mathf.Round(((float)value * 5f) * 10f) / 10f;
                }

                //Builder.MinValueBox(min, entryConfig.Info);

                float flValue = Builder.CreateSlider((float)value, min, max, entryConfig.Toggle);
                float rounding = attributes.Rounding == null ? 10f : attributes.Rounding.Value;
                value = Mathf.Round(flValue * rounding) / rounding;

                //Builder.MaxValueBox(max, entryConfig.Info);
            }
            else if (attributes.ValueType == typeof(int))
            {
                showResult = true;

                if (!MinMaxInt(out int intMin, out int intMax, attributes))
                {
                    min = Mathf.RoundToInt((int)value / 10f);
                    max = Mathf.RoundToInt((int)value * 5f);
                }
                else
                {
                    min = intMin;
                    max = intMax;
                }

                //Builder.MinValueBox(min, entryConfig.Info);

                float floatvalue = Builder.CreateSlider((int)value, min, max, entryConfig.Toggle);
                value = Mathf.RoundToInt(floatvalue);

                //Builder.MaxValueBox(max, entryConfig.Info);
            }
            if (showResult && value != null)
            {
                string dirtyString = Builder.TextField(value.ToString(), null, entryConfig.Result);
                value = Builder.CleanString(dirtyString, value);
                value = Clamp(value, min, max, attributes.ValueType);

                if (attributes.Default != null)
                {
                    if (Builder.Button("Reset", "Reset To Default Value", EUISoundType.ButtonClick, entryConfig.Reset))
                        value = attributes.Default;
                }
                else
                {
                    Builder.Box(" ", "No Default Value is assigned to this option.", entryConfig.Reset);
                }
            }

            if (beginHoriz)
            {
                Builder.Space(15);
                Builder.EndHorizontal();
            }
            wasEdited = originalValue.ToString() != value.ToString();
            return value;
        }

        private static int RoundObjectToInt(object value)
        {
            return Mathf.RoundToInt((float)value);
        }

        private static bool ExpandableList(AttributesInfoClass attributes, float height)
        {
            Builder.BeginHorizontal();

            string name = attributes.Name;
            if (!ListIsOpen.ContainsKey(name))
            {
                ListIsOpen.Add(name, false);
            }
            bool isOpen = ListIsOpen[name];
            isOpen = Builder.ExpandableMenu(name, isOpen, attributes.Description, height, 30f, false);
            ListIsOpen[name] = isOpen;

            Builder.EndHorizontal();
            return isOpen;
        }

        private static readonly Dictionary<string, bool> ListIsOpen = new Dictionary<string, bool>();

        private static object Clamp(object value, float min, float max, Type valueType)
        {
            float floatVal;
            if (valueType == typeof(float))
            {
                floatVal = (float)value;
                value = Mathf.Clamp(floatVal, min, max);
            }
            if (valueType == typeof(int))
            {
                int intVal = (int)value;
                floatVal = intVal;
                value = Mathf.RoundToInt(Mathf.Clamp(floatVal, min, max));
            }
            return value;
        }

        private static bool MinMaxFloat(out float min, out float max, AttributesInfoClass attributes)
        {
            min = default;
            max = default;
            if (attributes.Min != null)
            {
                min = attributes.Min.Value;

                if (attributes.Max != null)
                {
                    max = attributes.Max.Value;
                    return true;
                }
            }
            return false;
        }

        private static bool MinMaxInt(out int min, out int max, AttributesInfoClass attributes)
        {
            min = default;
            max = default;
            if (attributes.Min != null)
            {
                min = Mathf.RoundToInt(attributes.Min.Value);

                if (attributes.Max != null)
                {
                    max = Mathf.RoundToInt(attributes.Max.Value);
                    return true;
                }
            }
            return false;
        }

        public static object EditValue(object value, FieldInfo field, out bool wasEdited, GUIEntryConfig entryConfig = null)
        {
            return EditValue(value, GetAttributeInfo(field), out wasEdited, entryConfig);
        }

        public static void EditAllValuesInObj(object obj, out bool wasEdited, string search = null)
        {
            wasEdited = false;
            Builder.BeginVertical();
            Builder.Space(5);
            foreach (var field in obj.GetType().GetFields())
            {
                var attributes = GetAttributeInfo(field);
                if (SkipForSearch(attributes, search))
                {
                    continue;
                }
                object value = field.GetValue(obj);
                object newValue = EditValue(value, attributes, out bool newEdit);
                if (newEdit)
                {
                    if (SAINPlugin.DebugModeEnabled)
                    {
                        Logger.LogInfo($"{field.Name} was edited");
                    }
                    field.SetValue(obj, newValue);
                    wasEdited = true;
                }
            }

            Builder.Space(5);
            Builder.EndVertical();
        }

        public static void EditAllValuesInObj(Category category, object categoryObject, out bool wasEdited, string search = null)
        {
            wasEdited = false;
            Builder.BeginVertical();
            Builder.Space(5);
            foreach (var fieldAtt in category.FieldAttributes)
            {
                if (SkipForSearch(fieldAtt, search))
                {
                    continue;
                }
                object value = fieldAtt.Field.GetValue(categoryObject);
                object newValue = EditValue(value, fieldAtt, out bool newEdit);
                if (newEdit)
                {
                    if (SAINPlugin.DebugModeEnabled)
                    {
                        Logger.LogInfo($"{fieldAtt.Name} was edited");
                    }
                    fieldAtt.Field.SetValue(categoryObject, newValue);
                    wasEdited = true;
                }
            }

            Builder.Space(5);
            Builder.EndVertical();
        }

        public static bool SkipForSearch(AttributesInfoClass attributes, string search)
        {
            return !string.IsNullOrEmpty(search) && 
                (attributes.Name.ToLower().Contains(search) == false || 
                attributes.Description?.ToLower().Contains(search) == false);
        }

        public static void EditFieldInAllObjects(FieldInfo targetField, FieldInfo targetCategory, List<BotDifficulty> difficulties, List<SAINSettingsGroupClass> settings, out bool wasEdited)
        {
            wasEdited = false;
            if (settings.Count == 0)
            {
                return;
            }
            Builder.BeginVertical();
            Builder.Space(5);

            foreach (SAINSettingsGroupClass setting in settings)
            {
                Builder.Label($"{setting.Name}");
                Builder.Space(5);
                foreach (var keyValuePair in setting.Settings)
                {
                    if (difficulties.Contains(keyValuePair.Key))
                    {
                        if (targetCategory != null)
                        {
                            object targetCategoryObject = targetCategory.GetValue(keyValuePair.Value);
                            object value = targetField.GetValue(targetCategoryObject);

                            Builder.BeginHorizontal();
                            Builder.Label($"{keyValuePair.Key}");
                            object newValue = EditValue(value, targetField, out bool newEdit);
                            if (newEdit)
                            {
                                if (SAINPlugin.DebugModeEnabled)
                                {
                                    Logger.LogInfo($"{targetField.Name} was edited");
                                }
                                targetField.SetValue(setting, newValue);
                                wasEdited = true;
                            }
                            Builder.EndHorizontal();
                        }
                    }
                }
            }
            Builder.Space(5);
            Builder.EndVertical();
        }

        private static ButtonsClass Buttons => SAINPlugin.Editor.Buttons;
        private static BuilderClass Builder => SAINPlugin.Editor.Builder;
    }
}
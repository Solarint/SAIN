using EFT;
using EFT.UI;
using SAIN.Editor;
using SAIN.Editor.Util;
using SAIN.Helpers;
using SAIN.Preset;
using SAIN.Preset.BotSettings.SAINSettings;
using SAIN.Preset.GlobalSettings.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SAIN.Editor.SAINLayout;

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
                            ModifyLists.AddOrRemove(wildList, out wasEdited);
                            value = wildList;
                        }
                        else if (value is List<BotDifficulty> diffList)
                        {
                            ModifyLists.AddOrRemove(diffList, out wasEdited);
                            value = diffList;
                        }
                        else if (value is List<BotType> botList)
                        {
                            ModifyLists.AddOrRemove(botList, out wasEdited);
                            value = botList;
                        }
                        else if (value is List<BigBrainConfigClass> brainList)
                        {
                            ModifyLists.AddOrRemove(brainList, out wasEdited);
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
                BeginHorizontal();
                Space(15);
            }

            if (showLabel)
            {

                if (LabelStyle == null)
                {
                    GUIStyle boxstyle = GetStyle(Style.box);
                    LabelStyle = new GUIStyle(GetStyle(Style.label))
                    {
                        alignment = TextAnchor.MiddleLeft,
                        margin = boxstyle.margin,
                        padding = boxstyle.padding
                    };
                }

                Box(new GUIContent(
                    attributes.Name, 
                    attributes.Description), 
                    LabelStyle, 
                    Height(entryConfig.EntryHeight)
                    );
            }

            Space(8);

            bool showResult = false;
            object originalValue = value;
            if (attributes.ValueType == typeof(bool))
            {
                showResult = true;
                value = Toggle((bool)value, (bool)value ? "On" : "Off", EUISoundType.MenuCheckBox, entryConfig.Toggle);
            }
            else if (attributes.ValueType == typeof(float))
            {
                showResult = true;
                float flValue = BuilderClass.CreateSlider((float)value, attributes.Min, attributes.Max, entryConfig.Toggle);
                value = flValue.Round(attributes.Rounding);
            }
            if (showResult && value != null)
            {
                Space(8);

                string dirtyString = TextField(value.ToString(), null, entryConfig.Result);
                value = BuilderClass.CleanString(dirtyString, value);
                if (attributes.ValueType != typeof(bool))
                {
                    value = attributes.Clamp(value);
                }

                Space(5);

                if (attributes.Default != null)
                {
                    if (Button("Reset", "Reset To Default Value", EUISoundType.ButtonClick, entryConfig.Reset))
                        value = attributes.Default;
                }
                else
                {
                    Box(" ", "No Default Value is assigned to this option.", entryConfig.Reset);
                }
            }

            if (beginHoriz)
            {
                Space(15);
                EndHorizontal();
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
            BeginHorizontal();

            string name = attributes.Name;
            if (!ListIsOpen.ContainsKey(name))
            {
                ListIsOpen.Add(name, false);
            }
            bool isOpen = ListIsOpen[name];
            isOpen = BuilderClass.ExpandableMenu(name, isOpen, attributes.Description, height, false);
            ListIsOpen[name] = isOpen;

            EndHorizontal();
            return isOpen;
        }

        private static readonly Dictionary<string, bool> ListIsOpen = new Dictionary<string, bool>();

        private static object Clamp(object value, float min, float max)
        {
            return MathHelpers.ClampObject(value, min, max);
        }

        public static object EditValue(object value, FieldInfo field, out bool wasEdited, GUIEntryConfig entryConfig = null)
        {
            return EditValue(value, GetAttributeInfo(field), out wasEdited, entryConfig);
        }

        public static void EditAllValuesInObj(object obj, out bool wasEdited, string search = null)
        {
            wasEdited = false;
            BeginVertical();
            Space(5);
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

            Space(5);
            EndVertical();
        }

        public static void EditAllValuesInObj(Category category, object categoryObject, out bool wasEdited, string search = null)
        {
            wasEdited = false;
            BeginVertical();
            Space(5);
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

            Space(5);
            EndVertical();
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
            BeginVertical();
            Space(5);

            foreach (SAINSettingsGroupClass setting in settings)
            {
                Label($"{setting.Name}");
                Space(5);
                foreach (var keyValuePair in setting.Settings)
                {
                    if (difficulties.Contains(keyValuePair.Key))
                    {
                        if (targetCategory != null)
                        {
                            object targetCategoryObject = targetCategory.GetValue(keyValuePair.Value);
                            object value = targetField.GetValue(targetCategoryObject);

                            BeginHorizontal();
                            Label($"{keyValuePair.Key}");
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
                            EndHorizontal();
                        }
                    }
                }
            }
            Space(5);
            EndVertical();
        }
    }
}
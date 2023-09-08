using EFT;
using EFT.UI;
using SAIN.Editor;
using SAIN.Editor.Util;
using SAIN.Helpers;
using SAIN.Preset;
using SAIN.Preset.BotSettings.SAINSettings;
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
        public static AttributesInfoClass GetAttributeInfo(MemberInfo member)
        {
            string name = member.Name + member.DeclaringType.Name;
            AddAttributesToDictionary(name, member);
            if (AttributesClasses.TryGetValue(name, out var value))
            {
                return value;
            }
            return null;
        }

        private static void AddAttributesToDictionary(string name, MemberInfo member)
        {
            if (!AttributesClasses.ContainsKey(name) && !FailedAdds.Contains(name))
            {
                var attributes = new AttributesInfoClass(member);
                if (attributes.ValueType != null)
                {
                    AttributesClasses.Add(name, attributes);
                }
                else
                {
                    FailedAdds.Add(name);
                }
            }
        }

        private static readonly List<string> FailedAdds = new List<string>();

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
                else if (ExpandableList(attributes, entryConfig.EntryHeight + 5))
                {
                    if (value is Dictionary<ICaliber, float>)
                    {
                        EditFloatDictionary<ICaliber>(
                            value, attributes, out wasEdited);
                    }
                    else if (value is Dictionary<IWeaponClass, float>)
                    {
                        EditFloatDictionary<IWeaponClass>(
                            value, attributes, out wasEdited);
                    }
                    else if (value is List<WildSpawnType>)
                    {
                        ModifyLists.AddOrRemove(
                            value as List<WildSpawnType>, out wasEdited);
                    }
                    else if (value is List<BotDifficulty>)
                    {
                        ModifyLists.AddOrRemove(
                            value as List<BotDifficulty>, out wasEdited);
                    }
                    else if (value is List<BotType>)
                    {
                        ModifyLists.AddOrRemove(
                            value as List<BotType>, out wasEdited);
                    }
                    else if (value is List<string>)
                    {
                    }
                }
            }
            return value;
        }

        private static void CreateLabelStyle()
        {
            if (LabelStyle == null)
            {
                GUIStyle boxstyle = GetStyle(Style.box);
                LabelStyle = new GUIStyle(GetStyle(Style.label))
                {
                    alignment = TextAnchor.MiddleCenter,
                    margin = boxstyle.margin,
                    padding = boxstyle.padding
                };
            }
        }

        public static object EditFloatBoolInt(object value, AttributesInfoClass attributes, GUIEntryConfig entryConfig, out bool wasEdited, bool showLabel = true, bool beginHoriz = true)
        {
            if (beginHoriz)
            {
                BeginHorizontal(100f);
            }

            if (showLabel)
            {
                CreateLabelStyle();

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
            else if (attributes.ValueType == typeof(int))
            {
                Logger.LogError("Int Not Implemented!");
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
                EndHorizontal(100f);
            }
            wasEdited = originalValue.ToString() != value.ToString();
            return value;
        }

        private static bool ExpandableList(AttributesInfoClass attributes, float height)
        {
            BeginHorizontal(100f);

            string name = attributes.Name;
            if (!ListIsOpen.ContainsKey(name))
            {
                ListIsOpen.Add(name, false);
            }
            bool isOpen = ListIsOpen[name];
            isOpen = BuilderClass.ExpandableMenu(name, isOpen, attributes.Description, height);
            ListIsOpen[name] = isOpen;

            EndHorizontal(100f);
            return isOpen;
        }

        private static readonly Dictionary<string, bool> ListIsOpen = new Dictionary<string, bool>();

        public static object EditValue(object value, FieldInfo field, out bool wasEdited, GUIEntryConfig entryConfig = null)
        {
            return EditValue(value, GetAttributeInfo(field), out wasEdited, entryConfig);
        }

        public static void EditFloatDictionary<T>(object dictValue, AttributesInfoClass attributes, out bool wasEdited) where T : Enum
        {
            BeginVertical(5f);

            float min = attributes.Min;
            float max = attributes.Max;
            float rounding = attributes.Rounding;

            var defaultDictionary = attributes.DefaultDictionary as Dictionary<T, float>;
            var dictionary = dictValue as Dictionary<T, float>;

            T[] array = EnumValues.GetEnum<T>();
            if (array != null && array.Length > 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    Logger.LogInfo(array[i]);
                }
            }
            List<T> list = new List<T>();
            foreach (var entry in dictionary)
            {
                if (entry.Key.ToString() == "Default")
                {
                    continue;
                }
                list.Add(entry.Key);
            }

            CreateLabelStyle();

            wasEdited = false;
            for (int i = 0; i < list.Count; i++)
            {
                BeginHorizontal(150f);

                var item = list[i];
                float originalValue = dictionary[item];
                float floatValue = originalValue;

                Box(new GUIContent(item.ToString()), LabelStyle, Height(DefaultEntryConfig.EntryHeight));
                floatValue = BuilderClass.CreateSlider(floatValue, min, max, DefaultEntryConfig.Toggle).Round(rounding);
                Box(floatValue.ToString(), DefaultEntryConfig.Result);

                if (Button("Reset", EUISoundType.ButtonClick, DefaultEntryConfig.Reset))
                {
                    floatValue = defaultDictionary[item];
                }
                if (floatValue != originalValue)
                {
                    wasEdited = true;
                    dictionary[item] = floatValue;
                }

                EndHorizontal(150f);
            }

            list.Clear();
            EndVertical(5f);
        }

        public static void EditAllValuesInObj(object obj, out bool wasEdited, string search = null)
        {
            wasEdited = false;
            BeginVertical(5f);

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
                    if (SAINPlugin.DebugMode)
                    {
                        Logger.LogInfo($"{field.Name} was edited");
                    }
                    field.SetValue(obj, newValue);
                    wasEdited = true;
                }
            }

            EndVertical(5f);
        }

        public static void EditAllValuesInObj(Category category, object categoryObject, out bool wasEdited, string search = null)
        {
            BeginVertical(5);

            wasEdited = false;
            foreach (var fieldAtt in category.FieldAttributesList)
            {
                if (SkipForSearch(fieldAtt, search))
                {
                    continue;
                }
                object value = fieldAtt.GetValue(categoryObject);
                object newValue = EditValue(value, fieldAtt, out bool newEdit);
                if (newEdit)
                {
                    if (SAINPlugin.DebugMode)
                    {
                        Logger.LogInfo($"{fieldAtt.Name} was edited");
                    }
                    fieldAtt.SetValue(categoryObject, newValue);
                    wasEdited = true;
                }
            }

            EndVertical(5);
        }

        public static bool SkipForSearch(AttributesInfoClass attributes, string search)
        {
            return !string.IsNullOrEmpty(search) &&
                (attributes.Name.ToLower().Contains(search) == false &&
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
                                if (SAINPlugin.DebugMode)
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
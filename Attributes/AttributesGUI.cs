using EFT;
using EFT.UI;
using SAIN.Editor;
using SAIN.Preset;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SAIN.Attributes
{
    public class AttributesGUI
    {
        public AttributesGUI(FieldInfo field)
        {
            Attributes = CheckDictionary(field);
        }

        private static AttributesClass CheckDictionary(FieldInfo field)
        {
            if (!AttributesClasses.ContainsKey(field))
            {
                AttributesClasses.Add(field, new AttributesClass(field));
            }
            return AttributesClasses[field];
        }

        public static void ClearCacheStatic()
        {
            AttributesClasses.Clear();
        }

        public void ClearCache()
        {
            AttributesClasses.Clear();
        }

        private static GUIStyle LabelStyle;
        private static readonly Dictionary<FieldInfo, AttributesClass> AttributesClasses = new Dictionary<FieldInfo, AttributesClass>();

        public static object EditValue(object value, out bool wasEditable, AttributesClass attributes, GUIEntryConfig entryConfig = null)
        {
            wasEditable = false;

            if (value == null || attributes == null)
            {
                return null;
            }
            if (attributes.IsHidden)
            {
                return value;
            }
            if (attributes.IsAdvanced && SAINPlugin.Editor.AdvancedOptionsEnabled == false)
            {
                return value;
            }

            wasEditable = true;
            if (entryConfig == null)
            {
                entryConfig = new GUIEntryConfig();
            }

            Type type = value.GetType();
            var labelHeight = Builder.Height(entryConfig.EntryHeight);

            if (attributes.IsListObject)
            {
                bool isOpen = ExpandableList(attributes, entryConfig.EntryHeight);
                EditAllValuesInObj(value, out int count, isOpen);
            }
            else
            {
                Builder.BeginHorizontal();

                Buttons.InfoBox(attributes.Description, entryConfig.Info);

                if (LabelStyle == null)
                {
                    LabelStyle = Buttons.GetStyle(Style.label);
                    GUIStyle boxstyle = Buttons.GetStyle(Style.box);
                    LabelStyle.alignment = TextAnchor.MiddleLeft;
                    LabelStyle.margin = boxstyle.margin;
                    LabelStyle.padding = boxstyle.padding;
                }

                Buttons.Box(new GUIContent(attributes.Name), LabelStyle, labelHeight);

                bool showResult = false;
                float min;
                float max;
                if (type == typeof(bool))
                {
                    showResult = true;
                    value = Builder.Toggle((bool)value, (bool)value ? "On" : "Off", null, entryConfig.Toggle);
                }
                else if (type == typeof(float))
                {
                    showResult = true;

                    min = attributes.Min == null ? (float)value / 10f : attributes.Min.Value;
                    min = Mathf.Round(min * 10f) / 10f;
                    max = attributes.Max == null ? (float)value * 5f : attributes.Max.Value;
                    max = Mathf.Round(max * 10f) / 10f;

                    Builder.MinValueBox(min, entryConfig.Info);

                    float flValue = Builder.CreateSlider((float)value, min, max, entryConfig.Slider);
                    float rounding = attributes.Rounding == null ? 10f : attributes.Rounding.Value;
                    value = Mathf.Round(flValue * rounding) / rounding;

                    Builder.MaxValueBox(max, entryConfig.Info);
                }
                else if (type == typeof(int))
                {
                    showResult = true;

                    min = attributes.Min == null ? (int)value / 10f : attributes.Min.Value;
                    min = Mathf.Round(min * 10f) / 10f;
                    max = attributes.Max == null ? (int)value * 5f : attributes.Max.Value;
                    max = Mathf.Round(max * 10f) / 10f;

                    Builder.MinValueBox(min, entryConfig.Info);

                    float floatvalue = Builder.CreateSlider((int)value, min, max, entryConfig.Slider);
                    value = Mathf.RoundToInt(floatvalue);

                    Builder.MaxValueBox(max, entryConfig.Info);
                }
                else if (value is List<BotType> botTypeList)
                {
                    if (ExpandableList(attributes, entryConfig.EntryHeight))
                    {
                        Builder.ModifyLists.AddOrRemove(botTypeList);
                        value = botTypeList;
                    }
                }
                else if (value is List<WildSpawnType> wildSpawnList)
                {
                    if (ExpandableList(attributes, entryConfig.EntryHeight))
                    {
                        Builder.ModifyLists.AddOrRemove(wildSpawnList);
                        value = wildSpawnList;
                    }
                }
                else if (value is List<string> stringList)
                {
                    if (ExpandableList(attributes, entryConfig.EntryHeight))
                    {
                        Builder.ModifyLists.AddOrRemove(stringList);
                        value = stringList;
                    }
                }

                if (showResult)
                {
                    ShowResultAndEdit(value, attributes, entryConfig);
                }
                Builder.EndHorizontal();
            }
            return value;
        }

        private static bool ExpandableList(AttributesClass attributes, float height)
        {
            Builder.BeginHorizontal();
            if (!ListIsOpen.ContainsKey(attributes.Field))
            {
                ListIsOpen.Add(attributes.Field, false);
            }
            bool isOpen = ListIsOpen[attributes.Field];
            isOpen = Builder.ExpandableMenu(attributes.Name, isOpen, attributes.Description, height, 30f, false);
            ListIsOpen[attributes.Field] = isOpen;
            Builder.EndHorizontal();
            return isOpen;
        }

        private static readonly Dictionary<FieldInfo, bool> ListIsOpen = new Dictionary<FieldInfo, bool>();

        private static object ShowResultAndEdit(object value, AttributesClass attributes, GUIEntryConfig entryConfig)
        {
            value = Builder.ResultBox(value, entryConfig.Result);

            if (value is float floatVal)
            {
                float min = (float)attributes.Min;
                float max = (float)attributes.Max;
                value = Mathf.Clamp(floatVal, min, max);
            }
            if (value is int intVal)
            {
                int min = (int)attributes.Min;
                int max = (int)attributes.Max;
                value = Mathf.Clamp(intVal, min, max);
            }

            if (attributes.Default != null && Builder.Button("Reset", "Reset To Default Value", EUISoundType.ButtonClick, entryConfig.Reset))
            {
                value = attributes.Default;
            }
            else if (attributes.Default == null)
            {
                Builder.Box("Cannot Reset", "No Default Value is assigned to this option.", entryConfig.Reset);
            }

            return value;
        }

        public static object EditValue(object value, out bool wasEditable, FieldInfo field, GUIEntryConfig entryConfig = null)
        {
            return EditValue(value, out wasEditable, CheckDictionary(field), entryConfig);
        }

        public static void EditAllValuesInObj(object obj, out int optionsCount, bool menuIsOpen)
        {
            optionsCount = 0;
            if (!menuIsOpen)
            {
                return;
            }

            Builder.BeginVertical();
            foreach (var field in obj.GetType().GetFields())
            {
                object value = field.GetValue(obj);
                object newValue = EditValue(value, out bool canEdit, field);
                if (canEdit)
                {
                    optionsCount++;
                }
                if (value.ToString() != newValue.ToString())
                {
                    field.SetValue(obj, newValue);
                }
            }
            Builder.EndVertical();
        }

        /*
        public static bool EditValue(bool value, FieldInfo field, GUIEntryConfig entryConfig = null)
        {
            return (bool)EditValue(value, CheckDictionary(field), entryConfig);
        }
        public static float EditValue(float value, FieldInfo field, GUIEntryConfig entryConfig = null)
        {
            return (float)EditValue(value, CheckDictionary(field), entryConfig);
        }
        public static int EditValue(int value, FieldInfo field, GUIEntryConfig entryConfig = null)
        {
            return (int)EditValue(value, CheckDictionary(field), entryConfig);
        }
        */

        public static T EditValue<T>(T value, out bool wasEditable, FieldInfo field, GUIEntryConfig entryConfig = null)
        {
            return (T)EditValue(value, out wasEditable, CheckDictionary(field), entryConfig);
        }

        public object EditValue(object value, out bool wasEditable, GUIEntryConfig entryConfig = null)
        {
            return EditValue(value, out wasEditable, Attributes, entryConfig);
        }

        private readonly AttributesClass Attributes;
        private static ButtonsClass Buttons => SAINPlugin.Editor.Buttons;
        private static BuilderClass Builder => SAINPlugin.Editor.Builder;
    }
}
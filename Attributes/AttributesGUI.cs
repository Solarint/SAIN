using SAIN.Editor;
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

        static AttributesClass CheckDictionary(FieldInfo field)
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

        static readonly Dictionary<FieldInfo, AttributesClass> AttributesClasses = new Dictionary<FieldInfo, AttributesClass>();

        public static object EditValue(object value, AttributesClass attributes, GUIEntryConfig entryConfig = null)
        {
            if (value == null || attributes == null)
            {
                return null;
            }
            if (attributes.IsHidden)
            {
                return value;
            }
            if (attributes.IsAdvanced && SAINPlugin.SAINEditor.AdvancedOptionsEnabled == false)
            {
                return value;
            }
            if (entryConfig == null)
            {
                entryConfig = new GUIEntryConfig();
            }

            Builder.BeginHorizontal();

            Buttons.InfoBox(attributes.Description, entryConfig.Info);
            GUIStyle style = Buttons.GetStyle(Style.label);
            style.alignment = TextAnchor.MiddleLeft;
            style.margin = new RectOffset(3, 3, 3, 3);
            Buttons.Box(new GUIContent(attributes.Name), style);

            Type type = value.GetType();
            if (type == typeof(bool))
            {
                bool boolVal = (bool)value;
                value = Builder.Toggle(boolVal, boolVal ? "On" : "Off", entryConfig.Slider);
            }
            else
            {
                float min = attributes.Min == null ? 0f : attributes.Min.Value;
                float max = attributes.Max == null ? 1000f : attributes.Max.Value;

                Builder.MinValueBox(min, entryConfig.Info);
                if (type == typeof(float))
                {
                    float flValue = Builder.CreateSlider((float)value, min, max, entryConfig.Slider);
                    float rounding = attributes.Rounding == null ? 10f : attributes.Rounding.Value;
                    value = Mathf.Round(flValue * rounding) / rounding;
                }
                else if (type == typeof(int))
                {
                    float floatvalue = Builder.CreateSlider((int)value, min, max, entryConfig.Slider);
                    value = Mathf.RoundToInt(floatvalue);
                }

                Builder.MaxValueBox(max, entryConfig.Info);
            }

            Builder.ResultBox(value, entryConfig.Result);
            value = Builder.Reset(value, attributes.Default, entryConfig.Reset);

            Builder.EndHorizontal();

            return value;
        }

        public static object EditValue(object value, FieldInfo field, GUIEntryConfig entryConfig = null)
        {
            return EditValue(value, CheckDictionary(field), entryConfig);
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
        public static T EditValue<T>(T value, FieldInfo field, GUIEntryConfig entryConfig = null)
        {
            return (T)EditValue(value, CheckDictionary(field), entryConfig);
        }

        public object EditValue(object value, GUIEntryConfig entryConfig = null)
        {
            return EditValue(value, Attributes, entryConfig);
        }

        readonly AttributesClass Attributes;
        static ButtonsClass Buttons => SAINPlugin.SAINEditor.Buttons;
        static BuilderClass Builder => SAINPlugin.SAINEditor.Builder;
    }
}

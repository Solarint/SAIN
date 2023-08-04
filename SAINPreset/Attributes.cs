using SAIN.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace SAIN.SAINPreset.Attributes
{
    public static class GetAttributeValue
    {
        public class GUIEntryConfig
        {
            const float TargetWidthScale = 1920;

            public GUIEntryConfig(float entryHeight = 25f) 
            { 
                EntryHeight = entryHeight;
            }

            public GUIEntryConfig(float entryHeight, float infoWidth, float labelWidth, float minMaxWidth, float sliderWidth, float resultWidth, float resetWidth)
            {
                EntryHeight = entryHeight;
                InfoWidth = infoWidth;
                LabelWidth = labelWidth;
                MinMaxWidth = minMaxWidth;
                SliderWidth = sliderWidth;
                ResultWidth = resultWidth;
                ResetWidth = resetWidth;
            }

            public void OnGUIEditDimensions()
            {
                GUILayout.BeginVertical();
                EntryHeight = HorizontalSlider(EntryHeight, 10f, 50f);
                InfoWidth = HorizontalSlider(InfoWidth);
                LabelWidth = HorizontalSlider(LabelWidth);
                MinMaxWidth = HorizontalSlider(MinMaxWidth);
                SliderWidth = HorizontalSlider(SliderWidth);
                ResultWidth = HorizontalSlider(ResultWidth);
                ResetWidth = HorizontalSlider(ResetWidth);
                GUILayout.EndVertical();
            }

            static float HorizontalSlider(float value, float min = 0f, float max = 1f, float rounding = 100f) 
            { 
                value = GUILayout.HorizontalSlider(value, min, max); 
                value = Mathf.Round(value * rounding) / rounding;
                return value;
            }

            public float EntryHeight = 25f;

            // Widths of the Entry in the AttributesGUI, from 0 to 1. Which will be scaled to 1080p. All Values should add to 1 to scale properly
            public float InfoWidth = 0.03f;
            public float LabelWidth = 0.15f;
            public float MinMaxWidth = 0.08f;
            public float SliderWidth = 0.35f;
            public float ResultWidth = 0.1f;
            public float ResetWidth = 0.05f;

            public GUILayoutOption[] Info => Params(InfoWidth);
            public GUILayoutOption[] Label => Params(LabelWidth);
            public GUILayoutOption[] MinMax => Params(MinMaxWidth);
            public GUILayoutOption[] Slider => Params(SliderWidth);
            public GUILayoutOption[] Result => Params(ResultWidth);
            public GUILayoutOption[] Reset => Params(ResetWidth);

            GUILayoutOption[] Params(float width0to1) => new GUILayoutOption[]
            {
                GUILayout.Width(width0to1 * TargetWidthScale),
                GUILayout.Height(EntryHeight)
            };
        }

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

        public sealed class AttributesClass
        {
            public AttributesClass(FieldInfo field)
            {
                Name = field.GetCustomAttribute<NameAttribute>()?.Name ?? field.Name;
                Description = field.GetCustomAttribute<DescriptionAttribute>()?.Description;
                Default = field.GetCustomAttribute<DefaultValueAttribute>()?.Value;
                Min = field.GetCustomAttribute<MinimumAttribute>()?.Min;
                Max = field.GetCustomAttribute<MaximumAttribute>()?.Max;
                Rounding = field.GetCustomAttribute<RoundingAttribute>()?.Rounding;
                IsHidden = field.GetCustomAttribute<IsHiddenAttribute>()?.Value == true;
                IsAdvanced = field.GetCustomAttribute<IsAdvancedAttribute>()?.Value == true;
            }

            public readonly string Name;
            public readonly string Description;
            public readonly object Default;
            public readonly float? Min;
            public readonly float? Max;
            public readonly float? Rounding;
            public readonly bool IsHidden = false;
            public readonly bool IsAdvanced = false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NameAttribute : Attribute
    {
        public NameAttribute(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return name; }
        }

        readonly string name;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MinimumAttribute : Attribute
    {
        public MinimumAttribute(float min)
        {
            this.min = min;
        }

        public float Min
        {
            get { return min; }
        }

        readonly float min;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MaximumAttribute : Attribute
    {
        public MaximumAttribute(float max)
        {
            this.max = max;
        }

        public float Max
        {
            get { return max; }
        }

        readonly float max;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RoundingAttribute : Attribute
    {
        public RoundingAttribute(float rounding)
        {
            this.rounding = rounding;
        }

        public float Rounding
        {
            get { return rounding; }
        }

        readonly float rounding;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class IsHiddenAttribute : Attribute
    {
        public IsHiddenAttribute(bool value)
        {
            this.value = value;
        }

        public bool Value
        {
            get { return value; }
        }

        readonly bool value;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class IsAdvancedAttribute : Attribute
    {
        public IsAdvancedAttribute(bool value)
        {
            this.value = value;
        }

        public bool Value
        {
            get { return value; }
        }

        readonly bool value;
    }
}

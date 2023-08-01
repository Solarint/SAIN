using Mono.Security.Cryptography;
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
        public class GUI
        {
            public GUI(FieldInfo field)
            {
                Attributes = new AttributesClass(field);
            }

            public object EditValue(object value, params GUILayoutOption[] options)
            {
                if (Attributes.IsHidden)
                {
                    return value;
                }

                Builder.BeginHorizontal();

                Buttons.InfoBox(Attributes.Description, options);
                Buttons.Label(Attributes.Name ?? Attributes.Key, options);

                Type type = value.GetType();
                if (type == typeof(bool))
                {
                    bool boolVal = (bool)value;
                    value = Builder.Toggle(boolVal, boolVal ? "On" : "Off", options);
                }
                else
                {
                    object min = Attributes.Min ?? 0f;
                    object max = Attributes.Max ?? 500f;

                    Builder.MinValueBox(min, options);
                    if (type == typeof(float))
                    {
                        float flValue = Builder.CreateSlider((float)value, (float)min, (float)max, options);
                        float rounding = Attributes.Rounding == null ? 10f : Attributes.Rounding.Value;
                        value = Mathf.Round(flValue * rounding) / rounding;
                    }
                    else if (type == typeof(int))
                    {
                        value = Builder.CreateSlider((int)value, (int)min, (int)max, options);
                    }

                    Builder.MaxValueBox(max, options);
                }

                Builder.ResultBox(value, options);
                value = Builder.Reset(value, Attributes.Default, options);

                Builder.EndHorizontal();

                return value;
            }

            readonly AttributesClass Attributes;
            public static ButtonsClass Buttons => SAINPlugin.SAINEditor.Buttons;
            public static BuilderClass Builder => SAINPlugin.SAINEditor.Builder;
        }
        public sealed class AttributesClass
        {
            public AttributesClass(FieldInfo field)
            {
                Key = Key(field);
                Name = Name(field);
                Description = Description(field);
                Default = Default(field);
                Min = Minimum(field);
                Max = Maximum(field);
                Rounding = Rounding(field);
                IsHidden = IsHidden(field);
            }

            public readonly string Key;
            public readonly string Name;
            public readonly string Description;
            public readonly object Default;
            public readonly object Min;
            public readonly object Max;
            public readonly float? Rounding;
            public readonly bool IsHidden = false;
        }

        public static string Key(FieldInfo field)
            => field.GetCustomAttribute<KeyAttribute>()?.Key;
        public static string Name(FieldInfo field)
            => field.GetCustomAttribute<NameAttribute>()?.Name;
        public static string Description(FieldInfo field)
            => field.GetCustomAttribute<DescriptionAttribute>()?.Description;
        public static object Default(FieldInfo field)
            => field.GetCustomAttribute<DefaultValueAttribute>()?.Value;
        public static object Minimum(FieldInfo field)
            => field.GetCustomAttribute<MinimumAttribute>()?.Min;
        public static object Maximum(FieldInfo field)
            => field.GetCustomAttribute<MaximumAttribute>()?.Max;
        public static float? Rounding(FieldInfo field) 
            => field.GetCustomAttribute<RoundingAttribute>()?.Rounding;
        public static bool IsHidden(FieldInfo field) 
            => field.GetCustomAttribute<IsHiddenAttribute>()?.Value == true;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class KeyAttribute : Attribute
    {
        public KeyAttribute(string key)
        {
            this.key = key;
        }

        public string Key
        {
            get { return key; }
        }

        readonly string key;
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
        public MinimumAttribute(object min)
        {
            this.min = min;
        }

        public object Min
        {
            get { return min; }
        }

        readonly object min;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MaximumAttribute : Attribute
    {
        public MaximumAttribute(object max)
        {
            this.max = max;
        }

        public object Max
        {
            get { return max; }
        }

        readonly object max;
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
}

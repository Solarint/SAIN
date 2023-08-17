using SAIN.Editor;
using SAIN.Helpers;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SAIN.Attributes
{
    public sealed class AttributesInfoClass
    {
        public AttributesInfoClass(FieldInfo field)
        {
            Field = field;
            ValueType = field.FieldType;
            GetInfo();
        }

        public AttributesInfoClass(PropertyInfo property)
        {
            Property = property;
            ValueType = property.PropertyType;
            GetInfo();
        }

        public object Clamp(object value)
        {
            return MathHelpers.ClampObject(value, Min, Max);
        }

        private MinMaxAttribute MinMax;
        private Percentage0to1Attribute Percentage0to1;
        private PercentageAttribute Percentage;
        private RoundingValueAttribute RoundingValue;

        private void GetInfo()
        {
            var nameDescription = Get<NameAndDescriptionAttribute>();
            Name = nameDescription?.Name ?? Get<NameAttribute>()?.Name ?? Field?.Name ?? Property?.Name;
            Description = nameDescription?.Description ?? Get<DescriptionAttribute>()?.Description;

            SetMinMax();
            SetDefault();

            var advanced = Get<AdvancedAttribute>();
            if (advanced != null)
            {
                AdvancedOptions = advanced.Options;
            }

            var listObj = Get<DictionaryAttribute>();
            if (listObj != null)
            {
                EListType = EListType.Dictionary;
                PrimaryListType = listObj.TypeA;
                SecondaryListType = listObj.TypeB;
                return;
            }

            var list = Get<ListAttribute>();
            if (list != null)
            {
                EListType = EListType.List;
                PrimaryListType = list.Type;
                return;
            }
        }

        private T Get<T>() where T : Attribute
        {
            return Field?.GetCustomAttribute<T>() ?? Property?.GetCustomAttribute<T>();
        }

        public FieldInfo Field { get; private set; }

        public PropertyInfo Property { get; private set; }

        public Type ValueType { get; private set; }

        private void SetNameAndDescription()
        {
            var nameDescription = Get<NameAndDescriptionAttribute>();
            if (nameDescription != null)
            {
                Name = nameDescription.Name;
                Description = nameDescription.Description;
                return;
            }
            var nameAtt = Get<NameAttribute>();
            if (nameAtt != null)
            {
                Name = nameAtt.Name;
            }
            else
            {
                Name = Field?.Name ?? Property?.Name;
            }
            var descAtt = Get<DescriptionAttribute>();
            if (descAtt != null)
            {
                Description = descAtt.Description;
            }
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        private void SetDefault()
        {
            Default = Get<DefaultValueAttribute>()?.Value ?? Get<DefaultAttribute>()?.Value;

            if (Default == null)
            {
                if (ValueType == typeof(float))
                {
                    Default = ((Max - Min) / 2f).Round100();
                }
                else if (ValueType == typeof(int))
                {
                    Default = Mathf.RoundToInt((Max - Min) / 2f);
                }
                else if (ValueType == typeof(bool))
                {
                    Default = false;
                }
            }
        }

        public object Default { get; private set; }

        private void SetMinMax()
        {
            MinMax = Get<MinMaxAttribute>();
            if (MinMax != null)
            {
                Min = MinMax.Min;
                Max = MinMax.Max;
                Rounding = MinMax.Rounding;
                return;
            }
            Percentage = Get<PercentageAttribute>();
            if (Percentage != null)
            {
                Min = Percentage.Min;
                Max = Percentage.Max;
                Rounding = Percentage.Rounding;
                return;
            }
            Percentage0to1 = Get<Percentage0to1Attribute>();
            if (Percentage0to1 != null)
            {
                Min = Percentage0to1.Min;
                Max = Percentage0to1.Max;
                Rounding = Percentage0to1.Rounding;
                return;
            }
            RoundingValue = Get<RoundingValueAttribute>();
            if (MinMax != null)
            {
                Rounding = MinMax.Rounding;
                return;
            }
            Min = 0f;
            Max = 100f;
            Rounding = 10f;
        }

        public float Min { get; private set; }

        public float Max { get; private set; }

        public float Rounding { get; private set; }

        public bool DoNotShowGUI => AdvancedOptions.Contains(IAdvancedOption.Hidden) || SAINEditor.AdvancedBotConfigs == false && AdvancedOptions.Contains(IAdvancedOption.IsAdvanced);

        public IAdvancedOption[] AdvancedOptions { get; private set; } = new IAdvancedOption[0];

        public EListType EListType { get; private set; } = EListType.None;
        public Type PrimaryListType { get; private set; }
        public Type SecondaryListType { get; private set; }
    }
}
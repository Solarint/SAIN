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

        private void GetInfo()
        {
            var nameDescription = Get<NameAndDescriptionAttribute>();
            Name = nameDescription?.Name ?? Get<NameAttribute>()?.Name ?? Field?.Name ?? Property?.Name;
            Description = nameDescription?.Description ?? Get<DescriptionAttribute>()?.Description;

            Default = Get<DefaultValueAttribute>()?.Value;

            var minMax = Get<MinMaxRoundAttribute>();
            Min = minMax?.Min ?? Get<MinimumValueAttribute>()?.Min ?? 0f;
            Max = minMax?.Max ?? Get<MaximumValueAttribute>()?.Max ?? 100f;
            Rounding = minMax?.Rounding ?? Get<RoundingValueAttribute>()?.Rounding ?? 1f;

            if (Default == null && Min != null && Max != null)
            {
                float newDefault = (Max.Value - Min.Value) / 2f;
                Default = Mathf.Round(newDefault * 100f) / 100f;
            }

            var advanced = Get<AdvancedAttribute>();
            if (advanced != null)
            {
                AdvancedOptions = advanced.Options;
            }

            var listObj = Get<DictionaryAttribute>();
            if (listObj != null)
            {
                ListTypeEnum = AttributeListType.Dictionary;
                PrimaryListType = listObj.TypeA;
                SecondaryListType = listObj.TypeB;
                return;
            }

            var list = Get<ListAttribute>();
            if (list != null)
            {
                ListTypeEnum = AttributeListType.List;
                PrimaryListType = list.Type;
                return;
            }

            var array = Get<ArrayAttribute>();
            if (array != null)
            {
                ListTypeEnum = AttributeListType.Array;
                PrimaryListType = array.Type;
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

        public string Name { get; private set; }
        public string Description { get; private set; }

        public object Default { get; private set; }
        public float? Min { get; private set; }
        public float? Max { get; private set; }
        public float? Rounding { get; private set; }

        public bool DoNotShowGUI => AdvancedOptions.Contains(AdvancedEnum.Hidden) || SAINPlugin.Editor.AdvancedOptionsEnabled == false && AdvancedOptions.Contains(AdvancedEnum.IsAdvanced);

        public AdvancedEnum[] AdvancedOptions { get; private set; } = new AdvancedEnum[0];

        public AttributeListType ListTypeEnum { get; private set; } = AttributeListType.None;
        public Type PrimaryListType { get; private set; }
        public Type SecondaryListType { get; private set; }
    }
}
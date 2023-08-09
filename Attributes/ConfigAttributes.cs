using System;
using SAIN.Preset.GlobalSettings;
using UnityDiagnostics;
using System.Collections.Generic;
using AmplifyMotion;
using static AdvancedLight;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace SAIN.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public abstract class FieldAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class PropertyAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class FieldOrPropAttribute : Attribute
    {

    }

    public enum AttributeListType
    {
        None,
        List,
        Array,
        Dictionary,
    }

    public enum AdvancedEnum
    {
        None,
        IsAdvanced,
        Hidden,
        CopyValueFromEFT,
    }

    public sealed class AmmoCaliberAttribute : FieldOrPropAttribute
    {
        public AmmoCaliberAttribute(Caliber ammoCaliber)
        {
            this.ammoCaliber = ammoCaliber;
        }
        public Caliber AmmoCaliber
        {
            get { return ammoCaliber; }
        }

        readonly Caliber ammoCaliber;
    }

    public sealed class WeaponClassAttribute : FieldOrPropAttribute
    {
        public WeaponClassAttribute(WeaponClass weaponClass)
        {
            this.weaponClass = weaponClass;
        }

        public WeaponClass WeaponClass
        {
            get { return weaponClass; }
        }

        readonly WeaponClass weaponClass;
    }

    public sealed class DictionaryAttribute : FieldOrPropAttribute
    {
        public DictionaryAttribute(Type typeA, Type typeB)
        {
            this.typeA = typeA;
            this.typeB = typeB;
        }

        public Type TypeA
        {
            get { return typeA; }
        }
        public Type TypeB
        {
            get { return typeB; }
        }

        private readonly Type typeA;
        private readonly Type typeB;
    }

    public sealed class ListAttribute : FieldOrPropAttribute
    {
        public ListAttribute(Type type)
        {
            this.type = type;
        }

        public Type Type
        {
            get { return type; }
        }

        private readonly Type type;
    }

    public sealed class ArrayAttribute : FieldOrPropAttribute
    {
        public ArrayAttribute(Type type)
        {
            this.type = type;
        }

        public Type Type
        {
            get { return type; }
        }

        private readonly Type type;
    }

    public sealed class NameAndDescriptionAttribute : FieldOrPropAttribute
    {
        public NameAndDescriptionAttribute(string name, string description = null)
        {
            this.name = name;
            this.description = description;
        }

        public string Name
        {
            get { return name; }
        }
        public string Description
        {
            get { return description; }
        }

        readonly string name;
        readonly string description;
    }
    public sealed class NameAttribute : FieldOrPropAttribute
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
    public sealed class DescriptionAttribute : FieldOrPropAttribute
    {
        public DescriptionAttribute(string description)
        {
            this.description = description;
        }

        public string Description
        {
            get { return description; }
        }

        readonly string description;
    }

    public sealed class MinimumValueAttribute : FieldOrPropAttribute
    {
        public MinimumValueAttribute(float min)
        {
            this.min = min;
        }

        public float Min
        {
            get { return min; }
        }

        readonly float min;
    }

    public sealed class MaximumValueAttribute : FieldOrPropAttribute
    {
        public MaximumValueAttribute(float max)
        {
            this.max = max;
        }

        public float Max
        {
            get { return max; }
        }

        readonly float max;
    }

    public sealed class RoundingValueAttribute : FieldOrPropAttribute
    {
        public RoundingValueAttribute(float rounding = 1f)
        {
            this.rounding = rounding;
        }

        public float Rounding
        {
            get { return rounding; }
        }

        readonly float rounding;
    }

    public sealed class MinMaxRoundAttribute : FieldOrPropAttribute
    {
        public MinMaxRoundAttribute(float min, float max, float rounding = 1f)
        {
            this.min = min;
            this.max = max;
            this.rounding = rounding;
        }

        public float Min
        {
            get { return min; }
        }
        public float Max
        {
            get { return max; }
        }
        public float Rounding
        {
            get { return rounding; }
        }

        readonly float min;
        readonly float max;
        readonly float rounding;
    }

    public sealed class PercentageAttribute : FieldOrPropAttribute
    {
        public PercentageAttribute(float rounding = 1f, int min = 0, int max = 100)
        {
            this.rounding = rounding;
            this.min = min;
            this.max = max;
        }

        public int Min
        {
            get { return min; }
        }
        public int Max
        {
            get { return max; }
        }
        public float Rounding
        {
            get { return rounding; }
        }

        readonly float rounding;
        readonly int min;
        readonly int max;
    }

    public sealed class AdvancedAttribute : FieldOrPropAttribute
    {
        public AdvancedAttribute(params AdvancedEnum[] options)
        {
            if (options == null || options.Length == 0)
            {
                options = new AdvancedEnum[]
                {
                    AdvancedEnum.None,
                };
            }
            this.options = options;
        }

        public AdvancedEnum[] Options
        {
            get { return options; }
        }

        private readonly AdvancedEnum[] options;
    }

    public sealed class HiddenAttribute : FieldOrPropAttribute
    {
        public bool Value = true;
    }

    public sealed class CopyValueFromEFTAttribute : FieldOrPropAttribute
    {
        public bool Value = true;
    }
}
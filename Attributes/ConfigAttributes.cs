using System;
using SAIN.Preset.GlobalSettings;
using UnityDiagnostics;
using System.Collections.Generic;
using AmplifyMotion;
using static AdvancedLight;
using System.Windows.Forms;
using JetBrains.Annotations;
using UnityEngine;
using SAIN.Helpers;

namespace SAIN.Attributes
{

    public sealed class AmmoCaliberAttribute : BaseAttribute
    {
        public AmmoCaliberAttribute(Caliber ammoCaliber)
        {
            AmmoCaliber = ammoCaliber;
        }

        public readonly Caliber AmmoCaliber;
    }

    public sealed class WeaponClassAttribute : BaseAttribute
    {
        public WeaponClassAttribute(WeaponClass weaponClass)
        {
            WeaponClass = weaponClass;
        }

        public readonly WeaponClass WeaponClass;
    }

    public sealed class DictionaryAttribute : BaseAttribute
    {
        public DictionaryAttribute(Type typeA, Type typeB)
        {
            TypeA = typeA;
            TypeB = typeB;
        }

        public readonly Type TypeA;
        public readonly Type TypeB;
    }

    public sealed class ListAttribute : BaseAttribute
    {
        public ListAttribute(Type type)
        {
            Type = type;
        }

        public readonly Type Type;
    }

    public sealed class NameAndDescriptionAttribute : BaseAttribute
    {
        public NameAndDescriptionAttribute(string name, string description = null)
        {
            Name = name;
            Description = description;
        }

        public readonly string Name;
        public readonly string Description;
    }

    public sealed class NameAttribute : BaseAttribute
    {
        public NameAttribute(string name)
        {
            Name = name;
        }

        public readonly string Name;
    }

    public sealed class DefaultAttribute : BaseAttribute
    {
        public DefaultAttribute(object value)
        {
            Value = value;
        }

        public readonly object Value;
    }

    public sealed class DescriptionAttribute : BaseAttribute
    {
        public DescriptionAttribute(string description)
        {
            Description = description;
        }

        public readonly string Description;
    }

    public sealed class RoundingValueAttribute : BaseAttribute
    {
        public RoundingValueAttribute(float rounding = 1f)
        {
            Rounding = rounding;
        }

        public readonly float Rounding;
    }

    public sealed class MinMaxAttribute : GUIValuesAttribute
    {
        public MinMaxAttribute(float min, float max, float rounding = 1f) : base(min, max, rounding)
        {
        }
    }

    public sealed class PercentageAttribute : GUIValuesAttribute
    {
        public PercentageAttribute(float min = 0, float max = 100, float rounding = 1f) : base(min, max, rounding)
        {
        }
    }

    public sealed class Percentage0to1Attribute : GUIValuesAttribute
    {
        public Percentage0to1Attribute(float min = 0f, float max = 1f, float rounding = 100f) : base (min, max, rounding)
        {
        }
    }

    public abstract class GUIValuesAttribute : BaseAttribute
    {
        public GUIValuesAttribute(float min, float max, float rounding)
        {
            Min = min;
            Max = max;
            Rounding = rounding;
        }

        public float Clamp(object value)
        {
            return MathHelpers.ClampObject(value, Min, Max);
        }

        public float Round(float value)
        {
            return value.Round(Rounding);
        }

        public readonly float Min;
        public readonly float Max;
        public readonly float Rounding;
    }

    public sealed class AdvancedAttribute : BaseAttribute
    {
        public AdvancedAttribute(params AdvancedEnum[] options)
        {
            if (options != null && options.Length > 0)
            {
                Options = options;
            }
        }

        public readonly AdvancedEnum[] Options = new AdvancedEnum[] { AdvancedEnum.IsAdvanced };
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class BaseAttribute : Attribute
    {
    }

    public enum EListType
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
}
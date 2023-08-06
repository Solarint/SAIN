using System;
using System.ComponentModel;
using System.Reflection;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Attributes
{
    public sealed class AttributesClass
    {
        public AttributesClass(FieldInfo field)
        {
            var nameDescription = field.GetCustomAttribute<NameAndDescriptionAttribute>();
            Name = nameDescription?.Name ?? field.Name;
            Description = nameDescription?.Description;

            Default = field.GetCustomAttribute<DefaultValueAttribute>()?.Value;

            var minMax = field.GetCustomAttribute<MinMaxRoundAttribute>();
            Min = minMax?.Min;
            Max = minMax?.Max;
            Rounding = minMax?.Rounding;

            var custom = field.GetCustomAttribute<AdvancedOptionsAttribute>();
            IsHidden = custom?.IsHidden == true;
            IsAdvanced = custom?.IsAdvanced == true;
            GetDefaultFromEFT = custom?.CopyValueFromEFT == true;

            IsList = field.GetCustomAttribute<ListAttribute>()?.Value == true;
        }

        public readonly string Name;
        public readonly string Description;
        public readonly object Default;
        public readonly float? Min;
        public readonly float? Max;
        public readonly float? Rounding;
        public readonly bool IsHidden = false;
        public readonly bool IsAdvanced = false;
        public readonly bool GetDefaultFromEFT = false;
        public readonly bool IsList = false;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AmmoCaliberAttribute : Attribute
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

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class WeaponClassAttribute : Attribute
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

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ListAttribute : Attribute
    {
        public ListAttribute()
        {
        }

        public bool Value
        {
            get { return true; }
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NameAndDescriptionAttribute : Attribute
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

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MinMaxRoundAttribute : Attribute
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
    public sealed class AdvancedOptionsAttribute : Attribute
    {
        public AdvancedOptionsAttribute(bool isAdvanced = false, bool isHidden = false, bool copyValueFromEFT = false)
        {
            this.isAdvanced = isAdvanced;
            this.isHidden = isHidden;
            this.copyValueFromEFT = copyValueFromEFT;
        }

        public bool IsAdvanced
        {
            get { return isAdvanced; }
        }
        public bool IsHidden
        {
            get { return isHidden; }
        }
        public bool CopyValueFromEFT
        {
            get { return copyValueFromEFT; }
        }

        readonly bool isAdvanced;
        readonly bool isHidden;
        readonly bool copyValueFromEFT;
    }
}
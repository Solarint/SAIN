using SAIN.Helpers;
using System;

namespace SAIN.Attributes
{
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

    public abstract class StringAttribute : BaseAttribute
    {
        public StringAttribute(string value)
        {
            Value = value;
        }

        public readonly string Value;
    }

    public sealed class NameAttribute : StringAttribute
    {
        public NameAttribute(string name) : base(name)
        {
        }
    }

    public sealed class DescriptionAttribute : StringAttribute
    {
        public DescriptionAttribute(string description) : base(description)
        {
        }
    }

    public sealed class CategoryAttribute : StringAttribute
    {
        public CategoryAttribute(string category) : base(category)
        {
        }
    }

    public sealed class SectionAttribute : StringAttribute
    {
        public SectionAttribute(string section) : base(section)
        {
        }
    }

    public sealed class DefaultFloatAttribute : BaseAttribute
    {
        public DefaultFloatAttribute(float defaultVal)
        {
            Value = defaultVal;
        }

        public readonly float Value;
    }

    public class MinMaxAttribute : GUIValuesAttribute
    {
        public MinMaxAttribute(float min, float max, float rounding = 100f) : base(min, max, rounding)
        {
        }
    }

    public sealed class DifficultyModAttribute : MinMaxAttribute
    {
        public DifficultyModAttribute() : base(0.01f, 10f, 100f)
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
        public Percentage0to1Attribute(float min = 0f, float max = 1f, float rounding = 100f) : base(min, max, rounding)
        {
        }
    }

    public sealed class Percentage01to99Attribute : GUIValuesAttribute
    {
        public Percentage01to99Attribute() : base(0.01f, 0.99f, 100f)
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

    public sealed class DefaultDictionaryAttribute : StringAttribute
    {
        public DefaultDictionaryAttribute(string dictionaryName) : base(dictionaryName)
        {
        }
    }

    public sealed class DefaultListAttribute : StringAttribute
    {
        public DefaultListAttribute(string listName) : base(listName)
        {
        }
    }

    public sealed class AdvancedAttribute : BoolAttribute
    {
        public AdvancedAttribute() : base(true)
        {
        }
    }

    public sealed class DeveloperOptionAttribute : BoolAttribute
    {
        public DeveloperOptionAttribute() : base(true)
        {
        }
    }

    public sealed class SimpleValueAttribute : BoolAttribute
    {
        public SimpleValueAttribute() : base(true)
        {
        }
    }

    public sealed class DebugAttribute : BoolAttribute
    {
        public DebugAttribute() : base(true)
        {
        }
    }

    public sealed class ExperimentalAttribute : BoolAttribute
    {
        public ExperimentalAttribute() : base(true)
        {
        }
    }

    public sealed class HiddenAttribute : BoolAttribute
    {
        public HiddenAttribute() : base(true)
        {
        }
    }

    public sealed class CopyValueAttribute : BoolAttribute
    {
        public CopyValueAttribute() : base(true)
        {
        }
    }

    public abstract class BoolAttribute : BaseAttribute
    {
        public BoolAttribute(bool value)
        {
            Value = value;
        }

        public readonly bool Value;
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

    public enum IAdvancedOption
    {
        None,
        IsAdvanced,
        Hidden,
        CopyValueFromEFT,
    }
}
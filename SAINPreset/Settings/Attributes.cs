using System;

namespace SAIN.SAINPreset.Attributes
{
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

    //[AttributeUsage(AttributeTargets.Field)]
    //public sealed class DescriptionAttribute : Attribute
    //{
    //    public DescriptionAttribute(object description)
    //    {
    //        this.description = description;
    //    }
    //
    //    public object Description
    //    {
    //        get { return description; }
    //    }
    //
    //    readonly object description;
    //}

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

        public object Rounding
        {
            get { return rounding; }
        }

        readonly object rounding;
    }
}

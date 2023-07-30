using SAIN.BotPresets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAIN.Helpers
{
    internal class GetReflectionInfo
    {
        public static readonly Type[] FloatBoolInt = { typeof(bool), typeof(float), typeof(int) };

        public static List<FieldInfo> GetFields(Type type)
        {
            return GetFields(type, FloatBoolInt);
        }

        public static List<FieldInfo> GetFields(Type type, params Type[] acceptableTypes)
        {
            var FieldList = new List<FieldInfo>();
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (acceptableTypes.Contains(field.FieldType))
                {
                    FieldList.Add(field);
                }
            }
            return FieldList;
        }

        public static List<PropertyInfo> GetProperties(Type type)
        {
            return GetProperties(type, FloatBoolInt);
        }

        public static List<PropertyInfo> GetProperties(Type type, params Type[] acceptableTypes)
        {
            var PropertyList = new List<PropertyInfo>();
            foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (acceptableTypes.Contains(prop.PropertyType))
                {
                    PropertyList.Add(prop);
                }
            }
            return PropertyList;
        }

        public static List<PropertyInfo> GetBotPresetProperties()
        {
            var PropertyList = new List<PropertyInfo>();
            foreach (PropertyInfo property in typeof(BotPreset).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (IsSainProperty(property))
                {
                    PropertyList.Add(property);
                }
            }
            return PropertyList;
        }

        public static bool IsSainProperty(PropertyInfo property)
        {
            var propType = property.PropertyType;
            return propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(SAINProperty<>);
        }
    }
}

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAIN.Helpers;

internal class Reflection
{
    public static FieldInfo[] GetFieldsInType(Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
    {
        return type.GetFields(flags);
    }

    public static object GetStaticValue(Type type, string name)
    {
        if (name == null)
        {
            return null;
        }
        var field = type.GetField(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (field != null)
        {
            return field.GetValue(null);
        }
        var prop = type.GetProperty(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (prop != null)
        {
            return prop.GetValue(null);
        }
        return null;
    }

    public static FieldInfo FindFieldByName(string name, FieldInfo[] fields)
    {
        foreach (FieldInfo field in fields)
        {
            if (field.Name == name)
            {
                return field;
            }
        }
        return null;
    }

}
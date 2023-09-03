using Aki.Reflection.Utils;
using HarmonyLib;
using SAIN.Preset;
using SAIN.Preset.BotSettings.SAINSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAIN.Helpers
{
    internal class Reflection
    {
        public static Type AimingDataType = PatchConstants.EftTypes.Single(x => x.GetProperty("LastSpreadCount") != null && x.GetProperty("LastAimTime") != null);
        public static PropertyInfo EFTFileSettings = AccessTools.Property(typeof(BotDifficultySettingsClass), "FileSettings");
        public static FieldInfo[] EFTSettingsCategories => GetFieldsInType(EFTFileSettings.PropertyType);
        public static FieldInfo[] SAINSettingsCategories => GetFieldsInType<SAINSettingsClass>();

        public static FieldInfo[] GetFieldsInType<T>(BindingFlags flags = BindingFlags.Instance | BindingFlags.Public) where T : class
        {
            return typeof(T).GetFields(flags);
        }
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
        public static T GetValue<T>(object obj, FieldInfo field)
        {
            return (T)field.GetValue(obj);
        }

        public static T GetValue<T>(object obj, PropertyInfo prop)
        {
            return (T)prop.GetValue(obj);
        }

        public static FieldInfo GetField(Type type, string fieldName)
        {
            return AccessTools.Field(type, fieldName);
        }

        public static PropertyInfo GetProperty(Type type, string propName)
        {
            return AccessTools.Property(type, propName);
        }

        public static bool MeetsParameters<T>(T value, T[] array)
        {
            if (array == null || array.Length == 0 || array.Contains(value))
            {
                return true;
            }
            return false;
        }

        public static Dictionary<Type, Dictionary<KeyValuePair<FieldInfo, FieldInfo>, object>>
            GetAllValuesFloatBoolInt
            (object obj, Dictionary<Type, Dictionary<KeyValuePair<FieldInfo, FieldInfo>, object>> Dictionary)
        {
            return GetAllValues<float, bool, int>(obj, Dictionary);
        }

        public static Dictionary<Type, Dictionary<KeyValuePair<FieldInfo, FieldInfo>, object>>
            GetAllValues<A>
            (object obj, Dictionary<Type, Dictionary<KeyValuePair<FieldInfo, FieldInfo>, object>> Dictionary)
        {
            AddToDictionaryOfType<A>(obj, Dictionary);
            return Dictionary;
        }

        public static Dictionary<Type, Dictionary<KeyValuePair<FieldInfo, FieldInfo>, object>>
            GetAllValues<A, B>
            (object obj, Dictionary<Type, Dictionary<KeyValuePair<FieldInfo, FieldInfo>, object>> Dictionary)
        {
            AddToDictionaryOfType<A>(obj, Dictionary);
            AddToDictionaryOfType<B>(obj, Dictionary);
            return Dictionary;
        }

        public static Dictionary<Type, Dictionary<KeyValuePair<FieldInfo, FieldInfo>, object>>
            GetAllValues<A, B, C>
            (object obj, Dictionary<Type, Dictionary<KeyValuePair<FieldInfo, FieldInfo>, object>> Dictionary)
        {
            AddToDictionaryOfType<A>(obj, Dictionary);
            AddToDictionaryOfType<B>(obj, Dictionary);
            AddToDictionaryOfType<C>(obj, Dictionary);
            return Dictionary;
        }

        public static Dictionary<Type, Dictionary<KeyValuePair<FieldInfo, FieldInfo>, object>>
            GetAllValues<A, B, C, D>
            (object obj, Dictionary<Type, Dictionary<KeyValuePair<FieldInfo, FieldInfo>, object>> Dictionary)
        {
            AddToDictionaryOfType<A>(obj, Dictionary);
            AddToDictionaryOfType<B>(obj, Dictionary);
            AddToDictionaryOfType<C>(obj, Dictionary);
            AddToDictionaryOfType<D>(obj, Dictionary);
            return Dictionary;
        }

        static void AddToDictionaryOfType<T>(object obj, Dictionary<Type, Dictionary<KeyValuePair<FieldInfo, FieldInfo>, object>> Dictionary)
        {
            if (Dictionary == null)
            {
                Dictionary = new Dictionary<Type, Dictionary<KeyValuePair<FieldInfo, FieldInfo>, object>>();
            }

            Dictionary<KeyValuePair<FieldInfo, FieldInfo>, object> TypeDict = new Dictionary<KeyValuePair<FieldInfo, FieldInfo>, object>();

            Dictionary<FieldInfo, FieldInfo> TypeList = GetReflectionDictionary(obj.GetType());

            foreach (KeyValuePair<FieldInfo, FieldInfo> keyPair in TypeList)
            {
                if (keyPair.Value.FieldType == typeof(T))
                {
                    object variableResult = GetValue<T>(keyPair, obj);
                    UpdateDictionary(TypeDict, keyPair, variableResult);
                }
            }

            UpdateDictionary(Dictionary, typeof(T), TypeDict);
        }

        static void UpdateDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
            else
            {
                dictionary[key] = value;
            }
        }

        public static T GetValue<T>(KeyValuePair<FieldInfo, FieldInfo> KeyPair, object obj)
        {
            object key = KeyPair.Key.GetValue(obj);
            return (T)KeyPair.Value.GetValue(key);
        }

        public static Dictionary<FieldInfo, FieldInfo> GetReflectionDictionary(Type type, Type[] ParentTypes = null, Type[] ChildTypes = null, BindingFlags flags = BindingFlags.Default)
        {
            if (!ReflectionDictionary.ContainsKey(type))
            {
                Dictionary<FieldInfo, FieldInfo> fieldDict = new Dictionary<FieldInfo, FieldInfo>();

                foreach (FieldInfo Section in type.GetFields(flags))
                {
                    if (MeetsParameters(Section.FieldType, ParentTypes))
                    {
                        foreach (FieldInfo Variable in Section.FieldType.GetFields(flags))
                        {
                            if (MeetsParameters(Variable.FieldType, ChildTypes))
                            {
                                fieldDict.Add(Section, Variable);
                            }
                        }
                    }
                }

                ReflectionDictionary.Add(type, fieldDict);
            }
            return ReflectionDictionary[type];
        }

        static readonly Dictionary<Type, Dictionary<FieldInfo, FieldInfo>>
            ReflectionDictionary = new Dictionary<Type, Dictionary<FieldInfo, FieldInfo>>();

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
    }
}
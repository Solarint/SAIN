using BepInEx.Logging;
using SAIN.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAIN.Preset.GlobalSettings
{
    public class GetValuesFromClass
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(GetValuesFromClass));

        public static Dictionary<object, object> UpdateValues<T>(T defaultKey, object defaultValue, object classObject, Dictionary<object, object> Values)
        {
            Values.Clear();
            Values.Add(defaultKey, defaultValue);

            bool isCaliber = typeof(T) == typeof(ICaliber);
            bool isWeapon = typeof(T) == typeof(IWeaponClass);

            bool added = false;
            foreach (FieldInfo field in classObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                object value = field.GetValue(classObject);
                if (isCaliber)
                {
                    AddCaliberValue(field, value, Values);
                    added = true;
                }
                if (isWeapon)
                {
                    AddWeaponClassValue(field, value, Values);
                    added = true;
                }
                if (!added)
                {
                    Log("Not Supported Type", typeof(T));
                }
            }
            return Values;
        }

        private static void AddCaliberValue(FieldInfo field, object value, Dictionary<object, object> Values)
        {
            var attribute = field.GetCustomAttribute<AmmoCaliberAttribute>();
            if (attribute == null)
            {
                //BuildMessage("attribute is null", field.LayerName);
                return;
            }
            ICaliber enumValue = attribute.AmmoCaliber;
            if (!Values.ContainsKey(enumValue))
            {
                Values.Add(enumValue, value);
            }
            else
            {
                Log("Already Exists", enumValue);
            }
        }

        private static void AddWeaponClassValue(FieldInfo field, object value, Dictionary<object, object> Values)
        {
            var attribute = field.GetCustomAttribute<WeaponClassAttribute>();
            if (attribute == null)
            {
                //BuildMessage("attribute is null", field.LayerName);
                return;
            }
            IWeaponClass enumValue = attribute.WeaponClass;
            if (!Values.ContainsKey(enumValue))
            {
                Values.Add(enumValue, value);
            }
            else
            {
                Log("Already Exists", enumValue);
            }
        }

        private static void Log(string message, object key)
        {
            if (SAINPlugin.DebugModeEnabled)
            {
                Logger.LogWarning($"Key: [{key}] {message}");
            }
        }
    }
}
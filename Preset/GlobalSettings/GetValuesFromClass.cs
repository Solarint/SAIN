using BepInEx.Logging;
using SAIN.Attributes;
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

            bool isCaliber = typeof(T) == typeof(Caliber);
            bool isWeapon = typeof(T) == typeof(WeaponClass);

            bool added = false;
            foreach (FieldInfo field in classObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                object value = field.GetValue(classObject);
                if (isCaliber)
                {
                    AddCaliberValue<Caliber>(field, value, Values);
                    added = true;
                }
                if (isWeapon)
                {
                    AddWeaponClassValue<WeaponClass>(field, value, Values);
                    added = true;
                }
                if (!added)
                {
                    Log("Not Supported Type", typeof(T));
                }
            }
            return Values;
        }

        private static void AddCaliberValue<T>(FieldInfo field, object value, Dictionary<object, object> Values)
        {
            var attritbute = field.GetCustomAttribute<AmmoCaliberAttribute>();
            Caliber enumValue = attritbute.AmmoCaliber;
            if (attritbute != null && !Values.ContainsKey((T)(object)enumValue))
            {
                Values.Add((T)(object)enumValue, value);
            }
            else
            {
                Log("Does Not Exist", enumValue);
            }
        }

        private static void AddWeaponClassValue<T>(FieldInfo field, object value, Dictionary<object, object> Values)
        {
            var attritbute = field.GetCustomAttribute<WeaponClassAttribute>();
            WeaponClass enumValue = attritbute.WeaponClass;
            if (attritbute != null && !Values.ContainsKey((T)(object)enumValue))
            {
                Values.Add((T)(object)enumValue, value);
            }
            else
            {
                Log("Does Not Exist", enumValue);
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
using BepInEx.Configuration;
using SAIN.Plugin.Config;
using UnityEngine;

namespace SAIN.Editor
{
    internal class ConfigValues
    {
        public static bool ReturnFloat<T>(ConfigEntry<T> entry, out float value)
        {
            value = int.MaxValue;
            if (entry is ConfigEntry<float> floatEntry)
            {
                value = floatEntry.Value;
            }
            else if (entry is ConfigEntry<int> intEntry)
            {
                value = intEntry.Value;
            }
            return value != int.MaxValue;
        }

        public static bool ReturnInt<T>(ConfigEntry<T> entry, out int value)
        {
            if (entry is ConfigEntry<float> floatEntry)
            {
                value = Mathf.RoundToInt(floatEntry.Value);
                return true;
            }
            else if (entry is ConfigEntry<int> intEntry)
            {
                value = intEntry.Value;
                return true;
            }
            value = 0;
            return false;
        }

        public static void AssignValue<T>(ConfigEntry<T> entry, float value)
        {
            if (entry is ConfigEntry<float> floatEntry)
            {
                floatEntry.BoxedValue = value;
            }
            if (entry is ConfigEntry<int> intEntry)
            {
                intEntry.BoxedValue = Mathf.RoundToInt(value);
            }
        }

        public static void AssignValue<T>(ConfigEntry<T> entry, int value)
        {
            if (entry is ConfigEntry<float> floatEntry)
            {
                floatEntry.BoxedValue = value;
            }
            if (entry is ConfigEntry<int> intEntry)
            {
                intEntry.BoxedValue = value;
            }
        }

        public static void AssignValue(ConfigEntry<bool> entry, bool value)
        {
            entry.BoxedValue = value;
        }

        public static void DefaultValue<T>(ConfigEntry<T> entry)
        {
            if (entry is ConfigEntry<float> floatEntry)
            {
                floatEntry.BoxedValue = (float)floatEntry.DefaultValue;
            }
            if (entry is ConfigEntry<int> intEntry)
            {
                intEntry.BoxedValue = (int)intEntry.DefaultValue;
            }
            if (entry is ConfigEntry<bool> boolEntry)
            {
                boolEntry.BoxedValue = (bool)boolEntry.DefaultValue;
            }
        }
    }
}

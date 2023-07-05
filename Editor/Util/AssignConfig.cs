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
                floatEntry.Value = value;
            }
            if (entry is ConfigEntry<int> intEntry)
            {
                intEntry.Value = Mathf.RoundToInt(value);
            }
        }

        public static void AssignValue<T>(ConfigEntry<T> entry, int value)
        {
            if (entry is ConfigEntry<float> floatEntry)
            {
                floatEntry.Value = value;
            }
            if (entry is ConfigEntry<int> intEntry)
            {
                intEntry.Value = value;
            }
        }

        public static void AssignValue(ConfigEntry<bool> entry, bool value)
        {
            entry.Value = value;
        }

        public static void DefaultValue<T>(ConfigEntry<T> entry)
        {
            if (entry is ConfigEntry<float> floatEntry)
            {
                floatEntry.Value = (float)floatEntry.DefaultValue;
            }
            if (entry is ConfigEntry<int> intEntry)
            {
                intEntry.Value = (int)intEntry.DefaultValue;
            }
            if (entry is ConfigEntry<bool> boolEntry)
            {
                boolEntry.Value = (bool)boolEntry.DefaultValue;
            }
        }

        public static void DefaultValue<T>(SAINProperty<T> entry)
        {
            if (entry is SAINProperty<float> floatEntry)
            {
                floatEntry.Value = (float)floatEntry.DefaultVal;
            }
            if (entry is SAINProperty<int> intEntry)
            {
                intEntry.Value = (int)intEntry.DefaultVal;
            }
            if (entry is SAINProperty<bool> boolEntry)
            {
                boolEntry.Value = (bool)boolEntry.DefaultVal;
            }
        }
    }
}

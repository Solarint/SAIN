using BepInEx.Configuration;
using UnityEngine;

namespace SAIN.Editor
{
    internal class ConfigValues
    {
        public static T ReturnValue<T>(ConfigEntry<T> entry)
        {
            return (T)(object)entry.Value;
        }

        public static bool ReturnFloat<T>(T entry, out float value)
        {
            if (typeof(T) == typeof(float))
            {
                if (entry is ConfigEntry<float> floatEntry)
                {
                    value = floatEntry.Value;
                    return true;
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (entry is ConfigEntry<int> intEntry)
                {
                    value = Mathf.RoundToInt(intEntry.Value);
                    return true;
                }
            }
            value = 0f;
            return false;
        }

        public static void AssignValue<T>(ConfigEntry<T> entry, T value)
        {
            if (typeof(T) == typeof(float))
            {
                if (entry is ConfigEntry<float> floatEntry)
                {
                    floatEntry.Value = (float)(object)value;
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (entry is ConfigEntry<int> intEntry)
                {
                    intEntry.Value = Mathf.RoundToInt((int)(object)value);
                }
            }
            else if (typeof(T) == typeof(bool))
            {
                if (entry is ConfigEntry<bool> boolEntry)
                {
                    boolEntry.Value = (bool)(object)value;
                }
            }
        }

    }
}

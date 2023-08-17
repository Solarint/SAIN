using EFT.UI;
using SAIN.Editor;
using UnityEngine;

namespace SAIN.Helpers
{
    public static class Extensions
    {
        public static Vector3 Normalize(this Vector3 value, out float magnitude)
        {
            magnitude = value.magnitude;

            if (magnitude > 1E-05f)
            {
                return value / magnitude;
            }
            return Vector3.zero;
        }

        public static Vector3 RotateHoriz(this Vector3 value, float angle)
        {
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 result = rotation * value;
            return result;
        }

        public static float Sqr(this float value)
        {
            return value * value;
        }

        public static float Sqrt(this float value)
        {
            return Mathf.Sqrt(value);
        }

        public static float Scale0to1(this float value, float scalingFactor)
        {
            return value.Scale(0, 1f, 1f - scalingFactor, 1f + scalingFactor);
        }


        public static float Scale(this float value, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * ((value - inputMin) / (inputMax - inputMin));
        }

        public static bool GUIToggle(this bool value, GUIContent content, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            bool newvalue = GUILayout.Toggle(value, content, GetStyle(Style.toggle), options);
            CompareValuePlaySound(value, newvalue, sound);
            return newvalue;
        }
        public static bool GUIToggle(this bool value, string name, string toolTip, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            return GUIToggle(value, new GUIContent(name, toolTip), sound, options);
        }
        public static bool GUIToggle(this bool value, string name, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            return GUIToggle(value, new GUIContent(name), sound, options);
        }

        private static GUIStyle GetStyle(Style style)
        {
            return StylesClass.GetStyle(style);
        }

        private static bool CompareValuePlaySound(object oldValue, object newValue, EUISoundType? sound = null)
        {
            if (oldValue.ToString() != newValue.ToString() && sound != null)
            {
                Sounds.PlaySound(sound.Value);
                return true;
            }
            return false;
        }

        public static float Randomize(this float value, float a = 0.5f, float b = 2f)
        {
            return (value * Random(a, b)).Round100();
        }

        public static float RandomizeSum(this float value, float a = -1, float b = 1, float min = 0.001f)
        {
            float randomValue = value + Random(a, b);
            if (randomValue < min)
            {
                randomValue = min;
            }
            return randomValue.Round100();
        }

        public static float Random(float a, float b)
        {
            return UnityEngine.Random.Range(a, b);
        }

        public static float Round(this float value, float round)
        {
            return Mathf.Round(value * round) / round;
        }

        public static float Round1(this float value)
        {
            return value.Round(1);
        }

        public static float Round10(this float value)
        {
            return value.Round(10);
        }

        public static float Round100(this float value)
        {
            return value.Round(100);
        }

        public static float Round1000(this float value)
        {
            return value.Round(1000);
        }
    }
}
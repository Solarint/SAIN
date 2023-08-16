using UnityEngine;
using static SAIN.Editor.SAINLayout;

namespace SAIN.Editor
{
    public static class ButtonsClass
    {
        private const float InfoWidth = 25f;

        public static void InfoBox(string description, float height)
        {
            InfoBox(description, Width(InfoWidth), Height(height));
        }

        public static void InfoBox(string description, float height, float width)
        {
            InfoBox(description, Width(width), Height(height));
        }

        public static void InfoBox(string description, params GUILayoutOption[] options)
        {
            var alertStyle = GetStyle(Style.alert);
            Box(new GUIContent("?", description), alertStyle, options);
        }

        public static string Toggle(bool value, string on, string off)
        {
            return value ? on : off;
        }

        public static void SingleTextBool(string text, bool value, params GUILayoutOption[] options)
        {
            string status = value ? ": Detected" : ": Not Detected";
            Box(text + status, options);
        }
    }
}
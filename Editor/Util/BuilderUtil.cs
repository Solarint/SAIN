using BepInEx.Configuration;
using UnityEngine;
using static SAIN.Editor.EditorGUI;
using static SAIN.Editor.EditorParameters;
using static SAIN.Editor.UITextures;
using static SAIN.Editor.ConfigValues;
using static SAIN.Editor.Styles;
using static SAIN.Editor.ToolTips;
using SAIN.Plugin.Config;

namespace SAIN.Editor
{
    internal class BuilderUtil
    {
        public static void OnGUI()
        {
            if (OpenEditWindow)
            {
                EditWindow = GUI.Window(1, EditWindow, EditorWindow, "Editor Editor");
            }
        }
        public static bool ExpandableMenu(string name, bool value, string description = null, float indent = 0)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (indent != 0)
            {
                GUILayout.Space(indent);
            }
            Color oldBackgroundColor = GUI.backgroundColor;

            // Create a Rect for the background
            Rect backgroundRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(1));

            // Draw the solid color background
            Color backgroundcolor = Color.gray; // Set the desired background color
            GUI.backgroundColor = backgroundcolor;
            backgroundRect.height = 30;
            GUI.Box(backgroundRect, GUIContent.none);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (indent != 0)
            {
                GUILayout.Space(indent);
            }
            GUILayout.Space(15);
            Buttons.InfoBox(description);
            GUILayout.Label(name, GUILayout.Width(150));
            GUILayout.Space(25);
            if (GUILayout.Button(ExpandCollapse(value), GUILayout.Width(75)))
            {
                value = !value;
            }
            GUILayout.EndHorizontal();

            GUI.backgroundColor = oldBackgroundColor; 
            GUILayout.Space(5);
            return value;
        }

        private static string ExpandCollapse(bool value)
        {
            return value ? "[Collapse]" : "[Expand]";
        }

        public static Rect EditWindow = new Rect(50, 50, 600, 300);

        public static bool CreateButtonOption(ConfigEntry<bool> entry, string name, string description = null)
        {
            bool value = false;
            GUILayout.BeginHorizontal();
            if (Buttons.Button(entry, name))
            {
                value = true;
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static float HorizSlider<T>(string name, ConfigEntry<T> entry, float min, float max, float rounding = 1f, string description = "", bool assignConfig = true)
        {
            GUILayout.BeginHorizontal();

            TextStyle.alignment = TextAnchor.MiddleCenter;

            Buttons.InfoBox(description);
            CreateNameLabel(name);
            var defaultStyle = TextStyle.fontStyle;
            TextStyle.fontStyle = FontStyle.Normal;
            GUILayout.FlexibleSpace();
            GUILayout.Box(min.ToString(), BlankBoxBG, GUILayout.Width(SliderMinMaxWidth.Value));
            CheckMouse("Min");

            float result = CreateSlider(entry, min, max, rounding, assignConfig);

            GUILayout.Box(max.ToString(), BlankBoxBG, GUILayout.Width(SliderMinMaxWidth.Value));
            CheckMouse("Max");

            TextStyle.fontStyle = defaultStyle;

            GUILayout.Box(entry.Value.ToString(), TextStyle, GUILayout.Width(SliderResultWidth.Value));
            GUILayout.FlexibleSpace();
            Buttons.ResetButton(entry);
            GUILayout.EndHorizontal();
            return result;
        }

        public static void CreateNameLabel(string name)
        {
            GUILayout.Box(name,
            TextStyle, GUILayout.Width(SliderLabelWidth.Value));
        }

        public static float HorizSlider(string name, float value, float min, float max, float rounding = 1f, string description = null)
        {
            GUILayout.BeginHorizontal();

            TextStyle.alignment = TextAnchor.MiddleCenter;

            Buttons.InfoBox(description);
            CreateNameLabel(name);

            var defaultStyle = TextStyle.fontStyle;
            TextStyle.fontStyle = FontStyle.Normal;

            GUILayout.Box(min.ToString(), TextStyle);
            CheckMouse("Min");

            float result = CreateSlider(value, min, max, rounding);

            GUILayout.Box(max.ToString(), TextStyle);
            CheckMouse("Max");

            TextStyle.fontStyle = defaultStyle;

            GUILayout.Box(value.ToString(),
            TextStyle, GUILayout.Width(SliderResultWidth.Value));

            GUILayout.EndHorizontal();
            return result;
        }

        private static float CreateSlider<T>(ConfigEntry<T> entry, float min, float max, float rounding = 1f, bool assignConfig = true)
        {
            if (ReturnFloat(entry, out float sliderValue))
            {
                sliderValue = CreateSlider(sliderValue, min, max, rounding);
                if (assignConfig)
                {
                    AssignValue(entry, sliderValue);
                }
            }
            return sliderValue;
        }

        private static float CreateSlider(float value, float min, float max, float rounding = 1f)
        {
            float progress = (value - min) / (max - min); // Calculate the progress from 0 to 1
            var sliderRect = DrawSliderBackGrounds(progress);
            value = GUI.HorizontalSlider(sliderRect, value, min, max, SliderStyle, ThumbStyle);
            value = Mathf.Round(value * rounding) / rounding;
            return value;
        }

        public static Vector2 MousePos => new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y);

        public static void EditorWindow(int TWCWindowID)
        {
            GUI.DragWindow(new Rect(0, 0, 600, 20));
            GUILayout.BeginArea(new Rect(25, 55, 550, 500));
            GUILayout.BeginVertical();

            GUILayout.Box("Slider Style Modification");

            Z = HorizSlider("Info Width", Z, 10f, 50f);
            A = HorizSlider("Label Width", A, 1f, 300f);
            B = HorizSlider("Space Width", B, 0f, 300f);
            C = HorizSlider("Slider Width", C, 1f, 300f);
            D = HorizSlider("MinMax Width", D, 1f, 300f);
            E = HorizSlider("Result Width", E, 1f, 300f);

            Set();
            Close();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private static void Set()
        {
            if (GUILayout.Button("Set")) 
            {
                InfoIconWidth.Value = Z;
                SliderLabelWidth.Value = A;
                SliderSpaceWidth.Value = B;
                SliderWidth.Value = C;
                SliderMinMaxWidth.Value = D;
                SliderResultWidth.Value = E;
            }
        }

        private static void Close()
        {
            if (GUILayout.Button("Close")) OpenEditWindow = false;
        }
        private static float Z = InfoIconWidth.Value;
        private static float A = SliderLabelWidth.Value;
        private static float B = SliderSpaceWidth.Value;
        private static float C = SliderWidth.Value;
        private static float D = SliderMinMaxWidth.Value;
        private static float E = SliderResultWidth.Value;

        public static bool OpenEditWindow;
    }
}
using BepInEx.Configuration;
using UnityEngine;
using static SAIN.Editor.EditorGUI;
using static SAIN.Editor.EditorParameters;
using static SAIN.Editor.UITextures;
using static SAIN.Editor.ConfigValues;
using static SAIN.Editor.Styles;
using static SAIN.Editor.ToolTips;

namespace SAIN.Editor
{
    internal class BuilderUtil
    {
        private static GUILayoutOption Height => GUILayout.Height(25f);
        private static GUILayoutOption ExpandWidth => GUILayout.Width(150f);
        private static GUILayoutOption MinMaxWidth => GUILayout.Width(SliderMinMaxWidth.Value);
        private static GUILayoutOption SlWidth => GUILayout.Width(SliderWidth.Value);
        private static GUILayoutOption ResultWidth => GUILayout.Width(SliderResultWidth.Value);
        private static GUILayoutOption LabeltWidth => GUILayout.Width(SliderLabelWidth.Value);

        public static bool ExpandableMenu(string name, bool value, string description = null, float indent = 0f)
        {
            // Add a Rect for the background
            Rect backgroundRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Width(RectLayout.MainWindow.width), GUILayout.Height(25));
            //GUI.DrawTexture(backgroundRect, Util.Colors.TextureDarkBlue);

            GUILayout.BeginHorizontal();
            if (indent != 0f)
            {
                GUILayout.Space(indent);
            }
            Buttons.InfoBox(description);
            GUILayout.Label(name, Height, ExpandWidth);
            if (GUILayout.Button(ExpandCollapse(value), Height, ExpandWidth))
            {
                value = !value;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            return value;
        }

        private static string ExpandCollapse(bool value)
        {
            return value ? "[Collapse]" : "[Expand]";
        }

        public static bool CreateButtonOption(ConfigEntry<bool> entry, string name, string description = null)
        {
            bool value = false;
            GUILayout.BeginHorizontal();
            if (Buttons.ButtonConfigEntry(entry, name))
            {
                value = true;
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static bool CreateButtonOption(SAINProperty<bool> entry)
        {
            bool value = false;
            GUILayout.BeginHorizontal();
            if (Buttons.ButtonProperty(entry))
            {
                value = true;
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static float HorizSlider<T>(string name, ConfigEntry<T> entry, float min, float max, float rounding = 1f, string description = "", bool assignConfig = true)
        {
            GUILayout.BeginHorizontal();

            Buttons.InfoBox(description);
            GUILayout.Box(name, LabeltWidth, Height);
            GUILayout.Box(min.ToString(), BlankBoxBG, MinMaxWidth, Height);
            CheckMouse("Min");

            float result = CreateSlider(entry, min, max, rounding, assignConfig);

            GUILayout.Box(max.ToString(), BlankBoxBG, MinMaxWidth, Height);
            CheckMouse("Max");

            GUILayout.Box(entry.Value.ToString(), ResultWidth, Height);
            GUILayout.FlexibleSpace();
            Buttons.ResetButton(entry);
            GUILayout.EndHorizontal();
            return result;
        }

        public static float HorizSlider(SAINProperty<float> entry)
        {
            GUILayout.BeginHorizontal();

            Buttons.InfoBox(entry.Description);
            GUILayout.Box(entry.Name, LabeltWidth, Height);

            GUILayout.FlexibleSpace();

            GUILayout.Box(entry.Min.ToString(), BlankBoxBG, MinMaxWidth, Height);
            CheckMouse("Min");

            entry.Value = CreateSlider(entry.Value, entry.Min, entry.Max, entry.Rounding);

            GUILayout.Box(entry.Max.ToString(), BlankBoxBG, MinMaxWidth, Height);
            CheckMouse("Max");

            GUILayout.Box(entry.Value.ToString(), ResultWidth, Height);
            Buttons.ResetButton(entry);
            GUILayout.EndHorizontal();
            return entry.Value;
        }

        public static float HorizSlider(string name, float value, float min, float max, float rounding = 1f, string description = null)
        {
            GUILayout.BeginHorizontal();

            Buttons.InfoBox(description);
            GUILayout.Box(name, LabeltWidth, Height);

            GUILayout.Box(min.ToString(), BlankBoxBG, MinMaxWidth, Height);
            CheckMouse("Min");

            float result = CreateSlider(value, min, max, rounding);

            GUILayout.Box(max.ToString(), BlankBoxBG, MinMaxWidth, Height);
            CheckMouse("Max");

            GUILayout.Box(value.ToString(), ResultWidth, Height);

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
            float progress = (value - min) / (max - min);
            value = GUILayout.HorizontalSlider(value, min, max, SlWidth, Height);
            DrawSliderBackGrounds(progress);

            value = Mathf.Round(value * rounding) / rounding;
            return value;
        }
    }
}
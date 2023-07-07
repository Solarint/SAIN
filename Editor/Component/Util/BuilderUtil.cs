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


        public static bool ExpandableMenu(string name, bool value, string description = null)
        {
            Buttons.InfoBox(description);
            GUILayout.Label(name, Height, ExpandWidth);
            return GUILayout.Toggle(value, value ? "[Collapse]" : "[Expand]");
        }

        public static bool CreateButtonOption(ConfigEntry<bool> entry)
        {
            bool value = false;
            GUILayout.BeginHorizontal();
            if (Buttons.ButtonConfigEntry(entry))
            {
                value = true;
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static bool CreateButtonOption(SAINProperty<bool> entry, BotDifficulty difficulty)
        {
            bool value = false;
            GUILayout.BeginHorizontal();
            if (Buttons.ButtonProperty(entry, difficulty))
            {
                value = true;
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static void HorizSlider<T>(ConfigEntry<T> entry, float rounding)
        {
            GUILayout.BeginHorizontal();

            Buttons.InfoBox(entry.Description.Description);
            GUILayout.Box(entry.Definition.Key, LabeltWidth, Height);

            object min = MinMax(entry, out object max);

            if (min != null)
            {
                GUILayout.Box(min.ToString(), BlankBoxBG, MinMaxWidth, Height);
                CheckMouse("Min");
            }

            if (ReturnFloat(entry, out float sliderValue))
            {
                sliderValue = CreateSlider(sliderValue, min, max, rounding);
                AssignValue(entry, sliderValue);
            }
            else
            {
                bool boolValue = (bool)(object)entry.Value;
                boolValue = GUILayout.Toggle((bool)(object)entry.Value, boolValue ? "On" : "Off");
                AssignValue((ConfigEntry<bool>)(object)entry, boolValue);
            }

            if (max != null)
            {
                GUILayout.Box(max.ToString(), BlankBoxBG, MinMaxWidth, Height);
                CheckMouse("Max");
            }

            GUILayout.Box(entry.Value.ToString(), ResultWidth, Height);
            GUILayout.FlexibleSpace();
            Buttons.ResetButton(entry);
            GUILayout.EndHorizontal();
        }

        private static object MinMax<T>(ConfigEntry<T> entry, out object max)
        {
            if (entry?.Description?.AcceptableValues == null || entry.SettingType == typeof(bool))
            {
                max = null;
                return null;
            }
            if (entry.SettingType == typeof(float))
            {
                max = entry.Description.AcceptableValues.Clamp(float.MaxValue);
                return entry.Description.AcceptableValues.Clamp(float.MinValue);
            }
            else
            {
                max = entry.Description.AcceptableValues.Clamp(int.MaxValue);
                return entry.Description.AcceptableValues.Clamp(int.MinValue);
            }
        }

        public static void HorizSlider(SAINProperty<float> entry, BotDifficulty difficulty)
        {
            float value = entry.GetValue(difficulty);
            GUILayout.BeginHorizontal();

            Buttons.InfoBox(entry.Description);
            GUILayout.Box(entry.Name, LabeltWidth, Height);

            GUILayout.FlexibleSpace();

            GUILayout.Box(entry.Min.ToString(), BlankBoxBG, MinMaxWidth, Height);
            CheckMouse("Min");

            value = CreateSlider(value, entry.Min, entry.Max, entry.Rounding);

            GUILayout.Box(entry.Max.ToString(), BlankBoxBG, MinMaxWidth, Height);
            CheckMouse("Max");

            GUILayout.Box(value.ToString(), ResultWidth, Height);
            Buttons.ResetButton(entry);
            GUILayout.EndHorizontal();

            entry.SetValue(difficulty, value);
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

        private static float CreateSlider<T>(ConfigEntry<T> entry, object min, object max, float rounding = 1f)
        {
            if (ReturnFloat(entry, out float sliderValue))
            {
                sliderValue = CreateSlider(sliderValue, min, max, rounding);
                AssignValue(entry, sliderValue);
            }
            return sliderValue;
        }

        private static float CreateSlider(float value, object min, object max, float rounding = 1f)
        {
            float progress = (value - (float)min) / ((float)max - (float)min);
            value = GUILayout.HorizontalSlider(value, (float)min, (float)max, SlWidth, Height);
            DrawSliderBackGrounds(progress);

            value = Mathf.Round(value * rounding) / rounding;
            return value;
        }
    }
}
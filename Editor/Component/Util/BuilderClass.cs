using BepInEx.Configuration;
using UnityEngine;
using static SAIN.Editor.EditorParameters;
using static SAIN.Editor.UITextures;
using static SAIN.Editor.ConfigValues;
using static SAIN.Editor.StyleOptions;
using static SAIN.Editor.ToolTips;
using SAIN.BotPresets;
using System.Collections.Generic;
using static SAIN.Editor.Names.StyleNames;
using SAIN.Editor.Abstract;

namespace SAIN.Editor
{
    public class BuilderClass : EditorAbstract
    {
        public BuilderClass(GameObject go) : base(go) { }

        private static float ExpandWidth => 150f;
        private static float MinMaxWidth => SliderMinMaxWidth.Value;
        private static float SlWidth => SliderWidth.Value;
        private static float ResultWidth => SliderResultWidth.Value;
        private static float LabelWidth => SliderLabelWidth.Value;


        public bool ExpandableMenu(string name, bool value, string description = null)
        {
            GUILayout.BeginHorizontal();
            ButtonsClass.InfoBox(description, true);
            Label(name, ExpandWidth, true);
            value = Toggle(value, value ? "[Close]" : "[Open]", true);
            GUILayout.EndHorizontal();
            return value;
        }

        public bool CreateButtonOption(ConfigEntry<bool> entry)
        {
            bool value = false;
            GUILayout.BeginHorizontal();
            if (ButtonsClass.ButtonConfigEntry(entry))
            {
                value = true;
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public bool CreateButtonOption(SAINProperty<bool> entry, BotDifficulty difficulty)
        {
            bool value = false;
            GUILayout.BeginHorizontal();
            if (ButtonsClass.ButtonProperty(entry, difficulty))
            {
                value = true;
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public void HorizSlider<T>(ConfigEntry<T> entry, float rounding)
        {
            GUILayout.BeginHorizontal();

            ButtonsClass.InfoBox(entry.Description.Description);
            Box(entry.Definition.Key, LabelWidth);

            object min = MinMax(entry, out object max);

            if (min != null)
            {
                BlankBox(min.ToString(), MinMaxWidth);
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
                boolValue = Toggle((bool)(object)entry.Value, boolValue ? "On" : "Off");
                AssignValue((ConfigEntry<bool>)(object)entry, boolValue);
            }

            if (max != null)
            {
                BlankBox(max.ToString(), MinMaxWidth);
                CheckMouse("Max");
            }

            Box(entry.Value.ToString(), ResultWidth);
            GUILayout.FlexibleSpace();
            ButtonsClass.ResetButton(entry);
            GUILayout.EndHorizontal();
        }

        private object MinMax<T>(ConfigEntry<T> entry, out object max)
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

        public void HorizSlider(SAINProperty<float> entry, BotDifficulty difficulty)
        {
            float value = (float)entry.GetValue(difficulty);
            GUILayout.BeginHorizontal();

            ButtonsClass.InfoBox(entry.Description);
            Box(entry.Name, LabelWidth);

            GUILayout.FlexibleSpace();

            BlankBox(entry.Min.ToString(), MinMaxWidth);
            CheckMouse("Min");

            value = CreateSlider(value, entry.Min, entry.Max, entry.Rounding);

            BlankBox(entry.Max.ToString(), MinMaxWidth);
            CheckMouse("Max");

            Box(value.ToString(), ResultWidth);
            ButtonsClass.ResetButton(entry);
            GUILayout.EndHorizontal();

            entry.SetValue(difficulty, value);
        }

        public float HorizSlider(string name, float value, float min, float max, float rounding = 1f, string description = null)
        {
            GUILayout.BeginHorizontal();

            ButtonsClass.InfoBox(description);
            Box(name, LabelWidth);

            Box(min.ToString(), MinMaxWidth);
            CheckMouse("Min");

            float result = CreateSlider(value, min, max, rounding);

            Box(max.ToString(), MinMaxWidth);
            CheckMouse("Max");

            Box(value.ToString(), ResultWidth);

            GUILayout.EndHorizontal();
            return result;
        }

        private float CreateSlider<T>(ConfigEntry<T> entry, object min, object max, float rounding = 1f)
        {
            if (ReturnFloat(entry, out float sliderValue))
            {
                sliderValue = CreateSlider(sliderValue, min, max, rounding);
                AssignValue(entry, sliderValue);
            }
            return sliderValue;
        }

        private float CreateSlider(float value, object min, object max, float rounding = 1f)
        {
            float progress = (value - (float)min) / ((float)max - (float)min);
            value = HorizontalSlider(value, (float)min, (float)max, SlWidth);
            DrawSliderBackGrounds(progress);

            value = Mathf.Round(value * rounding) / rounding;
            return value;
        }
    }
}
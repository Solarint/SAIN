using SAIN.Editor;
using System;
using UnityEngine;

namespace SAIN.BotSettings.Categories.Util
{
    public class ModifySettings
    {
        static readonly float BaseHeight = 25f;
        static readonly float LabelWidth = 200f;
        static readonly float MinMaxWidth = 50f;
        static readonly float ResultWidth = 100f;
        static readonly float DefaultButtonWidth = 120f;

        public static object GUIModify(SettingsWrapper wrapper)
        {
            // If the wrapper has no set value, set it to default
            if (wrapper.Value == null)
            {
                wrapper.Value = wrapper.SAINDefault;
            }
            // If the wrapper still has no value, or it is set to be hidden return
            if (wrapper.Hidden == true || wrapper.Value == null)
            {
                return wrapper.Value;
            }

            Builder.BeginHorizontal();

            // Display the discription and name of the Setting
            Info(wrapper);

            // Create GUI Option based on the type of setting.
            if (wrapper.Type == typeof(bool))
            {
                BoolOption(wrapper);
            }
            else
            {
                SliderOption(wrapper);
            }

            // Add a button to reset the setting to default values
            ResetButton(wrapper);

            Builder.EndHorizontal();

            return wrapper.Value;
        }

        static void Info(SettingsWrapper wrapper)
        {
            Builder.ButtonsClass.InfoBox(wrapper.Description, BaseHeight);
            string text = wrapper.DisplayName ?? wrapper.Key;
            Builder.Label(text, LabelParams);
        }

        static void BoolOption(SettingsWrapper wrapper)
        {
            bool val = (bool)wrapper.Value;
            string text = val ? "On" : "Off";
            wrapper.Value = Builder.Toggle(val, text, HeightOption);
        }

        static void SliderOption(SettingsWrapper wrapper)
        {
            float val = (float)wrapper.Value;
            var min = wrapper.Min;
            var max = wrapper.Max;

            Builder.MinValueBox(min, MinMaxParams);
            val = Builder.CreateSlider(val, min, max, 1, BaseHeight);

            float rounding = wrapper.Rounding == null ? 1f : wrapper.Rounding.Value;
            val = Mathf.Round(val * rounding) / rounding;
            wrapper.Value = wrapper.Type == typeof(int) ? Mathf.RoundToInt(val) : val;

            Builder.MaxValueBox(max, MinMaxParams);
            Builder.ResultBox(val, ResultParams);
        }

        static void ResetButton(SettingsWrapper wrapper)
        {
            if (wrapper.SAINDefault != null && Builder.Button("Reset to SAIN Default", DefaultButtonParams))
            {
                wrapper.Value = wrapper.SAINDefault;
            }
            if (wrapper.EFTDefault != null && Builder.Button("Reset to EFT Default", DefaultButtonParams))
            {
                wrapper.Value = wrapper.EFTDefault;
            }
        }

        static GUILayoutOption[] LabelParams => new GUILayoutOption[] { GUILayout.Width(LabelWidth), HeightOption };
        static GUILayoutOption[] MinMaxParams => new GUILayoutOption[] { GUILayout.Width(MinMaxWidth), HeightOption };
        static GUILayoutOption[] ResultParams => new GUILayoutOption[] { GUILayout.Width(ResultWidth), HeightOption };
        static GUILayoutOption[] DefaultButtonParams => new GUILayoutOption[] { GUILayout.Width(DefaultButtonWidth), HeightOption };
        static GUILayoutOption HeightOption => GUILayout.Height(BaseHeight);
        static BuilderClass Builder => SAINPlugin.SAINEditor.Builder;
    }
}

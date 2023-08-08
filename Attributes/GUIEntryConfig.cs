using UnityEngine;

namespace SAIN.Attributes
{
    public class GUIEntryConfig
    {
        const float TargetWidthScale = 1920;

        public GUIEntryConfig(float entryHeight = 25f)
        {
            EntryHeight = entryHeight;
        }

        public GUIEntryConfig(float entryHeight, float infoWidth, float labelWidth, float minMaxWidth, float sliderWidth, float resultWidth, float resetWidth)
        {
            EntryHeight = entryHeight;
            InfoWidth = infoWidth;
            LabelWidth = labelWidth;
            MinMaxWidth = minMaxWidth;
            SliderWidth = sliderWidth;
            ResultWidth = resultWidth;
            ResetWidth = resetWidth;
        }

        public void OnGUIEditDimensions()
        {
            GUILayout.BeginVertical();
            EntryHeight = HorizontalSlider(EntryHeight, 10f, 50f);
            InfoWidth = HorizontalSlider(InfoWidth);
            LabelWidth = HorizontalSlider(LabelWidth);
            MinMaxWidth = HorizontalSlider(MinMaxWidth);
            SliderWidth = HorizontalSlider(SliderWidth);
            ResultWidth = HorizontalSlider(ResultWidth);
            ResetWidth = HorizontalSlider(ResetWidth);
            GUILayout.EndVertical();
        }

        static float HorizontalSlider(float value, float min = 0f, float max = 1f, float rounding = 100f)
        {
            value = GUILayout.HorizontalSlider(value, min, max);
            value = Mathf.Round(value * rounding) / rounding;
            return value;
        }

        public float EntryHeight = 25f;

        // Widths of the Entry in the AttributesGUI, from 0 to 1. Which will be scaled to 1080p. All Values should add to 1 to scale properly
        public float InfoWidth = 0.03f;
        public float LabelWidth = 0.15f;
        public float MinMaxWidth = 0.08f;
        public float SliderWidth = 0.35f;
        public float ResultWidth = 0.1f;
        public float ResetWidth = 0.05f;

        public GUILayoutOption[] Info => Params(InfoWidth);
        public GUILayoutOption[] Label => Params(LabelWidth);
        public GUILayoutOption[] MinMax => Params(MinMaxWidth);
        public GUILayoutOption[] Slider => Params(SliderWidth);
        public GUILayoutOption[] Toggle => Params(SliderWidth + MinMaxWidth);
        public GUILayoutOption[] Result => Params(ResultWidth);
        public GUILayoutOption[] Reset => Params(ResetWidth);

        GUILayoutOption[] Params(float width0to1) => new GUILayoutOption[]
        {
                GUILayout.Width(width0to1 * TargetWidthScale),
                GUILayout.Height(EntryHeight)
        };
    }
}

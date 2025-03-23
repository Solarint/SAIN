using SAIN.Plugin;
using UnityEngine;

namespace SAIN.Attributes
{
    public class GUIEntryConfig
    {
        private const float TARGT_WIDTH_SCALE = 1920;

        public float EntryHeight => PresetHandler.EditorDefaults.ConfigEntryHeight;
        public float SliderWidth => PresetHandler.EditorDefaults.ConfigSliderWidth;
        public float ResultWidth => PresetHandler.EditorDefaults.ConfigResultsWidth;
        public float ResetWidth => PresetHandler.EditorDefaults.ConfigResetWidth;
        public float SubList_Indent_Vertical => PresetHandler.EditorDefaults.SubList_Indent_Vertical;
        public float SubList_Indent_Horizontal => PresetHandler.EditorDefaults.SubList_Indent_Horizontal;

        public GUILayoutOption[] Toggle => Params(SliderWidth);
        public GUILayoutOption[] Result => Params(ResultWidth);
        public GUILayoutOption[] Reset => Params(ResetWidth);

        private GUILayoutOption[] Params(float width0to1) => new GUILayoutOption[]
        {
                GUILayout.Width(width0to1 * TARGT_WIDTH_SCALE),
                GUILayout.Height(EntryHeight)
        };
    }
}
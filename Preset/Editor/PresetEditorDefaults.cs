using SAIN.Attributes;
using SAIN.Plugin;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Editor
{
    public class PresetEditorDefaults : SAINSettingsBase<PresetEditorDefaults>, ISAINSettings
    {
        public PresetEditorDefaults()
        {
            DefaultPreset = PresetHandler.DefaultPreset;
        }

        public PresetEditorDefaults(string selectedPreset)
        {
            SelectedCustomPreset = selectedPreset;
            DefaultPreset = PresetHandler.DefaultPreset;
        }

        [Category("General")]
        [Name("Show Advanced Bot Configs")]
        [Description("Show Advanced Settings. Most of these should remain unchanged, unless you want to change something specific.")]
        public bool AdvancedBotConfigs = false;

        [Category("General")]
        [Name("Show Developer Options")]
        [Description("You shouldn't change any of these unless you know exactly what it does based on exploring SAIN's codebase.")]
        public bool DevBotConfigs = false;

        [Category("General")]
        [Hidden]
        public SAINDifficulty SelectedDefaultPreset = SAINDifficulty.none;

        [Category("General")]
        [Hidden]
        public string SelectedCustomPreset;

        [Category("General")]
        [Hidden]
        public string DefaultPreset;

        [Name("GUI Size Scaling")]
        [Category("GUI Dimensions")]
        [MinMax(1f, 2f, 100f)]
        [DefaultFloat(1f)]
        public float ConfigScaling = 1f;

        [Name("Config Entry Sliders")]
        [Category("GUI Dimensions")]
        public bool SliderToggle = true;

        [Category("GUI Dimensions")]
        [Advanced]
        [MinMax(12f, 40f, 1f)]
        [DefaultFloat(20f)]
        public float ConfigEntryHeight = 20f;

        [Category("GUI Dimensions")]
        [DeveloperOption]
        [SimpleValue]
        [MinMax(0.25f, 0.7f, 1000f)]
        [DefaultFloat(0.59f)]
        public float ConfigSliderWidth = 0.59f;

        [Category("GUI Dimensions")]
        [DeveloperOption]
        [SimpleValue]
        [MinMax(0.02f, 0.1f, 1000f)]
        [DefaultFloat(0.045f)]
        public float ConfigResultsWidth = 0.045f;

        [Category("GUI Dimensions")]
        [DeveloperOption]
        [SimpleValue]
        [MinMax(0.01f, 0.1f, 1000f)]
        [DefaultFloat(0.024f)]
        public float ConfigResetWidth = 0.024f;

        [Category("GUI Dimensions")]
        [DeveloperOption]
        [SimpleValue]
        [MinMax(0f, 5f, 1f)]
        [DefaultFloat(2f)]
        public float SubList_Indent_Vertical = 2f;

        [Category("GUI Dimensions")]
        [Advanced]
        [SimpleValue]
        [MinMax(0f, 100f, 1f)]
        [DefaultFloat(25f)]
        public float SubList_Indent_Horizontal = 25f;
    }
}
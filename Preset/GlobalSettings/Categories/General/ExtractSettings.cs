using Newtonsoft.Json;
using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings
{
    public class ExtractSettings : SAINSettingsBase<ExtractSettings>, ISAINSettings
    {
        [Name("SAIN Extract Behavior")]
        [Description("REQUIRES GAME RESTART. Disable vanilla bot extract behavior and use SAIN decision making instead.")]
        public bool SAIN_EXTRACT_TOGGLE = false;
    }
}
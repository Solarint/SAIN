using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class CoverSettings
    {
        [DefaultValue(0.75f)]
        [MinMax(0.5f, 1.5f, 100f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float CoverMinHeight = 0.75f;

        [DefaultValue(8f)]
        [MinMax(0f, 30f, 1f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float CoverMinEnemyDistance = 8f;

        [DefaultValue(0.33f)]
        [MinMax(0.01f, 1f, 100f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float CoverUpdateFrequency = 0.33f;

        [DefaultValue(false)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public bool DebugCoverFinder = false;
    }
}
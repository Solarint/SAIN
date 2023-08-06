using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class CoverSettings
    {
        [DefaultValue(0.75f)]
        [MinMaxRound(0.5f, 1.5f, 100f)]
        [AdvancedOptions(true)]
        public float CoverMinHeight = 0.75f;

        [DefaultValue(8f)]
        [MinMaxRound(0f, 30f, 1f)]
        [AdvancedOptions(true)]
        public float CoverMinEnemyDistance = 8f;

        [DefaultValue(0.33f)]
        [MinMaxRound(0.01f, 1f, 100f)]
        [AdvancedOptions(true)]
        public float CoverUpdateFrequency = 0.33f;

        [DefaultValue(false)]
        [AdvancedOptions(true)]
        public bool DebugCoverFinder = false;
    }
}
using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings.Categories
{
    public class MindSettings
    {
        [DefaultValue(1f)]
        [MinMaxRound(0.01f, 5f, 100f)]
        public float GlobalAggression = 1f;

        [DefaultValue(1f)]
        [MinMaxRound(0.01f, 5f, 100f)]
        public float GlobalDifficultyModifier = 1f;
    }
}
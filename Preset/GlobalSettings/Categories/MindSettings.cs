using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings.Categories
{
    public class MindSettings
    {
        [DefaultValue(1f)]
        [MinMaxRound(0.1f, 2f, 100f)]
        public float GlobalAggression = 1f;
    }
}
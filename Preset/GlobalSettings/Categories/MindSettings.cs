using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings.Categories
{
    public class MindSettings
    {
        [DefaultValue(1f)]
        [MinMax(0.1f, 3f, 10f)]
        public float GlobalAggression = 1f;
    }
}
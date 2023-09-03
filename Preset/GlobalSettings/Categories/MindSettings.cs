using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings.Categories
{
    public class MindSettings
    {
        [Default(1f)]
        [MinMax(0.1f, 3f, 100f)]
        public float GlobalAggression = 1f;
    }
}
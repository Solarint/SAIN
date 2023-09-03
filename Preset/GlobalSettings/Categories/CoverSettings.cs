using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class CoverSettings
    {
        [Default(0.75f)]
        [MinMax(0.5f, 1.5f, 100f)]
        [Advanced]
        public float CoverMinHeight = 0.75f;

        [Default(8f)]
        [MinMax(0f, 30f, 1f)]
        [Advanced]
        public float CoverMinEnemyDistance = 8f;

        [Default(0.33f)]
        [MinMax(0.01f, 1f, 100f)]
        [Advanced]
        public float CoverUpdateFrequency = 0.33f;

        [Default(false)]
        [Advanced]
        public bool DebugCoverFinder = false;
    }
}
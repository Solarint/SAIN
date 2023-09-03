using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class FlashlightSettings
    {
        [Default(3f)]
        [MinMax(0.25f, 10f, 100f)]
        public float DazzleEffectiveness = 3f;

        [Default(30f)]
        [MinMax(0f, 60f)]
        public float MaxDazzleRange = 30f;

        [Default(false)]
        [Advanced]
        public bool DebugFlash = false;

        [Default(false)]
        public bool SillyMode = false;
    }
}
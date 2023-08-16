using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class FlashlightSettings
    {
        [DefaultValue(3f)]
        [MinMax(0.25f, 10f, 100f)]
        public float DazzleEffectiveness = 3f;

        [DefaultValue(30f)]
        [MinMax(0f, 60f)]
        public float MaxDazzleRange = 30f;

        [DefaultValue(false)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public bool DebugFlash = false;

        [DefaultValue(false)]
        public bool SillyMode = false;
    }
}
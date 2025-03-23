using Newtonsoft.Json;
using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings
{
    public class PerformanceSettings : SAINSettingsBase<PerformanceSettings>, ISAINSettings
    {
        [Name("Performance Mode")]
        [Description("Limits the cover finder to maximize performance. Reduces frequency on some raycasts. " +
            "If your PC is CPU limited, this might let you regain some frames lost while using SAIN. Can cause bots to take too long to find cover to go to.")]
        public bool PerformanceMode = false;

        [Advanced]
        [MinMax(2f, 20f, 1f)]
        public float MaxBotsToCheckVisionPerFrame = 5;
    }
}
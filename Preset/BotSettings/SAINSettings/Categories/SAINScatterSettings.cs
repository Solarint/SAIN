using EFT;
using Newtonsoft.Json;
using SAIN.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINScatterSettings
    {
        [DefaultValue(0.03f)]
        [MinMaxRound(0.01f, 3f, 100f)]
        [AdvancedOptions(false, false, true)]
        public float MinScatter = 0.03f;

        [DefaultValue(0.15f)]
        [MinMaxRound(0.01f, 3f, 100f)]
        [AdvancedOptions(false, false, true)]
        public float WorkingScatter = 0.15f;

        [DefaultValue(0.4f)]
        [MinMaxRound(0.01f, 3f, 100f)]
        [AdvancedOptions(false, false, true)]
        public float MaxScatter = 0.4f;
    }
}

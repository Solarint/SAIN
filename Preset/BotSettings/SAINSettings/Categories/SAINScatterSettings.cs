using EFT;
using Newtonsoft.Json;
using SAIN.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINScatterSettings
    {
        [NameAndDescription(
            "EFT Scatter Multiplier",
            "Higher = more scattering. Modifies EFT's default scatter feature. 1.5 = 1.5x more scatter")]
        [DefaultValue(1f)]
        [MinMaxRound(0.1f, 10f, 100f)]
        public float ScatterMultiplier = 1f;

        [DefaultValue(0.03f)]
        [MinMaxRound(0.01f, 3f, 100f)]
        [Advanced(AdvancedEnum.CopyValueFromEFT, AdvancedEnum.Hidden)]
        public float MinScatter = 0.03f;

        [DefaultValue(0.15f)]
        [MinMaxRound(0.01f, 3f, 100f)]
        [Advanced(AdvancedEnum.CopyValueFromEFT, AdvancedEnum.Hidden)]
        public float WorkingScatter = 0.15f;

        [DefaultValue(0.4f)]
        [MinMaxRound(0.01f, 3f, 100f)]
        [Advanced(AdvancedEnum.CopyValueFromEFT, AdvancedEnum.Hidden)]
        public float MaxScatter = 0.4f;
    }
}

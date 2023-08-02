using EFT;
using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINScatterSettings
    {
        [DefaultValue(0.03f)]
        [Minimum(0f)]
        [Maximum(3f)]
        [Rounding(100)]
        public float MinScatter = 0.03f;

        [DefaultValue(0.15f)]
        [Minimum(0f)]
        [Maximum(3f)]
        [Rounding(100)]
        public float WorkingScatter = 0.15f;

        [DefaultValue(0.4f)]
        [Minimum(0f)]
        [Maximum(3f)]
        [Rounding(100)]
        public float MaxScatter = 0.4f;
    }
}

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
        [Default(1f)]
        [MinMax(0.1f, 10f, 100f)]
        public float ScatterMultiplier = 1f;
    }
}
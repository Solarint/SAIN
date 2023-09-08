using SAIN.Attributes;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINScatterSettings
    {
        [Name("EFT Scatter Multiplier")]
        [Description("Higher = more scattering. Modifies EFT's default scatter feature. 1.5 = 1.5x more scatter")]
        [Default(1f)]
        [MinMax(0.1f, 10f, 100f)]
        public float ScatterMultiplier = 1f;
    }
}
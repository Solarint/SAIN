using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories;

public class SAINScatterSettings : SAINSettingsBase<SAINScatterSettings>, ISAINSettings
{
    [Name("Arm Injury Scatter Multiplier")]
    [Description("Increase scatter when a bots arms are injured.")]
    [MinMax(1f, 5f, 100f)]
    [Advanced]
    public float HandDamageScatteringMinMax = 1.5f;

    [Name("Arm Injury Aim Speed Multiplier")]
    [Description("Increase scatter when a bots arms are injured.")]
    [MinMax(1f, 5f, 100f)]
    [Advanced]
    public float HandDamageAccuracySpeed = 1.5f;

    [JsonIgnore]
    [Hidden]
    public float DIST_NOT_TO_SHOOT = 0f;

    [JsonIgnore]
    [Hidden]
    public float FromShot = 0.002f;

    public override void Apply(BotSettingsComponents settings)
    {
        settings.Scattering.HandDamageScatteringMinMax = HandDamageScatteringMinMax;
        settings.Scattering.HandDamageAccuracySpeed = HandDamageAccuracySpeed;
        settings.Scattering.DIST_NOT_TO_SHOOT = DIST_NOT_TO_SHOOT;
        settings.Scattering.FromShot = FromShot;
    }
}
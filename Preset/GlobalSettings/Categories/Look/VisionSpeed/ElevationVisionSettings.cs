using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings;

public class ElevationVisionSettings : SAINSettingsBase<ElevationVisionSettings>, ISAINSettings
{
    public bool Enabled = true;

    [Name("High Elevation Angle Range")]
    [Description(
        "The difference of angle from the bot's vision to the enemy to fully apply HighElevationVisionModifier. " +
        "The modifier is smoothed out by the angle differnce. So 1.2x at +60 degree, 1.1x at +30 degrees...and so on.")]
    [MinMax(1f, 90f, 1f)]
    public float HighElevationMaxAngle = 60f;

    [Name("High Elevation Vision Modifier")]
    [Description(
        "Bots will see players this much slower when the enemy's altitude is higher than the bot when the vision angle difference is equal or greater than HighElevationMaxAngle. " +
        "Higher is slower speed, so 1.2 would result in bots taking 20% longer to spot an enemy")]
    [MinMax(1f, 5f, 100f)]
    public float HighElevationVisionModifier = 2.5f;

    [Name("Low Elevation Angle Range")]
    [Description(
        "The difference of angle from the bot's vision to the enemy to fully apply LowElevationVisionModifier. " +
        "The modifier is smoothed out by the angle differnce. So 0.85x at -30 degree, 0.95x at -10 degrees...and so on.")]
    [MinMax(1f, 90f, 1f)]
    public float LowElevationMaxAngle = 30f;

    [Name("Low Elevation Vision Modifier")]
    [Description(
        "Bots will see sprinting players this much slower when the enemy's altitude is lower than the bot when the vision angle difference is equal or greater than LowElevationMaxAngle. " +
        "Higher is slower speed, so 0.85 would result in bots taking 15% shorter to spot an enemy")]
    [MinMax(0.01f, 1f, 100f)]
    public float LowElevationVisionModifier = 0.75f;
}
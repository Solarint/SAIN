using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings;

public class VisionDistanceSettings : SAINSettingsBase<VisionDistanceSettings>, ISAINSettings
{
    [Name("Movement Vision Distance Modifier")]
    [Description(
        "Bots will see moving players this much further. " +
        "Higher is further distance, so 1.75 would result in bots seeing enemies 1.75x further at max player speed. " +
        "Scales with player velocity.")]
    [MinMax(1f, 3f, 100f)]
    public float MovementDistanceModifier = 1.5f;
}
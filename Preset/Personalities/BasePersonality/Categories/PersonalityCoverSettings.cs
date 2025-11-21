using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Preset.Personalities;

public class PersonalityCoverSettings : SAINSettingsBase<PersonalityCoverSettings>, ISAINSettings
{
    [JsonConstructor]
    public PersonalityCoverSettings() { }

    public PersonalityCoverSettings(bool createDefaults)
    {

    }

    [Advanced]
    public bool CanShiftCoverPosition = true;

    [Advanced]
    public float ShiftCoverTimeMultiplier = 1f;

    [Percentage0to1]
    [Advanced]
    public float MoveToCoverNoEnemySpeed = 1f;

    [Percentage0to1]
    [Advanced]
    public float MoveToCoverNoEnemyPose = 1f;

    [Percentage0to1]
    [Advanced]
    public float MoveToCoverHasEnemySpeed = 1f;

    [Percentage0to1]
    [Advanced]
    public float MoveToCoverHasEnemyPose = 1f;
}
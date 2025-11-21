using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings;

public class CoverSettings : SAINSettingsBase<CoverSettings>, ISAINSettings
{
    [MinMax(20f, 100f, 10f)]
    [Advanced]
    public float MaxCoverPathLength = 60f;

    [MinMax(1f, 30f, 1f)]
    [Advanced]
    public float ShiftCoverChangeDecisionTime = 6f;

    [MinMax(2f, 120f, 1f)]
    [Advanced]
    public float ShiftCoverTimeSinceSeen = 30f;

    [MinMax(1f, 120f, 1f)]
    [Advanced]
    public float ShiftCoverTimeSinceEnemyCreated = 30f;

    [MinMax(1f, 120f, 1f)]
    [Advanced]
    public float ShiftCoverNoEnemyResetTime = 10f;

    [MinMax(1f, 120f, 1f)]
    [Advanced]
    public float ShiftCoverNewCoverTime = 10f;

    [MinMax(1f, 120f, 1f)]
    [Advanced]
    public float ShiftCoverResetTime = 10f;

    [MinMax(0.5f, 1.5f, 100f)]
    [Advanced]
    public float CoverMinHeight = 0.75f;

    [MinMax(0f, 30f, 1f)]
    [Advanced]
    public float CoverMinEnemyDistance = 8f;

    [Advanced]
    public bool DebugCoverFinder = false;
}
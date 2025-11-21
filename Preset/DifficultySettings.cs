using SAIN.Attributes;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings;

public class DifficultySettings : SAINSettingsBase<DifficultySettings>, ISAINSettings
{
    [Name("Vision Distance Multiplier")]
    [Description("Higher is more difficult.")]
    [DifficultyModAttribute]
    public float VisibleDistCoef = 1f;

    [Name("Vision Speed Multiplier")]
    [Description("Higher Is More Difficult. A value of 2 means bots will spot enemies twice as fast.")]
    [DifficultyModAttribute]
    public float GainSightCoef = 1f;

    [Name("Scatter Multiplier")]
    [Description("Lower is more difficult.")]
    [DifficultyModAttribute]
    public float ScatteringCoef = 1f;

    [Name("Hearing Distance Multiplier")]
    [Description("Higher is more difficult.")]
    [DifficultyModAttribute]
    public float HearingDistanceCoef = 1f;

    [Name("Aggression Multiplier")]
    [Description("Higher is more difficult. Affects how long bots wait before entering search and how long they stand their ground to return fire when spotting an enemy.")]
    [DifficultyModAttribute]
    public float AggressionCoef = 1f;

    [Name("Precision Speed Multiplier")]
    [Description("Higher is more difficult.")]
    [DifficultyModAttribute]
    public float PRECISION_SPEED_COEF = 1f;

    [Name("Accuracy Speed Multiplier")]
    [Description("Lower is more difficult. Affects how long it takes for a bot to line up a shot when aiming.")]
    [DifficultyModAttribute]
    public float ACCURACY_SPEED_COEF = 1f;

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(this);
    }
}
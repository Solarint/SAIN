using SAIN.Attributes;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings;

public class HitEffectSettings : SAINSettingsBase<HitEffectSettings>, ISAINSettings
{
    [Name("New Hit Reactions")]
    [Description("Disable to get vanilla bot hit reactions. If this is disabled, all below settings will do nothing.")]
    public bool HIT_REACTION_TOGGLE = false;

    [Name("Hit Effect Multiplier")]
    [Description("Higher = getting shot has more affect on bot aim.")]
    [MinMax(0.01f, 5f, 100f)]
    public float DAMAGE_MANUAL_MODIFIER = 1f;

    [Name("Use Hit Point Direction")]
    [Description("Instead of randomly calculating an angle to affect bot aim. " +
        "Use the direction they were hit from, so if they are shot in the right arm, their aim gets kicked to the right. " +
        "If they are shot in the leg, kick their aim down towards their leg.")]
    public bool USE_HIT_POINT_DIRECTION = true;

    [MinMax(0.01f, 2f, 100f)]
    [Advanced]
    public float HIT_POINT_DIRECTION_BASE_DISTANCE = 0.5f;

    [Name("Min Base Hit Effect Angle")]
    [Description("If Use Hit Point Direction is On, this does nothing.")]
    [MinMax(1f, 60f, 10f)]
    [Advanced]
    public float DAMAGE_BASE_MIN_ANGLE = 7f;

    [Name("Max Base Hit Effect Angle")]
    [Description("If Use Hit Point Direction is On, this does nothing.")]
    [MinMax(1f, 90f, 10f)]
    [Advanced]
    public float DAMAGE_BASE_MAX_ANGLE = 10f;

    [Name("Damage Baseline")]
    [Description("The amount of damage a bot received is divided by this number to produce a multiplier for how much to kick their aim. " +
        "So if the value here is 50, and a bot is shot by a bullet that does 100 damage, it will result in their hit reaction being 2x, or twice as impactful.")]
    [MinMax(10f, 100f, 1f)]
    [Advanced]
    public float DAMAGE_RECEIVED_BASELINE = 50;

    [Name("Min Hit Damage Multiplier")]
    [Description("")]
    [MinMax(0.01f, 1f, 100f)]
    [Advanced]
    public float DAMAGE_MIN_MOD = 0.2f;

    [Name("Max Hit Damage Multiplier")]
    [Description("")]
    [MinMax(1f, 10f, 100f)]
    [Advanced]
    public float DAMAGE_MAX_MOD = 3f;

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(this);
    }
}
using SAIN.Attributes;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings;

public class AILimitSettings : SAINSettingsBase<AILimitSettings>, ISAINSettings
{
    [Name("Limit SAIN Function in AI vs AI - Global Toggle")]
    [Description("Disables certains functions when ai are fighting other ai, and they aren't close to a human player. Turn off if you are spectating ai in free-cam.")]
    public bool LimitAIvsAIGlobal = true;

    [Description("How often (in seconds) to check distances to all human players.")]
    [MinMax(1f, 5f, 10f)]
    public float AILimitUpdateFrequency = 3f;

    [Description("Defines the ranges that different tiers of AI limit are set. " +
        "If a bot is further than this number (in meters) from the closest Human Player, " +
        "they will be assigned this AI Limit setting.")]
    [MinMax(150f, 600f, 1f)]
    public Dictionary<AILimitSetting, float> AILimitRanges = new()
    {
        { AILimitSetting.Far, 150f },
        { AILimitSetting.VeryFar, 250f },
        { AILimitSetting.Narnia, 400f },
    };

    [Name("Limit AI vs AI Vision")]
    [Description("Reduces visible range for bots vs other bots if they are bot far from a human player.")]
    public bool LimitAIvsAIVision = true;

    [MinMax(10f, 200f, 1f)]
    public Dictionary<AILimitSetting, float> MaxVisionRanges = new()
    {
        { AILimitSetting.Far, 200f },
        { AILimitSetting.VeryFar, 100f },
        { AILimitSetting.Narnia, 50f },
    };

    [Name("Limit AI vs AI Hearing")]
    [Description("Reduces hearing distance for bots vs other bots if they are bot far from a human player.")]
    public bool LimitAIvsAIHearing = true;

    [MinMax(10f, 200f, 1f)]
    public Dictionary<AILimitSetting, float> MaxHearingRanges = new()
    {
        { AILimitSetting.Far, 100f },
        { AILimitSetting.VeryFar, 60f },
        { AILimitSetting.Narnia, 25f },
    };

}
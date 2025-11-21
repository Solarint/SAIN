using SAIN.Attributes;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Preset.Personalities;

public class PersonalityGeneralSettings : SAINSettingsBase<PersonalityGeneralSettings>, ISAINSettings
{
    [Name("Aggression Multiplier")]
    [Description("Linearly increases or decreases search time and hold ground time.")]
    [MinMax(0.01f, 5f, 100)]
    public float AggressionMultiplier = 1f;

    [Name("Kick Open All Possible Doors")]
    [Description("If this personality has an enemy, always kick open doors if they can.")]
    public bool KickOpenAllDoors = false;
    
    [Name("Dogfight - Path Distance - Start")]
    [MinMax(0.0f, 50f, 100)]
    public float DOGFIGHT_PATH_DIST_START = 10;

    [Name("Dogfight - Time Since Seen - End")]
    [MinMax(0.0f, 50f, 100)]
    public float DOGFIGHT_TIMESINCESEEN_START = 1;
    
    [Name("Dogfight - Path Distance - End")]
    [MinMax(0.0f, 50f, 100)]
    public float DOGFIGHT_PATH_DIST_END = 15;

    [Name("Dogfight - Time Since Seen - End")]
    [MinMax(0.0f, 60f, 100)]
    public float DOGFIGHT_TIMESINCESEEN_END = 8;

    [Name("Hold Ground Base Time")]
    [Description("The base time, before modifiers, that a personality will stand their ground and shoot or return fire on an enemy if caught out of cover.")]
    [Advanced]
    [MinMax(0, 3f, 10)]
    public float HoldGroundBaseTime = 1f;

    [Advanced]
    [MinMax(0f, 5f, 100)]
    public float HoldGroundMinRandom = 0.66f;

    [Advanced]
    [MinMax(0f, 5f, 100)]
    public float HoldGroundMaxRandom = 1.5f;

    [Name("Suppression Resistance")]
    [Description("Higher = Less affected by suppression. A Value of 0 means No Resistance. " +
        "A Value of 1 means Full Resistance. " +
        "The final resistance number is the mid-point between their personality and bot type resistance. " +
        "So a value of 0.25 for personality and a value of 0.75 for bot type would result in 0.5")]
    [MinMax(0.0f, 1f, 100)]
    public float SuppressionResistance = 0f;


    [Name("Enemy Suppression Toggle")]
    [Category("Enemy Suppression")]
    public bool TARGET_SUPPRESS_TOGGLE = true;
    
    [Name("Suppression Distance - Close")]
    [Category("Enemy Suppression")]
    [Description("If a enemy's visible path point is closer than this to where a bot thinks they are, they can suppress without checking the angle")]
    [MinMax(0f, 10f, 100f)]
    public float TARGET_SUPPRESS_DIST = 3f;

    [Name("Suppression Distance - Far")]
    [Category("Enemy Suppression")]
    [Description("If a enemy's visible path point is closer than this to where a bot thinks they are, they can suppress after checking the angle.")]
    [MinMax(0f, 30f, 100f)]
    public float TARGET_SUPPRESS_DIST_MAX = 12f;

    [Name("Suppression Distance - Far Angle")]
    [Category("Enemy Suppression")]
    [Description("If the horizontal angle from an enemy visible path point to where their last known position is less than this, they can suppress.")]
    [MinMax(0f, 180f, 1f)]
    public float MAX_TARGET_SUPPRESS_ANGLE = 45f;
    
    [Category("Enemy Suppression")]
    [MinMax(0f, 180f, 10f)]
    public float TimeSinceSeenToSuppress = 3f;

    [Category("Enemy Suppression")]
    [MinMax(0f, 180f, 10f)]
    public float TimeSinceShotAtToSuppress = 12f;

    [Category("Enemy Suppression")]
    [MinMax(0f, 180f, 10f)]
    public float TimeSinceShotToSuppress = 12f;

    [Category("Enemy Sniper Reaction")]
    [Description("If a bot thinks it is under fire from a sniper, they will always want to sprint to cover.")]
    [Name("Always Sprint")]
    public bool ENEMYSNIPER_ALWAYS_SPRINT_COVER = true;

    [Category("Enemy Sniper Reaction")]
    [Description("If a bot thinks it is under fire from a sniper, they will always sprint while seeking that sniper.")]
    [Name("Always Sprint")]
    public bool ENEMYSNIPER_ALWAYS_SPRINT_SEARCH = true;

    [Category("Enemy Sniper Reaction")]
    [Name("Distance Under Fire")]
    [Description("If an enemy is further than this distance, in meters. They will be considered a sniper if shooting at a bot.")]
    [MinMax(30f, 250f, 1f)]
    public float ENEMYSNIPER_DISTANCE = 85f;

    [Category("Enemy Sniper Reaction")]
    [Name("Distance Enemy is no longer Sniper")]
    [Description("If an enemy is closer than this distance, in meters. They will no longer be considered a sniper if they previously were.")]
    [MinMax(30f, 250f, 1f)]
    public float ENEMYSNIPER_DISTANCE_END = 75f;
}
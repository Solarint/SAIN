using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories;

public class SAINLookSettings : SAINSettingsBase<SAINLookSettings>, ISAINSettings
{
    [Name("Can Use Flashlights")]
    public bool CAN_USE_LIGHT = true;

    [Name("Full 360 Vision Cheat Vision")]
    [Advanced]
    public bool FULL_SECTOR_VIEW = false;

    [NameAndDescription("Vision Speed Distance Clamp",
        "Lower Bot Vision Speed by distance up to a maximum of this value")]
    [MinMax(50, 500f)]
    [Advanced]
    public float MAX_DIST_CLAMP_TO_SEEN_SPEED = 500f;

    [NameAndDescription("NightVision Visible Angle",
        "The Maximum Angle of a bot's cone of vision with NightVision Enabled")]
    [MinMax(25, 180)]
    [Advanced]
    public float VISIBLE_ANG_NIGHTVISION = 90f;

    //[Hidden]
    //public float LOOK_THROUGH_PERIOD_BY_HIT = 0f;

    [NameAndDescription("FlashLight Visible Angle",
        "The Maximum Angle of a bot's cone of vision with Flashlight Enabled")]
    [MinMax(10, 180)]
    [Advanced]
    public float VISIBLE_ANG_LIGHT = 35f;

    [NameAndDescription("White Light Visible Distance",
        "How far a bot can see enemies with a White Light Enabled")]
    [MinMax(10, 100f)]
    [Advanced]
    public float VISIBLE_DISNACE_WITH_LIGHT = 65f;

    [NameAndDescription("IR Light Visible Distance",
        "How far a bot can see enemies with an IR Light Enabled, if they are using NVGs")]
    [MinMax(10, 100f)]
    [Advanced]
    public float VISIBLE_DISNACE_WITH_IR_LIGHT = 65f;

    [NameAndDescription("Lose Vision Ability Time",
        "How Long after losing vision a bot will still be able to sense an enemy")]
    [MinMax(0.01f, 3f, 100f)]
    [Advanced]
    [JsonIgnore]
    [Hidden]
    public float GOAL_TO_FULL_DISSAPEAR = 0.33f;

    [NameAndDescription("Lose Vision Ability Foliage Time",
        "How Long after losing vision a bot will still be able to sense an enemy")]
    [MinMax(0.01f, 3f, 100f)]
    [Advanced]
    [JsonIgnore]
    [Hidden]
    public float GOAL_TO_FULL_DISSAPEAR_GREEN = 0.33f;

    [NameAndDescription("Lose Shoot Ability Time",
        "How Long after losing vision a bot will still be able to shoot an enemy")]
    [MinMax(0.01f, 3f, 100f)]
    [Advanced]
    [JsonIgnore]
    [Hidden]
    public float GOAL_TO_FULL_DISSAPEAR_SHOOT = 0.15f;

    //[NameAndDescription("Max Grass Vision",
    //    "How far into grass a bot will be able to see, how far the depth must be to lose visibilty")]
    //[Default(1f)]
    //[MinMax(0.0f, 1f, 100f)]
    //[Advanced]
    //public float MAX_VISION_GRASS_METERS = 1f;
    //
    //[Hidden]
    //public float MAX_VISION_GRASS_METERS_OPT = 1f;
    //
    //[Hidden]
    //public float MAX_VISION_GRASS_METERS_FLARE = 4f;
    //
    //[Hidden]
    //public float MAX_VISION_GRASS_METERS_FLARE_OPT = 0.25f;
    //
    //[NameAndDescription("Vision Distance No Foliage",
    //    "Bots will not see foliage at this distance or less, so if a target is below this number in distance, they will ignore foliage")]
    //[Default(3f)]
    //[MinMax(1f, 100f)]
    //[Advanced]
    //public float NO_GREEN_DIST = 3f;
    //
    //[NameAndDescription("Vision Distance No Grass",
    //    "Bots will not see grass at this distance or less, so if a target is below this number in distance, they will ignore grass")]
    //[Default(3f)]
    //[MinMax(1f, 100f)]
    //[Advanced]
    //public float NO_GRASS_DIST = 3f;

    [Hidden]
    [JsonIgnore]
    public bool SHOOT_FROM_EYES = false;

    [Hidden]
    [JsonIgnore]
    public float COEF_REPEATED_SEEN = 1f;

    public override void Apply(BotSettingsComponents settings)
    {
        settings.Look.CAN_USE_LIGHT = CAN_USE_LIGHT;
        settings.Look.FULL_SECTOR_VIEW = FULL_SECTOR_VIEW;
        settings.Look.MAX_DIST_CLAMP_TO_SEEN_SPEED = MAX_DIST_CLAMP_TO_SEEN_SPEED;
        settings.Look.VISIBLE_ANG_NIGHTVISION = VISIBLE_ANG_NIGHTVISION;
        settings.Look.VISIBLE_ANG_LIGHT = VISIBLE_ANG_LIGHT;
        settings.Look.VISIBLE_DISNACE_WITH_LIGHT = VISIBLE_DISNACE_WITH_LIGHT;
        settings.Look.GOAL_TO_FULL_DISSAPEAR = GOAL_TO_FULL_DISSAPEAR;
        settings.Look.GOAL_TO_FULL_DISSAPEAR_GREEN = GOAL_TO_FULL_DISSAPEAR_GREEN;
        settings.Look.GOAL_TO_FULL_DISSAPEAR_SHOOT = GOAL_TO_FULL_DISSAPEAR_SHOOT;
        settings.Look.SHOOT_FROM_EYES = SHOOT_FROM_EYES;
        settings.Look.COEF_REPEATED_SEEN = COEF_REPEATED_SEEN;
    }
}
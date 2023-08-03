
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;

namespace SAIN.BotSettings.Categories
{
    public class SAINLookSettings
    {
        public float VisionSpeedModifier = 1;
        public float CloseVisionSpeed = 1;
        public float FarVisionSpeed = 1;
        public float CloseFarThresh = 50;

        [Name("Can Use Flashlights")]
        [DefaultValue(true)]
        public bool CAN_USE_LIGHT = true;

        [Name("Full 360 Vision Cheat Vision")]
        [DefaultValue(false)]
        public bool FULL_SECTOR_VIEW = false;

        [Name("Vision Speed Distance Clamp")]
        [Description("Lower Bot Vision Speed by distance up to a maximum of this value")]
        [DefaultValue(500f)]
        [Minimum(50)]
        [Maximum(500f)]
        [Rounding(1)]
        public float MAX_DIST_CLAMP_TO_SEEN_SPEED = 500f;

        [Name("NightVision On Distance")]
        [Description("After a bot is below this number in their vision distance, they will turn on night vision if available")]
        [DefaultValue(75f)]
        [Minimum(10f)]
        [Maximum(250f)]
        [Rounding(1)]
        public float NIGHT_VISION_ON = 75f;

        [Name("NightVision Off Distance")]
        [Description("After a bot is above this number in their vision distance, they will turn off night vision if enabled")]
        [DefaultValue(125f)]
        [Minimum(10f)]
        [Maximum(250f)]
        [Rounding(1)]
        public float NIGHT_VISION_OFF = 125f;

        [Name("NightVision Visible Distance")]
        [Description("How far a bot can see with NightVision Enabled")]
        [DefaultValue(125f)]
        [Minimum(10f)]
        [Maximum(250f)]
        [Rounding(1)]
        public float NIGHT_VISION_DIST = 125f;

        [Name("NightVision Visible Angle")]
        [Description("The Maximum Angle of a bot's cone of vision with NightVision Enabled")]
        [DefaultValue(90f)]
        [Minimum(25)]
        [Maximum(180)]
        [Rounding(1)]
        public float VISIBLE_ANG_NIGHTVISION = 90f;

        [IsHidden(true)]
        [DefaultValue(0.0f)]
        public float LOOK_THROUGH_PERIOD_BY_HIT = 0f;

        [Name("FlashLight On Distance")]
        [Description("After a bot is below this number in their vision distance, they will turn on their flashlight if available")]
        [DefaultValue(40f)]
        [Minimum(10)]
        [Maximum(180)]
        [Rounding(1)]
        public float LightOnVisionDistance = 40f;

        [Name("FlashLight Visible Angle")]
        [Description("The Maximum Angle of a bot's cone of vision with Flashlight Enabled")]
        [DefaultValue(30f)]
        [Minimum(10)]
        [Maximum(180)]
        [Rounding(1)]
        public float VISIBLE_ANG_LIGHT = 30f;

        [Name("FlashLight Visible Distance")]
        [Description("How far a bot can see with a Flashlight Enabled")]
        [DefaultValue(50f)]
        [Minimum(10)]
        [Maximum(180)]
        [Rounding(1)]
        public float VISIBLE_DISNACE_WITH_LIGHT = 50f;

        [Name("Lose Vision Ability Time")]
        [Description("How Long after losing vision a bot will still be able to sense an enemy")]
        [DefaultValue(0.25f)]
        [Minimum(0.01f)]
        [Maximum(3f)]
        [Rounding(100)]
        public float GOAL_TO_FULL_DISSAPEAR = 0.25f;

        [Name("Lose Vision Ability Foliage Time")]
        [Description("How Long after losing vision a bot will still be able to sense an enemy")]
        [DefaultValue(0.15f)]
        [Minimum(0.01f)]
        [Maximum(3f)]
        [Rounding(100)]
        public float GOAL_TO_FULL_DISSAPEAR_GREEN = 0.15f;

        [Name("Lose Shoot Ability Time")]
        [Description("How Long after losing vision a bot will still be able to shoot an enemy")]
        [DefaultValue(0.01f)]
        [Minimum(0.01f)]
        [Maximum(3f)]
        [Rounding(100)]
        public float GOAL_TO_FULL_DISSAPEAR_SHOOT = 0.01f;

        [Name("Lose Shoot Ability Time")]
        [Description("How far into grass a bot will be able to see, how far the depth must be to lose visibilty")]
        [DefaultValue(1f)]
        [Minimum(0.0f)]
        [Maximum(1f)]
        [Rounding(100)]
        public float MAX_VISION_GRASS_METERS = 1f;

        [DefaultValue(1f)]
        [IsHidden(true)]
        public float MAX_VISION_GRASS_METERS_OPT = 1f;

        [DefaultValue(4f)]
        [IsHidden(true)]
        public float MAX_VISION_GRASS_METERS_FLARE = 4f;

        [DefaultValue(0.25f)]
        [IsHidden(true)]
        public float MAX_VISION_GRASS_METERS_FLARE_OPT = 0.25f;

        [Name("Vision Distance No Foliage")]
        [Description("Bots will not see foliage at this distance or less, so if a target is below this number in distance, they will ignore foliage")]
        [DefaultValue(3f)]
        [Minimum(10f)]
        [Maximum(1f)]
        [Rounding(10)]
        public float NO_GREEN_DIST = 3f;

        [Name("Vision Distance No Grass")]
        [Description("Bots will not see grass at this distance or less, so if a target is below this number in distance, they will ignore grass")]
        [DefaultValue(3f)]
        [Minimum(10f)]
        [Maximum(1f)]
        [Rounding(10)]
        public float NO_GRASS_DIST = 3f;
    }
}

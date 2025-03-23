using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINAimingSettings : SAINSettingsBase<SAINAimingSettings>, ISAINSettings
    {
        [Category("Aim Target")]
        [Name("Always Aim Center Mass")]
        [Description("Force this bot type to aim for center of mass.")]
        public bool AimCenterMass = true;

        [Category("Time to Aim")]
        [Name("Distance Aim Time Multiplier")]
        [Description("Multiplies the time a bot takes to aim based on distance. So higher values will cause bots to take longer to aim depending on distance.")]
        [MinMax(0.1f, 5f, 100f)]
        public float DistanceAimTimeMultiplier = 1f;

        [Category("Time to Aim")]
        [Name("Angle Aim Time Multiplier")]
        [Description("Multiplies the time a bot takes to aim based on the angle they have to turn to line up a shot. So higher values will cause bots to take longer to aim depending on the angle to turn.")]
        [MinMax(0.1f, 5f, 100f)]
        public float AngleAimTimeMultiplier = 1f;

        [Category("Time to Aim")]
        [Name("Faster CQB Reactions")]
        [Description("Sets whether this bot reacts and aims faster before being able to shoot at close ranges")]
        public bool FasterCQBReactions = true;

        [Category("Time to Aim")]
        [Name("Faster CQB Reactions Max Distance")]
        [Description("Max distance a bot can react faster for Faster CQB Reactions. Scales with distance." +
            "Example: If Max distance is set to 20 meters, and an enemy is 10 meters away. they will react 2x as fast as usual, " +
            "or if an enemy is 15 meters away, they will react 1.5x as fast as usual. " +
            "If the enemy is at 20 meters or further, nothing will happen.")]
        [NameAndDescription(
            "Faster CQB Reactions Max Distance",
            "Max distance a bot can react faster for Faster CQB Reactions. Scales with distance.")]
        [MinMax(5f, 100f)]
        public float FasterCQBReactionsDistance = 30f;

        [Category("Time to Aim")]
        [Name("Faster CQB Reactions Minimum Speed")]
        [Description("Absolute minimum speed (in seconds) that bot can react and shoot")]
        [MinMax(0.05f, 0.75f, 100f)]
        public float FasterCQBReactionsMinimum = 0.33f;

        //[Name("Accuracy Spread Multiplier")]
        //[Description("Higher = less accurate. Modifies a bot's base accuracy and spread. 1.5 = 1.5x higher accuracy spread")]
        //[MinMax(0.1f, 10f, 10f)]
        //public float AccuracySpreadMulti = 1f;

        [Category("Time to Aim")]
        [Name("Max Aiming Upgrade By Time")]
        [Description("Lower = Better. How much to cap this bots aim improvement based on time. 0.25 means they will be able to multiply their aim offset by 0.25x when aiming. Making their shots more accurate")]
        [MinMax(0.01f, 0.99f, 100f)]
        [Advanced]
        [CopyValue]
        public float MAX_AIMING_UPGRADE_BY_TIME = 0.25f;

        [Category("Scatter Modifiers")]
        [Name("Distance To Not Scatter Aim")]
        [Description("If an enemy is closer than this distance, ignore scatter.")]
        [MinMax(0.1f, 30f, 100f)]
        [Advanced]
        public float DIST_TO_SHOOT_NO_OFFSET = 3f;

        [Category("Scatter Modifiers")]
        [Name("Scatter Multiplier - Moving")]
        [MinMax(0.1f, 6f, 100f)]
        [Advanced]
        public float COEF_IF_MOVE = 1.5f;

        [Category("Time to Aim")]
        [Name("Aim Time Multiplier - Moving")]
        [Hidden]
        [JsonIgnore]
        public float TIME_COEF_IF_MOVE = 1.5f;

        [Category("Time to Aim")]
        [Name("Max Aim Time")]
        [Description("EFT CONFIG SETTING. A cap on how long it will take this bot to finish aiming at a target, and begin shooting.")]
        [MinMax(0.01f, 4f, 1000f)]
        [Advanced]
        [CopyValue]
        public float MAX_AIM_TIME = 1f;

        [Hidden]
        [JsonIgnore]
        public int AIMING_TYPE = 1;

        //[Name("Friendly Fire Spherecast Size")]
        //[Description("")]
        //[MinMax(0f, 0.5f, 100f)]
        //[Advanced]
        //public float SHPERE_FRIENDY_FIRE_SIZE = 0.15f;

        [Hidden]
        public float DAMAGE_TO_DISCARD_AIM_0_100 = 100;

        [Category("Time to Aim")]
        [NameAndDescription(
            "Hit Reaction Recovery Time",
            "How much time it takes to recover a bot's aim when they get hit by a bullet")]
        [MinMax(0.1f, 0.99f, 100f)]
        [Advanced]
        public float BASE_HIT_AFFECTION_DELAY_SEC = 0.65f;

        //[NameAndDescription(
        //    "Minimum Hit Reaction Angle",
        //    "How much to kick a bot's aim when they get hit by a bullet")]
        //[MinMax(0f, 25f, 10f)]
        //[Advanced]
        //public float BASE_HIT_AFFECTION_MIN_ANG = 3f;
        //
        //[NameAndDescription(
        //    "Maximum Hit Reaction Angle",
        //    "How much to kick a bot's aim when they get hit by a bullet")]
        //[MinMax(0f, 25f, 10f)]
        //[Advanced]
        //public float BASE_HIT_AFFECTION_MAX_ANG = 5f;

        [Category("Time to Aim")]
        [Name("Hit Aim Time Penalty Seconds - Min")]
        [MinMax(0f, 1f, 100f)]
        [Advanced]
        public float MIN_TIME_DISCARD_AIM_SEC = 0.5f;

        [Category("Time to Aim")]
        [Name("Hit Aim Time Penalty Seconds - Max")]
        [MinMax(0f, 2f, 100f)]
        [Advanced]
        public float MAX_TIME_DISCARD_AIM_SEC = 1.5f;

        [Hidden]
        [JsonIgnore]
        public float ANY_PART_SHOOT_TIME = 2f;

        [Category("Time to Aim")]
        [Name("Enemy First Contact Reaction Delay")]
        [MinMax(0f, 1f, 100f)]
        [Advanced]
        public float FIRST_CONTACT_ADD_SEC = 0.2f;

        [Hidden]
        [JsonIgnore]
        public float FIRST_CONTACT_ADD_CHANCE_100 = 100f;

        [Hidden]
        [JsonIgnore]
        public float OFFSET_RECAL_ANYWAY_TIME = 30f;

        //[Hidden]
        //[JsonIgnore]
        //public float RECALC_SQR_DIST = 2f * 2f;
    }
}
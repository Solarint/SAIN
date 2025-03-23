using SAIN.Attributes;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings
{
    public class SteeringSettings : SAINSettingsBase<SteeringSettings>, ISAINSettings
    {
        [Name("Smooth Bot Turn")]
        [Description("When this is enabled, bots will not turn at a constant speed, scale the speed based on the angle amount they are turning.")]
        [Category("Character Turning")]
        public bool SMOOTH_TURN_TOGGLE = true;

        [Name("Turn Angle Max")]
        [Description("The maximum angle, in degrees, to scale turn speed by.")]
        [Category("Character Turning")]
        [Advanced]
        [MinMax(30, 180f, 1f)]
        public float SteerSpeed_MaxAngle = 150f;

        [Name("Turn Angle Min")]
        [Description("The minimum angle, in degrees, to scale turn speed by.")]
        [Category("Character Turning")]
        [Advanced]
        [MinMax(0, 150f, 1f)]
        public float SteerSpeed_MinAngle = 5f;

        [Name("Turn Speed Max")]
        [Description("The maximum speed, in degrees per second, a bot can turn.")]
        [Category("Character Turning")]
        [Advanced]
        [MinMax(200, 500, 1f)]
        public float SteerSpeed_MaxSpeed = 360f;

        [Name("Turn Speed Min")]
        [Description("The minimum speed, in degrees per second, a bot can turn.")]
        [Category("Character Turning")]
        [Advanced]
        [MinMax(50, 300, 1f)]
        public float SteerSpeed_MinSpeed = 125f;

        [Name("Last Seen to Last Known Position Distance Threshold")]
        [Description("If the last known position of an enemy (something heard or reported by their squad) is within X distance (meters) to the place they last saw an enemy, focus on the place they were last seen.")]
        [Category("Look Point Decision")]
        [Advanced]
        [MinMax(0f, 50f, 1000f)]
        public float STEER_LASTSEEN_TO_LASTKNOWN_DISTANCE = 2.5f;

        [Name("Base Rotate Speed at Peace")]
        [Description("In Degrees per second.")]
        [Category("Character Turning")]
        [Advanced]
        [MinMax(1f, 500f, 100f)]
        public float STEER_BASE_ROTATE_SPEED_PEACE = 120f;

        [Name("Base Rotate Speed at Combat")]
        [Description("In Degrees per second")]
        [Category("Character Turning")]
        [Advanced]
        [MinMax(1f, 500f, 100f)]
        public float STEER_BASE_ROTATE_SPEED_COMBAT = 250f;

        [Name("Rotate Speed - Min Randomization")]
        [Description("When a bot is told to look around randomly to look for enemies, this is the minimum speed they will turn to a random point. Actual Speed is randomized between Min and Max.")]
        [Category("Look Point Decision: Random Look")]
        [Advanced]
        [MinMax(1f, 500f, 100f)]
        public float STEER_RANDOMLOOK_SPEED_MIN = 40f;

        [Name("Rotate Speed - Max Randomization")]
        [Description("When a bot is told to look around randomly to look for enemies, this is the maximum speed they will turn to a random point. Actual Speed is randomized between Min and Max.")]
        [Category("Look Point Decision: Random Look")]
        [Advanced]
        [MinMax(1f, 500f, 100f)]
        public float STEER_RANDOMLOOK_SPEED_MAX = 160f;

        [Name("Rotate Speed - Scaling Coeficient")]
        [Description("Multiplies the scaling in the smooth look function. Higher number = faster max speed depending on the turn angle of a bot.")]
        [Category("Look Point Decision: Random Look")]
        [Advanced]
        [MinMax(1f, 5f, 100f)]
        public float STEER_RANDOMLOOK_SPEED_MAX_COEF = 2f;

        [Name("Aim Turn Speed")]
        [Description("The maximum speed, in degrees per second, a bot can turn while they are aiming.")]
        [Category("Character Turning")]
        [Advanced]
        [MinMax(150f, 500f, 1f)]
        public float AimTurnSpeed = 300f;

        public override void Init(List<ISAINSettings> list)
        {
            list.Add(this);
        }
    }
}
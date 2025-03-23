using Newtonsoft.Json;
using SAIN.Attributes;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings
{
    public class MoveSettings : SAINSettingsBase<MoveSettings>, ISAINSettings
    {
        [Name("Can Lean - Global")]
        [Description("Can any bot lean while peeking and while outside of cover?")]
        [Category("Movement Option Toggles")]
        public bool LEAN_TOGGLE = true;

        [Name("Can Lean in Cover - Global")]
        [Description("Can any bot lean while in cover?")]
        [Category("Movement Option Toggles")]
        public bool LEAN_INCOVER_TOGGLE = true;

        [Name("Can Jump - Global")]
        [Description("Can any bot Jump?")]
        [Category("Movement Option Toggles")]
        public bool JUMP_TOGGLE = true;

        [Name("Can Auto Pose - Global")]
        [Description("Does any bot automatically adjust their crouch height depending on objects between them and their enemy?")]
        [Category("Movement Option Toggles")]
        public bool AUTOCROUCH_TOGGLE = true;

        [Name("Can Go Prone - Global")]
        [Description("Can any bot go Prone at all?")]
        [Category("Movement Option Toggles")]
        public bool PRONE_TOGGLE = true;

        [Name("Can Go Prone from Suppression - Global")]
        [Description("Can any bot go Prone as a panic response to being suppressed?")]
        [Category("Movement Option Toggles")]
        public bool PRONE_SUPPRESS_TOGGLE = true;

        [Name("Can Vault - Global")]
        [Description("Can any bot Vault?")]
        [Category("Movement Option Toggles")]
        public bool VAULT_TOGGLE = true;

        [Name("Can Vault to get Unstuck - Global")]
        [Description("Can any bot Vault if they are stuck on map geometry?")]
        [Category("Movement Option Toggles")]
        public bool VAULT_UNSTUCK_TOGGLE = true;

        [Name("Force Constant Sprint Speed")]
        [Description("In vanilla, bot movement speed while sprinting is set to a constant speed, if this is disabled, bots will use the same movement speed calculations as a normal player.")]
        [Category("Sprinting")]
        [Advanced]
        public bool EditSprintSpeed = false;

        [Name("Not Moving Distance Threshold")]
        [Description("How far a bot can be from their last position before they are considered Not Moving. In Meters Squared")]
        [Category("Sprinting")]
        [Advanced]
        [MinMax(0.01f, 1.5f, 100f)]
        public float BotSprintNotMovingThreshold = 0.5f;

        [Name("Not Moving Check Frequency")]
        [Description("Every X seconds, check if a bot has moved from their last position.")]
        [Category("Sprinting")]
        [Advanced]
        [MinMax(0.01f, 1.5f, 100f)]
        public float BotSprintNotMovingCheckFreq = 0.5f;

        [Name("Not Moving Vault Time")]
        [Description("If a bot is not moving for this length of time or longer, try vaulting to get themselves unstuck")]
        [Category("Sprinting")]
        [Advanced]
        [MinMax(0.01f, 1.5f, 100f)]
        public float BotSprintTryVaultTime = 0.25f;

        [Name("Not Moving Jump Time")]
        [Description("If a bot is not moving for this length of time or longer, try jumping to get themselves unstuck")]
        [Category("Sprinting")]
        [Advanced]
        [MinMax(0.01f, 1.5f, 100f)]
        public float BotSprintTryJumpTime = 0.66f;

        [Name("Not Moving Recalculate Time")]
        [Description("If a bot is not moving for this length of time or longer, recalculate a path to their destination")]
        [Category("Sprinting")]
        [Advanced]
        [MinMax(0.01f, 3f, 100f)]
        public float BotSprintRecalcTime = 1.5f;

        [Name("First Turn Dot Product Min")]
        [Description("The minimum Dot Product before a bot will activate sprint when they are first told to sprint to a position.")]
        [Category("Sprinting")]
        [Advanced]
        [MinMax(0f, 1f, 1000f)]
        public float BotSprintFirstTurnDotThreshold = 0.925f;

        [Name("Path Corner Reach Distance")]
        [Description("How far from a corner along a path before a bot considers it self arrived, and attemps to navigate to the next corner.")]
        [Category("Sprinting")]
        [Advanced]
        [MinMax(0.01f, 1f, 100f)]
        public float BotSprintCornerReachDist = 0.15f;

        [Name("Final Destination Reach Distance")]
        [Description("How far, in meters, to a bot's final destination before they will pause sprinting.")]
        [Category("Sprinting")]
        [Advanced]
        [MinMax(0.01f, 2f, 100f)]
        public float BotSprintDistanceToStopSprintDestination = 1.2f;

        [Name("Max Corner Angle to Pause Sprint")]
        [Description("When a bot approaches a turn along a path, this is the maximum degrees before they pause sprinting to turn instead of keeping sprint active while they turn.")]
        [Category("Sprinting")]
        [Advanced]
        [MinMax(1f, 90f, 1f)]
        public float BotSprintCurrentCornerAngleMax = 25f;

        [Name("Max Turning Speed - Sprint Active")]
        [Description("The maximum speed that bots can turn while sprinting is active. In Degrees per Second.")]
        [Category("Sprinting")]
        [Advanced]
        [MinMax(1f, 500f, 1f)]
        public float BotSprintTurnSpeedWhileSprint = 300f;

        [Name("Max Turning Speed - Sprint Paused")]
        [Description("The maximum speed that bots can turn after they stop sprinting to turn to a new direction. In Degrees per Second.")]
        [Category("Sprinting")]
        [Advanced]
        [MinMax(1f, 500f, 1f)]
        public float BotSprintTurningSpeed = 400f;

        public override void Init(List<ISAINSettings> list)
        {
            list.Add(this);
        }
    }
}
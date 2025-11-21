using SAIN.Attributes;
using SAIN.Components.RotationController;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings;

public class SteeringSettings : SAINSettingsBase<SteeringSettings>, ISAINSettings
{
    [Name("Max Path Length")]
    [Description("How far along a path to an enemy a bot will check vision to, in meters.")]
    [Category("Enemy Path Visibility System")]
    [MinMax(5f, 500f, 1)]
    [Advanced]
    public float MaxPathLengthPathVision = 75.0f;

    [Hidden]
    public float characterHeight = 1.5f;
    [Hidden]
    public float startHeight = 0.25f;

    [Name("Path Nodes Stack Height")]
    [Description("X number of points will be generated above each node in the path")]
    [Category("Enemy Path Visibility System")]
    [MinMax(2f, 6, 1)]
    [Advanced]
    public float GeneratePointStackHeight = 4;

    [Name("Distance Between Nodes")]
    [Category("Enemy Path Visibility System")]
    [MinMax(0.1f, 2, 1000)]
    [Advanced]
    public float DistanceBetweenPoints = 0.66f;

    [Name("Random Bot Aim Sway")]
    [Category("Random Sway")]
    public bool RANDOMSWAY_TOGGLE = true;

    [Category("Random Sway")]
    [MinMax(0f, 1f, 1000f)]
    [Advanced]
    public float RANDOMSWAY_CIRCLE_RADIUS = 0.02f;

    [Category("Random Sway")]
    [MinMax(0f, 10f, 10f)]
    [Advanced]
    public float RANDOMSWAY_LOOP_DURATION = 4f;

    [Category("Random Sway")]
    [MinMax(0f, 1f, 1000f)]
    [Advanced]
    public float RANDOMSWAY_CIRCLE_SCALE = 0.015f;

    [MinMax(-90f, -45f, 100f)]
    [Advanced]
    public float MIN_STEERING_PITCH = -65f;

    [Name("Last Seen to Last Known Position Distance Threshold")]
    [Description("If the last known position of an enemy (something heard or reported by their squad) is within X distance (meters) to the place they last saw an enemy, focus on the place they were last seen.")]
    [Advanced]
    [MinMax(0f, 50f, 1000f)]
    public float STEER_LASTSEEN_TO_LASTKNOWN_DISTANCE = 2.5f;

    [Name("Smooth Turn Settings")]
    [Hidden]
    public Dictionary<EBotLookMode, TurnSettings> SMOOTHING_BY_STATE = new() {
        { EBotLookMode.RandomLook, new TurnSettings(0.040f, 120f ) },
        { EBotLookMode.Peace, new TurnSettings(0.050f, 240) },
        { EBotLookMode.Combat, new TurnSettings(0.07f, 300f) },
        { EBotLookMode.CombatSprint, new TurnSettings(0.075f, 300f) },
        { EBotLookMode.CombatVisibleEnemy, new TurnSettings(0.090f, 360f) },
        { EBotLookMode.Aiming, new TurnSettings(0.10f, 360f ) },
    };

    [MinMax(1f, 3f, 1000f)]
    [Advanced]
    public float ConvergenceBoost = 1.1f;   // Multiplier when far from target

    [Advanced]
    [MinMax(16, 2048, 1f)]
    public float PathVisionMinCommandsPerJob = 256f; // Minimum commands per job for path vision jobs

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(this);
    }
}
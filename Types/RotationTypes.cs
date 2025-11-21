using SAIN.Attributes;
using SAIN.Preset.GlobalSettings;
using UnityEngine;

namespace SAIN.Components.RotationController;

public struct TurnSettings(float smoothingValue = 0.5f, float maxTurnSpeed = 360f)
{
    [MinMax(0f, 3f, 100f)]
    public float SmoothingValue = smoothingValue;

    [MinMax(0.01f, 1000f, 100f)]
    public float MaxTurnSpeed = maxTurnSpeed;
    
    [Advanced]
    [Hidden]
    public EBotLookSmoothingMode SmoothingMode = EBotLookSmoothingMode.SmoothDamp;
}

public enum EBotLookMode
{
    Peace,
    Combat,
    CombatSprint,
    CombatVisibleEnemy,
    Aiming,
    RandomLook,
}

public enum EBotLookSmoothingMode
{
    Linear,
    SmoothDamp,
    SmoothDampAngle,
}
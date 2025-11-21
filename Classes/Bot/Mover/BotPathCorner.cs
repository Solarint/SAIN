using SAIN.Components.PlayerComponentSpace;
using System;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Mover;

public struct BotPathCorner(Vector3 startPosition, Vector3 navCornerPosition, EBotCornerType type, int index)
{
    public readonly float GetPercentageOfCornerComplete(float currentSqrMagnitude)
    {
        float ratio = 1f - currentSqrMagnitude / DirectionFromPrevious.SqrMagnitude;
        return Mathf.Clamp(ratio, 0f, 1f) * 100f;
    }

    public EBotCornerStatus Status = EBotCornerStatus.Awaiting;
    public EBotCornerType Type = type;
    public int Index = index;
    public Vector3 Position = navCornerPosition;
    public DirectionCache DirectionFromPrevious = new(startPosition, navCornerPosition);
    public DirectionCache? DirectionToNext = null;
    public float TimeStarted = -1;
    public float TimeComplete = -1;
}

public struct CornerMoveData()
{
    public Vector3 CornerDirectionFromBot = Vector3.zero;
    public Vector3 CornerDirectionFromBotNormal = Vector3.zero;
    public float Dot = 1;
    public float SqrMagnitude = float.MaxValue;
    public float PercentageComplete = 0;
    public float Magnitude = float.MaxValue;
}
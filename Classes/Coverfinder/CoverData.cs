using SAIN.Components.PlayerComponentSpace;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.SubComponents.CoverFinder;

public class CoverData
{
    public Vector3 Position { get; set; }
    public float TimeLastUpdated { get; set; }
    public Vector3 ProtectionDirection { get; set; }
    public float BotDistance { get; set; }
    public bool IsBad { get; set; }
    public CoverStatus PathLengthStatus { get; set; }
    public CoverStatus StraightLengthStatus { get; set; }
    public DirectionData DirectionData { get; set; } = new();
    public float TimeSinceUpdated => Time.time - TimeLastUpdated;
}
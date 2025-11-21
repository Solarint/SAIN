using SAIN.Components.PlayerComponentSpace;
using UnityEngine;

namespace SAIN.Components;

public interface ISPlayer
{
    public Vector3 NavMeshPosition { get; }

    public float GetDistanceToPlayer(string ProfileId);
    public bool IsPlayerInRange(string ProfileId, float maxDistance, out float playerDistance);
}
using SAIN.Models.Structs;
using UnityEngine;

namespace SAIN.SAINComponent.SubComponents.CoverFinder;

/// <summary>
/// Calcs normalized directions from a collider's position to the target and bot positions.
/// </summary>
/// <param name="colliderPosition"></param>
/// <param name="targetPosition"></param>
/// <param name="botPosition"></param>
public struct ColliderDirections(Vector3 colliderPosition, Vector3 targetPosition, Vector3 botPosition)
{
    public Vector3 ColliderToTargetNormal = (targetPosition - colliderPosition).normalized;
    public Vector3 ColliderToBotNormal = (botPosition - colliderPosition).normalized;
}
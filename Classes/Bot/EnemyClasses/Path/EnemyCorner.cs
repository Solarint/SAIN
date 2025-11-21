using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class EnemyCorner(Vector3 groundPoint, int pathIndex)
{
    public int PathIndex { get; } = pathIndex;
    public Vector3 GroundPosition { get; } = groundPoint;
}
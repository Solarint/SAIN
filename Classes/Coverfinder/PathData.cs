using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.SubComponents.CoverFinder;

public class PathData
{
    public NavMeshPath Path { get; private set; }
    public float PathLength { get; private set; }
    public int RoundedPathLength { get; private set; }
    public float TimeSinceLastUpdated => Time.time - TimeLastUpdated;
    public float TimeLastUpdated { get; private set; }

    public void CalcPath(Vector3 origin, Vector3 destination)
    {
        Path ??= new NavMeshPath();
        Path.ClearCorners();
        NavMesh.CalculatePath(origin, destination, NavMesh.AllAreas, Path);
        if (Path.status == NavMeshPathStatus.PathComplete)
        {
            PathLength = Path.CalculatePathLength();
            RoundedPathLength = Mathf.FloorToInt(PathLength);
        }
        else
        {
            PathLength = float.MaxValue;
            RoundedPathLength = int.MaxValue;
        }
        TimeLastUpdated = Time.time;
    }

}
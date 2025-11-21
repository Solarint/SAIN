using SAIN.Components;
using SAIN.Helpers;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes;

public sealed class FlankRoute
{
    public Vector3 FlankPoint;
    public Vector3 FlankPoint2;
    public NavMeshPath FirstPath;
    public NavMeshPath SecondPath;
    public NavMeshPath ThirdPath;
}

public class SAINBotSpaceAwareness : BotComponentClassBase
{
    public SAINBotSpaceAwareness(BotComponent sain) : base(sain)
    {
        CanEverTick = false;
    }

    public static bool ArePathsDifferent(NavMeshPath path1, NavMeshPath path2, float minRatio = 0.5f, float sqrDistCheck = 0.05f)
    {
        Vector3[] path1Corners = path1.corners;
        int path1Length = path1Corners.Length;
        Vector3[] path2Corners = path2.corners;
        int path2Length = path2Corners.Length;

        int sameCount = 0;
        for (int i = 0; i < path1Length; i++)
        {
            Vector3 node = path1Corners[i];

            if (i < path2Length)
            {
                Vector3 node2 = path2Corners[i];
                if (node.IsEqual(node2, sqrDistCheck))
                {
                    sameCount++;
                }
            }
        }
        float ratio = sameCount / path1Length;
        //Logger.LogDebug($"Result = [{ratio <= minRatio}]Path 1 length: {path1.corners.Length} Path2 length: {path2.corners.Length} Same Node Count: {sameCount} ratio: {ratio}");
        return ratio <= minRatio;
    }
}

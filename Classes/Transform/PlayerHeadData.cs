using UnityEngine;

namespace SAIN.Classes.Transform;

public struct PlayerHeadData
{
    public Vector3 HeadLookDirection;
    public Vector3 HeadPosition;

    public static PlayerHeadData Update(PlayerHeadData headData, Vector3 headPosition, Quaternion headRotation, Vector3 headUpVector)
    {
        headData.HeadPosition = headPosition;
        //headData.HeadLookDirection = headRotation * headUpVector;
        headData.HeadLookDirection = headUpVector;
        return headData;
    }
}
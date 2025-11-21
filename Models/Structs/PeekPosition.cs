using SAIN.Helpers;
using System;
using UnityEngine;

namespace SAIN.Models.Structs;

public struct PeekPosition
{
    public PeekPosition(Vector3 point, Vector3 danger)
    {
        Point = point;
        Vector3 direction = danger - point;
        DangerDir = direction;
        DangerDirNormal = direction.Normalize(out float magnitude);
        DangerDistance = magnitude;
    }

    public readonly Vector3 DangerDir;
    public readonly Vector3 DangerDirNormal;
    public readonly float DangerDistance;
    public readonly Vector3 Point;
}

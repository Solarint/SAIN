using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace;

public struct FlashLightPoint
{
    public FlashLightPoint(Vector3 point, float expireTime = 0.25f)
    {
        Point = point;
        TimeCreated = Time.time;
        ExpireTime = expireTime;
    }

    private readonly float ExpireTime;
    public readonly Vector3 Point;
    public readonly float TimeCreated;
    public bool ShallExpire => Time.time - TimeCreated > ExpireTime;
}
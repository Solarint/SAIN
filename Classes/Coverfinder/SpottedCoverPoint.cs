using UnityEngine;

namespace SAIN.SAINComponent.SubComponents.CoverFinder;

public class SpottedCoverPoint(CoverPoint coverPoint)
{
    public const float SPOTTED_PERIOD = 2f;

    public bool TooClose(Vector3 coverPosition, float minDistanceSqr = 2f)
    {
        return (coverPosition - CoverPoint.Position).sqrMagnitude <= minDistanceSqr;
    }

    public CoverPoint CoverPoint { get; private set; } = coverPoint;
    public float TimeCreated { get; private set; } = Time.time;
    public float TimeSinceCreated => Time.time - TimeCreated;

    private readonly float ExpireTime = SPOTTED_PERIOD;
    public bool IsValidAgain => TimeSinceCreated > ExpireTime;
}
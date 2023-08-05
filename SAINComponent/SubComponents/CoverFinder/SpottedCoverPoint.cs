using UnityEngine;

namespace SAIN.SAINComponent.SubComponents.CoverFinder
{
    public class SpottedCoverPoint
    {
        public SpottedCoverPoint(Vector3 position, float expireTime = 2f)
        {
            ExpireTime = expireTime;
            Position = position;
            TimeCreated = Time.time;
        }

        public bool TooClose(Vector3 newPos, float sqrdist = 3f)
        {
            return (Position - newPos).sqrMagnitude > sqrdist;
        }

        public Vector3 Position { get; private set; }
        public float TimeCreated { get; private set; }
        public float TimeSinceCreated => Time.time - TimeCreated;

        private readonly float ExpireTime;
        public bool IsValidAgain => TimeSinceCreated > ExpireTime;
    }
}
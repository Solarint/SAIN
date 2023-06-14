using EFT;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Classes
{
    public class CoverPoint
    {
        public CoverPoint(BotOwner bot, Vector3 point, Collider collider)
        {
            BotOwner = bot;
            Position = point;
            Collider = collider;
            TimeCreated = Time.time;
            CalcPathLength();
        }

        public bool Spotted { get; set; }

        public void CalcPathLength()
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(BotOwner.Position, Position, -1, path))
            {
                PathLengthAtCreation = path.CalculatePathLength();
            }
            else
            {
                PathLengthAtCreation = Mathf.Infinity;
            }
        }

        public float PathLengthAtCreation { get; private set; }

        public float Distance => (BotOwner.Position - Position).magnitude;

        public CoverStatus Status()
        {
            CoverStatus status = CoverStatus.None;

            float distance = Vector3.Distance(Position, BotOwner.Position);

            if (distance <= InCoverDist)
            {
                status = CoverStatus.InCover;
            }
            else if (distance <= CloseCoverDist)
            {
                status = CoverStatus.CloseToCover;
            }
            else if (distance <= MidCoverDist)
            {
                status = CoverStatus.MidRangeToCover;
            }
            else if (distance <= FarCoverDist)
            {
                status = CoverStatus.FarFromCover;
            }

            return status;
        }

        public BotOwner BotOwner { get; private set; }
        public Collider Collider { get; private set; }
        public Vector3 Position { get; set; }
        public float TimeCreated { get; private set; }

        private const float InCoverDist = 0.75f;
        private const float CloseCoverDist = 10f;
        private const float MidCoverDist = 25f;
        private const float FarCoverDist = 50f;
    }
}
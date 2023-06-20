using EFT;
using System;
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
            ReCheckStatusTimer = Time.time;
            PathLengthAtCreation = CalcPathLength();
            Id = Guid.NewGuid().ToString();
        }

        public int HitInCoverUnknownCount { get; set; }
        public int HitInCoverCount { get; set; }

        public string Id { get; set; }

        public bool BotIsUsingThis { get; set; }

        public bool BotIsHere => BotIsUsingThis && Distance < 1f && !Spotted;

        public bool Spotted => HitInCoverCount > 1 || HitInCoverUnknownCount > 0;

        public float CalcPathLength()
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(BotOwner.Position, Position, -1, path))
            {
                return path.CalculatePathLength();
            }
            else
            {
                return Mathf.Infinity;
            }
        }

        public float PathDistance { get; private set; }

        public float PathLengthAtCreation { get; private set; }

        public float Distance => (BotOwner.Position - Position).magnitude;

        public CoverStatus CoverStatus
        {
            get
            {
                ReCheckStatusTimer += Time.deltaTime;
                if (ReCheckStatusTimer < 0.33f)
                {
                    return OldStatus;
                }
                ReCheckStatusTimer = 0f;

                CoverStatus status = CoverStatus.None;

                PathDistance = CalcPathLength();

                if (PathDistance <= InCoverDist)
                {
                    status = CoverStatus.InCover;
                }
                else if (PathDistance <= CloseCoverDist)
                {
                    status = CoverStatus.CloseToCover;
                }
                else if (PathDistance <= MidCoverDist)
                {
                    status = CoverStatus.MidRangeToCover;
                }
                else if (PathDistance <= FarCoverDist)
                {
                    status = CoverStatus.FarFromCover;
                }
                OldStatus = status;
                return status;
            }
        }

        private CoverStatus OldStatus;
        private float ReCheckStatusTimer;

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
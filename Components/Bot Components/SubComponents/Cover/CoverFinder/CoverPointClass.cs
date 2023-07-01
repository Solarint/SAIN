using EFT;
using SAIN.Components;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Classes
{
    public class CoverPoint
    {
        public CoverPoint(SAINComponent sain, Vector3 point, Collider collider)
        {
            SAIN = sain;
            Position = point;
            Collider = collider;
            TimeCreated = Time.time;
            ReCheckStatusTimer = Time.time;
            Id = Guid.NewGuid().ToString();
        }

        private readonly SAINComponent SAIN;
        public int HitInCoverUnknownCount { get; set; }
        public int HitInCoverCount { get; set; }

        public string Id { get; set; }

        public bool BotIsUsingThis { get; set; }

        public bool BotIsHere => CoverStatus == CoverStatus.InCover;

        public bool Spotted
        {
            get
            {
                if (HitInCoverCount > 1 || HitInCoverUnknownCount > 0)
                {
                    return true;
                }
                if (BotIsUsingThis && PointIsVisible())
                {
                    return true;
                }
                return false;
            }
        }

        public bool PointIsVisible()
        {
            if (VisTimer < Time.time)
            {
                VisTimer = Time.time + 0.5f;
                PointVis = false;
                if (SAIN.Enemy != null)
                {
                    Vector3 coverPos = Position;
                    coverPos += Vector3.up * 0.3f;
                    Vector3 start = SAIN.Enemy.EnemyHeadPosition;
                    Vector3 direction = coverPos - start;
                    PointVis = !Physics.Raycast(start, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask);
                }
            }
            return PointVis;
        }

        private bool PointVis;
        private float VisTimer;

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
                float distance = Distance;

                CoverStatus status;
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
                else
                {
                    status = CoverStatus.FarFromCover;
                }
                OldStatus = status;
                return status;
            }
        }

        private CoverStatus OldStatus;
        private float ReCheckStatusTimer;

        public BotOwner BotOwner => SAIN.BotOwner;
        public Collider Collider { get; private set; }
        public Vector3 Position { get; set; }
        public float TimeCreated { get; private set; }

        private const float InCoverDist = 0.75f;
        private const float CloseCoverDist = 10f;
        private const float MidCoverDist = 25f;
    }
}
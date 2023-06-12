using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;
using UnityEngine;
using BepInEx.Logging;
using UnityEngine.UI;

namespace SAIN.Classes
{
    public class NavigationPointObject : SAINBot
    {
        public NavigationPointObject(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            JumpHeight = BotOwner.GetPlayer.MovementContext.SkillManager.StrengthBuffJumpHeightInc;
            HeadOffsetY = SAIN.HeadPosition.y - SAIN.Position.y;
        }

        public bool HasPath => ActivePath != null && PathWayIndexMax != 0;
        public bool HasCurrentDestination => HasPath && PathWayIndex < PathWayIndexMax && PathWayIndexMax != 0;
        public bool MovingAlongPath => HasNextCorner || MovingToFinalCorner;
        public bool HasNextCorner => PathWayIndex < PathWayIndexMax;
        public bool MovingToFinalCorner => PathWayIndexMax > 0 && PathWayIndex == PathWayIndexMax;
        public Vector3 CurrentDestinationPosition => HasPath ? ActivePath[PathWayIndex].Point : Vector3.zero;
        public PathPoint[] ActivePath { get; private set; }
        public float HeadOffsetY { get; private set; }
        public float ReachDistance { get; private set; }
        public PathPoint FinalDestination { get; set; }
        public PathPoint CornerDestination { get; set; }
        public float CornerDestDistance => (BotOwner.Position - CornerDestination.Point).magnitude;
        public float CornerDestSqrDistance => (BotOwner.Position - CornerDestination.Point).sqrMagnitude;
        public int PathWayIndex { get; private set; }
        public int PathWayIndexMax { get; private set; }
        public float JumpHeight { get; private set; }
        public bool FinishedPath { get; private set; } = false;
        public bool BotIsStuck => SAIN.BotStuck.BotIsStuck;

        public void Activate()
        {
            if (!Activated)
            {
                Activated = true;
                if (ActivePath != null)
                {
                    MoveNext();
                }
                else
                {
                    Logger.LogError("Active Path is null, Cannot Start Move.");
                }
            }
            else
            {
                Logger.LogError("Already Activated Dummy.");
            }
        }

        public bool Activated = false;

        public void Update(float reachDist = -1f)
        {
            if (CornerDestination == null || ActivePath == null)
            {
                Logger.LogError("Active Path is null, Cannot Update.");
                return;
            }
            if (!!Activated)
            {
                Logger.LogError("Path is not Activated. Cannot Update.");
                return;
            }

            if (reachDist > 0)
            {
                ReachDistance = reachDist;
            }

            CheckMoveTimer += Time.deltaTime;
            if (CheckMoveTimer > 0.1f)
            {
                CheckMoveTimer = 0f;

                if (CornerDestination.ShallJump)
                {
                    if (BotIsAtPoint(true, 0.5f))
                    {
                        SAIN.Mover.TryJump();
                        MoveNext();
                        return;
                    }
                }

                if (BotIsAtPoint() && !MoveNext() && !FinishedPath)
                {
                    FinishedPath = BotIsAtFinal();
                }
            }
        }

        private bool BotIsAtFinal()
        {
            return BotIsAtPoint(FinalDestination.Point, true);
        }

        private bool MoveNext()
        {
            bool hasNext = HasNextCorner;
            if (hasNext)
            {
                PathWayIndex++;
                CornerDestination = ActivePath[PathWayIndex];
                BotOwner.GetPlayer.Move(CornerDestination.Point - BotOwner.Position);
            }
            return hasNext;
        }

        private bool CanJumpOffLedge(Vector3 direction)
        {
            return false;
        }

        public NavMeshPathStatus GoToPoint(Vector3 point, bool MustHavePath = true, float reachDist = -1f)
        {
            NavMeshPath Path = new NavMeshPath();
            NavMesh.CalculatePath(BotOwner.Position, point, -1, Path);
            if (Path.status != NavMeshPathStatus.PathInvalid)
            {
                if (Path.corners.Length < 2)
                {
                    Logger.LogError($"Corners Length too low! [{Path.corners.Length}]");
                }
                else if (Path.status == NavMeshPathStatus.PathComplete || !MustHavePath)
                {
                    var jumpPath = GetJumpPath(Path);

                    ReachDistance = reachDist > 0 ? reachDist : 0.5f;

                    ActivePath = jumpPath;

                    PathWayIndex = 0;
                    PathWayIndexMax = jumpPath.Length - 1;
                    FinalDestination = jumpPath[PathWayIndexMax];

                    return Path.status;
                }
            }

            Logger.LogWarning($"{Path.status}");
            return Path.status;
        }

        public PathPoint[] GetJumpPath(NavMeshPath Path)
        {
            List<PathPoint> newPath = new List<PathPoint>();

            int i = 0;
            while (i < Path.corners.Length - 1)
            {
                Vector3 cornerA = Path.corners[i];
                // Add the first corner to our new path
                newPath.Add(new PathPoint(cornerA));

                if (i < Path.corners.Length - 3)
                {
                    Vector3 cornerB = Path.corners[i + 1];
                    Vector3 cornerC = Path.corners[i + 2];

                    // If a there is an object in the path to the corner after the next, and the object's collider is short, we may be able to jump over it.
                    if (CanSkipCorner(cornerA, cornerC, out Vector3 result))
                    {
                        newPath.Add(new PathPoint(result, true));
                    }
                    else
                    {
                        newPath.Add(new PathPoint(cornerB));
                    }
                    newPath.Add(new PathPoint(cornerC));

                    // Skip the next 2 corners in the loop
                    i++;
                    i++;
                }
                // Next Corner
                i++;
            }
            return newPath.ToArray();
        }

        private bool CanSkipCorner(Vector3 cornerA, Vector3 cornerC, out Vector3 result)
        {
            var Mask = LayerMaskClass.HighPolyCollider;
            const float UpOffset = 0.15f;
            const float ShiftOffset = 0.1f;

            // The direction from the first corner to the corner after the next
            Vector3 direction = cornerC - cornerA;
            // Raise the position slightly to avoid hitting tiny objects.
            Vector3 rayPoint = cornerA + Vector3.up * UpOffset;

            bool canSkip = false;
            result = Vector3.zero;
            // If a there is an object in the path to the corner after the next, and the object's collider is short, we may be able to jump over it.
            if (Physics.Raycast(rayPoint, direction.normalized, out var hit, direction.magnitude, Mask) && CanJumpOverCollider(hit.collider))
            {
                Vector3 headCorner = cornerA;
                headCorner.y += HeadOffsetY;
                if (!Physics.Raycast(headCorner, direction, direction.magnitude, Mask))
                {
                    Vector3 newMovePoint = hit.point + Vector3.down * UpOffset;
                    Vector3 hitDir = (hit.point - cornerA).normalized * ShiftOffset;
                    newMovePoint -= hitDir;
                    result = newMovePoint;
                    canSkip = true;
                }
            }
            return canSkip;
        }

        public bool CanJumpOverCollider(Collider collider)
        {
            if (collider == null)
            {
                return false;
            }
            return collider.bounds.size.y < JumpHeight * 0.9f;
        }

        public bool BotIsAtPoint(bool Sqr = true, float reachDist = -1f)
        {
            if (reachDist < 0f)
            {
                reachDist = ReachDistance;
            }
            if (Sqr)
            {
                return CornerDestSqrDistance < reachDist;
            }
            return CornerDestDistance < reachDist;
        }

        public bool BotIsAtPoint(Vector3 point, bool Sqr = true, float reachDist = -1f)
        {
            if (reachDist < 0f)
            {
                reachDist = ReachDistance;
            }
            if (Sqr)
            {
                return DistanceToDestinationSqr(point) < reachDist;
            }
            return DistanceToDestination(point) < reachDist;
        }

        public float DistanceToDestinationSqr(Vector3 point)
        {
            return (point - BotOwner.Transform.position).sqrMagnitude;
        }

        public float DistanceToDestination(Vector3 point)
        {
            return (point - BotOwner.Transform.position).magnitude;
        }

        private readonly ManualLogSource Logger;
        private float CheckMoveTimer = 0f;
    }

    public class PathPoint
    {
        public PathPoint(Vector3 point, bool shallJump = false)
        {
            Point = point;
            ShallJump = shallJump;
        }

        public Vector3 Point { get; private set; }
        public bool ShallJump { get; private set; }
    }
}

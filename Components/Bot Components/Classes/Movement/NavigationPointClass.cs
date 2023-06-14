using EFT;
using System;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using BepInEx.Logging;
using SAIN.Helpers;

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
                DebugGizmos.SingleObjects.Line(BotOwner.Position, FinalDestination.Point, Color.white, 0.25f, true, 5f, true);
                Activated = true;
                if (ActivePath != null)
                {
                    MoveNext();
                }
            }
        }

        public bool Activated = false;

        public void Update( float targetSpeed = 1f, float targetPose = 1f )
        {
            SAIN.Mover.SetTargetMoveSpeed(targetSpeed);
            SAIN.Mover.SetTargetPose(targetPose);
            BotOwner.DoorOpener.Update();
        }


        private bool MoveNext()
        {
            bool hasNext = HasNextCorner;
            if (hasNext)
            {
                PathWayIndex++;
                CornerDestination = ActivePath[PathWayIndex];
                List<Vector3> movePath = new List<Vector3>
                {
                    BotOwner.Position,
                    CornerDestination.Point
                };
                DebugGizmos.SingleObjects.Line(BotOwner.Position, CornerDestination.Point, Color.green, 0.25f, true, 5f, true);
                BotOwner.GoToByWay(movePath.ToArray(), ReachDistance);
            }
            return hasNext;
        }

        public NavMeshPathStatus GoToPointJump(Vector3 point, bool MustHavePath = true, float reachDist = 0.5f)
        {
            Activated = false;
            if (NavMesh.SamplePosition(point, out var navHit, 10f, -1))
            {
                NavMeshPath Path = new NavMeshPath();
                if (NavMesh.CalculatePath(BotOwner.Position, navHit.position, -1, Path))
                {
                    if (Path.corners.Length < 2)
                    {
                        Logger.LogError($"Corners Length too low! [{Path.corners.Length}]");
                    }
                    else if (Path.status == NavMeshPathStatus.PathComplete || !MustHavePath)
                    {
                        if (reachDist < 0f)
                        {
                            reachDist = BotOwner.Settings.FileSettings.Move.REACH_DIST;
                        }
                        BotOwner.Mover.GoToByWay(Path.corners, reachDist, Vector3.zero);

                        return Path.status;
                    }
                }
            }

            Logger.LogWarning($"{NavMeshPathStatus.PathInvalid}");
            return NavMeshPathStatus.PathInvalid;
        }

        public bool GoToPoint(Vector3 point, float reachDist = -1f)
        {
            if (NavMesh.SamplePosition(point, out var navHit, 10f, -1))
            {
                NavMeshPath Path = new NavMeshPath();
                if (NavMesh.CalculatePath(BotPosition, navHit.position, -1, Path) && Path.corners.Length > 1)
                {
                    if (reachDist < 0f)
                    {
                        reachDist = BotOwner.Settings.FileSettings.Move.REACH_DIST;
                    }
                    BotOwner.Mover.GoToByWay(Path.corners, reachDist, Vector3.zero);
                    return true;
                }
            }

            Logger.LogWarning($"{NavMeshPathStatus.PathInvalid}");
            return false;
        }

        public NavMeshPathStatus GoToPoint(Vector3 point, bool MustHavePath = false, float reachDist = -1f)
        {
            Activated = false;
            ActivePath = null;
            FinalDestination = null;

            if (NavMesh.SamplePosition(point, out var navHit, 10f, -1))
            {
                NavMeshPath NavPath = new NavMeshPath();
                if (NavMesh.CalculatePath(BotPosition, navHit.position, -1, NavPath))
                {
                    bool start = NavPath.status == NavMeshPathStatus.PathComplete || !MustHavePath;
                    if (NavPath.corners.Length > 1 && start)
                    {
                        BotOwner.Mover.Stop();
                        var movePath = GetPath(NavPath.corners);
                        ReachDistance = reachDist > 0 ? reachDist : 0.5f;
                        ActivePath = movePath;
                        PathWayIndex = 0;
                        PathWayIndexMax = movePath.Length - 1;
                        FinalDestination = movePath[PathWayIndexMax];
                        Activate();
                        return NavPath.status;
                    }
                }
            }

            Logger.LogWarning($"{NavMeshPathStatus.PathInvalid}");
            return NavMeshPathStatus.PathInvalid;
        }

        public PathPoint[] GetPath(Vector3[] Path)
        {
            List<PathPoint> newPath = new List<PathPoint>();

            for (int i = 0;  i < Path.Length; i++)
            {
                newPath.Add(new PathPoint(Path[i]));
            }
            return newPath.ToArray();
        }

        public PathPoint[] GetJumpPath(NavMeshPath Path)
        {
            List<PathPoint> newPath = new List<PathPoint>();

            int i = 0;
            while (i < Path.corners.Length - 1)
            {
                Vector3 cornerA = Path.corners[i];
                // Add the first corner to our new movePath
                newPath.Add(new PathPoint(cornerA));

                if (CheckAllCornersShortcut(Path, i, out int intResult, out var pathAdd))
                {
                    newPath.AddRange(pathAdd);
                }
                i += intResult;
            }
            return newPath.ToArray();
        }

        private bool CheckAllCornersShortcut(NavMeshPath Path, int corner, out int shortcut, out PathPoint[] shortcutPath)
        {
            List<PathPoint> newPath = new List<PathPoint>();

            Vector3 cornerA = Path.corners[corner];
            int i = corner + 1;
            while (i < Path.corners.Length - 1)
            {
                Vector3 cornerB = Path.corners[i];

                // If a there is an object in the movePath to the corner after the next, and the object's collider is short, we may be able to jump over it.
                if (CanSkipCorner2(cornerA, cornerB, out Vector3 result, out bool jump))
                {
                    newPath.Add(new PathPoint(result, jump));
                    newPath.Add(new PathPoint(cornerB));
                    shortcut = i;
                    shortcutPath = newPath.ToArray();
                    return true;
                }
                // Next Corner
                i++;
            }
            shortcutPath = null;
            shortcut = 1;
            return false;
        }

        private bool CanSkipCorner2(Vector3 cornerA, Vector3 cornerC, out Vector3 result, out bool jump)
        {
            var Mask = LayerMaskClass.HighPolyWithTerrainMask;
            const float UpOffset = 1.25f;
            const float LowOffset = 0.5f;
            const float ShiftOffset = 0.5f;

            // The direction from the first corner to the corner after the next
            Vector3 direction = cornerC - cornerA;
            // Raise the position slightly to avoid hitting tiny objects.
            Vector3 rayPoint = cornerA + Vector3.up * UpOffset;

            jump = false;
            bool canSkip = false;
            result = Vector3.zero;
            // If a there is an object in the movePath to the corner after the next, and the object's collider is short, we may be able to jump over it.
            if (!Physics.SphereCast(rayPoint, 0.35f, direction.normalized, out var hita, direction.magnitude, Mask))
            {
                rayPoint = cornerA + Vector3.up * LowOffset;
                if (Physics.SphereCast(rayPoint, 0.15f, direction.normalized, out var hit, direction.magnitude, Mask))
                {
                    Vector3 newMovePoint = hit.point + Vector3.down * LowOffset;
                    Vector3 hitDir = (hit.point - cornerA).normalized * ShiftOffset;
                    newMovePoint -= hitDir;
                    result = newMovePoint;
                    jump = true;
                }
                else
                {
                    result = Vector3.Lerp(cornerA, cornerC, 0.5f);
                }
                canSkip = true;
            }
            return canSkip;
        }
        private bool CanSkipCorner(Vector3 cornerA, Vector3 cornerC, out Vector3 result)
        {
            var Mask = LayerMaskClass.HighPolyWithTerrainMask;
            const float UpOffset = 0.15f;
            const float ShiftOffset = 0.1f;

            // The direction from the first corner to the corner after the next
            Vector3 direction = cornerC - cornerA;
            // Raise the position slightly to avoid hitting tiny objects.
            Vector3 rayPoint = cornerA + Vector3.up * UpOffset;

            bool canSkip = false;
            result = Vector3.zero;
            // If a there is an object in the movePath to the corner after the next, and the object's collider is short, we may be able to jump over it.
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
            return collider.bounds.size.y < 0.66f;
        }

        public bool BotIsComeTo()
        {
            return BotOwner.Mover.IsComeTo(ReachDistance, false);
        }

        public bool BotIsAtPoint(bool Sqr = true, float reachDist = -1f)
        {
            if (reachDist < 0f)
            {
                reachDist = ReachDistance;
            }
            return CornerDestDistance < reachDist;
        }

        public bool BotIsAtPoint(Vector3 point, bool Sqr = true, float reachDist = -1f)
        {
            if (reachDist < 0f)
            {
                reachDist = ReachDistance;
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

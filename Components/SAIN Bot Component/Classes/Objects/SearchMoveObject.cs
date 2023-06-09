using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;
using UnityEngine;
using BepInEx.Logging;

namespace SAIN.Classes
{
    public class SearchMoveObject : SAINBot
    {
        public MoveDangerPoint SearchMovePoint { get; private set; }
        private readonly NavMeshAgent Agent;

        public SearchMoveObject(BotOwner bot, NavMeshAgent agent) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            Agent = agent;
        }

        public bool Update(float reachDist = -1f)
        {
            if (CornerDestination != null && Destination != null)
            {
                if (reachDist > 0)
                {
                    ReachDistance = reachDist;
                }

                if (CheckMoveTimer < Time.time)
                {
                    CheckMoveTimer = Time.time + 0.1f;
                    Vector3 FinalDest = Destination.Value;

                    if (BotIsStuck && UnstuckMoveTimer < Time.time)
                    {
                        UnstuckMoveTimer = Time.time + 1f;
                        var pathStatus = GoToPoint(FinalDest, false);
                        if (pathStatus == NavMeshPathStatus.PathInvalid)
                        {
                            return false;
                        }
                    }

                    Vector3 CornerDest = CornerDestination.Value;
                    if (BotIsAtPoint(FinalDest, 1f, true))
                    {
                        Logger.LogDebug("Bot Arrived at Final Destination");
                        return false;
                    }

                    if (!BotIsAtPoint(CornerDest))
                    {
                        if (!AgentDestinationCompare(CornerDest))
                        {
                            Agent.SetDestination(CornerDest);
                        }
                    }
                    else
                    {
                        Logger.LogDebug("Bot Arrived at Corner, Moving to Next");
                        MoveNext();
                    }
                }
                return true;
            }
            return false;
        }

        private float UnstuckMoveTimer = 0f;
        public bool BotIsStuck => SAIN.BotStuck.BotIsStuck;
        private float CheckMoveTimer = 0f;

        private void MoveNext()
        {
            PathWayIndex++;
            if (PathWayIndex <= PathWayIndexMax)
            {
                CornerDestination = PathWay[PathWayIndex];
            }
        }

        public bool AgentDestinationCompare(Vector3 point)
        {
            return (Agent.destination - point).sqrMagnitude < 0.01f;
        }

        public NavMeshPathStatus GoToPoint(Vector3 point, bool MustHavePath = true, float reachDist = 0.5f)
        {
            SearchMovePoint = null;
            if (NavMesh.SamplePosition(point, out var hit, 0.5f, -1))
            {
                NavMeshPath Path = new NavMeshPath();
                if (NavMesh.CalculatePath(BotOwner.Position, hit.position, -1, Path))
                {
                    if (Path.corners.Length < 3)
                    {
                        Logger.LogError($"Corners Length too low! [{Path.corners.Length}]");
                    }
                    else if (Path.status == NavMeshPathStatus.PathComplete || !MustHavePath)
                    {
                        ReachDistance = reachDist > 0 ? reachDist : 0.5f;

                        for (int i = 1; i < Path.corners.Length - 2; i++)
                        {
                            Vector3 Start = SAIN.WeaponRoot;
                            Vector3 dirNext = Path.corners[i + 1] - Start;
                            if (Physics.Raycast(Start, dirNext, dirNext.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                            {
                                Vector3 blindCorner = Path.corners[i];
                                Vector3 dangerPoint = Path.corners[i + 1];
                                Vector3 dirToBlind = blindCorner - Start;
                                Vector3 dirToDanger = dangerPoint - Start;
                                Vector3 startPeekPos = GetPeekStartAndEnd(blindCorner, dangerPoint, dirToBlind, dirToDanger, out var endPeekPos);

                                if (SampleNav(startPeekPos, out var startResult))
                                {
                                    if (SampleNav(endPeekPos, out var endResult))
                                    {
                                        // Calculate the signed angle between the corners, value will be negative if its to the left of the startPeekPos.
                                        float signAngle = GetSignedAngle(dirToBlind, dirToDanger);
                                        SearchMovePoint = new MoveDangerPoint(startResult, endResult, dangerPoint, blindCorner, signAngle);
                                        break;
                                    }
                                }
                            }
                        }

                        Destination = SearchMovePoint == null ? hit.position : SearchMovePoint?.StartPosition;

                        Logger.LogDebug($"{Path.status}");
                        return Path.status;
                    }
                }
                Logger.LogWarning($"{Path.status}");
                return Path.status;
            }

            Logger.LogError($"Couldn't Find NavMesh at source");
            return NavMeshPathStatus.PathInvalid;
        }

        private Vector3 GetPeekStartAndEnd(Vector3 blindCorner, Vector3 dangerPoint, Vector3 dirToBlindCorner, Vector3 dirToBlindDest, out Vector3 peekEnd)
        {
            const float maxMagnitude = 5f;
            const float minMagnitude = 1f;
            const float OppositePointMagnitude = 10f;

            Vector3 directionToStart = BotOwner.Position - blindCorner;

            Vector3 cornerStartDir;
            if (directionToStart.magnitude > maxMagnitude)
            {
                cornerStartDir = directionToStart.normalized * maxMagnitude;
            }
            else if (directionToStart.magnitude < minMagnitude)
            {
                cornerStartDir = directionToStart.normalized;
            }
            else
            {
                cornerStartDir = Vector3.zero;
            }

            Vector3 PeekStartPosition = blindCorner + cornerStartDir;
            Vector3 dirFromStart = dangerPoint - PeekStartPosition;

            // Rotate to the opposite side depending on the angle of the danger point to the start position.
            float signAngle = GetSignedAngle(dirToBlindCorner.normalized, dirFromStart.normalized);
            float rotationAngle = signAngle > 0 ? -90f : 90f;
            Quaternion rotation = Quaternion.Euler(0f, rotationAngle, 0f);

            var direction = rotation * dirToBlindDest.normalized;
            direction *= OppositePointMagnitude;

            Vector3 PeekEndPosition;
            // if we hit an object on the way to our Peek Destination, change the peek startPeekPos to be the resulting hit position;
            if (CheckForObstacles(PeekStartPosition, direction, out Vector3 result))
            {
                // Shorten the direction as to not try to path directly into a wall.
                Vector3 resultDir = result - PeekStartPosition;
                resultDir *= 0.9f;
                PeekEndPosition = PeekStartPosition + resultDir;
            }
            else
            {
                // Set the startPeekPos to be the result if no objects are in the way. This is resulting wide peek startPeekPos.
                PeekEndPosition = result;
            }

            peekEnd = PeekEndPosition;
            return PeekStartPosition;
        }

        private bool SampleNav(Vector3 point, out Vector3 result, float dist = 1f)
        {
            if (NavMesh.SamplePosition(point, out var hit, dist, -1))
            {
                result = hit.position;
                return true;
            }
            result = point;
            return false;
        }

        private bool CheckForObstacles(Vector3 start, Vector3 direction, out Vector3 result)
        {
            start.y += 0.1f;
            if (Physics.Raycast(start, direction, out var hit, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
            {
                result = hit.point;
                result.y -= 0.1f;
                return true;
            }
            else
            {
                result = start + direction;
                return false;
            }
        }

        private void SetLean(LeanSetting leanSetting)
        {
            float num;
            switch (leanSetting)
            {
                case LeanSetting.Left:
                    num = -1f; break;
                case LeanSetting.Right:
                    num = 1f; break;
                default:
                    num = 0f; break;
            }
            BotOwner.GetPlayer.SlowLean(num);
        }

        public float LeanSetNum { get; private set; }

        public bool BotIsAtPoint()
        {
            return CornerDestDistance < 0.5f;
        }

        public bool BotIsAtPoint(float reachDist = 1f, bool Sqr = true)
        {
            if (Sqr)
            {
                return CornerDestSqrDistance < reachDist;
            }
            return CornerDestDistance < reachDist;
        }

        public bool BotIsAtPoint(Vector3 point, float reachDist = 1f, bool Sqr = true)
        {
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

        private float GetSignedAngle(Vector3 dirCenter, Vector3 dirOther, Vector3? axis = null)
        {            
            // Calculate the signed angle between the corners, value will be negative if its to the left of the startPeekPos.
            Vector3 angleAxis = axis ?? Vector3.up;
            return Vector3.SignedAngle(dirCenter, dirOther, angleAxis);
        }

        public float ReachDistance { get; private set; }
        public Vector3? Destination { get; set; }
        public Vector3? CornerDestination { get; set; }
        public float CornerDestDistance => (BotOwner.Position - CornerDestination.Value).magnitude;
        public float CornerDestSqrDistance => (BotOwner.Position - CornerDestination.Value).sqrMagnitude;
        public Vector3[] PathWay { get; private set; }
        public int PathWayIndex { get; private set; }
        public int PathWayIndexMax { get; private set; }

        private readonly ManualLogSource Logger;
    }

    public class MoveDangerPoint
    {
        public MoveDangerPoint(Vector3 start, Vector3 destination, Vector3 dangerPoint, Vector3 corner, float signedAngle)
        {
            StartPosition = start;
            EndPosition = destination;
            DangerPoint = dangerPoint;
            Corner = corner;
            SignedAngle = signedAngle;
        }

        public Vector3 StartPosition { get; private set; }
        public Vector3 EndPosition { get; private set; }
        public Vector3 DangerPoint { get; private set; }
        public Vector3 Corner { get; private set; }
        public float SignedAngle { get; private set; }

        private bool CheckIfLeanable(float signAngle, float limit = 1f)
        {
            return Mathf.Abs(signAngle) > limit;
        }

        public LeanSetting GetDirectionToLean(float signAngle)
        {
            if (CheckIfLeanable(signAngle))
            {
                return signAngle > 0 ? LeanSetting.Right : LeanSetting.Left;
            }
            return LeanSetting.None;
        }
    }
}

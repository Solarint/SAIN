using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Classes
{
    public class SearchMoveObject : SAINBot
    {
        public bool PeekingCorner { get; private set; }
        public MoveDangerPoint SearchMovePoint { get; private set; }

        public SearchMoveObject(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private readonly ManualLogSource Logger;

        private void MoveToNextPoint(Vector3 point)
        {
            BotOwner.GoToPoint(point, true, ReachDistance, false, false);
        }


        public Vector3 ActiveDestination { get; private set; }
        private bool FirstCheck = false;
        private bool SecondCheck = false;
        private bool ThirdCheck = false;
        private float DebugDrawTimer = 0f;

        public bool Update(bool lean, bool sprint, float reachDist = -1f)
        {
            if (DebugLayers.Value && ActiveDestination != null && DebugDrawTimer < Time.time)
            {
                float freq = 0.05f;
                DebugDrawTimer = Time.time + freq;
                DebugGizmos.SingleObjects.Line(BotOwner.Position, ActiveDestination, Color.magenta, 0.1f, true, freq, true);
            }

            if (reachDist > 0)
            {
                ReachDistance = reachDist;
            }

            if (SearchMovePoint == null)
            {
                if (TargetPosition == null)
                {
                    TargetPosition = SAIN.CurrentTargetPosition;
                }
                if (!FirstCheck && TargetPosition != null)
                {
                    FirstCheck = true;
                    MoveToNextPoint(TargetPosition.Value);
                    ActiveDestination = TargetPosition.Value;
                    return true;
                }
                return false;
            }

            if (SearchMovePoint != null)
            {
                if (lean)
                {
                    UpdateLean();
                }

                if (!FirstCheck)
                {
                    FirstCheck = true;
                    MoveToNextPoint(SearchMovePoint.StartPosition);
                    ActiveDestination = SearchMovePoint.StartPosition;
                    return true;
                }

                if (FirstCheck && !SecondCheck)
                {
                    if (lean)
                    {
                        UpdateLean();
                    }
                    if (BotIsAtPoint(ActiveDestination))
                    {
                        PeekingCorner = true;
                        SecondCheck = true;
                        MoveToNextPoint(SearchMovePoint.EndPosition);
                        ActiveDestination = SearchMovePoint.EndPosition;
                        return true;
                    }
                }

                if (FirstCheck && SecondCheck && !ThirdCheck)
                {
                    if (BotIsAtPoint(ActiveDestination))
                    {
                        BotOwner.GetPlayer.MovementContext.SetTilt(0f);
                        PeekingCorner = false;
                        ThirdCheck = true;
                        MoveToNextPoint(SearchMovePoint.DangerPoint);
                        ActiveDestination = SearchMovePoint.DangerPoint;
                        return true;
                    }
                }

                if (FirstCheck && SecondCheck && ThirdCheck)
                {
                    if (BotIsAtPoint(ActiveDestination))
                    {
                        TargetPosition = SAIN.CurrentTargetPosition;
                        if (TargetPosition != null)
                        {
                            var pathStatus = GoToPoint(TargetPosition.Value, false);
                            if (pathStatus == NavMeshPathStatus.PathInvalid)
                            {
                                return false;
                            }
                            return true;
                        }
                    }
                }

                if (CheckMoveTimer < Time.time)
                {
                    CheckMoveTimer = Time.time + 0.25f;
                    if (BotIsStuck && UnstuckMoveTimer < Time.time)
                    {
                        UnstuckMoveTimer = Time.time + 1f;
                        TargetPosition = SAIN.CurrentTargetPosition;
                        if (TargetPosition != null)
                        {
                            var pathStatus2 = GoToPoint(TargetPosition.Value, false);
                            return pathStatus2 != NavMeshPathStatus.PathInvalid;
                        }
                    }

                }
                return true;
            }
            return false;
        }

        private void UpdateLean()
        {
            if (UpdateLeanTimer < Time.time)
            {
                UpdateLeanTimer = Time.time + 0.25f;
                Vector3 directionToDanger = SearchMovePoint.DangerPoint - BotOwner.Position;
                Vector3 directionToCorner = SearchMovePoint.Corner - BotOwner.Position;
                float signAngle = GetSignedAngle(directionToCorner, directionToDanger);
                var lean = SearchMovePoint.GetDirectionToLean(signAngle);
                SetLean(lean);
            }
        }

        private float UpdateLeanTimer = 0f;
        private float UnstuckMoveTimer = 0f;
        public bool BotIsStuck => SAIN.BotStuck.BotIsStuck;
        private float CheckMoveTimer = 0f;
        public Vector3? TargetPosition { get; private set; }

        private void Reset()
        {
            SearchMovePoint = null;
            FirstCheck = false;
            SecondCheck = false;
            ThirdCheck = false;
        }

        public NavMeshPathStatus GoToPoint(Vector3 point, bool MustHavePath = true, float reachDist = 0.5f)
        {
            Reset();

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
                        TargetPosition = hit.position;
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

                        //Logger.LogDebug($"{Path.status}");
                        return Path.status;
                    }
                }
                //Logger.LogWarning($"{Path.status}");
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
            // if we hit an object on the way to our Peek FinalDestination, change the peek startPeekPos to be the resulting hit position;
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
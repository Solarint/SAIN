using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Classes
{
    public class SearchClass : SAINBot
    {
        public bool PeekingCorner { get; private set; }
        public MoveDangerPoint SearchMovePoint { get; private set; }

        public SearchClass(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private readonly ManualLogSource Logger;

        private void MoveToNextPoint(Vector3 point)
        {
            BotOwner.GoToPoint(point, false, ReachDistance, false, false);
        }

        public Vector3 ActiveDestination { get; private set; }
        private bool DirectMove = false;
        private bool FirstCheck = false;
        private bool SecondCheck = false;
        private bool ThirdCheck = false;
        private float DebugDrawTimer = 0f;

        public void Update(bool shallLean, bool shallSprint, float reachDist = -1f)
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

            if (!SAIN.BotStuck.BotIsMoving && SAIN.BotStuck.TimeSpentNotMoving > 3f)
            {
                TargetPosition = SearchMovePos();
                if (TargetPosition != null)
                {
                    DirectMove = true;
                    MoveToNextPoint(TargetPosition.Value);
                    ActiveDestination = TargetPosition.Value;
                    return;
                }
            }

            if (MoveDirect(shallSprint))
            {
                return;
            }

            if (MoveToPeek(shallLean))
            {
                return;
            }

            if (CheckIfStuck())
            {
                return;
            }
        }

        private bool MoveDirect(bool shallSprint)
        {
            if (SearchMovePoint == null || shallSprint)
            {
                Vector3 pos = SearchMovePos();
                if (TargetPosition == null)
                {
                    TargetPosition = pos;
                }
                if (!DirectMove && TargetPosition != null)
                {
                    DirectMove = true;
                    BotOwner.BotRun.Run(pos, false);
                    ActiveDestination = pos;
                }
                return true;
            }
            return false;
        }

        public Vector3 SearchMovePos()
        {
            if (SAIN.Enemy != null && SAIN.Enemy.Seen)
            {
                if (SAIN.Enemy.IsVisible)
                {
                    return SAIN.Enemy.Position;
                }
                else
                {
                    if (SAIN.Enemy.ArrivedAtLastSeenPosition)
                    {
                        return RandomSearch();
                    }
                    else
                    {
                        return SAIN.Enemy.PositionLastSeen;
                    }
                }
            }
            else
            {
                var Target = BotOwner.Memory.GoalTarget;
                if (Target != null && Target?.Position != null)
                {
                    if ((Target.Position.Value - BotOwner.Position).sqrMagnitude < 2f)
                    {
                        Target.Clear();
                    }
                    else
                    {
                        return Target.Position.Value;
                    }
                }
                var sound = BotOwner.BotsGroup.YoungestPlace(BotOwner, 200f, true);
                if (sound != null && !sound.IsCome)
                {
                    if ((sound.Position - BotOwner.Position).sqrMagnitude < 2f)
                    {
                        sound.IsCome = true;
                    }
                    else
                    {
                        return sound.Position;
                    }
                }
            }

            return RandomSearch();
        }

        const float ComeToRandomDist = 3f;

        private Vector3 RandomSearch()
        {
            float dist = (RandomSearchPoint - BotOwner.Position).sqrMagnitude;
            if (dist < ComeToRandomDist || dist > 60f * 60f)
            {
                RandomSearchPoint = GenerateSearchPoint();
            }
            return RandomSearchPoint;
        }

        private Vector3 RandomSearchPoint;

        private Vector3 GenerateSearchPoint()
        {
            Vector3 start = BotOwner.Position;
            float dispersion = 30f;
            for (int i = 0; i < 10; i++)
            {
                float dispNum = EFTMath.Random(-dispersion, dispersion);
                Vector3 vector = new Vector3(start.x + dispNum, start.y, start.z + dispNum);
                if (NavMesh.SamplePosition(vector, out var hit, 2f, -1))
                {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(hit.position, start, -1, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        return hit.position;
                    }
                }
            }
            return start;
        }

        private bool MoveToPeek(bool shallLean)
        {
            if (SearchMovePoint != null)
            {
                if (!FirstCheck)
                {
                    FirstCheck = true;
                    MoveToNextPoint(SearchMovePoint.StartPosition);
                    ActiveDestination = SearchMovePoint.StartPosition;
                }
                else if (!SecondCheck)
                {
                    if (shallLean)
                    {
                        UpdateLean();
                    }
                    if (BotIsAtPoint(ActiveDestination))
                    {
                        PeekingCorner = true;
                        SecondCheck = true;
                        MoveToNextPoint(SearchMovePoint.EndPosition);
                        ActiveDestination = SearchMovePoint.EndPosition;
                    }
                }
                else if (!ThirdCheck)
                {
                    if (BotIsAtPoint(ActiveDestination))
                    {
                        BotOwner.GetPlayer.MovementContext.SetTilt(0f);
                        PeekingCorner = false;
                        ThirdCheck = true;
                        MoveToNextPoint(SearchMovePoint.DangerPoint);
                        ActiveDestination = SearchMovePoint.DangerPoint;
                    }
                }
                else
                {
                    if (BotIsAtPoint(ActiveDestination))
                    {
                        TargetPosition = SAIN.CurrentTargetPosition;
                        if (TargetPosition != null)
                        {
                            ActiveDestination = TargetPosition.Value;
                            GoToPoint(TargetPosition.Value, false);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private bool CheckIfStuck()
        {
            if (BotIsStuck)
            {
                if (UnstuckMoveTimer < Time.time)
                {
                    UnstuckMoveTimer = Time.time + 2f;
                    TargetPosition = SAIN.CurrentTargetPosition;
                    if (TargetPosition != null)
                    {
                        GoToPoint(TargetPosition.Value, false);
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
                SAIN.Mover.FastLean(lean);
            }
        }

        private float UpdateLeanTimer = 0f;
        private float UnstuckMoveTimer = 0f;
        public bool BotIsStuck => SAIN.BotStuck.BotIsStuck;
        public Vector3? TargetPosition { get; private set; }

        private void Reset()
        {
            TargetPosition = null;
            SearchMovePoint = null;
            FirstCheck = false;
            SecondCheck = false;
            ThirdCheck = false;
            PeekingCorner = false;
            DirectMove = false;
        }

        public NavMeshPathStatus GoToPoint(Vector3 point, bool MustHavePath = true, float reachDist = 0.5f)
        {
            var SqrMagnitude = (point - BotOwner.Position).sqrMagnitude;
            if (SqrMagnitude <= 0.5f)
            {
                //Logger.LogInfo($"Search Destination is too close. SqrMagnitude: [{SqrMagnitude}]");
                return NavMeshPathStatus.PathInvalid;
            }

            Reset();

            if (NavMesh.SamplePosition(point, out var hit, 10f, -1))
            {
                NavMeshPath Path = new NavMeshPath();
                if (NavMesh.CalculatePath(BotOwner.Position, hit.position, -1, Path))
                {
                    if (Path.corners.Length < 3)
                    {
                        if (DebugLayers.Value)
                        {
                            Logger.LogError($"Corners Length too low! [{Path.corners.Length}]");
                        }
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
                                if (dirToBlind.magnitude < 1f)
                                {
                                    continue;
                                }
                                Vector3 dirToDanger = dangerPoint - Start;
                                Vector3 startPeekPos = GetPeekStartAndEnd(blindCorner, dangerPoint, dirToBlind, dirToDanger, out var endPeekPos);

                                if (SampleNav(startPeekPos, out var startResult))
                                {
                                    if (SampleNav(endPeekPos, out var endResult))
                                    {
                                        // CalculateRecoil the signed angle between the corners, value will be negative if its to the left of the startPeekPos.
                                        float signAngle = GetSignedAngle(dirToBlind, dirToDanger);
                                        SearchMovePoint = new MoveDangerPoint(startResult, endResult, dangerPoint, blindCorner, signAngle);
                                        break;
                                    }
                                }
                            }
                        }
                        return Path.status;
                    }
                }
                return Path.status;
            }

            //Logger.LogError($"Couldn't Find NavMesh at Point {point}");
            return NavMeshPathStatus.PathInvalid;
        }

        private Vector3 GetPeekStartAndEnd(Vector3 blindCorner, Vector3 dangerPoint, Vector3 dirToBlindCorner, Vector3 dirToBlindDest, out Vector3 peekEnd)
        {
            const float maxMagnitude = 5f;
            const float minMagnitude = 1f;
            const float OppositePointMagnitude = 5f;

            Vector3 directionToStart = BotOwner.Position - blindCorner;

            Vector3 cornerStartDir;
            if (directionToStart.magnitude > maxMagnitude)
            {
                cornerStartDir = directionToStart.normalized * maxMagnitude;
            }
            else if (directionToStart.magnitude < minMagnitude)
            {
                cornerStartDir = directionToStart.normalized * minMagnitude;
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
            // CalculateRecoil the signed angle between the corners, value will be negative if its to the left of the startPeekPos.
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
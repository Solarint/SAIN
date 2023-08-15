using EFT;
using SAIN.Helpers;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes
{
    public enum SearchStates
    {
        None,
        FindRoute,
        MoveToCorner,
        CheckCorners,
        HoldPosition,
        Wait,
        RushEnemy,
    }

    public class SAINSearchClass : SAINBase, ISAINClass
    {
        public SAINSearchClass(SAINComponentClass sain) : base(sain)
        {
        }

        private SearchStates CurrentSearchState { get; set; } = SearchStates.None;
        private NavMeshPathStatus NavMeshPathStatus => SearchPath.status;
        private NavMeshPath SearchPath { get; set; } = new NavMeshPath();
        private Vector3 SearchDestination => SearchPathCornersCount == 0 ? Vector3.zero : SearchPath.corners[SearchPathCornersCount - 1];
        private int SearchPathCornersCount => SearchPath.corners.Length;
        private Vector3? SearchTarget { get; set; }

        public void Init()
        {
        }

        public void Update()
        {
            switch (CurrentSearchState)
            {
                case SearchStates.None:
                    break;

                case SearchStates.FindRoute:
                    //CalculatePath();
                    break;

                case SearchStates.RushEnemy:
                    break;

                case SearchStates.MoveToCorner:
                    break;

                case SearchStates.CheckCorners:
                    break;

                case SearchStates.HoldPosition:
                    break;

                case SearchStates.Wait:
                    break;
            }
        }

        private void CalculatePath()
        {
            if (SearchTarget != null)
            {
                Vector3 start = SAIN.Position;
                Vector3 target = SearchTarget.Value;

                SearchPath.ClearCorners();
                if (NavMesh.CalculatePath(start, target, -1, SearchPath))
                {
                    CurrentSearchState = SearchStates.MoveToCorner;
                }
            }
        }

        public void StartSearch(Vector3 destination)
        {
            SearchTarget = new Vector3?(destination);
            CurrentSearchState = SearchStates.FindRoute;
        }

        public void StopSearch()
        {
            CurrentSearchState = SearchStates.None;
        }

        public void Dispose()
        {
        }

        public MoveDangerPoint SearchMovePoint { get; private set; }
        public Vector3 ActiveDestination { get; private set; }

        public void Search(bool shallLean, bool shallSprint, float reachDist = -1f)
        {
            if (reachDist > 0)
            {
                ReachDistance = reachDist;
            }

            CheckIfStuck();

            SwitchSearchModes(shallLean, shallSprint);
        }

        public Vector3 SearchMovePos()
        {
            var enemy = SAIN.Enemy;
            if (enemy != null && enemy.Seen)
            {
                if (enemy.IsVisible)
                {
                    return SAIN.Enemy.EnemyPosition;
                }
                else
                {
                    var lastSeenPos = enemy.LastSeenPosition;
                    if (lastSeenPos == null)
                    {
                        return RandomSearch();
                    }
                    else
                    {
                        return lastSeenPos.Value;
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

        private const float ComeToRandomDist = 3f;

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
                    Path.ClearCorners();
                    if (NavMesh.CalculatePath(hit.position, start, -1, Path) && Path.status == NavMeshPathStatus.PathComplete)
                    {
                        return hit.position;
                    }
                }
            }
            return start;
        }

        public ESearchMove NextState = ESearchMove.None;
        public ESearchMove CurrentState = ESearchMove.None;
        public ESearchMove LastState = ESearchMove.None;
        public float WaitTimer { get; private set; }
        private float RecalcPathTimer;

        private bool ShallRecalcPath()
        {
            if (RecalcPathTimer < Time.time)
            {
                RecalcPathTimer = Time.time + 2;
                return true;
            }
            return false;
        }

        private bool WaitAtPoint()
        {
            if (WaitPointTimer < 0)
            {
                float baseTime = 6;
                var personalitySettings = SAIN.Info.PersonalitySettings;
                if (personalitySettings != null)
                {
                    baseTime /= personalitySettings.SearchAggressionModifier;
                }
                WaitPointTimer = Time.time + baseTime * Random.Range(0.5f, 2f);
            }
            if (WaitPointTimer < Time.time)
            {
                WaitPointTimer = -1;
                return false;
            }
            return true;
        }

        private float WaitPointTimer = -1;

        private int Index = 0;

        public Vector3 FinalDestination { get; private set; } = Vector3.zero;

        private Vector3 NextCorner()
        {
            int i = Index;
            if (Path.corners.Length < i)
            {
                Index++;
                return Path.corners[i];
            }
            return Path.corners[Path.corners.Length - 1];
        }

        private void MoveToPoint(bool shallSprint)
        {
            if (shallSprint)
            {
                BotOwner.BotRun.Run(ActiveDestination, false);
            }
            else
            {
                BotOwner.GoToPoint(ActiveDestination, false, ReachDistance, false, false);
            }
        }

        private bool SwitchSearchModes(bool shallLean, bool shallSprint)
        {
            LastState = CurrentState;
            switch (LastState)
            {
                case ESearchMove.None:
                    if (SearchMovePoint == null || shallSprint)
                    {
                        FinalDestination = SearchMovePos();
                        ActiveDestination = NextCorner();
                        MoveToPoint(shallSprint);
                        CurrentState = ESearchMove.DirectMove;
                    }
                    else
                    {
                        ActiveDestination = SearchMovePoint.StartPeekPosition;
                        MoveToPoint(shallSprint);
                        CurrentState = ESearchMove.Wait;
                        NextState = ESearchMove.StartPeekPosition;
                    }
                    break;

                case ESearchMove.DirectMove:

                    SAIN.Mover.SetTargetMoveSpeed(1f);
                    SAIN.Mover.SetTargetPose(1f);

                    if (BotIsAtPoint(ActiveDestination))
                    {
                        ActiveDestination = NextCorner();
                        MoveToPoint(shallSprint);
                    }
                    else if (ShallRecalcPath())
                    {
                        MoveToPoint(shallSprint);
                    }
                    break;

                case ESearchMove.StartPeekPosition:

                    SAIN.Mover.SetTargetMoveSpeed(1f);
                    SAIN.Mover.SetTargetPose(1f);

                    if (BotIsAtPoint(ActiveDestination))
                    {
                        ActiveDestination = SearchMovePoint.EndPeekPosition;
                        MoveToPoint(shallSprint);
                        CurrentState = ESearchMove.MoveToEndPeak;
                    }
                    else if (ShallRecalcPath())
                    {
                        MoveToPoint(shallSprint);
                    }
                    break;

                case ESearchMove.MoveToEndPeak:

                    SAIN.Mover.SetTargetMoveSpeed(0.1f);
                    SAIN.Mover.SetTargetPose(0.75f);

                    if (shallLean)
                    {
                        UpdateLean();
                    }
                    if (BotIsAtPoint(ActiveDestination))
                    {
                        ActiveDestination = SearchMovePoint.DangerPoint;
                        // Player.MovementContext.SetTilt(0f);
                        MoveToPoint(shallSprint);
                        CurrentState = ESearchMove.Wait;
                        NextState = ESearchMove.DangerPoint;
                    }
                    else if (ShallRecalcPath())
                    {
                        MoveToPoint(shallSprint);
                    }
                    break;

                case ESearchMove.DangerPoint:

                    SAIN.Mover.SetTargetMoveSpeed(0.5f);
                    SAIN.Mover.SetTargetPose(1f);

                    Vector3 start = SAIN.Position;
                    Vector3 cornerDir = SearchMovePoint.Corner - start;
                    Vector3 dangerDir = SearchMovePoint.DangerPoint - start;
                    float dot = Vector3.Dot(cornerDir.normalized, dangerDir.normalized);
                    if (dot < 0)
                    {
                        // bot is past corner
                    }

                    // Reset 
                    if (BotIsAtPoint(ActiveDestination))
                    {
                        var TargetPosition = SAIN.CurrentTargetPosition;
                        if (TargetPosition != null)
                        {
                            GoToPoint(TargetPosition.Value, false);
                        }
                    }
                    else if (ShallRecalcPath())
                    {
                        MoveToPoint(shallSprint);
                    }
                    break;

                case ESearchMove.Wait:
                    if (WaitAtPoint())
                    {
                        BotOwner.Mover.Stop();
                    }
                    else
                    {
                        CurrentState = NextState;
                    }
                    break;
            }
            return true;
        }

        private bool CheckIfStuck()
        {
            bool botIsStuck = 
                (!SAIN.BotStuck.BotIsMoving && SAIN.BotStuck.TimeSpentNotMoving > 3f) 
                || SAIN.BotStuck.BotIsStuck;

            if (botIsStuck && UnstuckMoveTimer < Time.time)
            {
                UnstuckMoveTimer = Time.time + 2f;

                var TargetPosition = SAIN.CurrentTargetPosition;
                if (TargetPosition != null)
                {
                    GoToPoint(TargetPosition.Value, false);
                }
            }
            return botIsStuck;
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

        private NavMeshPath Path = new NavMeshPath();

        public void Reset()
        {
            Index = 0;
            Path.ClearCorners();
            FinalDestination = Vector3.zero;
            SearchMovePoint = null;
            CurrentState = ESearchMove.None;
            LastState = ESearchMove.None;
            NextState = ESearchMove.None;
        }

        public NavMeshPathStatus GoToPoint(Vector3 point, bool MustHavePath = true, float reachDist = 0.5f)
        {
            Vector3 Start = SAIN.Position;
            if ((point - Start).sqrMagnitude <= 0.5f)
            {
                return NavMeshPathStatus.PathInvalid;
            }

            Reset();

            if (NavMesh.SamplePosition(point, out var hit, 10f, -1))
            {
                Path = new NavMeshPath();
                if (NavMesh.CalculatePath(Start, hit.position, -1, Path))
                {
                    ReachDistance = reachDist > 0 ? reachDist : 0.5f;
                    FinalDestination = hit.position;
                    int cornerLength = Path.corners.Length;

                    List<Vector3> newCorners = new List<Vector3>();
                    for (int i = 0; i < cornerLength - 1; i++)
                    {
                        if ((Path.corners[i] - Path.corners[i + 1]).sqrMagnitude > 0.66f)
                        {
                            newCorners.Add(Path.corners[i]);
                        }
                    }
                    if (cornerLength > 0)
                    {
                        newCorners.Add(Path.corners[cornerLength - 1]);
                    }

                    for (int i = 0; i < newCorners.Count - 1; i++)
                    {
                        Vector3 A = newCorners[i];
                        Vector3 ADirection = A - Start;

                        Vector3 B = newCorners[i + 1];
                        Vector3 BDirection = B - Start;
                        float BDirMagnitude = BDirection.magnitude;

                        if (Physics.Raycast(Start, BDirection.normalized, BDirMagnitude, LayerMaskClass.HighPolyWithTerrainMask))
                        {
                            Vector3 startPeekPos = GetPeekStartAndEnd(A, B, ADirection, BDirection, out var endPeekPos);

                            if (NavMesh.SamplePosition(startPeekPos, out var hit2, 2f, -1)
                                && NavMesh.SamplePosition(endPeekPos, out var hit3, 2f, -1))
                            {
                                // CalculateRecoil the signed angle between the corners, value will be negative if its to the left of the startPeekPos.
                                SearchMovePoint = new MoveDangerPoint
                                {
                                    StartPeekPosition = hit2.position,
                                    EndPeekPosition = hit3.position,
                                    Corner = A,
                                    DangerPoint = B,
                                    SignedAngle = GetSignedAngle(ADirection, BDirection),
                                };
                                break;
                            }
                        }
                    }
                }
                return Path.status;
            }

            //DefaultLogger.LogError($"Couldn't Find NavMesh at Point {point}");
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

            // Rotate to the opposite side depending on the angle of the danger point to the start DrawPosition.
            float signAngle = GetSignedAngle(dirToBlindCorner.normalized, dirFromStart.normalized);
            float rotationAngle = signAngle > 0 ? -90f : 90f;
            Quaternion rotation = Quaternion.Euler(0f, rotationAngle, 0f);

            var direction = rotation * dirToBlindDest.normalized;
            direction *= OppositePointMagnitude;

            Vector3 PeekEndPosition;
            // if we hit an object on the way to our Peek FinalDestination, change the peek startPeekPos to be the resulting hit DrawPosition;
            if (CheckForObstacles(PeekStartPosition, direction, out Vector3 result))
            {
                // Shorten the direction as to not try to path directly into a wall.
                Vector3 resultDir = result - PeekStartPosition;
                resultDir *= 0.9f;
                PeekEndPosition = PeekStartPosition + resultDir;
            }
            else
            {
                // Modify the startPeekPos to be the result if no objects are in the way. TypeofThis is resulting wide peek startPeekPos.
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
            // CalculateRecoil the signed angle between the corners, rounding will be negative if its to the left of the startPeekPos.
            Vector3 angleAxis = axis ?? Vector3.up;
            return Vector3.SignedAngle(dirCenter, dirOther, angleAxis);
        }

        public float ReachDistance { get; private set; }
    }
}
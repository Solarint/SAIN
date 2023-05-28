using BepInEx.Logging;
using EFT;
using SAIN.Classes;
using SAIN.Components;
using SAIN.Helpers;
using System;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.CoverConfig;

namespace SAIN.Classes
{
    public class CoverClass : SAINBot
    {
        public CoverClass(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            Component = BotOwner.GetOrAddComponent<CoverFinderComponent>();
            FallBackPointStatus = CoverStatus.None;
            CoverPointStatus = CoverStatus.None;
        }

        public bool BotIsAtCoverPoint => ActiveCoverPoint != null;

        public CoverPoint ActiveCoverPoint
        {
            get
            {
                if (CoverPointStatus == CoverStatus.InCover)
                {
                    return CurrentCoverPoint;
                }
                else if (FallBackPointStatus == CoverStatus.InCover)
                {
                    return CurrentFallBackPoint;
                }
                else
                {
                    return null;
                }
            }
        }

        public CoverFinderComponent Component { get; private set; }

        private const float InCoverDist = 1f;
        private const float CloseCoverDist = 10f;
        private const float FarCoverDist = 30f;

        public void ManualUpdate()
        {
            if (BotOwner.Memory.GoalEnemy == null && BotOwner.Memory.GoalTarget?.GoalTarget == null)
            {
                Component.StopLooking();
            }
            else if (UpdateTimer < Time.time)
            {
                AssignSettings();

                UpdateTimer = Time.time + CoverUpdateFrequency.Value;

                Vector3? targetPosition = GetPointToHideFrom();

                if (targetPosition != null)
                {
                    //DebugGizmos.SingleObjects.Line(targetPosition.Value, BotOwner.MyHead.position, Color.red, 0.05f, true, CoverUpdateFrequency.Value, true);

                    Component.LookForCover(targetPosition.Value, BotOwner.Transform.position);

                    //DrawDebug(targetPosition.Value);
                }

                FallBackPointStatus = CheckCoverPointStatus(CurrentFallBackPoint, InCoverDist, CloseCoverDist, FarCoverDist);
                CoverPointStatus = CheckCoverPointStatus(CurrentCoverPoint, InCoverDist, CloseCoverDist, FarCoverDist);

                //Logger.LogWarning($"Cover Statuses: {CoverPointStatus} : {FallBackPointStatus}");

                if (CurrentFallBackPoint == null && CurrentCoverPoint == null)
                {
                    //Logger.LogError($"CurrentFallBackPoint == null && CurrentCoverPoint == null");
                }
            }
        }

        private float UpdateTimer = 0f;

        private Vector3? GetPointToHideFrom()
        {
            if (SAIN.Decisions.CurrentDecision == SAINLogicDecision.RunAwayGrenade)
            {
                return SAIN.BotOwner.BewareGrenade.GrenadeDangerPoint.DangerPoint;
            }
            else if (BotOwner.Memory.GoalEnemy != null)
            {
                return SAIN.BotOwner.Memory.GoalEnemy.CurrPosition;
            }
            else if (BotOwner.Memory.GoalTarget?.GoalTarget != null)
            {
                return BotOwner.Memory.GoalTarget.GoalTarget.Position;
            }
            else { return null; }
        }

        private CoverStatus CheckCoverPointStatus(CoverPoint cover, float inCoverDist, float closeCoverDist, float farCoverDist)
        {
            CoverStatus status = CoverStatus.None;

            if (cover != null)
            {
                if (IsBotCloseToCover(cover, inCoverDist))
                {
                    status = CoverStatus.InCover;
                }
                else if (IsBotCloseToCover(cover, closeCoverDist))
                {
                    status = CoverStatus.CloseToCover;
                }
                else if (IsBotCloseToCover(cover, farCoverDist))
                {
                    status = CoverStatus.FarFromCover;
                }
            }

            return status;
        }

        public bool IsBotCloseToCover(CoverPoint cover, float distThreshold = 15f)
        {
            if (cover != null)
            {
                if (Vector3.Distance(cover.Position, BotOwner.Transform.position) <= distThreshold)
                {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(BotOwner.Transform.position, cover.Position, NavMesh.AllAreas, path))
                    {
                        float length = path.CalculatePathLength();

                        return length <= distThreshold * 1.5f;
                    }
                }
            }
            return false;
        }

        public CoverStatus FallBackPointStatus { get; private set; }
        public CoverStatus CoverPointStatus { get; private set; }

        public bool DuckInCover(bool SetPose1ifFalse = true)
        {
            bool duckInCover = false;
            if (FallBackPointStatus == CoverStatus.InCover)
            {
                duckInCover = true;
            }
            else if (CoverPointStatus == CoverStatus.InCover)
            {
                duckInCover = true;
            }

            if (duckInCover)
            {
                SAIN.BotOwner.SetPose(0f);
            }
            else if (SetPose1ifFalse)
            {
                SAIN.BotOwner.SetPose(1f);
            }

            return duckInCover;
        }

        public bool CheckSelfForCover(float minratio = 0.1f)
        {
            int rays = 0;
            int cover = 0;


            BotOwner.Memory.GoalEnemy.Person.MainParts.TryGetValue(BodyPartType.head, out BodyPartClass EnemyHead);
            var headPos = EnemyHead.Position;

            foreach (var part in BotOwner.MainParts.Values)
            {
                rays++;

                Vector3 direction = part.Position - headPos;

                if (Physics.Raycast(headPos, direction, direction.magnitude, SAINComponent.ShootMask))
                {
                    cover++;
                }
            }

            float coverRatio = (float)cover / rays;

            return coverRatio > minratio;
        }

        private void AssignSettings()
        {
            switch (SAIN.CurrentDecision)
            {
                case SAINLogicDecision.Surgery:
                case SAINLogicDecision.FirstAid:
                case SAINLogicDecision.RunAway:
                case SAINLogicDecision.RunAwayGrenade:
                    FallingBack = true;
                    Component.MinObstacleHeight = 1.55f;
                    break;

                default:
                    FallingBack = false;
                    Component.MinObstacleHeight = CoverMinHeight.Value;
                    break;
            }
        }

        public bool FallingBack = false;

        private void DrawDebug(Vector3 enemyPos)
        {
            if (DebugTimer < Time.time)
            {
                DebugTimer = Time.time + 0.5f;
                //DebugGizmos.SingleObjects.Line(SAIN.Core.HeadPosition, enemyPos, Color.red, 0.1f, true, 1f);

                if (CurrentCoverPoint != null)
                {
                    DebugGizmos.SingleObjects.Line(SAIN.HeadPosition, CurrentCoverPoint.Position, Color.blue, 0.05f, true, 0.5f);

                    //NavMeshPath path = new NavMeshPath();
                    //if (NavMesh.CalculatePath(BotOwner.Transform.position, CurrentCoverPoint.Position, NavMesh.AllAreas, path))
                    //{
                    //    SAIN.DebugDrawList.DrawTempPath(path, true, Color.magenta, Color.blue, 0.1f, 0.5f, false);
                    //}
                }

                if (CurrentFallBackPoint != null)
                {
                    DebugGizmos.SingleObjects.Line(SAIN.HeadPosition, CurrentFallBackPoint.Position, Color.magenta, 0.05f, true, 0.5f);

                    //NavMeshPath path2 = new NavMeshPath();
                    //if (NavMesh.CalculatePath(SAIN.BotOwner.Transform.position, CurrentFallBackPoint.Position, NavMesh.AllAreas, path2))
                    //{
                    //    SAIN.DebugDrawList.DrawTempPath(path2, true, Color.magenta, Color.magenta, 0.1f, 0.5f, false);
                    //}
                }
            }
        }

        public bool CanBotBackUp()
        {
            const float angleStep = 15f;
            const float rangeStep = 2f;
            const int max = 10;
            int i = 0;
            while (i < max)
            {
                float angleAdd = angleStep * i;
                float currentAngle = UnityEngine.Random.Range(-5f - angleAdd, 5f + angleAdd);
                float currentRange = rangeStep * i + 2f;

                Vector3 DodgeFallBack = HelperClasses.FindArcPoint(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition, currentRange, currentAngle);
                if (NavMesh.SamplePosition(DodgeFallBack, out NavMeshHit hit, 5f, -1))
                {
                }

                i++;
            }
            return false;
        }

        public CoverPoint CurrentCoverPoint => Component.CurrentCover;
        public CoverPoint CurrentFallBackPoint => Component.CurrentFallBackPoint;

        protected ManualLogSource Logger;
        private float DebugTimer = 0f;
    }
}
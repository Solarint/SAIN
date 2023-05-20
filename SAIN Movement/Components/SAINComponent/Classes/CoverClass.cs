using BepInEx.Logging;
using EFT;
using SAIN.Classes;
using SAIN.Components;
using SAIN.Helpers;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.CoverConfig;

namespace SAIN.Layers.Logic
{
    public class CoverClass : SAINBotExt
    {
        public CoverClass(BotOwner bot) : base(bot)
        {
            Component = BotOwner.GetOrAddComponent<CoverComponent>();
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

        public CoverComponent Component { get; private set; }

        private const float InCoverDist = 1f;
        private const float CloseCoverDist = 10f;
        private const float FarCoverDist = 30f;

        public void ManualUpdate()
        {
            if (BotOwner.Memory.GoalEnemy != null)
            {
                if (CheckSelfCoverTimer < Time.time)
                {
                    InCover = CheckSelfForCover(0.33f);

                    CheckSelfCoverTimer = Time.time + CheckSelfFreq;
                    CheckSelfCoverTimer += InCover ? 0.5f : 0f;
                }
            }

            if (BotOwner.Memory.GoalEnemy == null && BotOwner.Memory.GoalTarget?.GoalTarget == null)
            {
                Component.StopLooking();
                return;
            }

            if (UpdateTimer < Time.time)
            {
                AssignSettings();

                UpdateTimer = Time.time + CoverUpdateFrequency.Value;

                Vector3? targetPosition = GetPointToHideFrom();

                if (targetPosition != null)
                {
                    Component.LookForCover(targetPosition);

                    DrawDebug(targetPosition.Value);
                }

                Logger.LogWarning("Checking Debug");
                FallBackPointStatus = CheckCoverPointStatus(CurrentFallBackPoint, InCoverDist, CloseCoverDist, FarCoverDist);
                CoverPointStatus = CheckCoverPointStatus(CurrentCoverPoint, InCoverDist, CloseCoverDist, FarCoverDist);

                Logger.LogWarning($"Cover Statuses: {CoverPointStatus} : {FallBackPointStatus}");

                if (CurrentFallBackPoint == null)
                {
                    Logger.LogInfo($"CurrentFallBackPoint == null");
                }
                if (CurrentCoverPoint == null)
                {
                    Logger.LogInfo($"CurrentCoverPoint == null");
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

        private bool CheckSelfForCover(float minratio = 0.1f)
        {
            int rays = 0;
            int cover = 0;

            foreach (var part in BotOwner.MainParts.Values)
            {
                rays++;

                if (SAIN.Core.Enemy.EnemyHeadPosition == null)
                {
                    CoverRatio = 1f;
                    return true;
                }

                var head = SAIN.Core.Enemy.EnemyHeadPosition;

                Vector3 direction = part.Position - head.Value;

                if (Physics.Raycast(head.Value, direction, direction.magnitude, Components.SAINCoreComponent.ShootMask))
                {
                    cover++;
                }
            }

            CoverRatio = (float)cover / rays;

            return CoverRatio > minratio;
        }

        private void AssignSettings()
        {
            switch (SAIN.CurrentDecision)
            {
                case SAINLogicDecision.Heal:
                case SAINLogicDecision.CombatHeal:
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
                    DebugGizmos.SingleObjects.Line(SAIN.Core.HeadPosition, CurrentCoverPoint.Position, Color.blue, 0.05f, true, 0.5f);

                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(SAIN.BotOwner.Transform.position, CurrentCoverPoint.Position, NavMesh.AllAreas, path))
                    {
                        SAIN.DebugDrawList.DrawTempPath(path, true, Color.magenta, Color.blue, 0.1f, 0.5f, false);
                    }
                }

                if (CurrentFallBackPoint != null)
                {
                    DebugGizmos.SingleObjects.Line(SAIN.Core.HeadPosition, CurrentFallBackPoint.Position, Color.magenta, 0.05f, true, 0.5f);

                    NavMeshPath path2 = new NavMeshPath();
                    if (NavMesh.CalculatePath(SAIN.BotOwner.Transform.position, CurrentFallBackPoint.Position, NavMesh.AllAreas, path2))
                    {
                        SAIN.DebugDrawList.DrawTempPath(path2, true, Color.magenta, Color.magenta, 0.1f, 0.5f, false);
                    }
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

        public bool InCover { get; private set; }
        public float CoverRatio { get; private set; }

        private float CheckSelfCoverTimer = 0f;
        private readonly float CheckSelfFreq = 0.1f;
        public CoverPoint CurrentCoverPoint => Component.CurrentCover;
        public CoverPoint CurrentFallBackPoint => Component.CurrentFallBackPoint;

        protected ManualLogSource Logger;
        public Vector3? EnemyPosition;
        private float DebugTimer = 0f;
    }

    public enum CoverStatus
    {
        None = 0,
        InCover = 1,
        FarFromCover = 3,
        CloseToCover = 4
    }
}
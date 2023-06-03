using BepInEx.Logging;
using EFT;
using SAIN.Classes;
using SAIN.Components;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.CoverConfig;

namespace SAIN.Classes
{
    public class CoverComponent : MonoBehaviour
    {
        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            CoverFinder = BotOwner.GetOrAddComponent<CoverFinderComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            FallBackPointStatus = CoverStatus.None;
            CoverPointStatus = CoverStatus.None;
        }

        private const float InCoverDist = 0.25f;
        private const float CloseCoverDist = 10f;
        private const float MidCoverDist = 25f;
        private const float FarCoverDist = 50f;

        public static List<SAINLogicDecision> FindCoverActions = new List<SAINLogicDecision> { SAINLogicDecision.RunAwayGrenade, SAINLogicDecision.Surgery, SAINLogicDecision.Reload, SAINLogicDecision.RunForCover, SAINLogicDecision.RunAway, SAINLogicDecision.FirstAid, SAINLogicDecision.Stims, SAINLogicDecision.MoveToCover};

        private void Update()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                CoverFinder.StopLooking();
                return;
            }

            if (FindCoverActions.Contains(SAIN.CurrentDecision) && (SAIN.HasGoalEnemy || SAIN.HasGoalTarget))
            {
                if (UpdateTimer < Time.time)
                {
                    AssignSettings();

                    UpdateTimer = Time.time + CoverUpdateFrequency.Value;

                    if (GetPointToHideFrom(out var target))
                    {
                        CoverFinder.LookForCover(target, BotOwner.Position);
                    }

                    FallBackPointStatus = CheckCoverPointStatus(CurrentFallBackPoint);
                    CoverPointStatus = CheckCoverPointStatus(CurrentCoverPoint);
                }
            }
            else
            {
                CoverFinder.StopLooking();
            }
        }

        private bool GetPointToHideFrom(out Vector3 target)
        {
            target = Vector3.zero;

            if (CurrentDecision == SAINLogicDecision.RunAwayGrenade)
            {
                var grenade = BotOwner.BewareGrenade.GrenadeDangerPoint;

                if (grenade != null)
                {
                    target = grenade.DangerPoint;
                }
                else if (SAIN.HasGoalEnemy)
                {
                    target = SAIN.MidPoint(SAIN.GoalEnemyPos.Value);
                }
                else if (SAIN.HasGoalTarget)
                {
                    target = SAIN.MidPoint(SAIN.GoalTargetPos.Value);
                }
            }
            else if (SAIN.HasGoalEnemy)
            {
                target = SAIN.GoalEnemyPos.Value;
            }
            else if (SAIN.HasGoalTarget)
            {
                target = SAIN.GoalTargetPos.Value;
            }

            return target != Vector3.zero;
        }

        private CoverStatus CheckCoverPointStatus(CoverPoint cover)
        {
            CoverStatus status = CoverStatus.None;

            if (cover != null && Vector3.Distance(cover.Position, BotOwner.Position) < FarCoverDist)
            {
                float pathLength = GetPathLengthToPoint(cover.Position);

                if (pathLength < InCoverDist)
                {
                    status = CoverStatus.InCover;
                }
                else if (pathLength < CloseCoverDist)
                {
                    status = CoverStatus.CloseToCover;
                }
                else if (pathLength < MidCoverDist)
                {
                    status = CoverStatus.MidRangeToCover;
                }
                else if (pathLength < FarCoverDist)
                {
                    status = CoverStatus.FarFromCover;
                }
            }

            return status;
        }

        private float GetPathLengthToPoint(Vector3 point)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(BotOwner.Transform.position, point, -1, path))
            {
                return path.CalculatePathLength();
            }
            return 999f;
        }

        public bool DuckInCover(bool SetPose1ifFalse = true)
        {
            if (BotIsAtCoverPoint)
            {
                SAIN.BotOwner.SetPose(0f);
            }
            else if (SetPose1ifFalse)
            {
                SAIN.BotOwner.SetPose(1f);
            }

            return BotIsAtCoverPoint;
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

                Vector3 direction = headPos - part.Position;

                float distance = Mathf.Clamp(direction.magnitude, 0f, 5f);

                if (Physics.Raycast(part.Position, direction, distance, SAINComponent.ShootMask))
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
                case SAINLogicDecision.Reload:
                case SAINLogicDecision.FirstAid:
                    CoverFinder.MinObstacleHeight = 1.0f;
                    break;

                case SAINLogicDecision.Surgery:
                case SAINLogicDecision.RunAway:
                case SAINLogicDecision.RunAwayGrenade:
                    CoverFinder.MinObstacleHeight = 1.55f;
                    break;

                default:
                    CoverFinder.MinObstacleHeight = CoverMinHeight.Value;
                    break;
            }
        }

        private SAINLogicDecision CurrentDecision => SAIN.CurrentDecision;

        public CoverStatus FallBackPointStatus { get; private set; }

        public CoverStatus CoverPointStatus { get; private set; }

        private BotOwner BotOwner => SAIN.BotOwner;

        private SAINComponent SAIN;

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

        public CoverFinderComponent CoverFinder { get; private set; }

        public CoverPoint CurrentCoverPoint => CoverFinder.CurrentCover;

        public CoverPoint CurrentFallBackPoint => CoverFinder.CurrentFallBackPoint;

        protected ManualLogSource Logger;

        private float UpdateTimer = 0f;

        public void Dispose()
        {
            StopAllCoroutines();
            CoverFinder.Dispose();
            Destroy(this);
        }
    }
}
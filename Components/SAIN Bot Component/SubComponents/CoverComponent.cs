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
        }

        private const float InCoverDist = 0.75f;
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

            if (SAIN.HasGoalEnemy || SAIN.HasGoalTarget)
            {
                if (UpdateTimer < Time.time)
                {
                    AssignSettings();

                    UpdateTimer = Time.time + 0.1f;

                    if (GetPointToHideFrom(out var target))
                    {
                        CoverFinder.LookForCover(target, BotOwner.Position);
                    }
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

        public bool DuckInCover(bool SetPose1ifFalse = true)
        {
            if (BotIsAtCoverPoint && ActiveCoverPoint.Collider.bounds.size.y < 1f)
            {
                SAIN.BotOwner.SetPose(0f);
                return true;
            }

            return false;
        }

        public bool CheckLimbsForCover()
        {
            if (!SAIN.HasGoalEnemy)
            {
                return false;
            }

            if (SAIN.EnemyIsVisible && SAIN.HasEnemyAndCanShoot)
            {
                var headPos = BotOwner.Memory.GoalEnemy.Person.MainParts[BodyPartType.head].Position;

                if (CheckLimbForCover(BodyPartType.leftLeg, headPos) && CheckLimbForCover(BodyPartType.leftArm, headPos))
                {
                    return true;
                }

                if (CheckLimbForCover(BodyPartType.rightLeg, headPos) && CheckLimbForCover(BodyPartType.rightArm, headPos))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckLimbForCover(BodyPartType bodyPartType, Vector3 target, float dist = 2f)
        {
            var position = BotOwner.MainParts[bodyPartType].Position;
            Vector3 direction = target - position;
            return Physics.Raycast(position, direction, dist, SAINComponent.SightMask);
        }

        public bool CheckSelfForCover(float minratio = 0.1f)
        {
            if (!SAIN.HasGoalEnemy)
            {
                return false;
            }

            int rays = 0;
            int cover = 0;

            var headPos = BotOwner.Memory.GoalEnemy.Person.MainParts[BodyPartType.head].Position;

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

        public CoverStatus FallBackPointStatus
        {
            get
            {
                if (CurrentFallBackPoint != null)
                {
                    return CurrentFallBackPoint.Status();
                }
                return CoverStatus.None;
            }
        }

        public CoverStatus CoverPointStatus
        {
            get
            {
                if (CurrentCoverPoint != null)
                {
                    return CurrentCoverPoint.Status();
                }
                return CoverStatus.None;
            }
        }

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
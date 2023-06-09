using BepInEx.Logging;
using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Classes
{
    public class CoverClass : SAINBot
    {
        public CoverClass(BotOwner bot) : base(bot)
        {
            CoverFinder = BotOwner.GetOrAddComponent<CoverFinderComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        public void Update()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                CoverFinder.StopLooking();
                return;
            }
            if ((SAIN.HasGoalEnemy || SAIN.HasGoalTarget) && SAIN.CurrentDecision != SAINLogicDecision.None && SAIN.CurrentDecision != SAINLogicDecision.Search)
            {
                if (GetPointToHideFrom(out var target))
                {
                    CoverFinder.LookForCover(target.Value, BotOwner.Position);
                }
            }
            else
            {
                CoverFinder.StopLooking();
            }
        }

        private bool GetPointToHideFrom(out Vector3? target)
        {
            target = null;
            if (CurrentDecision == SAINLogicDecision.RunAwayGrenade)
            {
                target = BotOwner.BewareGrenade.GrenadeDangerPoint?.DangerPoint;
            }
            if (target == null)
            {
                target = SAIN.CurrentTargetPosition;
            }
            return target != null;
        }

        public bool DuckInCover()
        {
            if (BotIsAtCoverPoint)
            {
                if (ActiveCoverPoint.Collider.bounds.size.y < 1f && BotOwner.BotLay.CanProne)
                {
                    SAIN.BotOwner.BotLay.TryLay();
                    return true;
                }
                if (ActiveCoverPoint.Collider.bounds.size.y < 1.5f)
                {
                    SAIN.BotOwner.SetPose(0f);
                    return true;
                }
            }
            return false;
        }

        public bool CheckLimbsForCover()
        {
            if (!SAIN.HasGoalEnemy)
            {
                return false;
            }
            if (SAIN.Enemy.IsVisible && SAIN.HasEnemyAndCanShoot)
            {
                var target = BotOwner.Memory.GoalEnemy.Person.WeaponRoot.position;
                if (CheckLimbForCover(BodyPartType.leftLeg, target, 5f) && CheckLimbForCover(BodyPartType.leftArm, target, 5f))
                {
                    return true;
                }
                if (CheckLimbForCover(BodyPartType.rightLeg, target, 5f) && CheckLimbForCover(BodyPartType.rightArm, target, 5f))
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
            return Physics.Raycast(position, direction, dist, LayerMaskClass.HighPolyWithTerrainMask);
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

        public void Dispose()
        {
            StopAllCoroutines();
            CoverFinder.Dispose();
            Destroy(this);
        }
    }
}
using EFT;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class SAINCoverClass : SAINBase, ISAINClass
    {
        public SAINCoverClass(SAINComponentClass sain) : base(sain)
        {
            CoverFinder = sain.GetOrAddComponent<CoverFinderComponent>();
            Player.HealthController.ApplyDamageEvent += OnBeingHit;
        }

        public void Init()
        {
            CoverFinder.Init(SAIN);
        }

        public void Update()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                ActivateCoverFinder(false);
                return;
            }

            // If the config option is enabled. Let a bot find cover all the time when they have a target or enemy if the enemy is the player.
            if (GlobalSettings.General.EnhancedCoverFinding && SAIN.CurrentTargetPosition != null)
            {
                if (SAIN?.Enemy?.EnemyPlayer?.IsYourPlayer == true)
                {
                    ActivateCoverFinder(true);
                    return;
                }
                // No way to check if a GoalTarget is created by a player afaik, so enable it anyways
                else if (SAIN.Enemy == null)
                {
                    ActivateCoverFinder(true);
                    return;
                }
            }

            var CurrentDecision = SAIN.Memory.Decisions.Main.Current;
            if (CurrentDecision == SoloDecision.UnstuckMoveToCover || CurrentDecision == SoloDecision.Retreat || CurrentDecision == SoloDecision.RunToCover || CurrentDecision == SoloDecision.WalkToCover)
            {
                ActivateCoverFinder(true);
            }
            else if (CurrentDecision == SoloDecision.HoldInCover && (CoverInUse == null || CoverInUse.Spotted == true || Time.time - CoverInUse.TimeCreated > 5f))
            {
                ActivateCoverFinder(true);
            }
            else
            {
                ActivateCoverFinder(false);
            }
        }

        public void Dispose()
        {
            try
            {
                Player.HealthController.ApplyDamageEvent -= OnBeingHit;
                CoverFinder?.Dispose();
            }
            catch { }
        }

        private void OnBeingHit(EBodyPart part, float unused, DamageInfo damage)
        {
            LastHitTime = Time.time;
            CoverPoint activePoint = CoverInUse;
            if (activePoint != null && activePoint.CoverStatus == CoverStatus.InCover)
            {
                SAINEnemyClass enemy = SAIN.Enemy;
                if (enemy != null && damage.Player != null && enemy.EnemyPlayer.ProfileId == damage.Player.iPlayer.ProfileId)
                {
                    activePoint.HitInCoverCount++;
                    if (!enemy.IsVisible)
                    {
                        activePoint.HitInCoverCount++;
                    }
                }
                else
                {
                    activePoint.HitInCoverUnknownCount++;
                }
            }
        }

        private void ActivateCoverFinder(bool value)
        {
            if (value && GetPointToHideFrom(out var target))
            {
                CoverFinder?.LookForCover(target.Value, BotOwner.Position);
            }
            if (!value)
            {
                CoverFinder?.StopLooking();
            }
        }

        public CoverPoint ClosestPoint
        {
            get
            {
                if (CoverPoints.Count > 0)
                {
                    return CoverPoints[0];
                }
                return null;
            }
        }

        public CoverPoint FarPointEnemy
        {
            get
            {
                if (CoverPoints.Count > 0)
                {
                    return CoverPoints[CoverPoints.Count - 1];
                }
                return null;
            }
        }

        private bool GetPointToHideFrom(out Vector3? target)
        {
            target = SAIN.Grenade.GrenadeDangerPoint ?? SAIN.CurrentTargetPosition;
            return target != null;
        }

        public bool DuckInCover()
        {
            var point = CoverInUse;
            if (point != null)
            {
                var move = SAIN.Mover;
                var prone = move.Prone;
                if (point.Collider.bounds.size.y < 0.7f && prone.ShallProneHide())
                {
                    prone.SetProne(true);
                    return true;
                }
                if (move.Pose.SetPoseToCover())
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckLimbsForCover()
        {
            var enemy = SAIN.Enemy;
            if (enemy?.IsVisible == true)
            {
                if (CheckLimbTimer < Time.time)
                {
                    CheckLimbTimer = Time.time + 0.1f;
                    bool cover = false;
                    var target = enemy.EnemyIAIDetails.WeaponRoot.position;
                    if (CheckLimbForCover(BodyPartType.leftLeg, target, 4f) && CheckLimbForCover(BodyPartType.leftArm, target, 4f))
                    {
                        cover = true;
                    }
                    else if (CheckLimbForCover(BodyPartType.rightLeg, target, 4f) && CheckLimbForCover(BodyPartType.rightArm, target, 4f))
                    {
                        cover = true;
                    }
                    HasLimbCover = cover;
                }
            }
            else
            {
                HasLimbCover = false;
            }
            return HasLimbCover;
        }

        private bool HasLimbCover;
        private float CheckLimbTimer = 0f;

        private bool CheckLimbForCover(BodyPartType bodyPartType, Vector3 target, float dist = 2f)
        {
            var position = BotOwner.MainParts[bodyPartType].Position;
            Vector3 direction = target - position;
            return Physics.Raycast(position, direction, dist, LayerMaskClass.HighPolyWithTerrainMask);
        }

        public bool BotIsAtCoverPoint(out CoverPoint coverPoint)
        {
            coverPoint = CoverInUse;
            return BotIsAtCoverPoint(coverPoint);
        }

        public bool BotIsAtCoverPoint(CoverPoint coverPoint)
        {
            return coverPoint != null && coverPoint.BotIsHere;
        }

        public bool BotIsAtCoverPoint()
        {
            var coverPoint = CoverInUse;
            return BotIsAtCoverPoint(coverPoint);
        }

        public bool BotIsMovingToPoint
        {
            get
            {
                if (BotOwner?.Mover == null)
                {
                    return false;
                }
                var point = CoverInUse;
                return point != null && (point.Position - BotOwner.Mover.CurPathLastPoint).sqrMagnitude < 1f;
            }
        }

        public CoverPoint CoverInUse
        {
            get
            {
                foreach (var point in CoverPoints)
                {
                    if (point != null && point.BotIsUsingThis)
                    {
                        return point;
                    }
                }
                return null;
            }
        }

        public List<CoverPoint> CoverPoints => CoverFinder.CoverPoints;
        public CoverFinderComponent CoverFinder { get; private set; }
        public CoverPoint CurrentCoverPoint => ClosestPoint;
        public CoverPoint FallBackPoint => CoverFinder.FallBackPoint;

        public float LastHitTime { get; private set; }
    }
}
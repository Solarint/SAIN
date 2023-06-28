using BepInEx.Logging;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAIN.Classes
{
    public class CoverClass : MonoBehaviour
    {
        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN.BotOwner;

        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            CoverFinder = BotOwner.GetOrAddComponent<CoverFinderComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN.Player.BeingHitAction += OnBeingHit;
        }

        private void OnBeingHit(DamageInfo damage, EBodyPart part, float unused)
        {
            CoverPoint activePoint = CoverInUse;
            if (activePoint != null && activePoint.CoverStatus == CoverStatus.InCover)
            {
                SAINEnemy enemy = SAIN.Enemy;
                if (enemy != null && damage.Player != null && enemy.EnemyPlayer.ProfileId == damage.Player.ProfileId)
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

        private void Update()
        {
            if (BotOwner == null || SAIN == null) return;
            var CurrentDecision = SAIN.CurrentDecision;
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                ActivateCoverFinder(false);
                return;
            }
            if (CurrentDecision == SAINSoloDecision.UnstuckMoveToCover || CurrentDecision == SAINSoloDecision.Retreat || CurrentDecision == SAINSoloDecision.RunToCover || CurrentDecision == SAINSoloDecision.WalkToCover)
            {
                ActivateCoverFinder(true);
            }
            else if (CurrentDecision == SAINSoloDecision.HoldInCover && (CoverInUse == null || CoverInUse.Spotted == true || Time.time - CoverInUse.TimeCreated > 5f))
            {
                ActivateCoverFinder(true);
            }
            else
            {
                ActivateCoverFinder(false);
            }
        }

        private void ActivateCoverFinder(bool value)
        {
            if (value)
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

        public void OnDestroy()
        {
            SAIN.Player.BeingHitAction -= OnBeingHit;
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
                    var target = enemy.Person.WeaponRoot.position;
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

        public bool BotIsAtCoverPoint
        {
            get
            {
                var point = CoverInUse;
                return point != null && point.BotIsHere;
            }
        }

        public bool BotIsMovingToPoint
        {
            get
            {
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

        protected ManualLogSource Logger;
    }
}
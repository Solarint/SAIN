using BepInEx.Logging;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAIN.Classes
{
    public class CoverClass : SAINBot
    {
        public CoverClass(BotOwner bot) : base(bot)
        {
            CoverFinder = BotOwner.GetOrAddComponent<CoverFinderComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN.Player.BeingHitAction += OnBeingHit;
        }

        private void OnBeingHit(DamageInfo damage, EBodyPart part, float unused)
        {
            CoverPoint activePoint = CoverInUse;
            if (activePoint != null && activePoint.BotIsHere)
            {
                SAINEnemy enemy = SAIN.Enemy;
                if (enemy != null && damage.Player != null && enemy.EnemyPlayer.ProfileId == damage.Player.ProfileId)
                {
                    activePoint.HitInCoverCount++;
                    if (!enemy.IsVisible)
                    {
                        activePoint.HitInCoverCount += 2;
                    }
                    else
                    {
                        activePoint.HitInCoverCount += 1;
                    }
                }
                else
                {
                    activePoint.HitInCoverUnknownCount++;
                }
            }
        }

        public void Update()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                ActivateCoverFinder(false);
                return;
            }
            if (CurrentDecision == SAINSoloDecision.UnstuckMoveToCover || CurrentDecision == SAINSoloDecision.Retreat || CurrentDecision == SAINSoloDecision.RunToCover || CurrentDecision == SAINSoloDecision.WalkToCover)
            {
                ActivateCoverFinder(true);
            }
            else if (CurrentDecision == SAINSoloDecision.HoldInCover && CoverInUse?.Spotted == true)
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

        public void Dispose()
        {
            SAIN.Player.BeingHitAction -= OnBeingHit;
        }

        public CoverPoint ClosestPoint
        {
            get
            {
                var points = CoverPoints;
                if (points.Count == 1)
                {
                    return points.First();
                }
                if (points.Count > 0)
                {
                    var pointsArray = points.ToArray();
                    System.Array.Sort(pointsArray, CoverFinder.CoverPointPathComparerer);
                    CoverPoints.Clear();
                    CoverPoints.AddRange(pointsArray);
                    return CoverPoints.First();
                }
                return null;
            }
        }

        public CoverPoint FarPointEnemy
        {
            get
            {
                if (CoverPoints.Count == 1)
                {
                    return CoverPoints.First();
                }
                if (CoverPoints.Count > 1)
                {
                    var points = CoverFinder.CoverPoints.ToArray();
                    System.Array.Sort(points, CoverPointEnemyComparerer);
                    return points.Last();
                }
                return null;
            }
        }

        private int CoverPointEnemyComparerer(CoverPoint A,  CoverPoint B)
        {
            if (A == null && B != null)
            {
                return 1;
            }
            else if (A != null && B == null)
            {
                return -1;
            }
            else if (A == null && B == null)
            {
                return 0;
            }
            else
            {
                if (SAIN.CurrentTargetPosition == null)
                {
                    return 0;
                }
                Vector3 enemy = SAIN.CurrentTargetPosition.Value;
                float ADist = (enemy - A.Position).sqrMagnitude;
                float BDist = (enemy - B.Position).sqrMagnitude;
                return ADist.CompareTo(BDist);
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
            if (SAIN.Enemy?.IsVisible == true)
            {
                if (CheckLimbTimer < Time.time)
                {
                    CheckLimbTimer = Time.time + 0.1f;
                    bool cover = false;
                    var target = BotOwner.Memory.GoalEnemy.Person.WeaponRoot.position;
                    if (CheckLimbForCover(BodyPartType.leftLeg, target, 5f) && CheckLimbForCover(BodyPartType.leftArm, target, 5f))
                    {
                        cover = true;
                    }
                    else if (CheckLimbForCover(BodyPartType.rightLeg, target, 5f) && CheckLimbForCover(BodyPartType.rightArm, target, 5f))
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
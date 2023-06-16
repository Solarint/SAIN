using BepInEx.Logging;
using EFT;
using SAIN.Components;
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
        }

        public void Update()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                CoverFinder.StopLooking();
                return;
            }
            if ((SAIN.HasGoalEnemy || SAIN.HasGoalTarget))
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

        public CoverPoint ClosestPoint
        {
            get
            {
                if (CoverFinder.CoverPoints.Count > 0)
                {
                    return CoverFinder.CoverPoints[0];
                }
                return null;
            }
        }

        public CoverPoint FarPointEnemy
        {
            get
            {
                if (CoverFinder.CoverPoints.Count == 1)
                {
                    return CoverFinder.CoverPoints.First();
                }
                if (CoverFinder.CoverPoints.Count > 1)
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
            var point = ActiveCoverPoint;
            if (point != null)
            {
                if (point.Collider.bounds.size.y < 0.7f && SAIN.Mover.ShallProneHide())
                {
                    SAIN.Mover.SetBotProne(true);
                    return true;
                }
                if (point.Collider.bounds.size.y < 1.3f)
                {
                    SAIN.Mover.SetTargetPose(0f);
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

        public bool BotIsAtCoverPoint => ActiveCoverPoint != null;
        public CoverPoint ActiveCoverPoint
        {
            get
            {
                foreach (var point in CoverFinder.CoverPoints)
                {
                    if (point != null)
                    {
                        if (point.CoverStatus == CoverStatus.InCover)
                        {
                            return point;
                        }
                    }
                }
                return null;
            }
        }

        public CoverFinderComponent CoverFinder { get; private set; }

        public CoverPoint CurrentCoverPoint => ClosestPoint;

        public CoverPoint CurrentFallBackPoint => ClosestPoint;

        protected ManualLogSource Logger;
    }
}
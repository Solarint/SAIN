using EFT;
using EFT.Hideout.ShootingRange;
using SAIN.Components;
using SAIN.SAINComponent;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Decision
{
    public class TargetDecisionClass : SAINBase, ISAINClass
    {
        public TargetDecisionClass(SAINComponentClass sain) : base(sain)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public float FoundTargetTimer { get; private set; }

        public bool GetDecision(out SoloDecision Decision)
        {
            var CurrentDecision = SAIN.Memory.Decisions.Main.Current;
            Decision = SoloDecision.None;

            if (!BotOwner.Memory.GoalTarget.HaveMainTarget())
            {
                FoundTargetTimer = -1f;
                return false;
            }
            else
            {
                if (FoundTargetTimer < 0f)
                {
                    FoundTargetTimer = Time.time;
                }
            }

            bool shallNotSearch = ShallNotSearch();

            if (StartInvestigate() && !shallNotSearch)
            {
                Decision = SoloDecision.Investigate;
            }
            else if (BotOwner.Memory.IsUnderFire)
            {
                Decision = SoloDecision.RunToCover;
            }
            else if (StartSearch() && !shallNotSearch)
            {
                if (CurrentDecision != SoloDecision.Search)
                {
                    SAIN.Info.CalcTimeBeforeSearch();
                }
                Decision = SoloDecision.Search;
            }
            else if (SAIN.Decision.EnemyDecisions.StartHoldInCover())
            {
                Decision = SoloDecision.HoldInCover;
            }
            else
            {
                Decision = SoloDecision.WalkToCover;
            }

            return Decision != SoloDecision.None;
        }

        private bool StartInvestigate()
        {
            if (Time.time - FoundTargetTimer > 10f)
            {
                var sound = BotOwner.BotsGroup.YoungestPlace(BotOwner, 200f, true);
                if (sound != null)
                {
                    if (sound.IsDanger)
                    {
                        return true;
                    }
                    if (SAIN.Info.Profile.IsPMC)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ShallNotSearch()
        {
            Vector3? target = SAIN.CurrentTargetPosition;
            if (target != null && !SAIN.Info.Profile.IsPMC && SAIN.Memory.BotZoneCollider != null)
            {
                Vector3 closestPointInZone = SAIN.Memory.BotZoneCollider.ClosestPointOnBounds(target.Value);
                float distance = (target.Value - closestPointInZone).magnitude;
                if (distance > 50f)
                {
                    return true;
                }
            }
            return false;
        }

        private bool StartSearch()
        {
            return Time.time - FoundTargetTimer > SAIN.Info.TimeBeforeSearch;
        }
    }
}

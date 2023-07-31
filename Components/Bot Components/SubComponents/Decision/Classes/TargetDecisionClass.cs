using EFT;
using EFT.Hideout.ShootingRange;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Classes
{
    public class TargetDecisionClass : SAINBot
    {
        public TargetDecisionClass(SAINComponent bot) : base(bot) { }
        public float FoundTargetTimer { get; private set; }

        public bool GetDecision(out SAINSoloDecision Decision)
        {
            Decision = SAINSoloDecision.None;

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
                Decision = SAINSoloDecision.Investigate;
            }
            else if (BotOwner.Memory.IsUnderFire)
            {
                Decision = SAINSoloDecision.RunToCover;
            }
            else if (StartSearch() && !shallNotSearch)
            {
                if (CurrentDecision != SAINSoloDecision.Search)
                {
                    SAIN.Info.CalcTimeBeforeSearch();
                }
                Decision = SAINSoloDecision.Search;
            }
            else if (SAIN.Decision.EnemyDecisions.StartHoldInCover())
            {
                Decision = SAINSoloDecision.HoldInCover;
            }
            else
            {
                Decision = SAINSoloDecision.WalkToCover;
            }

            return Decision != SAINSoloDecision.None;
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
                    if (SAIN.Info.IsPMC)
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
            if (target != null && !SAIN.Info.IsPMC && SAIN.BotZoneCollider != null)
            {
                Vector3 closestPointInZone = SAIN.BotZoneCollider.ClosestPointOnBounds(target.Value);
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

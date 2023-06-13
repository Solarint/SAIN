using EFT;
using EFT.Hideout.ShootingRange;
using UnityEngine;

namespace SAIN.Classes
{
    public class TargetDecisionClass : SAINBot
    {
        public TargetDecisionClass(BotOwner bot) : base(bot) { }

        public bool GetDecision(out SAINSoloDecision Decision)
        {
            Decision = SAINSoloDecision.None;

            if (!SAIN.HasGoalTarget)
            {
                return false;
            }
            var realTarget = BotOwner.Memory.GoalTarget.GoalTarget;
            if (realTarget?.IsDanger == true || SAIN.Info.IsPMC)
            {
                if (realTarget?.Position != null)
                {
                    if (BotOwner.Memory.IsUnderFire)
                    {
                        Decision = SAINSoloDecision.RunForCover;
                    }
                    else if (StartSearch())
                    {
                        Decision = SAINSoloDecision.Search;
                    }
                    else
                    {
                        Decision = SAINSoloDecision.MoveToCover;
                    }
                }
            }

            return Decision != SAINSoloDecision.None;
        }

        private bool StartSearch()
        {
            return Time.time - BotOwner.Memory.GoalTarget?.CreatedTime > SAIN.Info.TimeBeforeSearch;
        }
    }
}

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

            if (StartInvestigate())
            {
                Decision = SAINSoloDecision.Investigate;
                return true;
            }

            var realTarget = BotOwner.Memory.GoalTarget.GoalTarget;
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

            return Decision != SAINSoloDecision.None;
        }

        private bool StartInvestigate()
        {
            var sound = BotOwner.BotsGroup.YoungestPlace(BotOwner, 100f, true);
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
            return false;
        }

        private bool StartSearch()
        {
            return Time.time - BotOwner.Memory.GoalTarget?.CreatedTime > SAIN.Info.TimeBeforeSearch;
        }
    }
}

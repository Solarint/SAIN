using EFT;
using EFT.Hideout.ShootingRange;
using UnityEngine;

namespace SAIN.Classes
{
    public class TargetDecisionClass : SAINBot
    {
        public TargetDecisionClass(BotOwner bot) : base(bot) { }

        public float FoundTargetTimer;

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

            if (StartInvestigate())
            {
                Decision = SAINSoloDecision.Investigate;
            }
            else if (BotOwner.Memory.IsUnderFire)
            {
                Decision = SAINSoloDecision.RunToCover;
            }
            else if (StartSearch())
            {
                Decision = SAINSoloDecision.Search;
            }
            else if (SAIN.Cover.CoverInUse == null)
            {
                Decision = SAINSoloDecision.WalkToCover;
            }
            else
            {
                Decision = SAINSoloDecision.HoldInCover;
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
            return Time.time - FoundTargetTimer > SAIN.Info.TimeBeforeSearch;
        }
    }
}

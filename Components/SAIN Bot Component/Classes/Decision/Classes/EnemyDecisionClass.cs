using BepInEx.Logging;
using EFT;
using UnityEngine;

namespace SAIN.Classes
{
    public class EnemyDecisionClass : SAINBot
    {
        public EnemyDecisionClass(BotOwner bot) : base(bot) { }

        protected ManualLogSource Logger => SAIN.Decision.Logger;

        public bool GetDecision(out SAINSoloDecision Decision)
        {
            if (SAIN.Enemy == null || BotOwner.Memory.GoalEnemy == null)
            {
                Decision = SAINSoloDecision.None;
                return false;
            }

            if (StartDogFightAction())
            {
                Decision = SAINSoloDecision.DogFight;
            }
            else if (StartStandAndShoot())
            {
                Decision = SAINSoloDecision.StandAndShoot;
            }
            else if (StartSearch())
            {
                Decision = SAINSoloDecision.Search;
            }
            else if (StartHoldInCover())
            {
                Decision = SAINSoloDecision.HoldInCover;
            }
            else if (StartMoveToCover())
            {
                Decision = SAINSoloDecision.MoveToCover;

                if (StartRunForCover())
                {
                    Decision = SAINSoloDecision.RunForCover;
                }
            }
            else
            {
                Decision = SAINSoloDecision.Shoot;
            }

            if (Decision != SAINSoloDecision.MoveToCover && Decision != SAINSoloDecision.RunForCover)
            {
                StartRunCoverTimer = 0f;
            }

            return true;
        }

        private bool StartDogFightAction()
        {
            var pathStatus = SAIN.Enemy.CheckPathDistance();
            return pathStatus == SAINEnemyPath.VeryClose && SAIN.Enemy.IsVisible;
        }

        private bool HideFromFlankShot()
        {
            if (BotOwner.Memory.IsUnderFire)
            {
            }
            return false;
        }

        private bool StartRunForCover()
        {
            return StartRunCoverTimer < Time.time && SAIN.BotHasStamina && BotOwner.CanSprintPlayer;
        }

        private bool StartMoveToCover()
        {
            bool start = !SAIN.Cover.BotIsAtCoverPoint && (SAIN.Cover.CurrentCoverPoint != null || SAIN.Cover.CurrentFallBackPoint != null);

            if (start)
            {
                if (CurrentDecision != SAINSoloDecision.MoveToCover && CurrentDecision != SAINSoloDecision.RunForCover)
                {
                    StartRunCoverTimer = Time.time + 3f * Random.Range(0.66f, 1.33f);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool StartSearch()
        {
            if (SAIN.Enemy?.IsVisible == true)
            {
                return false;
            }

            if (SAIN.Enemy?.TimeSinceSeen >= SAIN.Info.TimeBeforeSearch)
            {
                return true;
            }
            return false;
        }

        private bool StartHoldInCover()
        {
            if (SAIN.Cover.BotIsAtCoverPoint)
            {
                return true;
            }
            return false;
        }

        private bool StartStandAndShoot()
        {
            if (SAIN.Enemy.IsVisible || SAIN.HasEnemyAndCanShoot)
            {
                float holdGround = SAIN.Info.HoldGroundDelay;

                if (holdGround <= 0f)
                {
                    return false;
                }

                float changeVis = Time.time - BotOwner.Memory.GoalEnemy.LastChangeVisionTime;
                float lastShoot = Time.time - BotOwner.Memory.GoalEnemy.PersonalLastShootTime;

                if (changeVis < holdGround && lastShoot < holdGround)
                {
                    if (changeVis < holdGround / 2f && lastShoot < holdGround / 2f)
                    {
                        return true;
                    }
                    else
                    {
                        return SAIN.Cover.CheckLimbsForCover();
                    }
                }
            }
            return false;
        }

        private float StartRunCoverTimer;
    }
}

using BepInEx.Logging;
using EFT;
using UnityEngine;

namespace SAIN.Classes
{
    public class EnemyDecisionClass : SAINBot
    {
        public EnemyDecisionClass(BotOwner bot) : base(bot) 
        {
        }

        protected ManualLogSource Logger => SAIN.Decision.Logger;

        public bool GetDecision(out SAINSoloDecision Decision)
        {
            if (SAIN.Enemy == null)
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
            else if (StartRushEnemy())
            {
                Decision = SAINSoloDecision.RushEnemy;
            }
            else if (StartSearch())
            {
                Decision = SAINSoloDecision.Search;
            }
            else if (StartShiftCover())
            {
                Decision = SAINSoloDecision.ShiftCover;
            }
            else if (StartHoldInCover())
            {
                Decision = SAINSoloDecision.HoldInCover;
            }
            else if (StartMoveToCover())
            {
                Decision = SAINSoloDecision.WalkToCover;

                if (StartRunForCover())
                {
                    Decision = SAINSoloDecision.RunToCover;
                }
            }
            else if (StartThrowNade())
            {
                Decision = SAINSoloDecision.ThrowGrenade;
            }
            else
            {
                Decision = SAINSoloDecision.DogFight;
            }

            if (Decision != SAINSoloDecision.WalkToCover && Decision != SAINSoloDecision.RunToCover)
            {
                StartRunCoverTimer = 0f;
            }

            return true;
        }

        private bool StartRushEnemy()
        {
            var Personality = SAIN.Info.BotPersonality;
            if (SAIN.Info.IsPMC)
            {
                var enemy = SAIN.Enemy;
                if (enemy != null && enemy.PathDistance < 25f)
                {
                    if (!SAIN.Decision.SelfActionDecisions.CheckLowAmmo(0.4f) && SAIN.HealthStatus != ETagStatus.Dying)
                    {
                        if (enemy.EnemyIsReloading || enemy.EnemyIsHealing || enemy.EnemyHasGrenadeOut)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool StartShiftCover()
        {
            if (ContinueShiftCover())
            {
                return true;
            }

            if (CurrentDecision == SAINSoloDecision.HoldInCover)
            {
                if (SAIN.Decision.TimeSinceChangeDecision > 3f)
                {
                    var enemy = SAIN.Enemy;
                    if (enemy != null && enemy.Seen && !enemy.IsVisible && enemy.TimeSinceSeen > 5f)
                    {
                        ShiftResetTimer = Time.time + 5f;
                        return true;
                    }
                    if (enemy == null && SAIN.Decision.TimeSinceChangeDecision > 6f)
                    {
                        ShiftResetTimer = Time.time + 5f;
                        return true;
                    }
                }
            }

            ShiftResetTimer = -1f;
            return false;
        }

        private bool ContinueShiftCover()
        {
            if (CurrentDecision == SAINSoloDecision.ShiftCover)
            {
                if (ShiftResetTimer > 0f && ShiftResetTimer < Time.time)
                {
                    ShiftResetTimer = -1f;
                    return false;
                }
                if (!ShiftCoverComplete)
                {
                    return true;
                }
            }
            return false;
        }

        private float ShiftResetTimer;
        public bool ShiftCoverComplete { get; set; }

        private bool StartDogFightAction()
        {
            var pathStatus = SAIN.Enemy.CheckPathDistance();
            return pathStatus == SAINEnemyPath.VeryClose && SAIN.Enemy.IsVisible;
        }

        private bool StartThrowNade()
        {
            if (ContinueThrow())
            {
                return true;
            }

            var nade = BotOwner.WeaponManager.Grenades;

            if (!nade.HaveGrenade)
            {
                return false;
            }

            var enemy = SAIN.Enemy;

            if (enemy == null)
            {
                return false;
            }
            if (enemy.IsVisible)
            {
                return false;
            }

            if (enemy.TimeSinceSeen > 3f && enemy.TimeSinceSeen < 15f && enemy.Seen)
            {
                if (SAIN.Grenade.EFTBotGrenade.CanThrowGrenade(enemy.Position))
                {
                    EndThrowTimer = Time.time;
                    return true;
                }
            }

            return false;
        }

        private float EndThrowTimer = 0f;

        private bool ContinueThrow()
        {
            if (SAIN.Grenade.EFTBotGrenade.AIGreanageThrowData == null || Time.time - EndThrowTimer > 3f)
            {
                return false;
            }
            return CurrentDecision == SAINSoloDecision.ThrowGrenade && SAIN.Grenade.EFTBotGrenade.AIGreanageThrowData?.ThrowComplete == false;
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
            bool start = !SAIN.Cover.BotIsAtCoverPoint;

            if (start)
            {
                if (CurrentDecision != SAINSoloDecision.WalkToCover && CurrentDecision != SAINSoloDecision.RunToCover)
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
            var cover = SAIN.Cover.CoverInUse;
            if (cover != null && !cover.Spotted && (cover.Position - BotOwner.Position).sqrMagnitude < 1.5f)
            {
                return true;
            }
            return false;
        }

        private bool StartStandAndShoot()
        {
            var enemy = SAIN.Enemy;
            if (SAIN.Enemy.IsVisible && SAIN.Enemy.CanShoot)
            {
                float holdGround = SAIN.Info.HoldGroundDelay;

                if (holdGround <= 0f)
                {
                    return false;
                }

                float visibleFor = Time.time - enemy.VisibleStartTime;

                if (visibleFor < holdGround)
                {
                    if (visibleFor < holdGround / 2f)
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

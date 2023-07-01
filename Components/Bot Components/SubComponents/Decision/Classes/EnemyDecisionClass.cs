using BepInEx.Logging;
using EFT;
using UnityEngine;

namespace SAIN.Classes
{
    public class EnemyDecisionClass : SAINBot
    {
        public EnemyDecisionClass(BotOwner bot) : base(bot) { }

        public bool GetDecision(out SAINSoloDecision Decision)
        {
            SAINEnemy enemy = SAIN.Enemy;
            if (enemy == null)
            {
                Decision = SAINSoloDecision.None;
                return false;
            }

            if (StartDogFightAction(enemy))
            {
                Decision = SAINSoloDecision.DogFight;
            }
            else if (StartMoveToEngage(enemy))
            {
                Decision = SAINSoloDecision.MoveToEngage;
            }
            else if (StartStandAndShoot(enemy))
            {
                if (CurrentDecision != SAINSoloDecision.StandAndShoot)
                {
                    SAIN.Info.CalcHoldGroundDelay();
                }
                Decision = SAINSoloDecision.StandAndShoot;
            }
            else if (StartRushEnemy(enemy))
            {
                Decision = SAINSoloDecision.RushEnemy;
            }
            else if (StartSearch(enemy))
            {
                if (CurrentDecision != SAINSoloDecision.Search)
                {
                    SAIN.Info.CalcTimeBeforeSearch();
                }
                Decision = SAINSoloDecision.Search;
            }
            else if (StartShiftCover(enemy))
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
            else if (StartThrowNade(enemy))
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

        private bool StartRushEnemy(SAINEnemy enemy)
        {
            if (SAIN.Info.Personality == SAINPersonality.Chad || SAIN.Info.Personality == SAINPersonality.GigaChad)
            {
                if (enemy != null && enemy.PathDistance < 30f)
                {
                    if (!SAIN.Decision.SelfActionDecisions.LowOnAmmo(0.5f) && SAIN.HealthStatus != ETagStatus.Dying && BotOwner.CanSprintPlayer)
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

        private bool StartShiftCover(SAINEnemy enemy)
        {
            if (ContinueShiftCover())
            {
                return true;
            }

            if (CurrentDecision == SAINSoloDecision.HoldInCover)
            {
                if (SAIN.Decision.TimeSinceChangeDecision > 3f && TimeForNewShift < Time.time)
                {
                    if (enemy != null)
                    {
                        if (enemy.Seen && !enemy.IsVisible && enemy.TimeSinceSeen > 5f)
                        {
                            TimeForNewShift = Time.time + 10f;
                            ShiftResetTimer = Time.time + 5f;
                            return true;
                        }
                        if (!enemy.Seen && enemy.TimeSinceEnemyCreated > 8f)
                        {
                            TimeForNewShift = Time.time + 10f;
                            ShiftResetTimer = Time.time + 5f;
                            return true;
                        }
                    }
                    if (enemy == null && SAIN.Decision.TimeSinceChangeDecision > 6f)
                    {
                        TimeForNewShift = Time.time + 10f;
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

        private float TimeForNewShift;

        private float ShiftResetTimer;
        public bool ShiftCoverComplete { get; set; }

        private bool StartDogFightAction(SAINEnemy enemy)
        {
            var pathStatus = enemy.CheckPathDistance();
            return (pathStatus == SAINEnemyPath.VeryClose && SAIN.Enemy.IsVisible) || SAIN.Cover.CoverInUse?.Spotted == true;
        }

        private bool StartThrowNade(SAINEnemy enemy)
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

        private bool StartMoveToEngage(SAINEnemy enemy)
        {
            if (!enemy.Seen)
            {
                return false;
            }
            if (enemy.IsVisible && enemy.EnemyLookingAtMe)
            {
                return false;
            }
            if (BotOwner.Memory.IsUnderFire || Time.time - BotOwner.Memory.UnderFireTime > TimeBeforeSearch)
            {
                return false;
            }
            if (enemy.RealDistance > SAIN.Info.WeaponInfo.EffectiveWeaponDistance)
            {
                return true;
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

        private bool StartRunForCover()
        {
            return StartRunCoverTimer < Time.time && BotOwner.CanSprintPlayer;
        }

        private bool StartMoveToCover()
        {
            bool inCover = SAIN.Cover.BotIsAtCoverPoint();

            if (!inCover)
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

        private bool StartSearch(SAINEnemy enemy)
        {
            if (enemy.IsVisible == true)
            {
                return false;
            }
            if (enemy.Seen && enemy.TimeSinceSeen >= TimeBeforeSearch)
            {
                return true;
            }
            if (!enemy.Seen && enemy.TimeSinceEnemyCreated >= TimeBeforeSearch)
            {
                return true;
            }
            return false;
        }

        private float TimeBeforeSearch => SAIN.Info.TimeBeforeSearch;

        public bool StartHoldInCover()
        {
            var cover = SAIN.Cover.CoverInUse;
            if (cover != null && !cover.Spotted && (cover.Position - BotOwner.Position).sqrMagnitude < 1f)
            {
                return true;
            }
            return false;
        }

        private bool StartStandAndShoot(SAINEnemy enemy)
        {
            if (enemy.IsVisible && enemy.CanShoot)
            {
                if (enemy.RealDistance > SAIN.Info.WeaponInfo.EffectiveWeaponDistance * 1.5f)
                {
                    return false;
                }
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

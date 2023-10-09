using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.SAINComponent;
using System;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Decision
{
    public class EnemyDecisionClass : SAINBase, ISAINClass
    {
        public EnemyDecisionClass(SAINComponentClass sain) : base(sain)
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

        public bool GetDecision(out SoloDecision Decision)
        {
            SAINEnemyClass enemy = SAIN.Enemy;
            if (enemy == null)
            {
                Decision = SoloDecision.None;
                return false;
            }

            var CurrentDecision = SAIN.Memory.Decisions.Main.Current;

            if (StartDogFightAction(enemy))
            {
                Decision = SoloDecision.DogFight;
            }
            else if (StartThrowGrenade(enemy))
            {
                Decision = SoloDecision.ThrowGrenade;
            }
            else if (StartMoveToEngage(enemy))
            {
                Decision = SoloDecision.MoveToEngage;
            }
            else if (StartStandAndShoot(enemy))
            {
                if (CurrentDecision != SoloDecision.StandAndShoot)
                {
                    SAIN.Info.CalcHoldGroundDelay();
                }
                Decision = SoloDecision.StandAndShoot;
            }
            else if (StartRushEnemy(enemy))
            {
                Decision = SoloDecision.RushEnemy;
            }
            else if (StartSearch(enemy))
            {
                if (CurrentDecision != SoloDecision.Search)
                {
                    SAIN.Info.CalcTimeBeforeSearch();
                }
                Decision = SoloDecision.Search;
            }
            else if (StartShiftCover(enemy))
            {
                Decision = SoloDecision.ShiftCover;
            }
            else if (StartHoldInCover())
            {
                Decision = SoloDecision.HoldInCover;
            }
            else if (StartMoveToCover())
            {
                Decision = SoloDecision.WalkToCover;

                if (StartRunForCover())
                {
                    Decision = SoloDecision.RunToCover;
                }
            }
            else
            {
                Decision = SoloDecision.DogFight;
            }

            if (Decision != SoloDecision.WalkToCover && Decision != SoloDecision.RunToCover)
            {
                StartRunCoverTimer = 0f;
            }

            return true;
        }

        private bool StartThrowGrenade(SAINEnemyClass enemy)
        {
            if (!GlobalSettings.General.BotsUseGrenades)
            {
                var core = BotOwner.Settings.FileSettings.Core;
                if (core.CanGrenade)
                {
                    core.CanGrenade = false;
                }
                return false;
            }

            var grenades = BotOwner.WeaponManager.Grenades;
            if (!grenades.HaveGrenade)
            {
                return false;
            }
            if (!enemy.IsVisible && enemy.TimeSinceSeen > SAIN.Info.FileSettings.Grenade.TimeSinceSeenBeforeThrow && enemy.RealDistance < 100f)
            {
                if (grenades.ReadyToThrow && grenades.AIGreanageThrowData.IsUpToDate())
                {
                    grenades.DoThrow();
                    return true;
                }
                grenades.CanThrowGrenade(enemy.EnemyPosition + Vector3.up);
                return false;
            }
            return false;
        }

        private bool StartRushEnemy(SAINEnemyClass enemy)
        {
            if (SAIN.Info.PersonalitySettings?.CanRushEnemyReloadHeal == true)
            {
                if (enemy != null && enemy.PathDistance < 30f)
                {
                    if (!SAIN.Decision.SelfActionDecisions.LowOnAmmo(0.5f) && SAIN.Memory.HealthStatus != ETagStatus.Dying && BotOwner?.CanSprintPlayer == true)
                    {
                        var enemyStatus = enemy.EnemyStatus;
                        if (enemyStatus.EnemyIsReloading || enemyStatus.EnemyIsHealing || enemyStatus.EnemyHasGrenadeOut)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool StartShiftCover(SAINEnemyClass enemy)
        {
            if (ContinueShiftCover())
            {
                return true;
            }
            var CurrentDecision = SAIN.Memory.Decisions.Main.Current;

            if (CurrentDecision == SoloDecision.HoldInCover)
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
            var CurrentDecision = SAIN.Memory.Decisions.Main.Current;
            if (CurrentDecision == SoloDecision.ShiftCover)
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

        private bool StartDogFightAction(SAINEnemyClass enemy)
        {
            var currentSolo = SAIN.Decision.CurrentSoloDecision;
            if (Time.time - SAIN.Cover.LastHitTime < 2f 
                && currentSolo != SoloDecision.RunAway 
                && currentSolo != SoloDecision.RunToCover
                && currentSolo != SoloDecision.Retreat
                && currentSolo != SoloDecision.WalkToCover)
            {
                return true;
            }
                var pathStatus = enemy.CheckPathDistance();
            return (pathStatus == EnemyPathDistance.VeryClose && SAIN.Enemy.IsVisible) || SAIN.Cover.CoverInUse?.Spotted == true;
        }

        private bool StartThrowNade(SAINEnemyClass enemy)
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
                //if (SAIN.Grenade.EFTBotGrenade.CanThrowGrenade(enemy.EnemyPosition))
                //{
                //    EndThrowTimer = Time.time;
                //    return true;
                //}
            }

            return false;
        }

        private bool StartMoveToEngage(SAINEnemyClass enemy)
        {
            if (!enemy.Seen)
            {
                return false;
            }
            if (enemy.IsVisible && enemy.EnemyLookingAtMe)
            {
                return false;
            }
            if (BotOwner.Memory.IsUnderFire || Time.time - BotOwner.Memory.UnderFireTime < TimeBeforeSearch * 0.25f)
            {
                return false;
            }
            if (SAIN.Decision.CurrentSoloDecision == SoloDecision.HoldInCover)
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
            return false;
            //if (SAIN.Grenade.EFTBotGrenade.AIGreanageThrowData == null || Time.time - EndThrowTimer > 3f)
            //{
            //    return false;
            //}
            //return CurrentDecision == SoloDecision.ThrowGrenadeAction && SAIN.Grenade.EFTBotGrenade.AIGreanageThrowData?.ThrowComplete == false;
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
                var CurrentDecision = SAIN.Memory.Decisions.Main.Current;
                if (CurrentDecision != SoloDecision.WalkToCover && CurrentDecision != SoloDecision.RunToCover)
                {
                    StartRunCoverTimer = Time.time + 3f * UnityEngine.Random.Range(0.66f, 1.33f);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool StartAmbush()
        {
            bool startCheck = false;
            if (SAIN.Info.PersonalitySettings.Sneaky)
            {
                startCheck = true;
            }
            else if (AmbushCheckTimer < Time.time)
            {
                AmbushCheckTimer = Time.time + 5f;
                startCheck = EFTMath.RandomBool(25);
            }

            if (startCheck)
            {

            }

            return false;
        }

        private float AmbushCheckTimer;

        private bool StartSearch(SAINEnemyClass enemy)
        {
            if (enemy.IsVisible == true)
            {
                return false;
            }
            if (BotOwner.Memory.IsUnderFire || Time.time - BotOwner.Memory.UnderFireTime < TimeBeforeSearch * 0.33f)
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

        private bool StartStandAndShoot(SAINEnemyClass enemy)
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

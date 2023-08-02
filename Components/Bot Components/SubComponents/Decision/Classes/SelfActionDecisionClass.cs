using BepInEx.Logging;
using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Classes
{
    public class SelfActionDecisionClass : SAINBot
    {
        public SelfActionDecisionClass(SAINComponent bot) : base(bot) { }

        public SAINSelfDecision CurrentSelfAction => SAIN.Decision.CurrentSelfDecision;
        private SAINEnemyPathEnum EnemyDistance => SAIN.Decision.EnemyDistance;

        public bool GetDecision(out SAINSelfDecision Decision)
        {
            if ( SAIN.Enemy == null && !BotOwner.Medecine.Using && LowOnAmmo(0.75f) )
            {
                SAIN.SelfActions.TryReload();
                Decision = SAINSelfDecision.None;
                return false;
            }

            if (!CheckContinueSelfAction(out Decision))
            {
                if (StartRunGrenade())
                {
                    Decision = SAINSelfDecision.RunAwayGrenade;
                }
                else if (StartBotReload())
                {
                    Decision = SAINSelfDecision.Reload;
                }
                else
                {
                    if (LastHealCheckTime < Time.time && !SAIN.Healthy)
                    {
                        LastHealCheckTime = Time.time + 1f;
                        if (StartUseStims())
                        {
                            Decision = SAINSelfDecision.Stims;
                        }
                        else if (StartSurgery())
                        {
                            Decision = SAINSelfDecision.Surgery;
                        }
                        else if (StartFirstAid())
                        {
                            Decision = SAINSelfDecision.FirstAid;
                        }
                    }
                }
            }

            return Decision != SAINSelfDecision.None;
        }

        private float LastHealCheckTime;

        private bool StartRunGrenade()
        {
            var grenadePos = SAIN.Grenade.GrenadeDangerPoint; 
            if (grenadePos != null)
            {
                Vector3 botPos = BotPosition;
                Vector3 direction = grenadePos.Value - botPos;
                if (!Physics.Raycast(SAIN.HeadPosition, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    return true;
                }
            }
            return false;
        }

        private void TryFixBusyHands()
        {
            if (BusyHandsTimer > Time.time)
            {
                return;
            }
            BusyHandsTimer = Time.time + 1f;
            var selector = BotOwner.WeaponManager?.Selector;
            if (selector == null)
            {
                return;
            }
            if (selector.TryChangeWeapon(true))
            {
                return;
            }
            if (selector.TakePrevWeapon())
            {
                return;
            }
            if (selector.TryChangeToMain())
            {
                return;
            }
            if (selector.CanChangeToSecondWeapons)
            {
                selector.ChangeToSecond();
                return;
            }
        }

        private float BusyHandsTimer;

        private bool CheckContinueSelfAction(out SAINSelfDecision Decision)
        {
            if (CurrentSelfAction != SAINSelfDecision.None)
            {
                float timesinceChange = Time.time - SAIN.Decision.ChangeDecisionTime;
                if (timesinceChange > 5f)
                {
                    Decision = SAINSelfDecision.None;
                    if (CurrentSelfAction != SAINSelfDecision.Surgery)
                    {
                        TryFixBusyHands();
                        return false;
                    }
                    else if (timesinceChange > 30f)
                    {
                        TryFixBusyHands();
                        return false;
                    }
                }
            }
            bool continueAction = UsingMeds || ContinueReload || ContinueRunGrenade;
            Decision = continueAction ? CurrentSelfAction : SAINSelfDecision.None;
            return continueAction;
        }

        private bool ContinueRunGrenade => CurrentSelfAction == SAINSelfDecision.RunAwayGrenade && SAIN.Grenade.GrenadeDangerPoint != null;
        public bool UsingMeds => BotOwner.Medecine.Using;
        private bool ContinueReload => BotOwner.WeaponManager.Reload?.Reloading == true && !StartCancelReload();
        public bool CanUseStims
        {
            get
            {
                var stims = BotOwner.Medecine.Stimulators;
                return stims.HaveSmt && Time.time - stims.LastEndUseTime > 3f && stims.CanUseNow() && !SAIN.Healthy;
            }
        }
        public bool CanUseFirstAid => BotOwner.Medecine.FirstAid.ShallStartUse();
        public bool CanUseSurgery => BotOwner.Medecine.SurgicalKit.ShallStartUse();
        public bool CanReload => BotOwner.WeaponManager.IsReady && !BotOwner.WeaponManager.HaveBullets;

        private bool StartUseStims()
        {
            bool takeStims = false;
            if (CanUseStims)
            {
                var enemy = SAIN.Enemy;
                if (enemy == null)
                {
                    if (SAIN.Dying || SAIN.BadlyInjured)
                    {
                        takeStims = true;
                    }
                }
                else
                {
                    var pathStatus = EnemyDistance;
                    bool SeenRecent = enemy.TimeSinceSeen < 3f;
                    if (!enemy.InLineOfSight && !SeenRecent)
                    {
                        takeStims = true;
                    }
                    else if (pathStatus == SAINEnemyPathEnum.Far || pathStatus == SAINEnemyPathEnum.VeryFar)
                    {
                        takeStims = true;
                    }
                }
            }
            return takeStims;
        }

        private bool StartFirstAid()
        {
            bool useFirstAid = false;
            if (CanUseFirstAid)
            {
                var enemy = SAIN.Enemy;
                if (enemy == null)
                {
                    useFirstAid = true;
                }
                else
                {
                    var pathStatus = EnemyDistance;
                    bool SeenRecent = enemy.TimeSinceSeen < 4f;
                    var status = SAIN;
                    if (status.Injured)
                    {
                        if (!enemy.InLineOfSight && !SeenRecent && pathStatus != SAINEnemyPathEnum.VeryClose && pathStatus != SAINEnemyPathEnum.Close)
                        {
                            useFirstAid = true;
                        }
                    }
                    else if (status.BadlyInjured)
                    {
                        if (!enemy.InLineOfSight && pathStatus != SAINEnemyPathEnum.VeryClose && enemy.TimeSinceSeen < 2f)
                        {
                            useFirstAid = true;
                        }

                        if (pathStatus == SAINEnemyPathEnum.VeryFar)
                        {
                            useFirstAid = true;
                        }
                    }
                    else if (status.Dying)
                    {
                        if (!enemy.InLineOfSight && enemy.TimeSinceSeen < 1f)
                        {
                            useFirstAid = true;
                        }
                        if (pathStatus == SAINEnemyPathEnum.VeryFar || pathStatus == SAINEnemyPathEnum.Far)
                        {
                            useFirstAid = true;
                        }
                    }
                }
            }

            return useFirstAid;
        }

        public bool StartCancelReload()
        {
            if (!BotOwner.WeaponManager?.IsReady == true || BotOwner.WeaponManager.Reload.BulletCount == 0 || BotOwner.WeaponManager.CurrentWeapon.ReloadMode == EFT.InventoryLogic.Weapon.EReloadMode.ExternalMagazine)
            {
                return false;
            }

            var enemy = SAIN.Enemy;
            if (enemy != null && BotOwner.WeaponManager.Reload.Reloading && SAIN.Enemy != null)
            {
                var pathStatus = enemy.CheckPathDistance();
                bool SeenRecent = Time.time - enemy.TimeSinceSeen > 3f;

                if (SeenRecent && Vector3.Distance(BotOwner.Position, enemy.Person.Position) < 8f)
                {
                    return true;
                }

                if (!LowOnAmmo(0.15f) && enemy.IsVisible)
                {
                    return true;
                }
                if (pathStatus == SAINEnemyPathEnum.VeryClose)
                {
                    return true;
                }
                if (BotOwner.WeaponManager.Reload.BulletCount > 1 && pathStatus == SAINEnemyPathEnum.Close)
                {
                    return true;
                }
            }

            return false;
        }

        private bool StartBotReload()
        {
            bool needToReload = false;
            if (CanReload)
            {
                if (BotOwner.WeaponManager.Reload.BulletCount == 0)
                {
                    needToReload = true;
                }
                else if (LowOnAmmo())
                {
                    var enemy = SAIN.Enemy;
                    if (enemy == null)
                    {
                        needToReload = true;
                    }
                    else if (enemy.TimeSinceSeen > 3f)
                    {
                        needToReload = true;
                    }
                    else if (EnemyDistance != SAINEnemyPathEnum.VeryClose && !enemy.IsVisible)
                    {
                        needToReload = true;
                    }
                }
            }

            return needToReload;
        }

        private bool StartSurgery()
        {
            bool useSurgery = false;

            if (CanUseSurgery)
            {
                var enemy = SAIN.Enemy;
                if (enemy == null)
                {
                    useSurgery = true;
                }
                else
                {
                    var pathStatus = enemy.CheckPathDistance();
                    bool SeenRecent = enemy.TimeSinceSeen < 10f;

                    if (!SeenRecent && pathStatus != SAINEnemyPathEnum.VeryClose && pathStatus != SAINEnemyPathEnum.Close)
                    {
                        useSurgery = true;
                    }
                }
            }

            return useSurgery;
        }

        public bool LowOnAmmo(float ratio = 0.3f)
        {
            int currentAmmo = BotOwner.WeaponManager.Reload.BulletCount;
            int maxAmmo = BotOwner.WeaponManager.Reload.MaxBulletCount;

            if (maxAmmo > 30)
            {

            }

            return AmmoRatio < ratio;
        }

        public float AmmoRatio
        {
            get
            {
                int currentAmmo = BotOwner.WeaponManager.Reload.BulletCount;
                int maxAmmo = BotOwner.WeaponManager.Reload.MaxBulletCount;
                return (float)currentAmmo / maxAmmo;
            }
        }
    }
}

using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.SAINComponent;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Decision
{
    public class SelfActionDecisionClass : SAINBase, ISAINClass
    {
        public SelfActionDecisionClass(SAINComponentClass sain) : base(sain)
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

        public SelfDecision CurrentSelfAction => SAIN.Decision.CurrentSelfDecision;
        private EnemyPathDistance EnemyDistance => SAIN.Decision.EnemyDistance;

        public bool GetDecision(out SelfDecision Decision)
        {
            if ( SAIN.Enemy == null && !BotOwner.Medecine.Using && LowOnAmmo(0.75f) )
            {
                SAIN.SelfActions.TryReload();
                Decision = SelfDecision.None;
                return false;
            }

            if (!CheckContinueSelfAction(out Decision))
            {
                if (StartRunGrenade())
                {
                    Decision = SelfDecision.RunAwayGrenade;
                }
                else if (StartBotReload())
                {
                    Decision = SelfDecision.Reload;
                }
                else
                {
                    if (LastHealCheckTime < Time.time && !SAIN.Memory.Healthy)
                    {
                        LastHealCheckTime = Time.time + 1f;
                        if (StartUseStims())
                        {
                            Decision = SelfDecision.Stims;
                        }
                        else if (StartSurgery())
                        {
                            Decision = SelfDecision.Surgery;
                        }
                        else if (StartFirstAid())
                        {
                            Decision = SelfDecision.FirstAid;
                        }
                    }
                }
            }

            return Decision != SelfDecision.None;
        }

        private float LastHealCheckTime;

        private bool StartRunGrenade()
        {
            var grenadePos = SAIN.Grenade.GrenadeDangerPoint; 
            if (grenadePos != null)
            {
                Vector3 headPos = SAIN.Transform.Head;
                Vector3 direction = grenadePos.Value - headPos;
                if (!Physics.Raycast(headPos, direction.normalized, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
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

        private bool CheckContinueSelfAction(out SelfDecision Decision)
        {
            if (CurrentSelfAction != SelfDecision.None)
            {
                float timesinceChange = Time.time - SAIN.Decision.ChangeDecisionTime;
                if (timesinceChange > 5f)
                {
                    Decision = SelfDecision.None;
                    if (CurrentSelfAction != SelfDecision.Surgery)
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
            Decision = continueAction ? CurrentSelfAction : SelfDecision.None;
            return continueAction;
        }

        private bool ContinueRunGrenade => CurrentSelfAction == SelfDecision.RunAwayGrenade && SAIN.Grenade.GrenadeDangerPoint != null;
        public bool UsingMeds => BotOwner.Medecine.Using;
        private bool ContinueReload => BotOwner.WeaponManager.Reload?.Reloading == true && !StartCancelReload();
        public bool CanUseStims
        {
            get
            {
                var stims = BotOwner.Medecine.Stimulators;
                return stims.HaveSmt && Time.time - stims.LastEndUseTime > 3f && stims.CanUseNow() && !SAIN.Memory.Healthy;
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
                    if (SAIN.Memory.Dying || SAIN.Memory.BadlyInjured)
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
                    else if (pathStatus == EnemyPathDistance.Far || pathStatus == EnemyPathDistance.VeryFar)
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
                    if (status.Memory.Injured)
                    {
                        if (!enemy.InLineOfSight && !SeenRecent && pathStatus != EnemyPathDistance.VeryClose && pathStatus != EnemyPathDistance.Close)
                        {
                            useFirstAid = true;
                        }
                    }
                    else if (status.Memory.BadlyInjured)
                    {
                        if (!enemy.InLineOfSight && pathStatus != EnemyPathDistance.VeryClose && enemy.TimeSinceSeen < 2f)
                        {
                            useFirstAid = true;
                        }

                        if (pathStatus == EnemyPathDistance.VeryFar)
                        {
                            useFirstAid = true;
                        }
                    }
                    else if (status.Memory.Dying)
                    {
                        if (!enemy.InLineOfSight && enemy.TimeSinceSeen < 1f)
                        {
                            useFirstAid = true;
                        }
                        if (pathStatus == EnemyPathDistance.VeryFar || pathStatus == EnemyPathDistance.Far)
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

                if (SeenRecent && Vector3.Distance(BotOwner.Position, enemy.EnemyIAIDetails.Position) < 8f)
                {
                    return true;
                }

                if (!LowOnAmmo(0.15f) && enemy.IsVisible)
                {
                    return true;
                }
                if (pathStatus == EnemyPathDistance.VeryClose)
                {
                    return true;
                }
                if (BotOwner.WeaponManager.Reload.BulletCount > 1 && pathStatus == EnemyPathDistance.Close)
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
                    else if (EnemyDistance != EnemyPathDistance.VeryClose && !enemy.IsVisible)
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

                    if (!SeenRecent && pathStatus != EnemyPathDistance.VeryClose && pathStatus != EnemyPathDistance.Close)
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

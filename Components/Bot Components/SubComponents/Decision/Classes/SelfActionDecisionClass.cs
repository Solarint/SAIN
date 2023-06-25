using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using EFT;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Classes
{
    public class SelfActionDecisionClass : SAINBot
    {
        public SelfActionDecisionClass(BotOwner bot) : base(bot) { }

        protected ManualLogSource Logger => SAIN.Decision.Logger;
        private bool DebugBotDecision => DebugBotDecisions.Value;
        public SAINSelfDecision CurrentSelfAction => SAIN.Decision.CurrentSelfDecision;
        private SAINEnemyPath EnemyDistance => SAIN.Decision.EnemyDistance;

        public bool GetDecision(out SAINSelfDecision Decision)
        {
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
            if (selector.TakePrevWeapon())
            {
                return;
            }
            if (selector.TryChangeToMain())
            {
                return;
            }
            if (selector.TryChangeWeapon(true))
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
                float timesinceChange = Time.time - SAIN.Decision.ChangeSelfDecisionTime;
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
                    else if (pathStatus == SAINEnemyPath.Far || pathStatus == SAINEnemyPath.VeryFar)
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
                        if (!enemy.InLineOfSight && !SeenRecent && pathStatus != SAINEnemyPath.VeryClose && pathStatus != SAINEnemyPath.Close)
                        {
                            useFirstAid = true;
                        }
                    }
                    else if (status.BadlyInjured)
                    {
                        if (!enemy.InLineOfSight && pathStatus != SAINEnemyPath.VeryClose && enemy.TimeSinceSeen < 2f)
                        {
                            useFirstAid = true;
                        }

                        if (pathStatus == SAINEnemyPath.VeryFar)
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
                        if (pathStatus == SAINEnemyPath.VeryFar || pathStatus == SAINEnemyPath.Far)
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
            if (!BotOwner.WeaponManager?.IsReady == true || !BotOwner.WeaponManager.HaveBullets || BotOwner.WeaponManager.CurrentWeapon.ReloadMode == EFT.InventoryLogic.Weapon.EReloadMode.ExternalMagazine)
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

                if (!CheckLowAmmo(0.15f) && enemy.IsVisible)
                {
                    return true;
                }
                if (pathStatus == SAINEnemyPath.VeryClose)
                {
                    return true;
                }
                if (BotOwner.WeaponManager.Reload.BulletCount > 1 && pathStatus == SAINEnemyPath.Close)
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
                else if (CheckLowAmmo())
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
                    else if (EnemyDistance != SAINEnemyPath.VeryClose && !enemy.IsVisible)
                    {
                        if (DebugBotDecision)
                        {
                            Logger.LogDebug($"I'm low on ammo, and I can't see my enemy and he isn't close, so I should reload.");
                        }

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

                    if (!SeenRecent && pathStatus != SAINEnemyPath.VeryClose && pathStatus != SAINEnemyPath.Close)
                    {
                        useSurgery = true;
                    }
                }
            }

            return useSurgery;
        }

        public bool CheckLowAmmo(float ratio = 0.3f)
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

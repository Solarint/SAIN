using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using SAIN.Classes;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Components
{
    public class DecisionLogic : SAINBotExt
    {
        public DecisionLogic(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource($": {BotOwner.name}" + GetType().Name);
        }

        private const float FightIn = 60f;
        private const float FightOut = 70f;

        private const float DogFightIn = 10f;
        private const float DogFightOut = 15f;

        private const float LowAmmoThresh0to1 = 0.3f;

        public void BotHeal()
        {
            Logger.LogDebug($"I healed!");

            BotOwner.Medecine.FirstAid.TryApplyToCurrentPart(null, null);
        }

        public void BotUseStims()
        {
            Logger.LogDebug($"I'm Popping Stims");

            BotOwner.Medecine.Stimulators.TryApply(true, null, null);
        }

        public void BotReload()
        {
            Logger.LogDebug($"Reloading!");

            BotOwner.WeaponManager.Reload.Reload();
        }

        public void BotStopReload()
        {
            if (DebugMode)
            {
                Logger.LogDebug($"I need to stop reloading!");
            }

            BotOwner.WeaponManager.Reload.TryStopReload();
        }

        public bool StartFight
        {
            get
            {
                bool active = FightActive;

                if (BotOwner.Memory.GoalEnemy == null)
                {
                    active = false;
                }
                else if (SAIN.Core.Enemy.LastSeen.EnemyStraightDistance < 100f)
                {
                    float distance = SAIN.Core.Enemy.Path.Length;

                    if (distance < FightIn)
                    {
                        active = true;
                    }
                    else if (distance > FightOut)
                    {
                        active = false;
                    }

                    if (DebugMode)
                    {
                        DebugDrawPath(distance, FightOut, active);
                    }
                }

                FightActive = active;
                return active;
            }
        }

        public bool StartDogFight
        {
            get
            {
                bool active = DogFightActive;

                if (BotOwner.Memory.GoalEnemy == null)
                {
                    active = false;
                }
                else if (SAIN.Core.Enemy.LastSeen.EnemyStraightDistance < 50f)
                {
                    float distance = SAIN.Core.Enemy.Path.Length;

                    if (distance < DogFightIn)
                    {
                        active = true;
                    }
                    else if (distance > DogFightOut)
                    {
                        active = false;
                    }

                    if (DebugMode)
                    {
                        DebugDrawPath(distance, DogFightOut, active);
                    }
                }

                DogFightActive = active;
                return active;
            }
        }

        public bool ShouldBotPopStims
        {
            get
            {
                bool takeStims = false;
                if (SAIN.Core.Medical.HasStims && LastStimTime < Time.time)
                {
                    var status = SAIN.Core.BotStatus;
                    if (status.Healthy)
                    {
                        takeStims = false;
                    }
                    else if (BotOwner.Memory.GoalEnemy == null)
                    {
                        if (status.Dying || status.BadlyInjured)
                        {
                            takeStims = true;
                        }
                    }
                    else
                    {
                        var enemy = SAIN.Core.Enemy;
                        var path = enemy.Path;

                        if (status.Injured)
                        {
                            if (!enemy.CanSee)
                            {
                                if (!path.RangeVeryClose && !path.RangeClose)
                                {
                                    takeStims = true;
                                }
                            }
                            else if (path.RangeFar)
                            {
                                takeStims = true;
                            }
                        }
                        else if (status.BadlyInjured)
                        {
                            if (!enemy.CanSee)
                            {
                                if (!path.RangeVeryClose)
                                {
                                    takeStims = true;
                                }
                            }
                            else if (path.RangeFar || path.RangeMid)
                            {
                                takeStims = true;
                            }
                        }
                        else if (status.Dying)
                        {
                            if (!enemy.CanSee)
                            {
                                takeStims = true;
                            }
                            else if (path.RangeFar || path.RangeMid || path.RangeClose)
                            {
                                takeStims = true;
                            }
                        }
                    }
                }

                if (takeStims)
                {
                    LastStimTime = Time.time + 30f;

                    if (BotOwner.Memory.GoalEnemy == null)
                    {
                        Logger.LogDebug($"Popped Stims Because: I have no enemy and I'm [{Classes.Debug.Reason(SAIN.Core.BotStatus)}]");
                    }
                    else
                    {
                        string healthReason = Classes.Debug.Reason(SAIN.Core.BotStatus);
                        string enemydistReason = Classes.Debug.Reason(SAIN.Core.Enemy);
                        string canSee = Classes.Debug.Reason(SAIN.Core.Enemy.CanSee);
                        Logger.LogDebug($"Popped Stims Because: I'm [{healthReason}] and my enemy is [{enemydistReason}] and I [{canSee}] them.");
                    }
                }

                return takeStims;
            }
        }

        public bool ShouldBotHeal
        {
            get
            {
                bool BotShouldHeal = false;
                var status = SAIN.Core.BotStatus;
                if (SAIN.Core.Medical.CanHeal && !status.Healthy)
                {
                    if (BotOwner.Memory.GoalEnemy == null)
                    {
                        BotShouldHeal = true;
                    }
                    else
                    {
                        var enemy = SAIN.Core.Enemy;
                        var path = enemy.Path;

                        if (status.Injured)
                        {
                            if (!enemy.CanSee && (path.RangeFar || path.RangeMid))
                            {
                                BotShouldHeal = true;
                            }
                        }
                        else if (status.BadlyInjured)
                        {
                            if (!enemy.CanSee)
                            {
                                if (!path.RangeVeryClose)
                                {
                                    BotShouldHeal = true;
                                }
                            }
                            else if (path.RangeFar)
                            {
                                BotShouldHeal = true;
                            }
                        }
                        else if (status.Dying)
                        {
                            if (!enemy.CanSee)
                            {
                                BotShouldHeal = true;
                            }
                            else if (path.RangeFar || path.RangeMid)
                            {
                                BotShouldHeal = true;
                            }
                        }
                    }
                }

                if (BotShouldHeal)
                {
                    if (BotOwner.Memory.GoalEnemy == null)
                    {
                        Logger.LogDebug($"Healed Because: I have no enemy");
                    }
                    else
                    {
                        string healthReason = Classes.Debug.Reason(SAIN.Core.BotStatus);
                        string enemydistReason = Classes.Debug.Reason(SAIN.Core.Enemy);
                        string canSee = Classes.Debug.Reason(SAIN.Core.Enemy.CanSee);
                        Logger.LogDebug($"Healed Because: I'm [{healthReason}] and my enemy is [{enemydistReason}] and I [{canSee}] them.");
                    }
                }

                return BotShouldHeal;
            }
        }

        public bool ShouldBotCancelReload
        {
            get
            {
                bool botShouldCancel = false;

                if (BotOwner.Memory.GoalEnemy != null && BotOwner.WeaponManager.Reload.Reloading)
                {
                    if (!LowAmmo && SAIN.Core.Enemy.Path.RangeClose)
                    {
                        if (DebugMode)
                        {
                            Logger.LogDebug($"My Enemy is Close, and I have ammo!");
                        }

                        botShouldCancel = true;
                    }
                    if (BotOwner.WeaponManager.HaveBullets && SAIN.Core.Enemy.Path.RangeVeryClose)
                    {
                        if (DebugMode)
                        {
                            Logger.LogDebug($"My Enemy is Very Close And I have [{AmmoRatio * 100f}] percent of my capacity.");
                        }

                        botShouldCancel = true;
                    }
                }

                return botShouldCancel;
            }
        }

        public bool ShouldBotReload
        {
            get
            {
                if (!BotOwner.WeaponManager.IsReady || !BotOwner.WeaponManager.Reload.CanReload(false))
                {
                    return false;
                }

                bool needToReload = false;

                if (!BotOwner.WeaponManager.Reload.Reloading)
                {
                    if (!BotOwner.WeaponManager.HaveBullets)
                    {
                        if (DebugMode)
                        {
                            Logger.LogDebug($"I'm Empty! Need to reload!");
                        }

                        needToReload = true;
                    }
                    else if (LowAmmo)
                    {
                        float randomrange = Random.Range(2f, 5f);
                        var enemy = SAIN.Core.Enemy;
                        if (BotOwner.Memory.GoalEnemy == null)
                        {
                            if (DebugMode)
                            {
                                Logger.LogDebug($"I'm low on ammo, and I have no enemy, so I should reload");
                            }

                            needToReload = true;
                        }
                        else if (enemy.LastSeen.TimeSinceSeen > randomrange)
                        {
                            if (DebugMode)
                            {
                                Logger.LogDebug($"I'm low on ammo, and I haven't seen my enemy in [{randomrange}] seconds. so I should reload. Last Saw Enemy [{SAIN.Core.Enemy.LastSeen.TimeSinceSeen}] seconds ago");
                            }

                            needToReload = true;
                        }
                        else if (!enemy.Path.RangeClose && !enemy.CanSee)
                        {
                            if (DebugMode)
                            {
                                Logger.LogDebug($"I'm low on ammo, and I can't see my enemy and he isn't close, so I should reload.");
                            }

                            needToReload = true;
                        }
                    }
                }
                if (needToReload)
                {
                    BotReload();
                }
                return needToReload;
            }
        }

        public bool LowAmmo => AmmoRatio < LowAmmoThresh0to1;

        public float AmmoRatio
        {
            get
            {
                int currentAmmo = BotOwner.WeaponManager.Reload.BulletCount;
                int maxAmmo = BotOwner.WeaponManager.Reload.MaxBulletCount;
                return (float)currentAmmo / maxAmmo;
            }
        }

        private bool DebugMode => DebugBotDecisions.Value;

        private bool FightActive = false;
        private bool DogFightActive = false;
        private float DebugTimer = 0f;

        protected ManualLogSource Logger;

        private float LastStimTime = 0f;

        private void DebugDrawPath(float enemyDistance, float minDistance, bool active)
        {
            if (DebugMode && DebugTimer < Time.time && enemyDistance < minDistance)
            {
                DebugTimer = Time.time + 1f;
                SAIN.DebugDrawList.DrawTempPath(SAIN.Core.Enemy.Path.Path, active, Color.red, Color.green, 0.1f, 1f, true);
            }
        }
    }
}
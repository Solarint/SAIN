using BepInEx.Logging;
using EFT;
using UnityEngine;
using static Movement.UserSettings.Debug;

namespace Movement.Components
{
    public class BotDecision : MonoBehaviour
    {
        private readonly BotOwner bot;

        public bool BotNeedsToHeal { get; private set; }
        public bool BotNeedsToReload { get; private set; }
        public bool BotShouldAttack { get; private set; }
        public bool BotShouldCancelReload { get; private set; }

        public BotDecision(BotOwner botOwner_0)
        {
            bot = botOwner_0;
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(BotDecision) + $": {bot.name}");
        }

        protected ManualLogSource Logger;
        private float DecisionTimer = 0f;

        public void GetDecision()
        {
            if (DecisionTimer < Time.time)
            {
                DecisionTimer = Time.time + 0.33f;

                if (ShouldBotReload())
                {
                    return;
                }
                if (ShouldBotPopStims())
                {
                    return;
                }
                if (ShouldBotHeal())
                {
                    return;
                }
            }
        }

        private bool ShouldBotPopStims()
        {
            bool stimsTaken = false;
            bool botCanTake = true;
            if (bot.Medecine.Using || bot.WeaponManager.Reload.Reloading || EnemyClose)
            {
                botCanTake = false;
            }

            if (botCanTake && !BotHasStims)
            {
                if (StimulatorRefreshTime < Time.time)
                {
                    StimulatorRefreshTime = Time.time + 3f;
                    bot.Medecine.Stimulators.Refresh();
                }
            }

            if (botCanTake && BotHasStims && LastStimTime < Time.time)
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"Popping Stims");
                }

                LastStimTime = Time.time + 20f;
                bot.Medecine.Stimulators.TryApply(true, null, null);
                stimsTaken = true;
            }

            return stimsTaken;
        }

        private bool BotHasStims => bot.Medecine.Stimulators.HaveSmt;
        private float StimulatorRefreshTime = 0f;
        private float LastStimTime = 0f;

        private bool ShouldBotHeal()
        {
            bool BotNeedsToHeal = bot.Medecine.FirstAid.ShallStartUse();
            bool BotCanHeal = true;
            if (BotNeedsToHeal)
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"I Need to Heal!");
                }

                if (bot.GetPlayer.HealthStatus != ETagStatus.BadlyInjured && EnemyClose)
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"But an enemy is infront of me, and I'm not bleeding.");
                    }
                    BotCanHeal = false;
                }
                else if (bot.WeaponManager.Reload.Reloading)
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"But I'm reloading");
                    }
                    BotCanHeal = false;
                }
            }

            bool hasHealed = false;
            if (BotCanHeal && BotNeedsToHeal)
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"So I healed!");
                }
                bot.Medecine.FirstAid.TryApplyToCurrentPart(null, null);
                hasHealed = true;
            }

            return hasHealed;
        }

        private bool ShouldBotCancelReload()
        {
            bool botShouldCancel = false;
            if (bot.Memory.GoalEnemy != null && bot.WeaponManager.IsReady)
            {
                if (bot.WeaponManager.Reload.Reloading && !LowAmmo() && EnemyClose)
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"I need to cancel my reload!");
                    }

                    botShouldCancel = true;
                    bot.WeaponManager.Reload.TryStopReload();
                }
            }

            return botShouldCancel;
        }

        private bool ShouldBotReload()
        {
            if (!bot.WeaponManager.IsReady)
            {
                return false;
            }

            bool needToReload = false;

            if (bot.WeaponManager.Reload.Reloading)
            {
                needToReload = false;
            }
            else if (!bot.WeaponManager.HaveBullets)
            {
                if (DebugMode) 
                {
                    Logger.LogDebug($"I'm Empty! Need to reload!");
                }

                needToReload = true;
            }
            else if (LowAmmo())
            {
                if (bot.Memory.GoalEnemy == null)
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"I'm low on ammo, and I have no enemy, so I should reload");
                    }

                    needToReload = true;
                }
                else if (bot.Memory.GoalEnemy.PersonalSeenTime < Time.time - 2f)
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"I'm low on ammo, and I haven't seen my enemy in a few seconds, so I should reload. Last Seen time = [{bot.Memory.GoalEnemy.PersonalSeenTime - Time.time}]");
                    }

                    needToReload = true;
                }
                else if (bot.Memory.GoalEnemy.Distance > 15f && !bot.Memory.GoalEnemy.CanShoot)
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"I'm low on ammo, and I can't see my enemy and he isn't close, so I should reload. Enemy Distance = [{bot.Memory.GoalEnemy.Distance}]");
                    }

                    needToReload = true;
                }
            }

            if (needToReload)
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"Reloading!");
                }

                bot.WeaponManager.Reload.TryReload();
            }

            return needToReload;
        }

        private bool ShouldBotAttack()
        {
            return false;
        }

        bool EnemyClose => Vector3.Distance(bot.Memory.GoalEnemy.CurrPosition, bot.Transform.position) < 10f;

        private bool LowAmmo()
        {
            int currentAmmo = bot.WeaponManager.Reload.BulletCount;
            int maxAmmo = bot.WeaponManager.Reload.MaxBulletCount;
            float ammoRatio = (float)currentAmmo / maxAmmo;
            bool lowAmmo = ammoRatio < 0.3f;
            return lowAmmo;
        }

        private bool DebugMode => DebugBotDecisions.Value;
    }
}
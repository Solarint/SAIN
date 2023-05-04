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
                DecisionTimer = Time.time + 0.5f;

                if (ShouldBotReload())
                {
                    return;
                }
                else if (ShouldBotPopStims())
                {
                    return;
                }
                else if (ShouldBotHeal())
                {
                    return;
                }
            }
        }

        private bool ShouldBotPopStims()
        {
            if (bot.Medecine.Using || bot.Medecine.Stimulators == null || bot.WeaponManager.Reload.Reloading)
            {
                return false;
            }

            if (!BotHasStims)
            {
                if (StimulatorRefreshTime < Time.time)
                {
                    StimulatorRefreshTime = Time.time + 10f;
                    bot.Medecine.Stimulators.Refresh();
                }
                if (!BotHasStims)
                {
                    return false;
                }
            }

            if (DebugMode) Logger.LogDebug($"Popping Stims");

            bot.Medecine.Stimulators.TryApply(true, null, null);

            return false;
        }

        private bool BotHasStims => bot.Medecine.Stimulators.HaveSmt;
        private float StimulatorRefreshTime = 0f;

        private bool ShouldBotHeal()
        {
            if (bot.Medecine.FirstAid.ShallStartUse())
            {
                if (DebugMode) Logger.LogDebug($"I Need to Heal!");
                BotNeedsToHeal = true;

                if (bot.WeaponManager.Reload.Reloading)
                {
                    if (DebugMode) Logger.LogDebug($"But I'm reloading");
                    return false;
                }
                else
                {
                    if (DebugMode) Logger.LogDebug($"So I healed!");
                    bot.Medecine.FirstAid.TryApplyToCurrentPart(null, null);
                    return true;
                }
            }
            else
            {
                BotNeedsToHeal = false;
                return false;
            }
        }

        private bool ShouldBotCancelReload()
        {
            if (bot?.Memory?.GoalEnemy == null || bot?.WeaponManager?.CurrentWeapon == null)
            {
                BotShouldCancelReload = false;
                return false;
            }

            if (bot.WeaponManager.Reload.Reloading && !LowAmmo() && bot.Memory.GoalEnemy.CanShoot)
            {
                if (DebugMode) Logger.LogDebug($"I need to cancel my reload!");

                BotShouldCancelReload = true;

                bot.WeaponManager.Reload.TryStopReload();

                return true;
            }
            else
            {
                BotShouldCancelReload = false;
                return false;
            }
        }

        private bool ShouldBotReload()
        {
            if (bot?.WeaponManager?.Reload == null || bot?.WeaponManager?.CurrentWeapon == null)
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
                if (DebugMode) Logger.LogDebug($"I'm Empty! Need to reload!");
                needToReload = true;
            }
            else if (LowAmmo())
            {
                if (bot.Memory.GoalEnemy == null)
                {
                    if (DebugMode) Logger.LogDebug($"I'm low on ammo, and I have no enemy, so I should reload");
                    needToReload = true;
                }
                else if (bot.Memory.GoalEnemy.PersonalSeenTime < Time.time - 2f)
                {
                    if (DebugMode) Logger.LogDebug($"I'm low on ammo, and I haven't seen my enemy in a few seconds, so I should reload. Last Seen time = [{bot.Memory.GoalEnemy.PersonalSeenTime - Time.time}]");
                    needToReload = true;
                }
                else if (bot.Memory.GoalEnemy.Distance > 15f && !bot.Memory.GoalEnemy.CanShoot)
                {
                    if (DebugMode) Logger.LogDebug($"I'm low on ammo, and I can't see my enemy and he isn't close, so I should reload. Enemy Distance = [{bot.Memory.GoalEnemy.Distance}]");
                    needToReload = true;
                }
            }

            if (needToReload)
            {
                if (DebugMode) Logger.LogDebug($"Reloading!");
                bot.WeaponManager.Reload.TryReload();
                return true;
            }

            return false;
        }

        private bool ShouldBotAttack()
        {
            return false;
        }

        private bool LowAmmo()
        {
            int currentAmmo = bot.WeaponManager.Reload.BulletCount;
            int maxAmmo = bot.WeaponManager.Reload.MaxBulletCount;

            float ammoRatio = (float)currentAmmo / maxAmmo; // Cast one of the integers to a float before dividing
            bool lowAmmo = ammoRatio < 0.3f;

            //Logger.LogDebug($"Ammo: [{currentAmmo}] MaxAmmo: [{maxAmmo}] Ratio: [{ammoRatio}] Low Ammo? {lowAmmo}");

            return lowAmmo;
        }

        private bool DebugMode => DebugBotDecisions.Value;
    }
}
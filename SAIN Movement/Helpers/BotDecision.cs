using BepInEx.Logging;
using EFT;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static SAIN_Helpers.SAIN_Math;
using Movement.Helpers;
using SAIN_Helpers;

namespace Movement.Components
{
    public class BotDecision : MonoBehaviour
    {
        private readonly BotOwner bot;
        public BotDecision(BotOwner botOwner_0)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(BotDecision));
            bot = botOwner_0;
        }

        protected static ManualLogSource Logger { get; private set; }

        public bool GetDecision()
        {
            if (Execute.CancelReload())
            {
                return true;
            }
            if (Execute.Reload())
            {
                return true;
            }
            if (Execute.Heal())
            {
                return true;
            }
            return false;
        }

        public DoDecision Execute { get; private set; }

        public class DoDecision
        {
            public DoDecision(BotOwner bot)
            {
                this.bot = bot;
                ShouldI = new ShouldBot(bot);
            }

            private readonly BotOwner bot;
            public ShouldBot ShouldI { get; private set; }

            public bool Heal()
            {
                if (bot.Medecine.Using)
                {
                    return true;
                }

                if (ShouldI.Heal())
                {
                    bot.Medecine.RefreshCurMeds();
                    if (bot.Medecine.FirstAid.ShallStartUse())
                    {
                        bot.Medecine.FirstAid.TryApplyToCurrentPart(null, null);
                        Logger.LogDebug($"Healing");
                        return true;
                    }
                }

                return false;
            }

            public bool CancelReload()
            {
                if (!bot.WeaponManager.Reload.Reloading)
                {
                    return false;
                }

                if (ShouldI.CancelReload())
                {
                    bot.WeaponManager.Reload.TryStopReload();
                    Logger.LogDebug($"Stopped reload to shoot");
                    return true;
                }
                return false;
            }

            public bool Reload()
            {
                if (bot.WeaponManager.Reload.Reloading)
                {
                    return true;
                }

                if (ShouldI.Reload())
                {
                    Logger.LogDebug($"Reloading");
                    bot.WeaponManager.Reload.TryReload();
                    return true;
                }
                return false;
            }
        }

        public class ShouldBot
        {
            public ShouldBot(BotOwner bot)
            {
                this.bot = bot;
            }

            private readonly BotOwner bot;

            public bool Heal()
            {
                if (bot.WeaponManager.Reload.Reloading)
                {
                    return false;
                }

                if (bot?.Medecine?.FirstAid != null)
                {
                    if (bot.Medecine.FirstAid.IsBleeding)
                    {
                        if (bot.Medecine.FirstAid.HaveSmth2Use)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public bool Reload()
            {
                if (bot?.WeaponManager?.Reload == null || bot?.WeaponManager?.CurrentWeapon == null)
                {
                    return false;
                }

                if (!bot.WeaponManager.Reload.Reloading && bot.WeaponManager.Reload.CanReload(false))
                {
                    if (!bot.WeaponManager.HaveBullets)
                    {
                        return true;
                    }

                    int currentAmmo = bot.WeaponManager.Reload.BulletCount;
                    int maxAmmo = bot.WeaponManager.Reload.MaxBulletCount;
                    float ammoRatio = (float)currentAmmo / maxAmmo; // Cast one of the integers to a float before dividing
                    bool lowAmmo = ammoRatio < 0.3f;

                    if (lowAmmo && bot.Memory.GoalEnemy != null)
                    {
                        if (bot.Memory.GoalEnemy.PersonalLastSeenTime + 2f < Time.time)
                        {
                            Logger.LogDebug($"Ammo: [{currentAmmo}] MaxAmmo: [{maxAmmo}] Ratio: [{ammoRatio}]");
                            return true;
                        }
                    }
                }
                return false;
            }

            public bool CancelReload()
            {
                if (bot?.Memory?.GoalEnemy == null || bot?.WeaponManager?.CurrentWeapon == null)
                {
                    return false;
                }

                if (bot.WeaponManager.Reload.Reloading)
                {
                    if (bot.WeaponManager.HaveBullets)
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool Attack()
            {
                return false;
            }
        }
    }
}
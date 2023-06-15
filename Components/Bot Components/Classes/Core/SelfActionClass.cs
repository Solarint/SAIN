using BepInEx.Logging;
using EFT;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Classes
{
    public class SelfActionClass : SAINBot
    {
        public SelfActionClass(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            BotOwner.Medecine.RefreshCurMeds();
        }

        private bool WasUsingMeds = false;
        private bool UsingMeds => BotOwner.Medecine.Using;

        public void Update()
        {
            if (!UsingMeds)
            {
                if (WasUsingMeds)
                {
                    BotOwner.Medecine.RefreshCurMeds();
                }
                if (SAIN.Decision.SelfActionDecisions.StartCancelReload())
                {
                    BotCancelReload();
                }
                else
                {
                    switch (SelfDecision)
                    {
                        case SAINSelfDecision.Reload:
                            TryReload();
                            break;

                        case SAINSelfDecision.Surgery:
                            DoSurgery();
                            break;

                        case SAINSelfDecision.FirstAid:
                            DoFirstAid();
                            break;

                        case SAINSelfDecision.Stims:
                            DoStims();
                            break;

                        default:
                            break;
                    }
                }
            }

            WasUsingMeds = UsingMeds;
        }

        public void DoFirstAid()
        {
            var heal = BotOwner.Medecine.FirstAid;
            if (HealTimer < Time.time && heal.ShallStartUse())
            {
                HealTimer = Time.time + 5f;
                if (DebugBotDecisions.Value)
                {
                    Logger.LogDebug($"I healed!");
                }
                heal.TryApplyToCurrentPart();
            }
        }

        public void DoSurgery()
        {
            var surgery = BotOwner.Medecine.SurgicalKit;
            if (HealTimer < Time.time && surgery.ShallStartUse())
            {
                HealTimer = Time.time + 5f;
                if (DebugBotDecisions.Value)
                {
                    Logger.LogDebug($"Used Surgery");
                }
                surgery.ApplyToCurrentPart();
            }
        }

        public void DoStims()
        {
            var stims = BotOwner.Medecine.Stimulators;
            if (StimTimer < Time.time && stims.CanUseNow())
            {
                StimTimer = Time.time + 5f;
                if (DebugBotDecisions.Value)
                {
                    Logger.LogDebug($"I'm Popping Stims");
                }

                try { stims.TryApply(); }
                catch { }
            }
        }

        public void TryReload()
        {
            var reloadClass = BotOwner.WeaponManager.Reload;
            if (reloadClass.Reloading)
            {
                reloadClass.CheckReloadLongTime();
                return;
            }
            if (reloadClass.CanReload(false))
            {
                if (DebugBotDecisions.Value)
                {
                    Logger.LogDebug($"Reloading!");
                }
                reloadClass.Reload();
                return;
            }
            if (reloadClass.NoAmmoForReloadCached)
            {
                BotOwner.WeaponManager.Selector.TryChangeWeapon(true);
            }
        }

        public void BotCancelReload()
        {
            if (BotOwner.WeaponManager.Reload.Reloading)
            {
                BotOwner.WeaponManager.Reload.TryStopReload();
            }
        }

        protected ManualLogSource Logger;

        private float StimTimer = 0f;
        private float HealTimer = 0f;
    }
}

using BepInEx.Logging;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Classes
{
    public class SelfActionClass : MonoBehaviour
    {
        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN.BotOwner;

        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            BotOwner.Medecine.RefreshCurMeds();
        }

        private SAINSelfDecision SelfDecision => SAIN.Decision.CurrentSelfDecision;

        private bool WasUsingMeds = false;
        private bool UsingMeds => BotOwner.Medecine.Using;

        private void Update()
        {
            if (BotOwner == null || SAIN == null) return;
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
                WasUsingMeds = UsingMeds;
            }
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
                surgery.ApplyToCurrentPart();
            }
        }

        public void DoStims()
        {
            var stims = BotOwner.Medecine.Stimulators;
            if (StimTimer < Time.time && stims.CanUseNow())
            {
                StimTimer = Time.time + 5f;
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
                reloadClass.Reload();
                return;
            }
            if (SAIN.Decision.TimeSinceChangeDecision > 3f && reloadClass.NoAmmoForReloadCached)
            {
                //BotOwner.WeaponManager.Reload.TryFillMagazines();
                //return;
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

using BepInEx.Logging;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Decision
{
    public class SelfActionClass : SAINBase, ISAINClass
    {
        public SelfActionClass(SAINComponentClass sain) : base(sain)
        {
        }

        public void Init()
        {
            BotOwner.Medecine.RefreshCurMeds();
        }

        public void Update()
        {
            if (!UsingMeds)
            {
                if (WasUsingMeds)
                {
                    BotOwner.Medecine.RefreshCurMeds();
                }
                switch (SAIN.Memory.Decisions.Self.Current)
                {
                    case SelfDecision.Reload:
                        TryReload();
                        break;

                    case SelfDecision.Surgery:
                        DoSurgery();
                        break;

                    case SelfDecision.FirstAid:
                        DoFirstAid();
                        break;

                    case SelfDecision.Stims:
                        DoStims();
                        break;

                    default:
                        break;
                }
                WasUsingMeds = UsingMeds;
            }
        }

        public void Dispose()
        {
        }


        private bool WasUsingMeds = false;

        private bool UsingMeds => BotOwner.Medecine?.Using == true;


        public void DoFirstAid()
        {
            var heal = BotOwner.Medecine.FirstAid;
            if (HealTimer < Time.time && heal.ShallStartUse())
            {
                HealTimer = Time.time + 5f;
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
            BotOwner.WeaponManager.Reload.TryReload();
            if (BotOwner.WeaponManager.Reload.NoAmmoForReloadCached)
            {
                //System.Console.WriteLine("NoAmmoForReloadCached");
                BotOwner.WeaponManager.Reload.TryFillMagazines();
            }
        }

        public void BotCancelReload()
        {
            if (BotOwner.WeaponManager.Reload.Reloading)
            {
                BotOwner.WeaponManager.Reload.TryStopReload();
            }
        }

        private float StimTimer = 0f;
        private float HealTimer = 0f;
    }
}

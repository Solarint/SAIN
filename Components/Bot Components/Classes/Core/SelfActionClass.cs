using BepInEx.Logging;
using EFT;
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
                            DoReload();
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

        public void DoReload()
        {
            var reload = BotOwner.WeaponManager.Reload;
            if (!reload.Reloading)
            {
                if (DebugBotDecisions.Value)
                {
                    Logger.LogDebug($"Reloading!");
                }
                reload.Reload();
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

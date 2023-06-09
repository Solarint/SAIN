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

        public void DoSelfAction()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding || BotOwner.Medecine.Using)
            {
                return;
            }

            if (SAIN.Decision.StartCancelReload())
            {
                BotCancelReload();
                return;
            }

            var Decision = SAIN.CurrentDecision;
            switch (Decision)
            {
                case SAINLogicDecision.Reload:
                    DoReload();
                    break;

                case SAINLogicDecision.Surgery:
                    DoSurgery();
                    break;

                case SAINLogicDecision.FirstAid:
                    DoFirstAid();
                    break;

                case SAINLogicDecision.Stims:
                    DoStims();
                    break;

                default:
                    break;
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

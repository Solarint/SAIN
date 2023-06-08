using BepInEx.Logging;
using EFT;
using UnityEngine;

namespace SAIN.Classes
{
    public class SelfActionClass : SAINBot
    {
        public SelfActionClass(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public void Activate()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                return;
            }

            if (BotOwner.Medecine.Using)
            {
                return;
            }

            if (SAIN.Decisions.StartCancelReload())
            {
                BotCancelReload();
                return;
            }

            switch (SAIN.CurrentDecision)
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

                if (UserSettings.DebugConfig.DebugLayers.Value)
                {
                    Logger.LogDebug($"I healed!");
                }

                heal.TryApplyToCurrentPart(null, null);
            }
        }

        public void DoSurgery()
        {
            var surgery = BotOwner.Medecine.SurgicalKit;
            if (surgery.ShallStartUse() && HealTimer < Time.time)
            {
                HealTimer = Time.time + 20f;

                if (UserSettings.DebugConfig.DebugLayers.Value)
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

                if (UserSettings.DebugConfig.DebugLayers.Value)
                {
                    Logger.LogDebug($"I'm Popping Stims");
                }

                stims.TryApply(true, null, null);
            }
        }

        public void DoReload()
        {
            if (!BotOwner.WeaponManager.Reload.Reloading)
            {
                if (UserSettings.DebugConfig.DebugLayers.Value)
                {
                    Logger.LogDebug($"Reloading!");
                }

                BotOwner.WeaponManager.Reload.Reload();
            }
        }

        public void BotCancelReload()
        {
            if (BotOwner.WeaponManager.Reload.Reloading)
            {
                //Logger.LogDebug($"I need to stop reloading!");

                BotOwner.WeaponManager.Reload.TryStopReload();
            }
        }

        protected ManualLogSource Logger;

        private float StimTimer = 0f;
        private float HealTimer = 0f;
    }
}

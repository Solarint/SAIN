using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;

namespace SAIN.Layers
{
    internal class SAINExtractLayer : CustomLayer
    {
        public override string GetName()
        {
            return Name;
        }

        public static string Name = "SAIN Extract";

        public SAINExtractLayer(BotOwner bot, int priority) : base(bot, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            SAIN = bot.GetComponent<SAINComponentClass>();
        }

        private float PercentageToExtract => SAIN.Info.PercentageBeforeExtract;

        public override Action GetNextAction()
        {
            return new Action(typeof(ExtractAction), "Extract");
        }

        private bool EnableExtracts
        {
            get
            {
                if (SAINPlugin.BotController.GetBot(BotOwner.ProfileId, out var bot))
                {
                    return bot.Info.FileSettings.Mind.EnableExtracts;
                }
                return false;
            }
        }

        public override bool IsActive()
        {
            if (SAIN.ExfilPosition == null || !EnableExtracts)
            {
                return false;
            }

            if (!SAIN.Info.Profile.IsPMC && !SAIN.Info.Profile.IsScav)
            {
                return false;
            }

            float percentageLeft = BotController.BotExtractManager.PercentageRemaining;
            if (percentageLeft <= PercentageToExtract)
            {
                if (!Logged)
                {
                    Logged = true;
                    Logger.LogInfo($"[{BotOwner.name}] Is Moving to Extract with [{percentageLeft}] of the raid remaining.");
                }
                if (SAIN.Enemy == null)
                {
                    return true;
                }
                else if (BotController.BotExtractManager.TimeRemaining < 120)
                {
                    return true;
                }
            }
            if (SAIN.Dying && !BotOwner.Medecine.FirstAid.HaveSmth2Use)
            {
                if (!Logged)
                {
                    Logged = true;
                    Logger.LogInfo($"[{BotOwner.name}] Is Moving to Extract because of heavy injury and lack of healing items.");
                }
                if (SAIN.Enemy == null)
                {
                    return true;
                }
                else if (SAIN.Enemy.TimeSinceSeen > 30f)
                {
                    return true;
                }
            }
            return false;
        }

        private bool Logged = false;

        public override bool IsCurrentActionEnding()
        {
            return false;
        }

        private SAINBotController BotController => SAINPlugin.BotController;
        public SoloDecision LastDecision => SAIN.Decision.OldMainDecision;
        public SoloDecision CurrentDecision => SAIN.CurrentDecision;

        private readonly SAINComponentClass SAIN;
        protected ManualLogSource Logger;
    }
}
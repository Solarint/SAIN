using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using static SAIN.UserSettings.ExtractConfig;

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
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
        }

        private float PercentageToExtract => SAIN.Info.PercentageBeforeExtract;

        public override Action GetNextAction()
        {
            return new Action(typeof(ExtractAction), "Extract");
        }

        public override bool IsActive()
        {
            if (SAIN.ExfilPosition == null || !EnableExtracts.Value)
            {
                return false;
            }

            if (!SAIN.Info.IsPMC && !SAIN.Info.IsScav)
            {
                return false;
            }

            float percentageLeft = BotController.BotExtractManager.PercentageRemaining;
            if (percentageLeft <= PercentageToExtract)
            {
                if (!Logged)
                {
                    Logged = true;
                    Logger.LogInfo($"[{BotOwner.name}] Is Moving to Extract with [{percentageLeft}] percent of the raid remaining.");
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
        public SAINSoloDecision LastDecision => SAIN.Decision.OldMainDecision;
        public SAINSoloDecision CurrentDecision => SAIN.CurrentDecision;

        private readonly SAINComponent SAIN;
        protected ManualLogSource Logger;
    }
}
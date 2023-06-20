using BepInEx.Logging;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using System;
using UnityEngine;

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
            if (SAIN.CanNotExfil == true || SAIN.Exfil == null)
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
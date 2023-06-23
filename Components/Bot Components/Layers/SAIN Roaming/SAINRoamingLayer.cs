using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;

namespace SAIN.Layers
{
    internal class SAINRoamingLayer : CustomLayer
    {
        public override string GetName()
        {
            return "SAIN Roaming Bot";
        }

        public SAINRoamingLayer(BotOwner bot, int priority) : base(bot, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
        }

        public override Action GetNextAction()
        {
            return new Action(typeof(RoamingAction), "Roaming");
        }

        public override bool IsActive()
        {
            return false;
        }


        public override bool IsCurrentActionEnding()
        {
            return false;
        }

        public SAINSoloDecision LastDecision => SAIN.Decision.OldMainDecision;
        public SAINSoloDecision CurrentDecision => SAIN.CurrentDecision;

        private readonly SAINComponent SAIN;
        protected ManualLogSource Logger;
    }
}
using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;

namespace SAIN.Layers
{
    internal class SAINExtractLayer : CustomLayer
    {
        public override string GetName()
        {
            return "SAIN Extract";
        }

        public SAINExtractLayer(BotOwner bot, int priority) : base(bot, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
        }

        public override Action GetNextAction()
        {
            return new Action(typeof(ExtractAction), "Extract");
        }

        public override bool IsActive()
        {
            return Active;
        }

        private bool Active => CurrentDecision == SAINSoloDecision.None && !SAIN.HasGoalEnemy && !SAIN.HasGoalTarget && BotOwner.Memory.IsPeace;

        public override bool IsCurrentActionEnding()
        {
            return !Active;
        }

        public SAINSoloDecision LastDecision => SAIN.Decision.OldMainDecision;
        public SAINSoloDecision CurrentDecision => SAIN.CurrentDecision;

        private readonly SAINComponent SAIN;
        protected ManualLogSource Logger;
    }
}
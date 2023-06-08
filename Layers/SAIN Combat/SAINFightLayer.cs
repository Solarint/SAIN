using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;

namespace SAIN.Layers
{
    internal class SAINFightLayer : CustomLayer
    {
        public override string GetName()
        {
            return "SAIN Combat";
        }

        public SAINFightLayer(BotOwner bot, int priority) : base(bot, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
        }

        public override Action GetNextAction()
        {
            Action nextAction;

            var Decision = CurrentDecision;
            switch (Decision)
            {
                case SAINLogicDecision.RegroupSquad:
                    nextAction = new Action(typeof(RegroupAction), $"{Decision}");
                    break;

                case SAINLogicDecision.Stims:
                case SAINLogicDecision.Surgery:
                case SAINLogicDecision.Reload:
                case SAINLogicDecision.FirstAid:
                case SAINLogicDecision.RunAway:
                case SAINLogicDecision.RunAwayGrenade:
                    nextAction = new Action(typeof(RetreatAction), $"{Decision}");
                    break;

                case SAINLogicDecision.MoveToCover:
                case SAINLogicDecision.UnstuckMoveToCover:
                    nextAction = new Action(typeof(MoveToCoverAction), $"{Decision}");
                    break;

                case SAINLogicDecision.RunForCover:
                    nextAction = new Action(typeof(RunToCoverAction), $"{Decision}");
                    break;

                case SAINLogicDecision.Suppress:
                    nextAction = new Action(typeof(SuppressAction), $"{Decision}");
                    break;

                case SAINLogicDecision.DogFight:
                case SAINLogicDecision.UnstuckDogFight:
                    nextAction = new Action(typeof(DogfightAction), $"{Decision}");
                    break;

                case SAINLogicDecision.HoldInCover:
                    nextAction = new Action(typeof(StandAndShootAction), $"{Decision}");
                    break;

                case SAINLogicDecision.StandAndShoot:
                    nextAction = new Action(typeof(HoldInCoverAction), $"{Decision}");
                    break;

                case SAINLogicDecision.Shoot:
                    nextAction = new Action(typeof(ShootAction), $"{Decision}");
                    break;

                case SAINLogicDecision.GroupSearch:
                case SAINLogicDecision.Search:
                case SAINLogicDecision.UnstuckSearch:
                    nextAction = new Action(typeof(SearchAction), $"{Decision}");
                    break;

                default:
                    nextAction = new Action(typeof(RetreatAction), $"DEFAULT!");
                    break;
            }

            if (nextAction == null)
            {
                Logger.LogError("Action Null?");
                nextAction = new Action(typeof(RetreatAction), $"DEFAULT!");
            }

            LastActionDecision = Decision;

            return nextAction;
        }

        public override bool IsActive()
        {
            bool Active = CurrentDecision != SAINLogicDecision.None && (SAIN.HasGoalEnemy || SAIN.HasGoalTarget);

            if (Active)
            {
                BotOwner.PatrollingData.Pause();
                return true;
            }
            else
            {
                BotOwner.PatrollingData.Unpause();
                return false;
            }
        }

        public override bool IsCurrentActionEnding()
        {
            return CurrentDecision != LastActionDecision;
        }

        private SAINLogicDecision LastActionDecision = SAINLogicDecision.None;
        public SAINLogicDecision CurrentDecision => SAIN.CurrentDecision;

        private readonly SAINComponent SAIN;
        protected ManualLogSource Logger;
    }
}
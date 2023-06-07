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
            return "SAIN Combat System";
        }

        public SAINFightLayer(BotOwner bot, int priority) : base(bot, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
        }

        public override Action GetNextAction()
        {
            Action nextAction;

            switch (CurrentDecision)
            {
                case SAINLogicDecision.RegroupSquad:
                    nextAction = new Action(typeof(RegroupAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.Stims:
                case SAINLogicDecision.Surgery:
                case SAINLogicDecision.Reload:
                case SAINLogicDecision.FirstAid:
                case SAINLogicDecision.RunAway:
                case SAINLogicDecision.RunAwayGrenade:
                    nextAction = new Action(typeof(RetreatAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.MoveToCover:
                    nextAction = new Action(typeof(MoveToCoverAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.RunForCover:
                    nextAction = new Action(typeof(RunToCoverAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.Suppress:
                    nextAction = new Action(typeof(SuppressAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.DogFight:
                    nextAction = new Action(typeof(DogfightAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.HoldInCover:
                    nextAction = new Action(typeof(StandAndShootAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.StandAndShoot:
                    nextAction = new Action(typeof(HoldInCoverAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.Shoot:
                    nextAction = new Action(typeof(ShootAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.GroupSearch:
                case SAINLogicDecision.Search:
                    nextAction = new Action(typeof(SearchAction), $"{CurrentDecision}");
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
            return CurrentDecision != LastDecision;
        }

        public SAINLogicDecision LastDecision => SAIN.Decisions.LastDecision;
        public SAINLogicDecision CurrentDecision => SAIN.CurrentDecision;

        private readonly SAINComponent SAIN;
        protected ManualLogSource Logger;
    }
}
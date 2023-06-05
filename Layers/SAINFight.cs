using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;

namespace SAIN.Layers
{
    internal class SAINFight : CustomLayer
    {
        public override string GetName()
        {
            return "SAIN Combat System";
        }

        public SAINFight(BotOwner bot, int priority) : base(bot, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            LastDecision = CurrentDecision;
        }

        public override Action GetNextAction()
        {
            Action nextAction;

            switch (CurrentDecision)
            {
                case SAINLogicDecision.Stims:
                case SAINLogicDecision.Surgery:
                case SAINLogicDecision.Reload:
                case SAINLogicDecision.FirstAid:
                case SAINLogicDecision.RunAway:
                case SAINLogicDecision.RunForCover:
                case SAINLogicDecision.RunAwayGrenade:
                    nextAction = new Action(typeof(RetreatAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.MoveToCover:
                    nextAction = new Action(typeof(MoveToCoverAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.Fight:
                    nextAction = new Action(typeof(FightAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.Suppress:
                    nextAction = new Action(typeof(SuppressAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.DogFight:
                    nextAction = new Action(typeof(DogfightAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.HoldInCover:
                case SAINLogicDecision.StandAndShoot:
                    nextAction = new Action(typeof(HoldInCoverAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.Skirmish:
                    nextAction = new Action(typeof(SkirmishAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.Search:
                    nextAction = new Action(typeof(SearchAction), $"{CurrentDecision}");
                    break;

                default:
                    nextAction = new Action(typeof(RetreatAction), $"DEFAULT!");
                    break;
            }

            LastDecision = CurrentDecision;

            //Logger.LogInfo($"New Action for {BotOwner.name}. {CurrentDecision}");

            return nextAction;
        }

        public override bool IsActive()
        {
            return CurrentDecision != SAINLogicDecision.None && (SAIN.HasGoalEnemy || SAIN.HasGoalTarget);
        }

        public override bool IsCurrentActionEnding()
        {
            return CurrentDecision != LastDecision;
        }

        public SAINLogicDecision LastDecision;
        public SAINLogicDecision CurrentDecision => SAIN.CurrentDecision;

        private readonly SAINComponent SAIN;
        protected ManualLogSource Logger;
    }
}
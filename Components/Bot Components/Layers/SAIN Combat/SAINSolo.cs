using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;

namespace SAIN.Layers
{
    internal class SAINSolo : CustomLayer
    {
        public override string GetName()
        {
            return Name;
        }

        public static string Name => "SAIN Combat Solo";

        public SAINSolo(BotOwner bot, int priority) : base(bot, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
        }

        public override Action GetNextAction()
        {
            Action nextAction;

            var Decision = CurrentDecision;
            var SelfDecision = SAIN.Decision.SelfDecision;
            switch (Decision)
            {
                case SAINSoloDecision.Retreat:
                    nextAction = new Action(typeof(MoveToCoverAction), $"{Decision} + {SelfDecision}");
                    break;

                case SAINSoloDecision.RunForCover:
                case SAINSoloDecision.MoveToCover:
                case SAINSoloDecision.UnstuckMoveToCover:
                    nextAction = new Action(typeof(MoveToCoverAction), $"{Decision}");
                    break;

                case SAINSoloDecision.DogFight:
                case SAINSoloDecision.UnstuckDogFight:
                    nextAction = new Action(typeof(DogfightAction), $"{Decision}");
                    break;

                case SAINSoloDecision.HoldInCover:
                    nextAction = new Action(typeof(StandAndShootAction), $"{Decision}");
                    break;

                case SAINSoloDecision.StandAndShoot:
                    nextAction = new Action(typeof(HoldInCoverAction), $"{Decision}");
                    break;

                case SAINSoloDecision.Shoot:
                    nextAction = new Action(typeof(ShootAction), $"{Decision}");
                    break;

                case SAINSoloDecision.Search:
                case SAINSoloDecision.UnstuckSearch:
                    nextAction = new Action(typeof(SearchAction), $"{Decision}");
                    break;

                case SAINSoloDecision.Investigate:
                    nextAction = new Action(typeof(InvestigateAction), $"{Decision}");
                    break;

                default:
                    nextAction = new Action(typeof(MoveToCoverAction), $"DEFAULT!");
                    break;
            }

            if (nextAction == null)
            {
                Logger.LogError("Action Null?");
                nextAction = new Action(typeof(MoveToCoverAction), $"DEFAULT!");
            }

            LastActionDecision = Decision;
            return nextAction;
        }

        public override bool IsActive()
        {
            bool Active = CurrentDecision != SAINSoloDecision.None;
            return Active;
        }

        private GClass430 Patrol => BotOwner.PatrollingData;
        private PatrolStatus PatrolStatus => Patrol.Status;

        public override bool IsCurrentActionEnding()
        {
            return CurrentDecision != LastActionDecision;
        }

        private SAINSoloDecision LastActionDecision = SAINSoloDecision.None;
        public SAINSoloDecision CurrentDecision => SAIN.CurrentDecision;

        private readonly SAINComponent SAIN;
        protected ManualLogSource Logger;
    }
}
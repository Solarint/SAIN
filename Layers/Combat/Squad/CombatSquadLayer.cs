using EFT;
using SAIN.Layers.Combat.Solo;

namespace SAIN.Layers.Combat.Squad
{
    internal class CombatSquadLayer : SAINLayer
    {
        public static readonly string Name = BuildLayerName<CombatSquadLayer>();

        public CombatSquadLayer(BotOwner bot, int priority) : base(bot, priority, Name)
        {
        }

        public override Action GetNextAction()
        {
            var Decision = SquadDecision;
            LastActionDecision = Decision;
            switch (Decision)
            {
                case SquadDecision.Regroup:
                    return new Action(typeof(RegroupAction), $"{Decision}");

                case SquadDecision.Suppress:
                    return new Action(typeof(SuppressAction), $"{Decision}");

                case SquadDecision.Search:
                    return new Action(typeof(SearchAction), $"{Decision}");

                case SquadDecision.Help:
                    return new Action(typeof(SearchAction), $"{Decision}");

                default:
                    return new Action(typeof(RegroupAction), $"DEFAULT!");
            }
        }

        public override bool IsActive()
        {
            if (SAIN == null) return false;

            return SAIN.Decision.CurrentSquadDecision != SquadDecision.None;
        }

        public override bool IsCurrentActionEnding()
        {
            if (SAIN == null) return true;
            return SquadDecision != LastActionDecision;
        }

        private SquadDecision LastActionDecision = SquadDecision.None;
        public SquadDecision SquadDecision => SAIN.Decision.CurrentSquadDecision;
    }
}
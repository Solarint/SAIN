namespace SAIN.SAINComponent.Classes.Decision
{
    public class DecisionWrapper : SAINComponentAbstract, ISAINClass
    {
        public DecisionWrapper(SAINComponentClass sain) : base(sain)
        {
            Main = new MainDecisionWrapper(sain);
            Squad = new SquadDecisionWrapper(sain);
            Self = new SelfDecisionWrapper(sain);
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public MainDecisionWrapper Main { get; private set; }
        public SquadDecisionWrapper Squad { get; private set; }
        public SelfDecisionWrapper Self { get; private set; }

        public class MainDecisionWrapper : SAINComponentAbstract, ISAINClass
        {
            public MainDecisionWrapper(SAINComponentClass sain) : base(sain)
            {
            }

            public void Init()
            {
            }

            public void Update()
            {
            }

            public void Dispose()
            {
            }

            public SoloDecision Current => SAIN.Decision.CurrentSoloDecision;
            public SoloDecision Last => SAIN.Decision.OldSoloDecision;
        }

        public class SquadDecisionWrapper : SAINComponentAbstract, ISAINClass
        {
            public SquadDecisionWrapper(SAINComponentClass sain) : base(sain)
            {
            }

            public void Init()
            {
            }

            public void Update()
            {
            }

            public void Dispose()
            {
            }

            public SquadDecision Current => SAIN.Decision.CurrentSquadDecision;
            public SquadDecision Last => SAIN.Decision.OldSquadDecision;
        }

        public class SelfDecisionWrapper : SAINComponentAbstract, ISAINClass
        {
            public SelfDecisionWrapper(SAINComponentClass sain) : base(sain)
            {
            }

            public void Init()
            {
            }

            public void Update()
            {
            }

            public void Dispose()
            {
            }

            public SelfDecision Current => SAIN.Decision.CurrentSelfDecision;
            public SelfDecision Last => SAIN.Decision.OldSelfDecision;
        }
    }
}
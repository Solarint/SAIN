using SAIN.SAINComponent.Classes.Decision;

namespace SAIN.SAINComponent.Classes
{
    public class MemoryClass : SAINBase, ISAINClass
    {
        public MemoryClass(SAINComponentClass sain) : base(sain)
        {
            Decisions = new DecisionWrapper(sain);
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

        public DecisionWrapper Decisions { get; private set; }
    }
}
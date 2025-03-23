using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.SAINComponent;
using System.Text;

namespace SAIN.Layers
{
    public abstract class BotAction : CustomLogic
    {
        public string Name { get; private set; }

        public BotAction(BotOwner botOwner, string name) : base(botOwner)
        {
            Name = name;
        }

        protected void ToggleAction(bool value)
        {
            switch (value) {
                case true:
                    BotOwner.PatrollingData?.Pause();
                    break;

                case false:
                    BotOwner.PatrollingData?.Unpause();
                    break;
            }
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            DebugOverlay.AddBaseInfo(Bot, BotOwner, stringBuilder);
        }

        public BotComponent Bot {
            get
            {
                if (_bot == null &&
                    SAINBotController.Instance.GetSAIN(BotOwner, out var bot)) {
                    _bot = bot;
                }
                if (_bot == null) {
                    _bot = BotOwner.GetComponent<BotComponent>();
                }
                return _bot;
            }
        }

        protected void StartProfilingSample(string functionName)
        {
            if (SAINPlugin.ProfilingMode) {
                UnityEngine.Profiling.Profiler.BeginSample(this.Name + "_" + functionName);
            }
        }

        protected void EndProfilingSample()
        {
            if (SAINPlugin.ProfilingMode) {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        private BotComponent _bot;
    }
}
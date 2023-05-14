using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Layers.Logic;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class SkirmishLogic : CustomLogic
    {
        public SkirmishLogic(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);

            Targeting = new UpdateTarget(bot);
            Move = new UpdateMove(bot);
            Steering = new UpdateSteering(bot);

            SAIN = bot.GetComponent<SAINCore>();
        }

        private SAINCore SAIN;

        public override void Start()
        {
            BotOwner.PatrollingData.Pause();
        }

        public override void Stop()
        {
            BotOwner.PatrollingData.Unpause();
        }

        public override void Update()
        {
            Move.ManualUpdate();

            Steering.ManualUpdate();

            if (SAIN.Enemy.CanSee)
            {
                Targeting.ManualUpdate();
            }
        }

        private readonly UpdateTarget Targeting;
        private readonly UpdateMove Move;
        private readonly UpdateSteering Steering;
        public bool DebugMode => DebugLayers.Value;
        public bool DebugDrawPoints => DebugLayersDraw.Value;

        public ManualLogSource Logger;
    }
}
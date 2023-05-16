using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Layers.Logic;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class SkirmishAction : CustomLogic
    {
        public SkirmishAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);

            SAIN = bot.GetComponent<SAINComponent>();
        }

        private SAINComponent SAIN;

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
            SAIN.Steering.ManualUpdate();

            if (SAIN.Core.Enemy.CanSee && SAIN.Core.Enemy.CanShoot)
            {
                SAIN.Targeting.ManualUpdate();
            }
        }

        public bool DebugMode => DebugLayers.Value;
        public bool DebugDrawPoints => DebugLayersDraw.Value;

        public ManualLogSource Logger;
    }
}
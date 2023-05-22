using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Layers.Logic;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class DogfightAction : CustomLogic
    {
        public DogfightAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            gclass105_0 = new GClass105(bot);
        }

        private GClass105 gclass105_0;

        public override void Update()
        {
            SAIN.Steering.ManualUpdate();

            if (SAIN.HasEnemyAndCanShoot)
            {
                gclass105_0.Update();
            }
        }

        private readonly SAINComponent SAIN;
        public bool DebugMode => DebugLayers.Value;
        public bool DebugDrawPoints => DebugLayersDraw.Value;

        public ManualLogSource Logger;

        public override void Start()
        {
            BotOwner.PatrollingData.Pause();
            //BotOwner.GetPlayer.MovementContext.SetAimingSlowdown(false, 0.75f);
        }

        public override void Stop()
        {
            BotOwner.GetPlayer.MovementContext.SetAimingSlowdown(true, 0.6f);
            //BotOwner.PatrollingData.Unpause();
        }
    }
}
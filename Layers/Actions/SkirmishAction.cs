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

            this.gclass105_0 = new GClass105(bot);
        }

        private GClass105 gclass105_0;

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
            SAIN.Movement.DecideMovementSpeed();

            SAIN.Steering.ManualUpdate();

            if (SAIN.HasEnemyAndCanShoot)
            {
                gclass105_0.Update();
            }

            BotOwner.SetPose(1f);
        }

        public bool DebugMode => DebugLayers.Value;

        public ManualLogSource Logger;
    }
}
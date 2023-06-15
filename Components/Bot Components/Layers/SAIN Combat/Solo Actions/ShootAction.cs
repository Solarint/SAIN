using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using SAIN.Layers;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class ShootAction : CustomLogic
    {
        public ShootAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot);
        }

        private ShootClass Shoot;

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
            //SAIN.Movement.DecideMovementSpeed();

            SAIN.Steering.SteerByPriority();

            if (SAIN.HasEnemyAndCanShoot)
            {
                Shoot.Update();
            }

            SAIN.Mover.SetTargetPose(1f);
        }

        public bool DebugMode => DebugLayers.Value;

        public ManualLogSource Logger;
    }
}
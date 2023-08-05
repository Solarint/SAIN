using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.SAINComponent;

namespace SAIN.Layers
{
    internal class ShootAction : CustomLogic
    {
        public ShootAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponentClass>();
            Shoot = new ShootClass(bot, SAIN);
        }

        private ShootClass Shoot;

        private SAINComponentClass SAIN;

        public override void Start()
        {
        }

        public override void Stop()
        {
        }

        public override void Update()
        {
            SAIN.Steering.SteerByPriority();
            Shoot.Update();
        }

        public ManualLogSource Logger;
    }
}
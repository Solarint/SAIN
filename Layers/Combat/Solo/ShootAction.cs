using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.SAINComponent;

namespace SAIN.Layers.Combat.Solo
{
    internal class ShootAction : SAINAction
    {
        public ShootAction(BotOwner bot) : base(bot, nameof(ShootAction))
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            SAIN = bot.GetComponent<SAINComponentClass>();
            Shoot = new ShootClass(bot);
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
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
        }


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
    }
}
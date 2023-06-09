using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Classes;
using UnityEngine;

namespace SAIN.Layers
{
    internal class MoveToCoverAction : CustomLogic
    {
        public MoveToCoverAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
        }

        public override void Update()
        {
            MoveToCover?.MoveToCoverPoint(SAIN.Cover.ClosestPoint);

            SAIN.Steering.ManualUpdate();

            if (SAIN.Enemy?.IsVisible == true && SAIN.Enemy?.CanShoot == true)
            {
                Shoot?.Update();
            }

            BotOwner.DoorOpener.Update();
        }

        public override void Start()
        {
            MoveToCover = new MoveToCoverObject(BotOwner);
            Shoot = new GClass105(BotOwner);
        }

        private MoveToCoverObject MoveToCover;
        private GClass105 Shoot;

        public override void Stop()
        {
        }

        private readonly ManualLogSource Logger;
        private readonly SAINComponent SAIN;
    }
}
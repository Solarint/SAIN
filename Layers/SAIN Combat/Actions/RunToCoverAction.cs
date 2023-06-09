using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Classes;
using UnityEngine;

namespace SAIN.Layers
{
    internal class RunToCoverAction : CustomLogic
    {
        public RunToCoverAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
        }

        private CoverPoint CoverPoint => SAIN.Cover.CurrentCoverPoint ?? SAIN.Cover.CurrentFallBackPoint;

        public override void Update()
        {
            if (MoveToCover == null)
            {
                Logger.LogError("Move To Cover Is Null");
                return;
            }

            if (MoveToCover.MoveToCoverPoint(SAIN.Cover.ClosestPoint))
            {
                float distance = MoveToCover.CoverDestination.Distance;
                if (distance < 1f)
                {
                    SAIN.Steering.ManualUpdate();
                    MoveToCover?.ToggleSprint(false);
                }
                else if (distance > 2f)
                {
                    BotOwner.Steering.LookToMovingDirection();
                    MoveToCover?.ToggleSprint(false);
                }
            }
            else
            {
                SAIN.Steering.ManualUpdate();
            }
        }

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            MoveToCover = new MoveToCoverObject(BotOwner);
        }

        private MoveToCoverObject MoveToCover;

        public override void Stop()
        {
            MoveToCover?.ToggleSprint(false);
        }

        public ManualLogSource Logger;
    }
}
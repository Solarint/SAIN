using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
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
            if (CoverPoint != null)
            {
                if (!SAIN.BotIsMoving && !SAIN.Cover.BotIsAtCoverPoint)
                {
                    MoveToPoint(CoverPoint.Position);
                }
                if (Vector3.Distance(CoverPoint.Position, BotOwner.Position) < 0.5f)
                {
                    BotOwner.GetPlayer.EnableSprint(false);
                }
                else
                {
                    BotOwner.GetPlayer.EnableSprint(true);
                }
            }

            if (CoverPoint == null)
            {
                //SAIN.Steering.ManualUpdate();
            }
        }

        private void MoveToPoint(Vector3 point)
        {
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);

            BotOwner.GoToPoint(point, false, 0.75f, false, false);

            BotOwner.DoorOpener.Update();

            BotOwner.Steering.LookToMovingDirection();
        }

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            if (CoverPoint != null)
            {
                MoveToPoint(CoverPoint.Position);
            }
            else
            {
                Logger.LogError($"Point null?!");
            }
        }

        public override void Stop()
        {
            BotOwner.GetPlayer.EnableSprint(false);
        }

        public ManualLogSource Logger;
    }
}
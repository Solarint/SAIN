using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Layers
{
    internal class MoveToCoverAction : CustomLogic
    {
        public MoveToCoverAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            AimData = new GClass105(bot);
        }

        private readonly GClass105 AimData;

        private CoverPoint CoverPoint => SAIN.Cover.CurrentCoverPoint ?? SAIN.Cover.CurrentFallBackPoint;

        public override void Update()
        {
            SAIN.Steering.ManualUpdate();

            if (SAIN.HasEnemyAndCanShoot)
            {
                AimData.Update();
            }

            BotOwner.DoorOpener.Update();

            if (CoverPoint != null && !SAIN.BotIsMoving && !SAIN.Cover.BotIsAtCoverPoint)
            {
                MoveToPoint(CoverPoint.Position);
            }
        }

        private void MoveToPoint(Vector3 point)
        {
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);
            BotOwner.GoToPoint(point, true, -1, false, false);
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
        }

        public ManualLogSource Logger;
    }
}
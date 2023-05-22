using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Layers.Logic;
using UnityEngine;

namespace SAIN.Layers
{
    internal class RetreatAction : CustomLogic
    {
        public RetreatAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            AimData = new GClass105(bot);
        }

        private readonly GClass105 AimData;

        private CoverClass Cover => SAIN.Cover;
        private CoverStatus CurrentFallBackStatus => Cover.FallBackPointStatus;
        private CoverStatus CurrentCoverStatus => Cover.CoverPointStatus;
        private bool InCoverPosition => CurrentFallBackStatus == CoverStatus.InCover || CurrentCoverStatus == CoverStatus.InCover;

        public override void Update()
        {
            if (SAIN.CurrentDecision == SAINLogicDecision.WalkToCover)
            {
                SAIN.Steering.ManualUpdate();
                AimData.Update();
            }

            if (SAIN.Cover.BotIsAtCoverPoint && SAIN.Cover.DuckInCover())
            {
                SAIN.Steering.ManualUpdate();
                return;
            }

            CoverPoint PointToGo = null;
            if (SAIN.Cover.CurrentFallBackPoint != null)
            {
                PointToGo = SAIN.Cover.CurrentFallBackPoint;
            }
            else if (SAIN.Cover.CurrentCoverPoint != null)
            {
                PointToGo = SAIN.Cover.CurrentCoverPoint;
            }

            if (PointToGo != null)
            {
                BotOwner.GoToPoint(PointToGo.Position, false);

                if (!BotOwner.GetPlayer.MovementContext.IsSprintEnabled)
                {
                    BotOwner.GetPlayer.EnableSprint(true);
                    //BotOwner.Sprint(true);
                }
            }
            else
            {
                Logger.LogError("Point Null");
            }

            BotOwner.DoorOpener.Update();
        }

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            BotOwner.PatrollingData.Pause();

            BotOwner.Steering.LookToMovingDirection();
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);
        }

        public override void Stop()
        {
            if (BotOwner.GetPlayer.MovementContext.IsSprintEnabled)
            {
                BotOwner.GetPlayer.EnableSprint(false);
                BotOwner.Sprint(false);
            }

            SAIN.Steering.ManualUpdate();

            BotOwner.PatrollingData.Unpause();
        }

        public ManualLogSource Logger;
    }
}
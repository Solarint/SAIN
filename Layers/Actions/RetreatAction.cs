using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Classes;
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

        public override void Update()
        {
            if (SAIN.CurrentDecision == SAINLogicDecision.WalkToCover)
            {
                SAIN.Steering.ManualUpdate();
                AimData.Update();
            }
            else
            {
                BotOwner.Steering.LookToMovingDirection();
            }

            if (SAIN.Cover.BotIsAtCoverPoint && SAIN.Cover.DuckInCover())
            {
                SAIN.Steering.ManualUpdate();

                BotOwner.DoorOpener.Update();

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
                BotOwner.SetPose(1f);
                BotOwner.SetTargetMoveSpeed(1f);

                BotOwner.GoToPoint(PointToGo.Position, false, -1, false, false);

                if (SAIN.CurrentDecision != SAINLogicDecision.WalkToCover)
                {
                    if (!BotOwner.GetPlayer.MovementContext.IsSprintEnabled)
                    {
                        BotOwner.GetPlayer.EnableSprint(true);
                        BotOwner.Sprint(true);
                    }
                }
            }
            else
            {
                BotOwner.SetPose(0.66f);
                BotOwner.SetTargetMoveSpeed(1f);
                SAIN.Dodge.Execute();
            }

            BotOwner.DoorOpener.Update();
        }

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            BotOwner.PatrollingData.Pause();
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
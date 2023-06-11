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
            SAIN = bot.GetComponent<SAINComponent>();
            MoveToCover = new MoveToCoverObject(BotOwner);
        }

        private SAINSoloDecision Decision => SAIN.CurrentDecision;
        private bool Sprint => Decision == SAINSoloDecision.RunForCover || Decision == SAINSoloDecision.Retreat;

        public override void Update()
        {
            if (SAIN.Cover.DuckInCover())
            {
                MoveToCover.ToggleSprint(false);
                return;
            }
            BotOwner.SetTargetMoveSpeed(1f);
            BotOwner.SetPose(1f);
            MoveToCover.MoveToCoverPoint(SAIN.Cover.ClosestPoint, Sprint);
        }

        public override void Start()
        {
            BotOwner.SetTargetMoveSpeed(1f);
            BotOwner.SetPose(1f);
            MoveToCover.MoveToCoverPoint(SAIN.Cover.ClosestPoint, Sprint);
        }

        public override void Stop()
        {
            MoveToCover.ToggleSprint(false);
        }

        private MoveToCoverObject MoveToCover;
        private readonly SAINComponent SAIN;
    }
}
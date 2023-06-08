using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;

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

        private CoverPoint CoverPoint => SAIN.Cover.CurrentFallBackPoint ?? SAIN.Cover.CurrentCoverPoint;

        private readonly GClass105 AimData;

        public override void Update()
        {
            BotOwner.DoorOpener.Update();

            //if (CheckMoveToLeanPos())
            //{
            //    return;
            //}

            if (SAIN.Cover.DuckInCover())
            {
                BotOwner.StopMove();
                BotOwner.GetPlayer.EnableSprint(false);
                SAIN.Steering.ManualUpdate();
                return;
            }

            if (CoverPoint != null)
            {
                if (Vector3.Distance(CoverPoint.Position, BotOwner.Position) < 0.5f)
                {
                    BotOwner.GetPlayer.EnableSprint(false);
                }
                else
                {
                    BotOwner.GetPlayer.EnableSprint(true);
                }
            }

            if (CoverPoint != null && !SAIN.BotIsMoving && !SAIN.Cover.BotIsAtCoverPoint)
            {
                BotOwner.Steering.LookToMovingDirection();
                MoveToPoint(CoverPoint.Position);
            }
        }

        private bool CheckMoveToLeanPos()
        {
            var LeanPos = SAIN.Lean.CheckLeanPositions;
            if (LeanPos != null)
            {
                if (NavMesh.SamplePosition(LeanPos.Value, out var hit, 0.1f, -1))
                {
                    SAIN.Steering.ManualUpdate();

                    BotOwner.SetPose(1f);
                    BotOwner.SetTargetMoveSpeed(0.5f);

                    BotOwner.GoToPoint(hit.position, true, -1, false, false);

                    return true;
                }
            }
            return false;
        }

        private void MoveToPoint(Vector3 point)
        {
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);

            BotOwner.GetPlayer.EnableSprint(true);

            BotOwner.GoToPoint(point, true, 0.75f, false, false);
        }

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            if (SAIN.Cover.BotIsAtCoverPoint)
            {
                SAIN.Steering.ManualUpdate();
            }
            else
            {
                if (CoverPoint != null)
                {
                    BotOwner.Steering.LookToMovingDirection();
                    MoveToPoint(CoverPoint.Position);
                }
                else
                {
                    SAIN.Steering.ManualUpdate();
                }
            }
        }

        public override void Stop()
        {
        }

        public ManualLogSource Logger;
    }
}
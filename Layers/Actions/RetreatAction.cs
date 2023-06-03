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
                SAIN.Steering.ManualUpdate();

                if (SAIN.HasEnemyAndCanShoot)
                {
                    AimData.Update();
                }

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

            if (PointToGo != null && SAIN.GoToPointRetreat(PointToGo.Position))
            {
                BotOwner.Steering.LookToMovingDirection();
                SetForMove();
            }
            else if (LastResultFallback())
            {
                BotOwner.Steering.LookToMovingDirection();
                SetForMove();
            }
            else
            {
                if (SAIN.HasEnemyAndCanShoot)
                {
                    AimData.Update();
                }

                SAIN.Steering.ManualUpdate();

                var direction = (BotOwner.Position - SAIN.CurrentTargetPosition.Value).normalized;
                var movePoint = BotOwner.Position + direction;

                if (NavMesh.SamplePosition(movePoint, out var hit, 0.25f, -1))
                {
                    SAIN.GoToPointRetreat(hit.position);
                }
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

        private bool LastResultFallback()
        {
            if (SAIN.CurrentTargetPosition == null)
            {
                return false;
            }

            if (TemporaryPoint == null)
            {
                var enemyPos = SAIN.CurrentTargetPosition.Value;

                var enemyDirection = enemyPos - BotOwner.Position;
                enemyDirection.y = 0f;

                enemyDirection = enemyDirection.normalized * 5f;

                var randomPoint = Random.onUnitSphere * 5f;
                randomPoint.y = 0f;

                var point = BotOwner.Position - enemyDirection + randomPoint;

                if (NavMesh.SamplePosition(point, out var hit, 1f, -1))
                {
                    var pointDirection = enemyPos - hit.position;
                    if (Physics.Raycast(hit.position, pointDirection, pointDirection.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        TemporaryPoint = hit.position;
                    }
                }
            }

            if (TemporaryPoint != null)
            {
                if (SAIN.GoToPointRetreat(TemporaryPoint.Value))
                {
                    return true;
                }
            }

            return false;
        }

        private void SetForMove()
        {
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);

            BotOwner.GetPlayer.EnableSprint(true);
        }

        private Vector3? TemporaryPoint;

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            BotOwner.PatrollingData.Pause();
        }

        public override void Stop()
        {
            BotOwner.GetPlayer.EnableSprint(false);

            SAIN.Steering.ManualUpdate();

            BotOwner.PatrollingData.Unpause();
        }

        public ManualLogSource Logger;
    }
}
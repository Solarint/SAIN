using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
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

            if (SAIN.CurrentDecision == SAINLogicDecision.WalkToCover)
            {
                SAIN.Steering.ManualUpdate();
                AimData.Update();
            }
            else
            {
                if (SAIN.Cover.DuckInCover())
                {
                    SAIN.Steering.ManualUpdate();
                    return;
                }

                BotOwner.Steering.LookToMovingDirection();
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
                MoveToPoint(PointToGo.Position);
            }
            else if (TemporaryPoint == null)
            {
                LastResultFallback();
            }
        }

        private void LastResultFallback()
        {
            var enemyPos = BotOwner.Memory.GoalEnemy.CurrPosition;

            var enemyDirection = enemyPos - BotOwner.Position;
            enemyDirection.y = 0f;

            enemyDirection = enemyDirection.normalized * 10f;

            var randomPoint = Random.onUnitSphere * 5f;
            randomPoint.y = 0f;

            var point = BotOwner.Position - enemyDirection + randomPoint;

            if (NavMesh.SamplePosition(point, out var hit, 1f, -1))
            {
                var pointDirection = enemyPos - hit.position;
                if (Physics.Raycast(hit.position, pointDirection, pointDirection.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(BotOwner.Position, hit.position, -1, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        if (path.corners.Length > 1)
                        {
                            var cornerADir = path.corners[1] - BotOwner.Position;
                            cornerADir.y = 0f;

                            if (Vector3.Dot(cornerADir.normalized, enemyDirection.normalized) < 0.5f)
                            {
                                TemporaryPoint = hit.position;
                                MoveToPoint(hit.position);
                            }
                        }
                    }
                }
            }
        }

        private void MoveToPoint(Vector3 point)
        {
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);

            if (SAIN.CurrentDecision != SAINLogicDecision.WalkToCover)
            {
                BotOwner.GetPlayer.EnableSprint(true);
                BotOwner.Sprint(true);
                BotOwner.GetPlayer.EnableSprint(true);
            }

            if ((BotOwner.Mover.RealDestPoint - point).magnitude < 0.25f)
            {
                return;
            }

            BotOwner.GoToPoint(point, true, -1, false, false);
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
            BotOwner.Sprint(false);

            SAIN.Steering.ManualUpdate();

            BotOwner.PatrollingData.Unpause();
        }

        public ManualLogSource Logger;
    }
}
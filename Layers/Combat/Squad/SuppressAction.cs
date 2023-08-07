using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Layers.Combat.Solo;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes;
using UnityEngine;

namespace SAIN.Layers.Combat.Squad
{
    internal class SuppressAction : SAINAction
    {
        public SuppressAction(BotOwner bot) : base(bot, nameof(SuppressAction))
        {
        }

        public override void Update()
        {
            var enemy = SAIN.Enemy;
            if (enemy != null)
            {
                if (enemy.IsVisible && enemy.CanShoot)
                {
                    BotOwner.StopMove();
                    Shoot.Update();
                }
                else if (CanSeeLastCorner(out var pos))
                {
                    BotOwner.StopMove();

                    SAIN.Steering.LookToPoint(pos.Value);

                    if (BotOwner.WeaponManager.HaveBullets)
                    {
                        SAIN.Shoot();
                    }
                }
                else
                {
                    SAIN.Steering.SteerByPriority();
                    BotOwner.MoveToEnemyData.TryMoveToEnemy(enemy.EnemyIAIDetails.Position);
                }
            }
        }

        private bool CanSeeLastCorner(out Vector3? pos)
        {
            pos = null;
            var pathCorners = SAIN.Enemy.NavMeshPath.corners;
            if (pathCorners.Length > 2)
            {
                var corner = pathCorners[pathCorners.Length - 2];

                if ((corner - BotOwner.Position).magnitude > 5f)
                {
                    return false;
                }

                corner.y += 1f;
                Vector3 headPos = SAIN.Transform.Head;
                var direction = corner - headPos;

                if (!Physics.Raycast(headPos, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    pos = corner + Random.onUnitSphere * 0.15f;
                    return true;
                }
            }
            return false;
        }

        public override void Start()
        {
        }

        public override void Stop()
        {
        }
    }
}
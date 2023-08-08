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

                    if (WaitShootTimer < Time.time && BotOwner.WeaponManager.HaveBullets)
                    {
                        WaitShootTimer = Time.time + 0.5f * Random.Range(0.66f, 1.33f);
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

        private float WaitShootTimer;

        private bool CanSeeLastCorner(out Vector3? pos)
        {
            pos = SAIN.Enemy?.Path.LastCornerToEnemy;
            return SAIN.Enemy?.Path.CanSeeLastCornerToEnemy == true;
        }

        public override void Start()
        {
        }

        public override void Stop()
        {
        }
    }
}
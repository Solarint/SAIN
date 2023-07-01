using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Layers
{
    internal class SuppressAction : CustomLogic
    {
        public SuppressAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot, SAIN);
        }

        // Token: 0x060008C4 RID: 2244 RVA: 0x0002AE8C File Offset: 0x0002908C
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
                        SAIN.Shoot(false);
                    }
                }
                else
                {
                    SAIN.Steering.SteerByPriority();
                    BotOwner.MoveToEnemyData.TryMoveToEnemy(enemy.Person.Position);
                }
            }
        }

        private bool CanSeeLastCorner(out Vector3? pos)
        {
            pos = null;
            var pathCorners = SAIN.Enemy.Path.corners;
            if (pathCorners.Length > 2)
            {
                var corner = pathCorners[pathCorners.Length - 2];

                if ((corner - BotOwner.Position).magnitude > 5f)
                {
                    return false;
                }

                corner.y += 1f;
                var direction = corner - SAIN.HeadPosition;

                if (!Physics.Raycast(SAIN.HeadPosition, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    pos = corner + Random.onUnitSphere * 0.15f;
                    return true;
                }
            }
            return false;
        }

        private readonly ShootClass Shoot;

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            if (SAIN.Enemy != null)
            {
                BotOwner.SuppressShoot.InitToPoint(SAIN.Enemy.PositionLastSeen);
            }
        }

        public override void Stop()
        {
        }

        private ManualLogSource Logger;
    }
}
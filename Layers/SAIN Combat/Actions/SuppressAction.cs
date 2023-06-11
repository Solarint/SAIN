using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
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
            Shoot = new GClass112(bot);
        }

        // Token: 0x060008C4 RID: 2244 RVA: 0x0002AE8C File Offset: 0x0002908C
        public override void Update()
        {
            var enemy = SAIN.Enemy;
            if (enemy != null)
            {
                if (enemy.IsVisible)
                {
                    BotOwner.StopMove();
                    Shoot.Update();
                }
                else if (CanSeeLastCorner(out var pos))
                {
                    BotOwner.StopMove();

                    BotOwner.Steering.LookToPoint(pos.Value);

                    if (BotOwner.WeaponManager.HaveBullets)
                    {
                        BotOwner.ShootData.Shoot();
                    }
                }
                else
                {
                    SAIN.Steering.ManualUpdate();
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

        private readonly GClass112 Shoot;

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            var list = new List<Vector3>();
            foreach (var enemy in BotOwner.BotsGroup.Enemies.Keys)
            {
                if (enemy != null)
                {
                    list.Add(enemy.Position);
                }
            }
            if (BotOwner.SuppressShoot.InitToPoints(list))
            {
                //Logger.LogDebug("Init to Points List Success");
            }
            else if (BotOwner.SuppressShoot.InitToPoint(BotOwner.Memory.GoalEnemy.EnemyLastPositionReal))
            {
                //Logger.LogDebug("Init to Point Success");
            }
            else if (BotOwner.SuppressShoot.Init(BotOwner.Memory.GoalEnemy))
            {
                //Logger.LogDebug("Init to GoalEnemy Success");
            }
            else
            {
                Logger.LogError("Could Not Start Suppression");
            }
        }

        public override void Stop()
        {
        }

        private ManualLogSource Logger;
    }
}
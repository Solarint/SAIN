using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class MoveToEngage : CustomLogic
    {
        public MoveToEngage(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot, SAIN);
        }

        private readonly ShootClass Shoot;
        private float RecalcPathTimer;

        public override void Update()
        {
            SAINEnemy enemy = SAIN.Enemy;
            if (enemy == null)
            {
                return;
            }

            SAIN.Mover.SetTargetPose(1f);
            SAIN.Mover.SetTargetMoveSpeed(1f);

            if (CheckShoot(enemy))
            {
                Shoot.Update();
                return;
            }

            if (SAIN.Decision.SelfActionDecisions.CheckLowAmmo(0.66f))
            {
                SAIN.SelfActions.TryReload();
            }

            Vector3 lastSeenPos = enemy.PositionLastSeen;
            if ((BotOwner.Position - lastSeenPos).sqrMagnitude < 2f)
            {
                lastSeenPos = enemy.Position;
            }

            float distance = enemy.RealDistance;
            if (distance > 40f && !BotOwner.Memory.IsUnderFire)
            {
                if (RecalcPathTimer < Time.time)
                {
                    RecalcPathTimer = Time.time + 2f;
                    BotOwner.BotRun.Run(lastSeenPos, false);
                }
            }
            else
            {
                SAIN.Mover.Sprint(false);

                if (RecalcPathTimer < Time.time)
                {
                    RecalcPathTimer = Time.time + 2f;
                    BotOwner.MoveToEnemyData.TryMoveToEnemy(lastSeenPos);
                }

                if (!SAIN.Steering.SteerByPriority(false))
                {
                    SAIN.Steering.LookToPoint(lastSeenPos + Vector3.up * 1f);
                }
            }
        }

        private bool CheckShoot(SAINEnemy enemy)
        {
            float distance = enemy.RealDistance;
            bool enemyLookAtMe = enemy.EnemyLookingAtMe;
            float EffDist = SAIN.Info.WeaponInfo.EffectiveWeaponDistance;

            if (enemy.IsVisible)
            {
                if (enemyLookAtMe)
                {
                    return true;
                }
                if (distance <= EffDist && enemy.CanShoot)
                {
                    return true;
                }
            }
            return false;
        }


        private readonly SAINComponent SAIN;

        public ManualLogSource Logger;

        public override void Start()
        {
        }

        public override void Stop()
        {
        }
    }
}
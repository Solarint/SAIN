using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
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
            if (SAIN.Enemy == null)
            {
                return;
            }

            SAIN.Mover.SetTargetPose(1f);
            SAIN.Mover.SetTargetMoveSpeed(1f);
            Vector3 lastSeenPos = SAIN.Enemy.PositionLastSeen;
            float distance = SAIN.Enemy.RealDistance;
            bool enemyLookAtMe = SAIN.Enemy.EnemyLookingAtMe;
            float EffDist = SAIN.Info.WeaponInfo.EffectiveWeaponDistance;

            if (enemyLookAtMe || distance <= EffDist)
            {
                Shoot.Update();
                return;
            }

            if (distance > 40f && !BotOwner.Memory.IsUnderFire && !enemyLookAtMe)
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
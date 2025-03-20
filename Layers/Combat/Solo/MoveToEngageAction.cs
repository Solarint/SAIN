using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections;
using UnityEngine;

namespace SAIN.Layers.Combat.Solo
{
    internal class MoveToEngageAction : CombatAction, ISAINAction
    {
        public MoveToEngageAction(BotOwner bot) : base(bot, nameof(MoveToEngageAction))
        {
        }

        private float RecalcPathTimer;

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public override void Update(CustomLayer.ActionData data)
        {
            Enemy enemy = Bot.Enemy;
            if (enemy == null) {
                Bot.Steering.SteerByPriority();
                return;
            }

            Bot.Mover.SetTargetPose(1f);
            Bot.Mover.SetTargetMoveSpeed(1f);

            if (CheckShoot(enemy)) {
                Bot.Steering.SteerByPriority();
                Shoot.CheckAimAndFire();
                return;
            }

            //if (Bot.Decision.SelfActionDecisions.LowOnAmmo(0.66f))
            //{
            //    Bot.SelfActions.TryReload();
            //}

            Vector3? lastKnown = enemy.KnownPlaces.LastKnownPosition;
            Vector3 movePos;
            if (lastKnown != null) {
                movePos = lastKnown.Value;
            }
            else if (enemy.TimeSinceSeen < 5f) {
                movePos = enemy.EnemyPosition;
            }
            else {
                Bot.Steering.SteerByPriority();
                Shoot.CheckAimAndFire();
                return;
            }

            var cover = Bot.Cover.FindPointInDirection(movePos - Bot.Position, 0.5f, 3f);
            if (cover != null) {
                movePos = cover.Position;
            }

            float distance = enemy.RealDistance;
            if (distance > 40f && !BotOwner.Memory.IsUnderFire) {
                if (RecalcPathTimer < Time.time) {
                    RecalcPathTimer = Time.time + 2f;
                    BotOwner.BotRun.Run(movePos, false, SAINPlugin.LoadedPreset.GlobalSettings.General.SprintReachDistance);
                    Bot.Steering.LookToMovingDirection(500f, true);
                }
                return;
            }

            Bot.Mover.Sprint(false);

            if (RecalcPathTimer < Time.time) {
                RecalcPathTimer = Time.time + 2f;
                BotOwner.MoveToEnemyData.TryMoveToEnemy(movePos);
            }

            if (!Bot.Steering.SteerByPriority(null, false)) {
                Bot.Steering.LookToMovingDirection();
                //SAIN.Steering.LookToPoint(movePos + Vector3.up * 1f);
            }
        }

        private bool CheckShoot(Enemy enemy)
        {
            float distance = enemy.RealDistance;
            bool enemyLookAtMe = enemy.EnemyLookingAtMe;
            float EffDist = Bot.Info.WeaponInfo.EffectiveWeaponDistance;

            if (enemy.IsVisible) {
                if (enemyLookAtMe) {
                    return true;
                }
                if (distance <= EffDist && enemy.CanShoot) {
                    return true;
                }
            }
            return false;
        }

        public override void Start()
        {
            Toggle(true);
        }

        public override void Stop()
        {
            Toggle(false);
        }
    }
}
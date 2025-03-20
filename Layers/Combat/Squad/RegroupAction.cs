using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Layers.Combat.Solo;
using SAIN.SAINComponent;
using System.Collections;
using UnityEngine;

namespace SAIN.Layers.Combat.Squad
{
    internal class RegroupAction : CombatAction, ISAINAction
    {
        public RegroupAction(BotOwner bot) : base(bot, nameof(RegroupAction))
        {
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public override void Update(CustomLayer.ActionData data)
        {
            var SquadLeadPos = Bot.Squad.LeaderComponent?.Position;
            if (SquadLeadPos != null) {
                Bot.Mover.GoToPoint(SquadLeadPos.Value, out _);
                CheckShouldSprint(SquadLeadPos.Value);
            }

            Bot.Mover.SetTargetPose(1f);
            Bot.Mover.SetTargetMoveSpeed(1f);

            if (!Bot.Mover.SprintController.Running) {
                Shoot.CheckAimAndFire();
                Bot.Steering.SteerByPriority();
            }
        }

        public override void Start()
        {
            Toggle(true);
        }

        private void CheckShouldSprint(Vector3 pos)
        {
            bool hasEnemy = Bot.HasEnemy;
            bool enemyLOS = Bot.Enemy?.InLineOfSight == true;
            float leadDist = (pos - BotOwner.Position).magnitude;
            float enemyDist = hasEnemy ? (Bot.Enemy.EnemyIPlayer.Position - BotOwner.Position).magnitude : 999f;

            bool sprint =
                hasEnemy &&
                leadDist > 30f &&
                !enemyLOS &&
                enemyDist > 50f;

            if (Bot.Steering.SteerByPriority(null, false)) {
                sprint = false;
            }

            if (_nextChangeSprintTime < Time.time) {
                _nextChangeSprintTime = Time.time + 1f;
                if (sprint) {
                    Bot.Mover.Sprint(true);
                }
                else {
                    Bot.Mover.Sprint(false);
                    Bot.Steering.SteerByPriority();
                }
            }
        }

        private float _nextChangeSprintTime;

        public override void Stop()
        {
            Toggle(false);
        }
    }
}
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Layers.Combat.Solo;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using UnityEngine;

namespace SAIN.Layers.Combat.Squad
{
    internal class AssaultAction : CombatAction
    {
        public AssaultAction(BotOwner bot) : base(bot, nameof(AssaultAction))
        {
        }

        public override void Update(CustomLayer.ActionData data)
        {
            Shoot.CheckAimAndFire();

            Enemy enemy = Bot.Enemy;
            if (!Bot.Steering.SteerByPriority(enemy, false) && enemy != null) {
                Bot.Steering.LookToEnemy(enemy);
            }

            if (enemy != null) {
                if (PointDestination == null) {
                    PointDestination = Bot.Cover.FindPointInDirection(enemy.EnemyDirection);
                }
                if (PointDestination != null) {
                    Vector3 destination = PointDestination.Position;

                    if ((destination - Bot.Position).sqrMagnitude < 1f) {
                        PointDestination = null;
                        return;
                    }
                    if (_recalcPathTime < Time.time) {
                        bool sprint = true;

                        if (sprint && BotOwner.BotRun.Run(destination, false, SAINPlugin.LoadedPreset.GlobalSettings.General.SprintReachDistance)) {
                            Bot.Steering.LookToMovingDirection(500f, true);
                            _recalcPathTime = Time.time + 1f;
                        }
                        else if (Bot.Mover.GoToPoint(destination, out _)) {
                            _recalcPathTime = Time.time + 1f;
                        }
                        else {
                            _recalcPathTime = Time.time + 0.5f;
                        }
                    }
                }
            }
        }

        private float _recalcPathTime;
        private CoverPoint PointDestination;

        public override void Start()
        {
        }

        public override void Stop()
        {
        }
    }
}
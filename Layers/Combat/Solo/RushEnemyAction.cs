using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers.Combat.Solo
{
    internal class RushEnemyAction : CombatAction, ISAINAction
    {
        public RushEnemyAction(BotOwner bot) : base(bot, nameof(RushEnemyAction))
        {
        }

        public override void Update(CustomLayer.ActionData data)
        {
            Bot.Mover.SetTargetPose(1f);
            Bot.Mover.SetTargetMoveSpeed(1f);
            if (!checkHasEnemy()) {
                Bot.Steering.SteerByPriority(null, true);
                return;
            }

            if (_enemy.InLineOfSight) {
                enemyInSight();
                return;
            }

            checkUpdateMove();
            checkJump();
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        private bool checkHasEnemy()
        {
            if (_enemy != null) {
                return true;
            }

            _enemy = Bot.Enemy;
            if (_enemy != null) {
                _enemy.Events.OnPathUpdated += onPathUpdated;
                _pathUpdated = true;
                return true;
            }

            return false;
        }

        private void enemyInSight()
        {
            _pathUpdated = true;
            checkJumpEnemyInSight();

            Shoot.CheckAimAndFire();
            Bot.Mover.Sprint(false);
            Bot.Mover.DogFight.DogFightMove(true);

            if (_enemy.IsVisible && _enemy.CanShoot) {
                Bot.Steering.SteerByPriority();
                return;
            }
            Bot.Steering.LookToEnemy(_enemy);
        }

        private void checkJump()
        {
            if (_shallTryJump && TryJumpTimer < Time.time && Bot.Player.IsSprintEnabled) {
                //&& Bot.Enemy.Path.PathDistance > 3f
                var corner = _enemy.Path.EnemyCorners.GroundPosition(ECornerType.Last);
                if (corner != null &&
                    (corner.Value - Bot.Position).sqrMagnitude < 1f) {
                    TryJumpTimer = Time.time + 3f;
                    Bot.Mover.TryJump();
                }
            }
        }

        private void checkJumpEnemyInSight()
        {
            if (_shallTryJump) {
                if (_shallBunnyHop) {
                    Bot.Mover.TryJump();
                }
                else if (TryJumpTimer < Time.time &&
                        Bot.Player.IsSprintEnabled) {
                    TryJumpTimer = Time.time + 3f;
                    if (!_shallBunnyHop
                        && EFTMath.RandomBool(Bot.Info.PersonalitySettings.Rush.BunnyHopChance)) {
                        _shallBunnyHop = true;
                    }
                    Bot.Mover.TryJump();
                }
            }
        }

        private void checkUpdateMove()
        {
            if (_pathUpdated && _updateMoveTime < Time.time) {
                if (Bot.Mover.SprintController.Running && Bot.Mover.SprintController.Canceling) {
                    return;
                }
                _updateMoveTime = Time.time + 0.1f;
                if (updateMove(_enemy)) {
                    _pathUpdated = false;
                }
            }
        }

        private Vector3 _lastMovePos;

        private const float CHANGE_MOVE_THRESHOLD = 1f;

        private float TryJumpTimer;

        private bool updateMove(Enemy enemy)
        {
            Vector3? lastKnown = enemy.KnownPlaces.LastKnownPosition;
            if (lastKnown == null) {
                return false;
            }

            var sprintController = Bot.Mover.SprintController;
            float pathDistance = enemy.Path.PathDistance;
            if (pathDistance <= 1f && (sprintController.Running || BotOwner.Mover.IsMoving)) {
                return true;
            }
            if ((sprintController.Running || BotOwner.Mover.IsMoving) &&
                (_lastMovePos - lastKnown.Value).sqrMagnitude < CHANGE_MOVE_THRESHOLD) {
                return true;
            }
            _lastMovePos = lastKnown.Value;
            if (pathDistance > BotOwner.Settings.FileSettings.Move.RUN_TO_COVER_MIN && sprintController.RunToPointByWay(enemy.Path.PathToEnemy, SAINComponent.Classes.Mover.ESprintUrgency.High, true)) {
                return true;
            }
            if (sprintController.Running) {
                return true;
            }
            if (Bot.Mover.GoToEnemy(enemy, -1, false, true)) {
                return true;
            }
            return false;
        }

        private bool _shallBunnyHop = false;
        private float _updateMoveTime = 0f;

        public override void Start()
        {
            checkHasEnemy();
            Toggle(true);

            _shallTryJump = Bot.Info.PersonalitySettings.Rush.CanJumpCorners
                //&& Bot.Decision.CurrentSquadDecision != SquadDecision.PushSuppressedEnemy
                && EFTMath.RandomBool(Bot.Info.PersonalitySettings.Rush.JumpCornerChance);

            _shallBunnyHop = false;
        }

        private Enemy _enemy;

        private void onPathUpdated(Enemy enemy, NavMeshPathStatus status)
        {
            if (_enemy != enemy) {
                enemy.Events.OnPathUpdated -= onPathUpdated;
                return;
            }

            if (status != NavMeshPathStatus.PathInvalid)
                _pathUpdated = true;
        }

        private bool _pathUpdated;

        private bool _shallTryJump = false;

        public override void Stop()
        {
            Toggle(false);
            if (_enemy != null) {
                _enemy.Events.OnPathUpdated -= onPathUpdated;
                _enemy = null;
            }

            Bot.Mover.DogFight.ResetDogFightStatus();
            Bot.Mover.SprintController.CancelRun(0.25f);
        }
    }
}
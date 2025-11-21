using DrakiaXYZ.BigBrain.Brains;
using EFT;
using MonoMod.Cil;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers.Combat.Solo;

internal class RushEnemyAction(BotOwner bot) : BotAction(bot, nameof(RushEnemyAction)), IBotAction
{
    public override void Update(CustomLayer.ActionData data)
    {
        
        Bot.Mover.SetTargetPose(1f);
        Bot.Mover.SetTargetMoveSpeed(1f);
        updateRushBehavior();
        
    }

    private void updateRushBehavior()
    {
        _enemy = Bot.GoalEnemy;
        if (_enemy == null)
        {
            Bot.Steering.SteerByPriority(null, true);
        }
        else if (_enemy.InLineOfSight)
        {
            enemyInSight();
        }
        else
        {
            checkUpdateMove();
            checkJump();
        }
    }

    private void enemyInSight()
    {
        checkJumpEnemyInSight();
        if (Bot.Mover.Running) Bot.Mover.ActivePath?.RequestEndSprint(SAINComponent.Classes.Mover.ESprintUrgency.None, "enemy in sight");
        Bot.Mover.DogFight.DogFightMove(true, _enemy);
    }

    public override void OnSteeringTicked()
    {
        if (!Shoot.ShootAnyVisibleEnemies(_enemy))
        {
            Bot.Suppression.TrySuppressAnyEnemy(_enemy, Bot.EnemyController.KnownEnemies);
        }
        if (!Bot.Steering.SteerByPriority(_enemy, false))
        {
            Bot.Steering.LookToLastKnownEnemyPosition(_enemy);
        }
    }

    private void checkJump()
    {
        if (!Bot.Info.FileSettings.Move.JUMP_TOGGLE || !GlobalSettingsClass.Instance.Move.JUMP_TOGGLE)
        {
            return;
        }
        if (_shallTryJump && TryJumpTimer < Time.time && Bot.Player.IsSprintEnabled)
        {
            //&& Bot.Enemy.Path.PathDistance > 3f
            NavMeshPath enemyPath = _enemy.Path.PathToEnemy;
            if (enemyPath.corners.Length > 2 && 
                (enemyPath.corners[enemyPath.corners.Length - 2] - Bot.Position).sqrMagnitude < 1f)
            {
                TryJumpTimer = Time.time + 3f;
                Bot.Mover.TryJump();
            }
        }
    }

    private void checkJumpEnemyInSight()
    {
        if (!Bot.Info.FileSettings.Move.JUMP_TOGGLE || !GlobalSettingsClass.Instance.Move.JUMP_TOGGLE)
        {
            return;
        }
        if (_shallTryJump)
        {
            if (_shallBunnyHop)
            {
                Bot.Mover.TryJump();
            }
            else if (TryJumpTimer < Time.time &&
                    Bot.Player.IsSprintEnabled)
            {
                TryJumpTimer = Time.time + 3f;
                if (!_shallBunnyHop
                    && EFTMath.RandomBool(Bot.Info.PersonalitySettings.Rush.BunnyHopChance))
                {
                    _shallBunnyHop = true;
                }
                Bot.Mover.TryJump();
            }
        }
    }

    private void checkUpdateMove()
    {
        if (_updateMoveTime < Time.time)
        {
            if (updateMove(_enemy))
            {
                _updateMoveTime = Time.time + 2f;
                return;
            }
            _updateMoveTime = Time.time + 0.1f;
        }
    }

    private Vector3 _lastMovePos;

    private const float CHANGE_MOVE_THRESHOLD = 1f;

    private float TryJumpTimer;

    private bool updateMove(Enemy enemy)
    {
        Vector3? lastKnown = enemy.KnownPlaces.LastKnownPosition;
        if (lastKnown == null)
        {
            return false;
        }

        var sprintController = Bot.Mover;
        float pathDistance = enemy.Path.PathLength;
        if (pathDistance <= 1f && sprintController.Moving)
        {
            return true;
        }
        if (sprintController.Moving && (_lastMovePos - lastKnown.Value).sqrMagnitude < CHANGE_MOVE_THRESHOLD)
        {
            return true;
        }
        _lastMovePos = lastKnown.Value;
        if (pathDistance > BotOwner.Settings.FileSettings.Move.RUN_TO_COVER_MIN && sprintController.RunToPointByWay(enemy.Path.PathToEnemy, true, -1, SAINComponent.Classes.Mover.ESprintUrgency.High, true))
        {
            return true;
        }
        if (sprintController.Running)
        {
            return true;
        }
        if (Bot.Mover.WalkToPointByWay(enemy.Path.PathToEnemy))
        {
            return true;
        }
        return false;
    }

    private bool _shallBunnyHop = false;
    private float _updateMoveTime = 0f;

    public override void Start()
    {
        base.Start();
        _shallTryJump = Bot.Info.PersonalitySettings.Rush.CanJumpCorners
            //&& Bot.Decision.CurrentSquadDecision != SquadDecision.PushSuppressedEnemy
            && EFTMath.RandomBool(Bot.Info.PersonalitySettings.Rush.JumpCornerChance);

        _shallBunnyHop = false;
    }

    private Enemy _enemy;
    private bool _shallTryJump = false;

    public override void Stop()
    {
        base.Stop();
        Bot.Mover.DogFight.ResetDogFightStatus();
    }
}
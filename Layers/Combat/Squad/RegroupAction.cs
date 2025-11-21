using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.Layers.Combat.Squad;

internal class RegroupAction(BotOwner bot) : BotAction(bot, nameof(RegroupAction)), IBotAction
{
    public override void Update(CustomLayer.ActionData data)
    {
        
        Enemy enemy = Bot.GoalEnemy;
        var SquadLeadPos = Bot.Squad.LeaderComponent?.Position;
        if (SquadLeadPos != null)
        {
            bool hasEnemy = enemy != null;
            bool enemyLOS = enemy?.InLineOfSight == true;
            float leadDist = (SquadLeadPos.Value - BotOwner.Position).magnitude;
            float enemyDist = hasEnemy ? enemy.KnownPlaces.BotDistanceFromLastKnown : 999f;

            bool sprint =
                hasEnemy &&
                leadDist > 30f &&
                !enemyLOS &&
                enemyDist > 50f;

            if (_nextChangeSprintTime < Time.time)
            {
                _nextChangeSprintTime = Time.time + 1f;
                if (sprint)
                {
                    Bot.Mover.RunToPoint(SquadLeadPos.Value);
                }
                else
                {
                    Bot.Mover.WalkToPoint(SquadLeadPos.Value);
                }
            }
        }

        Bot.Mover.SetTargetPose(1f);
        Bot.Mover.SetTargetMoveSpeed(1f);
        
    }

    public override void OnSteeringTicked()
    {
        Enemy enemy = Bot.GoalEnemy;
        if (!Shoot.ShootAnyVisibleEnemies(enemy))
        {
            Bot.Suppression.TrySuppressAnyEnemy(enemy, Bot.EnemyController.KnownEnemies);
        }
        if (!Bot.Steering.SteerByPriority(enemy))
        {
            Bot.Steering.LookToMovingDirection();
        }
    }


    private float _nextChangeSprintTime;
}
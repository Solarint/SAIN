using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.Layers.Combat.Squad;

internal class SuppressAction(BotOwner bot) : BotAction(bot, nameof(SuppressAction)), IBotAction
{
    public override void Update(CustomLayer.ActionData data)
    {
        var enemy = Bot.GoalEnemy;
        if (enemy != null)
        {
            Vector3? lastKnown = enemy.LastKnownPosition;
            if (lastKnown != null)
            {
                Bot.Mover.WalkToPointByWay(enemy.Path.PathToEnemy);
            }
        }
    }

    public override void OnSteeringTicked()
    {
        var enemy = Bot.GoalEnemy;
        if (Shoot.ShootAnyVisibleEnemies(enemy))
        {
            Bot.Steering.SteerByPriority(enemy, false);
            return;
        }

        if (Bot.Suppression.TrySuppressAnyEnemy(enemy, Bot.EnemyController.KnownEnemies))
        {
            Bot.Steering.SteerByPriority(enemy, false);
            return;
        }

        Bot.Suppression.ResetSuppressing();
        if (!Bot.Steering.SteerByPriority(enemy, false))
        {
            Bot.Steering.LookToLastKnownEnemyPosition(enemy);
        }
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Stop()
    {
        base.Stop();
        Bot.Suppression.ResetSuppressing();
    }
}
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.Layers.Combat.Solo;

internal class MoveToEngageAction(BotOwner bot) : BotAction(bot, nameof(MoveToEngageAction)), IBotAction
{
    private float RecalcPathTimer;

    public override void Update(CustomLayer.ActionData data)
    {
        
        Enemy enemy = Bot.GoalEnemy;
        if (enemy == null)
        {
            Bot.Steering.SteerByPriority();
            
            return;
        }

        Bot.Mover.SetTargetPose(1f);
        Bot.Mover.SetTargetMoveSpeed(1f);

        if (CheckShoot(enemy))
        {
            Shoot.ShootAnyVisibleEnemies(enemy);
            Bot.Steering.SteerByPriority(enemy);
            
            return;
        }

        //if (Bot.Decision.SelfActionDecisions.LowOnAmmo(0.66f))
        //{
        //    Bot.SelfActions.TryReload();
        //}

        Vector3? lastKnown = enemy.KnownPlaces.LastKnownPosition;
        Vector3 movePos;
        if (lastKnown != null)
        {
            movePos = lastKnown.Value;
        }
        else if (enemy.TimeSinceSeen < 5f)
        {
            movePos = enemy.EnemyPosition;
        }
        else
        {
            Shoot.ShootAnyVisibleEnemies(enemy);
            Bot.Steering.SteerByPriority(enemy);
            
            return;
        }

        var cover = Bot.Cover.FindPointInDirection(movePos - Bot.Position, 0.5f, 3f);
        if (cover != null)
        {
            movePos = cover.Position;
        }

        float distance = enemy.RealDistance;
        if (distance > 40f && !BotOwner.Memory.IsUnderFire)
        {
            if (RecalcPathTimer < Time.time)
            {
                RecalcPathTimer = Time.time + 2f;
                //BotOwner.BotRun.Run(movePos, false, SAINPlugin.LoadedPreset.GlobalSettings.General.SprintReachDistance);
                Bot.Steering.LookToMovingDirection(true);
            }
            
            return;
        }

        if (Bot.Mover.Moving)
        {
            Bot.Mover.ActivePath?.RequestStartSprint(SAINComponent.Classes.Mover.ESprintUrgency.None, "enemy in sight");
        }

        if (RecalcPathTimer < Time.time)
        {
            RecalcPathTimer = Time.time + 2f;
            BotOwner.MoveToEnemyData.TryMoveToEnemy(movePos);
        }

        if (!Bot.Steering.SteerByPriority(null, false))
        {
            Bot.Steering.LookToMovingDirection();
            //SAIN.Steering.LookToPoint(movePos + Vector3.up * 1f);
        }
        
    }

    public override void OnSteeringTicked()
    {
        if (!TryShootAnyTarget(Bot.GoalEnemy) && !Bot.Steering.LookToMovingDirection()) {
            Bot.Steering.LookToLastKnownEnemyPosition(Bot.GoalEnemy);
        }
    }

    private bool CheckShoot(Enemy enemy)
    {
        float distance = enemy.RealDistance;
        bool enemyLookAtMe = enemy.EnemyLookingAtMe;
        float EffDist = Bot.Info.WeaponInfo.EffectiveWeaponDistance;

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
}
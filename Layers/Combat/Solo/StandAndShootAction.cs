using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes.Transform;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace.PersonClasses;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers.Combat.Solo;

public class StandAndShootAction(BotOwner bot) : BotAction(bot, nameof(StandAndShootAction)), IBotAction
{
    public override void Update(CustomLayer.ActionData data)
    {
        Enemy enemy = Bot.GoalEnemy;
        //if (!shallMoveShoot)
        //{
            Bot.Mover.Pose.SetPoseToCover(enemy);
        //}
    }

    private bool shallMoveShoot = false;

    public override void Start()
    {
        const float STAND_AND_SHOOT_HOLDLEAN_DURATION = 0.66f;
        base.Start();
        shallMoveShoot = moveShoot(Bot.GoalEnemy);
        if (!shallMoveShoot)
        {
            Bot.Mover.Stop();
        }
        Bot.Mover.Lean.HoldLean(STAND_AND_SHOOT_HOLDLEAN_DURATION);
    }

    private bool moveShoot(Enemy enemy)
    {
        if (Bot.Player.IsInPronePose)
        {
            return false;
        }
        if (FindSwingMovePosition(Bot.Transform.NavData, enemy, out Vector3 movePosition))
        {
            return Bot.Mover.WalkToPoint(movePosition, false);
        }
        return false;
    }

    private static bool FindSwingMovePosition(PlayerNavData navData, Enemy enemy, out Vector3 movePosition)
    {
        movePosition = Vector3.zero;
        if (enemy != null && 
            navData.IsOnNavMesh &&
            enemy.RealDistance < 50)
        {
            float angle = UnityEngine.Random.Range(70, 110);
            if (EFTMath.RandomBool())
            {
                angle *= -1;
            }

            Vector3 directionToEnemy = enemy.EnemyDirection.normalized;
            Vector3 rotated = Vector.Rotate(directionToEnemy, 0, angle, 0);
            rotated.y = 0;
            rotated *= 6f;
            rotated += Random.insideUnitSphere;
            if (NavMesh.SamplePosition(navData.Position + rotated, out var hit, 3f, -1))
            {
                movePosition = hit.position;
                if (NavMesh.Raycast(navData.Position, movePosition, out var rayHit, -1))
                {
                    movePosition = rayHit.position;
                }
                if ((movePosition - navData.Position).sqrMagnitude > 0.75f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public override void Stop()
    {
        base.Stop();
    }
}
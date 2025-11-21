using EFT;
using SAIN.Models.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Mover;

public class SteerPriorityClass(SAINSteeringClass steering) : BotSubClass<SAINSteeringClass>(steering)
{
    public ESteerPriority CurrentSteerPriority { get; private set; }
    public ESteerPriority LastSteerPriority { get; private set; }
    public Enemy EnemyWhoLastShotMe { get; private set; }

    /// <summary>
    /// How long a bot will look at where they last saw an enemy instead of something they hear
    /// </summary>
    private readonly float Steer_TimeSinceLocationKnown_Threshold = 3f;

    /// <summary>
    /// How long a bot will look at where they last saw an enemy if they don't hear any other threats
    /// </summary>
    private readonly float Steer_TimeSinceSeen_Long = 60f;

    public ESteerPriority GetCurrentSteerPriority(bool lookRandom, bool ignoreRunningPath, Enemy enemy)
    {
        var lastPriority = CurrentSteerPriority;
        CurrentSteerPriority = FindSteerPriority(lookRandom, ignoreRunningPath, enemy);

        if (CurrentSteerPriority != lastPriority)
            LastSteerPriority = lastPriority;

        return CurrentSteerPriority;
    }

    private ESteerPriority FindSteerPriority(bool lookRandom, bool ignoreRunningPath, Enemy enemy)
    {
        ESteerPriority result = StrickChecks(ignoreRunningPath, enemy);

        if (result != ESteerPriority.None)
        {
            return result;
        }

        result = ReactiveSteering();

        if (result != ESteerPriority.None)
        {
            return result;
        }

        result = SenseSteering();

        if (result != ESteerPriority.None)
        {
            return result;
        }

        if (lookRandom)
        {
            return ESteerPriority.RandomLook;
        }
        return ESteerPriority.None;
    }

    private ESteerPriority StrickChecks(bool ignoreRunningPath, Enemy enemy)
    {
        //if (!ignoreRunningPath && Bot.Mover.Running)
        //    return ESteerPriority.RunningPath;

        if (LookToAimTarget(enemy))
            return ESteerPriority.Aiming;

        if (Bot.ManualShoot.Reason != EShootReason.None
            && Bot.ManualShoot.ShootPosition != Vector3.zero)
            return ESteerPriority.ManualShooting;

        if (EnemyVisible(enemy))
            return ESteerPriority.EnemyVisible;

        return ESteerPriority.None;
    }

    private ESteerPriority ReactiveSteering()
    {
        if (EnemyShotMe())
        {
            return ESteerPriority.LastHit;
        }

        //if (BotOwner.Memory.IsUnderFire && !Bot.Memory.LastUnderFireEnemy.IsCurrentEnemy)
        if (BotOwner.Memory.IsUnderFire)
            return ESteerPriority.UnderFire;

        return ESteerPriority.None;
    }

    private ESteerPriority SenseSteering()
    {
        EnemyPlace lastKnownPlace = Bot.GoalEnemy?.KnownPlaces?.LastKnownPlace;

        if (HeardThreat())
            return ESteerPriority.HeardThreat;

        if (lastKnownPlace != null && lastKnownPlace.TimeSincePositionUpdated < Steer_TimeSinceLocationKnown_Threshold)
            return ESteerPriority.EnemyLastKnown;

        if (lastKnownPlace != null && lastKnownPlace.TimeSincePositionUpdated < Steer_TimeSinceSeen_Long)
            return ESteerPriority.EnemyLastKnownLong;

        return ESteerPriority.None;
    }

    private bool HeardThreat()
    {
        if (BaseClass.HeardSoundSteering.LastHeardVisibleDanger?.ShallLook == true)
        {
            return true;
        }
        if (Bot.Search.SearchActive)
        {
            return false;
        }
        if (BaseClass.HeardSoundSteering.LastHeardDanger?.ShallLook == true)
        {
            return true;
        }
        return false;
    }

    private bool EnemyShotMe()
    {
        float timeSinceShot = Bot.Medical.TimeSinceShot;
        if (timeSinceShot > 3f || timeSinceShot < 0.2f)
        {
            EnemyWhoLastShotMe = null;
            return false;
        }

        Enemy enemy = Bot.Medical.HitByEnemy.EnemyWhoLastShotMe;
        if (enemy != null &&
            enemy.CheckValid() &&
            !enemy.IsCurrentEnemy)
        {
            EnemyWhoLastShotMe = enemy;
            return true;
        }
        EnemyWhoLastShotMe = null;
        return false;
    }

    private bool LookToAimTarget(Enemy enemy)
    {
        return Bot.Aim.AimStatus != AimStatus.NoTarget && (CanSeeAndShoot(enemy) || CanSeeAndShoot(Bot.Shoot.LastShotEnemy));
    }

    private static bool CanSeeAndShoot(Enemy enemy)
    {
        return enemy != null && enemy.IsVisible && enemy.CanShoot;
    }

    private static bool EnemyVisible(Enemy enemy)
    {
        if (enemy != null)
        {
            if (enemy.IsVisible)
            {
                return true;
            }

            if (enemy.Seen &&
                enemy.TimeSinceSeen < 0.5f)
            {
                return true;
            }
        }
        return false;
    }
}
using SAIN.Components;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class BotSurgery : BotBase
{
    public BotSurgery(BotComponent bot) : base(bot)
    {
        CanEverTick = false;
    }

    public bool SurgeryStarted {
        get
        {
            return _surgeryStarted;
        }
        set
        {
            if (_surgeryStarted != value && value)
            {
                SurgeryStartTime = Time.time;
            }
            _surgeryStarted = value;
        }
    }

    public void StartSurgery()
    {

    }

    public float SurgeryStartTime { get; private set; }

    private bool _surgeryStarted;

    public bool AreaClearForSurgery { get; private set; }

    public bool CheckAreaClearForSurgery()
    {
        return CheckCanStartUsingKit() && CheckEnemies();
    }

    public bool CheckCanStartUsingKit()
    {
        return BotOwner?.Medecine?.FirstAid?.IsBleeding == false && BotOwner?.Medecine?.SurgicalKit?.ShallStartUse() == true;
    }

    private bool CheckEnemies()
    {
        if (_nextCheckEnemiesTime < Time.time)
        {
            _nextCheckEnemiesTime = Time.time + 0.1f;
            const float minPathDist = 80f;
            const float minTimeSinceLastKnown = 60f;
            const float minTimeSinceSeen = 60f;

            _allClear = true;
            EnemyList enemies = Bot.EnemyController.KnownEnemies;
            foreach (Enemy enemy in enemies)
            {
                if (enemy.IsVisible)
                {
                    _allClear = false;
                    break;
                }
                if (enemy.Seen && enemy.TimeSinceSeen < minTimeSinceSeen)
                {
                    _allClear = false;
                    break;
                }
                if (enemy.TimeSinceLastKnownUpdated < minTimeSinceLastKnown)
                {
                    _allClear = false;
                    break;
                }
                if (enemy.Path.PathLength < minPathDist)
                {
                    _allClear = false;
                    break;
                }
            }
        }
        return _allClear;
    }

    private bool checkEnemies(float minPathDist, float minTimeSinceLastKnown)
    {
        bool allClear = true;
        var enemies = Bot.EnemyController.Enemies;
        foreach (var enemy in enemies.Values)
        {
            if (!checkThisEnemy(enemy, minPathDist, minTimeSinceLastKnown))
            {
                allClear = false;
                break;
            }
        }
        return allClear;
    }

    private bool checkThisEnemy(Enemy enemy, float minPathDist, float minTimeSinceLastKnown)
    {
        if (enemy?.EnemyPlayer?.HealthController.IsAlive == true
            && (enemy.Seen || enemy.Heard)
            && enemy.TimeSinceLastKnownUpdated < 360f)
        {
            if (enemy.IsVisible)
            {
                return false;
            }
            if (enemy.TimeSinceLastKnownUpdated < minTimeSinceLastKnown)
            {
                return false;
            }
            if (enemy.Path.PathLength < minPathDist)
            {
                return false;
            }
        }
        return true;
    }

    private bool _allClear;
    private float _nextCheckEnemiesTime;
}
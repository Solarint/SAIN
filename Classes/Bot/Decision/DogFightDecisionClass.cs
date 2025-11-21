using SAIN.Components;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Decision;

public class DogFightDecisionClass : BotBase
{
    public bool DogFightActive => _lastDogFightTarget != null;

    public DogFightDecisionClass(BotComponent bot) : base(bot)
    {
        CanEverTick = false;
    }

    public override void Init()
    {
        Bot.EnemyController.Events.OnEnemyRemoved += checkClear;
        base.Init();
    }

    public override void Dispose()
    {
        Bot.EnemyController.Events.OnEnemyRemoved -= checkClear;
        base.Dispose();
    }

    public bool CheckShallDogFight(EnemyList KnownEnemies, out Enemy result)
    {
        BotWeaponManager weaponManager = BotOwner?.WeaponManager;
        if (weaponManager == null || !weaponManager.HaveBullets || weaponManager.Reload.Reloading)
        {
            _lastDogFightTarget = null;
            result = null;
            return false;
        }
        var decision = Bot.Decision.CurrentCombatDecision;
        switch (decision)
        {
            case ECombatDecision.RushEnemy:
                result = null;
                return false;

            case ECombatDecision.Retreat:
            case ECombatDecision.SeekCover:
                if (decision == ECombatDecision.SeekCover && !Bot.Cover.SprintingToCover)
                {
                    break;
                }
                if (Bot.Decision.SelfActionDecisions.LowOnAmmo(0.3f))
                {
                    result = null;
                    return false;
                }
                break;

            default:
                break;
        }

        if (!KnownEnemies.Contains(_lastDogFightTarget)) _lastDogFightTarget = null;
        if (_lastDogFightTarget != null)
        {
            if (ShallDogfightEnemy(_lastDogFightTarget))
            {
                result = _lastDogFightTarget;
                return true;
            }
            if (shallClearDogfightTarget(_lastDogFightTarget))
            {
                _lastDogFightTarget = null;
            }
        }

        if (_changeDFTargetTime < Time.time)
        {
            _changeDFTargetTime = Time.time + 0.5f;
            KnownEnemies.Sort((x, y) => x.Path.PathLength.CompareTo(y.Path.PathLength));
            for (int i = 0; i < KnownEnemies.Count; i++)
            {
                Enemy enemy = KnownEnemies[i];
                if (ShallDogfightEnemy(enemy))
                {
                    _lastDogFightTarget = enemy;
                    result = enemy;
                    return true;
                }
            }
        }
        result = _lastDogFightTarget;
        return _lastDogFightTarget != null;
    }

    private bool ShallDogfightEnemy(Enemy enemy)
    {
        return enemy.EnemyKnown && enemy.LastKnownPosition != null && enemy.Path.PathLength <= Bot.Info.PersonalitySettings.General.DOGFIGHT_PATH_DIST_START && 
            ((enemy.Seen && enemy.TimeSinceSeen < Bot.Info.PersonalitySettings.General.DOGFIGHT_TIMESINCESEEN_START) 
            ||  enemy.Status.ShotMeRecently);
    }

    private bool shallClearDogfightTarget(Enemy enemy)
    {
        if (!enemy.EnemyKnown || enemy.LastKnownPosition == null)
        {
            return true;
        }
        float pathDist = enemy.Path.PathLength;
        var settings = Bot.Info.PersonalitySettings.General;
        if (pathDist > settings.DOGFIGHT_PATH_DIST_END)
        {
            return true;
        }
        return !enemy.IsVisible && enemy.TimeSinceSeen > settings.DOGFIGHT_TIMESINCESEEN_END;
    }

    private float _changeDFTargetTime;

    private Enemy _lastDogFightTarget;

    private void checkClear(string profileID, Enemy enemy)
    {
        if (_lastDogFightTarget != null &&
            _lastDogFightTarget.EnemyProfileId == profileID)
        {
            _lastDogFightTarget = null;
        }
    }
}
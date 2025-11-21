using EFT;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Mover;

public struct SoundStruct
{
    private const float TIME_TO_LOOK = 3f;
    private const float TIME_TO_CLEAR = 10f;

    public SoundStruct(Enemy enemy, EnemyPlace place)
    {
        Enemy = enemy;
        Place = place;
    }

    public readonly Enemy Enemy;

    public readonly EnemyPlace Place;

    public float TimeSinceHeard => Place.TimeSincePositionUpdated;

    public Vector3 Position => Place.Position;

    public bool ShallLook =>
        clearForLook &&
        TimeSinceHeard < TIME_TO_LOOK;

    private bool clearForLook => Place != null && Enemy != null && Enemy.WasValid;

    public bool ShallClear => !clearForLook || TimeSinceHeard >= TIME_TO_CLEAR;
}

public class HeardSoundSteeringClass : BotSubClass<SAINSteeringClass>, IBotClass
{
    public SoundStruct? LastHeardDanger { get; private set; }
    public SoundStruct? LastHeardVisibleDanger { get; private set; }

    public HeardSoundSteeringClass(SAINSteeringClass steering) : base(steering)
    {
    }

    public override void Init()
    {
        Bot.EnemyController.Events.OnEnemyHeard += enemyHeard;
        base.Init();
    }

    public override void ManualUpdate()
    {
        checkClearPlaces();
        base.ManualUpdate();
    }

    private void checkClearPlaces()
    {
        if (_nextCheckClearTime < Time.time)
        {
            _nextCheckClearTime = Time.time + CHECK_CLEAR_FREQ;
            if (LastHeardDanger?.ShallClear == true)
            {
                clearPlace(LastHeardDanger.Value.Place);
            }
            if (LastHeardVisibleDanger?.ShallClear == true)
            {
                clearPlace(LastHeardVisibleDanger.Value.Place);
            }
        }
    }

    private float _nextCheckClearTime;
    private const float CHECK_CLEAR_FREQ = 1f;

    private void clearPlace(EnemyPlace place)
    {
        if (place == null)
        {
            return;
        }
        if (LastHeardDanger?.Place == place)
        {
            LastHeardDanger.Value.Place.OnDispose -= clearPlace;
            LastHeardDanger = null;
        }
        if (LastHeardVisibleDanger?.Place == place)
        {
            LastHeardVisibleDanger.Value.Place.OnDispose -= clearPlace;
            LastHeardVisibleDanger = null;
        }
    }

    public override void Dispose()
    {
        Bot.EnemyController.Events.OnEnemyHeard -= enemyHeard;
        base.Dispose();
    }

    public void LookToHeardPosition()
    {
        var visibleDanger = LastHeardVisibleDanger;
        if (visibleDanger?.ShallLook == true)
        {
            if (visibleDanger.Value.Enemy.InLineOfSight)
            {
                BaseClass.LookToFloorPoint(visibleDanger.Value.Position);
                return;
            }
            if (visibleDanger.Value.Enemy.GetVisibilePathPoint(out Vector3 point))
            {
                BaseClass.LookToPoint(point);
                return;
            }
            BaseClass.LookToFloorPoint(visibleDanger.Value.Position);
            return;
        }
        var heardSound = LastHeardDanger;
        if (heardSound?.ShallLook == true)
        {
            if (heardSound.Value.Enemy.InLineOfSight)
            {
                BaseClass.LookToFloorPoint(heardSound.Value.Position);
                return;
            }
            if (heardSound.Value.Enemy.GetVisibilePathPoint(out Vector3 point))
            {
                BaseClass.LookToPoint(point);
                return;
            }
            BaseClass.LookToFloorPoint(heardSound.Value.Position);
            return;
        }
        BaseClass.LookToRandomPosition();
    }

    private void enemyHeard(Enemy enemy, SAINSoundType soundType, bool isDanger, EnemyPlace place)
    {
        if (place == null)
        {
            return;
        }
        if (!place.Visible)
        {
            return;
        }
        if (!isDanger)
        {
            return;
        }
        if (place.PlaceData.OwnerID == Bot.ProfileId)
        {
            setLastVisSound(place, enemy);
            return;
        }
        var myEnemy = Bot.EnemyController.GetEnemy(enemy.EnemyProfileId, true);
        if (myEnemy == null)
        {
            return;
        }
        if (myEnemy.InLineOfSight)
        {
            setLastVisSound(place, myEnemy);
            return;
        }
    }

    private void setLastVisSound(EnemyPlace place, Enemy enemy)
    {
        if (place.Visible && place.PlaceData.Owner == Bot)
        {
            LastHeardVisibleDanger = new SoundStruct(enemy, place);
            return;
        }
        var activeEnemy = Bot.GoalEnemy;
        if (activeEnemy == null || enemy != activeEnemy)
        {
            LastHeardDanger = new SoundStruct(enemy, place);
        }
    }
}
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Models.Structs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public struct PlaceData
{
    public bool IsAI;
    public Enemy OwnerEnemy;
    public BotComponent Owner;
    public string OwnerID;
}

public enum EEnemyPlaceType
{
    Vision,
    Hearing,
    Flashlight,
    Injury,
}

public class EnemyPlace : IDisposable
{
    public event Action<EnemyPlace> OnDispose;

    public PlaceData PlaceData { get; }
    public EEnemyPlaceType PlaceType { get; }
    public SAINSoundType? SoundType { get; set; }

    public bool Visible { get; private set; }
    public bool IsDanger { get; set; }

    public Vector3 BotPositionWhenUpdated { get; protected set; }
    public Vector3 EnemyRealPositionWhenUpdated { get; protected set; }

    public Dictionary<EBodyPart, Vector3> BodyPartPositions { get; } = [];

    public Vector3 EnemyHeadAtPosition()
    {
        if (BodyPartPositions.ContainsKey(EBodyPart.Head))
        {
            return BodyPartPositions[EBodyPart.Head];
        }
        return Position + Vector3.up * 1.5f;
    }

    public bool ShallClear {
        get
        {
                return PlayerLeftArea;
        }
    }

    private bool PlayerLeftArea {
        get
        {
            if (_nextCheckLeaveTime < Time.time)
            {
                _nextCheckLeaveTime = Time.time + ENEMY_DIST_TO_PLACE_CHECK_FREQ;
                // If the person this place was created for is AI and left the area, just forget it and move on.
                float dist = DistanceToEnemyRealPosition;
                if (PlaceData.IsAI)
                {
                    return dist > ENEMY_DIST_TO_PLACE_FOR_LEAVE_AI;
                }
                return dist > ENEMY_DIST_TO_PLACE_FOR_LEAVE;
            }
            return false;
        }
    }

    public void SetDistances(float BotDistanceToPlace, float EnemyDistanceToPlace, BotComponent bot)
    {
        if (bot == PlaceData.Owner)
        {
            DistanceToBot = BotDistanceToPlace;
            if (BotDistanceToPlace < 1f)
            {
                HasArrivedPersonal = true;
            }
        }
        else if (BotDistanceToPlace < 1f)
        {
            HasArrivedSquad = true;
        }
        DistanceToEnemyRealPosition = EnemyDistanceToPlace;
    }

    public void SetVisibilityOfPlace(bool Value, BotComponent bot)
    {
        if (bot == PlaceData.Owner)
        {
            Visible = Value;
            if (Value)
            {
                HasSeenPersonal = true;
            }
        }
        else if (Value)
        {
            HasSeenSquad = true;
        }
    }

    private const float ENEMY_DIST_TO_PLACE_CHECK_FREQ = 10;
    private const float ENEMY_DIST_TO_PLACE_FOR_LEAVE = 150;
    private const float ENEMY_DIST_TO_PLACE_FOR_LEAVE_AI = 125f;

    public EnemyPlace(PlaceData placeData, Vector3 position, bool isDanger, EEnemyPlaceType placeType, SAINSoundType? soundType)
    {
        PlaceData = placeData;
        Visible = placeData.OwnerEnemy.InLineOfSight;
        IsDanger = isDanger;
        PlaceType = placeType;
        SoundType = soundType;
        SetPosition(position);
    }

    public EnemyPlace(PlaceData placeData, SAINHearingReport report)
    {
        PlaceData = placeData;
        Visible = placeData.OwnerEnemy.InLineOfSight;
        IsDanger = report.isDanger;
        PlaceType = report.placeType;
        SoundType = report.soundType;
        SetPosition(report.position);
    }

    private void SetPosition(Vector3 position)
    {
        BotPositionWhenUpdated = PlaceData.Owner.Position;
        EnemyRealPositionWhenUpdated = PlaceData.OwnerEnemy.EnemyPosition;
        _position = position;
        updateDistancesNow(position);
        SetLastKnownPartPositions(PlaceData.OwnerEnemy);
        _timeLastUpdated = Time.time;
    }

    private void SetLastKnownPartPositions(Enemy enemy)
    {
        BodyPartPositions.Clear();
        Vector3 position = _position;
        Vector3 enemyRealPos = enemy.EnemyPosition;
        foreach (EnemyPartDataClass part in enemy.Vision.EnemyParts.PartsArray)
        {
            Vector3 translatedPartPos = part.Transform.position - enemyRealPos + position;
            BodyPartPositions.Add(part.BodyPart, translatedPartPos);
        }
    }

    public void Dispose()
    {
        OnDispose?.Invoke(this);
    }

    public Vector3 Position {
        get
        {
            return _position;
        }
    }

    public void UpdatePosition(Vector3 value)
    {
        checkNewValue(value, _position);
        _position = value;
        _timeLastUpdated = Time.time;
        BotPositionWhenUpdated = PlaceData.Owner.Position;
        Visible = PlaceData.OwnerEnemy.InLineOfSight;
        SetLastKnownPartPositions(PlaceData.OwnerEnemy);
        HasArrivedPersonal = false;
        HasArrivedSquad = false;
        HasSeenPersonal = false;
        HasSeenSquad = false;
    }

    private void checkNewValue(Vector3 value, Vector3 oldValue)
    {
        if ((value - oldValue).sqrMagnitude > ENEMY_DIST_RECHECK_MIN_SQRMAG)
            updateDistancesNow(value);
    }

    private const float ENEMY_DIST_RECHECK_MIN_SQRMAG = 0.25f;

    public float TimeSincePositionUpdated => Time.time - _timeLastUpdated;
    public float DistanceToBot { get; private set; }
    public float DistanceToEnemyRealPosition { get; private set; }

    private void updateDistancesNow(Vector3 position)
    {
        DistanceToBot = (position - PlaceData.Owner.Position).magnitude;
        DistanceToEnemyRealPosition = (position - PlaceData.OwnerEnemy.EnemyTransform.Position).magnitude;
    }

    public float Distance(Vector3 point)
    {
        return (_position - point).magnitude;
    }

    public float DistanceSqr(Vector3 toPoint)
    {
        return (_position - toPoint).sqrMagnitude;
    }

    public bool HasArrivedPersonal {
        get
        {
            return _hasArrivedPers;
        }
        set
        {
            if (value)
            {
                _timeArrivedPers = Time.time;
                HasSeenPersonal = true;
            }
            _hasArrivedPers = value;
        }
    }

    public bool HasArrivedSquad {
        get
        {
            return _hasArrivedSquad;
        }
        set
        {
            if (value)
            {
                _timeArrivedSquad = Time.time;
            }
            _hasArrivedSquad = value;
        }
    }

    public bool HasSeenPersonal {
        get
        {
            return _hasSeenPers;
        }
        set
        {
            if (value)
            {
                _timeSeenPers = Time.time;
            }
            _hasSeenPers = value;
        }
    }

    public bool HasSeenSquad {
        get
        {
            return _hasSquadSeen;
        }
        set
        {
            if (value)
            {
                _timeSquadSeen = Time.time;
            }
            _hasSquadSeen = value;
        }
    }

    private Vector3 _position;
    private float _nextCheckLeaveTime;
    public float _timeLastUpdated;
    private bool _hasArrivedPers;
    public float _timeArrivedPers;
    private bool _hasArrivedSquad;
    public float _timeArrivedSquad;
    private bool _hasSeenPers;
    public float _timeSeenPers;
    private bool _hasSquadSeen;
    public float _timeSquadSeen;
}
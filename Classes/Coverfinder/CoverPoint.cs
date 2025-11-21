using EFT;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using SAIN.Models.Structs;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.SubComponents.CoverFinder;

public class CoverPointClass
{
    public CoverPointClass(Collider collider, Vector3 colliderPos, Vector3 coverPosition)
    {
        Collider = collider;
        ColliderPosition = colliderPos;
        Vector3 size = collider.bounds.size;
        Height = size.y;
        Value = (size.x + size.y + size.z).Round10();
        Id = _count;
        _count++;

        _position = coverPosition;
        Vector3 dir = ColliderPosition - coverPosition;
        dir.y = 0;
        ProtectionDirection = dir.normalized;
        TimeLastUpdated = Time.time;
    }

    public event Action<Vector3> OnPositionUpdated;

    public Collider Collider { get; }
    public Vector3 ColliderPosition { get; }
    public int Id { get; }
    public float Height { get; }
    public float Value { get; }

    public bool IsInUse { get; set; }
    public Vector3 ProtectionDirection { get; private set; }
    public float TimeLastUpdated { get; private set; }
    public float TimeSinceUpdated => Time.time - TimeLastUpdated;

    public Vector3 CoverPosition {
        get
        {
            return _position;
        }
        set
        {
            var current = _position;
            if ((value - current).sqrMagnitude < 0.001)
            {
                //Logger.LogWarning($"new Pos is the same as old pos!");
                return;
            }
            _position = value;
            Vector3 dir = ColliderPosition - value;
            dir.y = 0;
            ProtectionDirection = dir.normalized;
            TimeLastUpdated = Time.time;
            OnPositionUpdated?.Invoke(value);
        }
    }

    private Vector3 _position;

    private static int _count = 0;
}

public class CoverPointBotDataClass
{
    private const int SPOTTED_HITINCOVER_COUNT_TOTAL = 3;
    private const int SPOTTED_HITINCOVER_COUNT_THIRDPARTY = 1;
    private const int SPOTTED_HITINCOVER_COUNT_CANTSEE = 2;
    private const int SPOTTED_HITINCOVER_COUNT_LEGS = 2;
    private const int SPOTTED_HITINCOVER_COUNT_UNKNOWN = 1;
    private const float HITINCOVER_MAX_DAMAGE = 120f;
    private const float HITINCOVER_MIN_DAMAGE = 40f;
    private const float HITINCOVER_DAMAGE_COEF = 3f;
    private const float DIST_COVER_INCOVER = 1f;
    private const float DIST_COVER_INCOVER_STAY = 1.25f;
    private const float DIST_COVER_CLOSE = 10f;
    private const float DIST_COVER_MID = 20f;

    public CoverPointClass CoverPoint { get; }

    public Vector3 Position {
        get
        {
            return CoverPoint.CoverPosition;
        }
        set
        {
            CoverPoint.CoverPosition = value;
        }
    }

    public float BotDistance { get; set; }

    public void SetInUse(bool inInUse)
    {
        CoverPoint.IsInUse = inInUse;
    }

    public bool Spotted {
        get
        {
            // are we already spotted? check if it has expired
            var hits = _hitsInCover;
            if (hits.Spotted)
            {
                if (Time.time - hits.TimeSpotted > SpottedCoverPoint.SPOTTED_PERIOD)
                    ResetGetHit();

                return hits.Spotted;
            }

            // we aren't currently spotted, check to make sure we weren't hit too many times
            hits.Spotted = CheckSpotted();

            if (hits.Spotted)
                hits.TimeSpotted = Time.time;

            return hits.Spotted;
        }
    }

    public CoverStatus StraightDistanceStatus {
        get
        {
            float distance = BotDistance;
            if (_straightDistStatus == CoverStatus.InCover && distance <= DIST_COVER_INCOVER_STAY)
            {
                return _straightDistStatus;
            }
            _straightDistStatus = CheckStatus(distance);
            return _straightDistStatus;
        }
    }

    private CoverStatus _straightDistStatus;

    public CoverStatus PathDistanceStatus {
        get
        {
            float pathLength = PathData.PathLength;
            if (_pathLengthStatus == CoverStatus.InCover && pathLength <= DIST_COVER_INCOVER_STAY)
            {
                return _pathLengthStatus;
            }
            _pathLengthStatus = CheckStatus(pathLength);
            return _pathLengthStatus;
        }
    }

    private CoverStatus _pathLengthStatus;

    public NavMeshPath PathToPoint => PathData.Path;
    public float CoverHeight => CoverPoint.Height;
    public Collider Collider => CoverPoint.Collider;
    public float LastHitInCoverTime { get; private set; }

    public void GetHit(DamageInfoStruct DamageInfoStruct, EBodyPart partHit, Enemy currentEnemy)
    {
        int hitCount = CalcHitCount(DamageInfoStruct);
        bool islegs = partHit.IsLegs();

        var hits = _hitsInCover;
        LastHitInCoverTime = Time.time;
        hits.Total += hitCount;

        IPlayer hitFrom = DamageInfoStruct.Player?.iPlayer;
        if (currentEnemy == null || hitFrom == null)
        {
            hits.Unknown += hitCount;
            return;
        }

        bool sameEnemy = currentEnemy.EnemyPlayer.ProfileId == hitFrom.ProfileId;
        if (!sameEnemy)
        {
            Enemy thirdParty = Bot.EnemyController.GetEnemy(hitFrom.ProfileId, false);
            if (thirdParty == null)
            {
                hits.Unknown += hitCount;
                return;
            }

            // Did I get shot in the legs and can't see them?
            if (islegs && !thirdParty.IsVisible)
                hits.Legs += hitCount;

            // Did the player who shot me shoot me from a direction that this cover doesn't protect from?
            //if (Vector3.Dot(thirdParty.EnemyDirectionNormal, CoverData.ProtectionDirection) < 0.25f)
            //    hits.ThirdParty += hitCount;

            return;
        }

        if (!currentEnemy.IsVisible)
        {
            if (islegs)
                hits.Legs += hitCount;

            hits.CantSee += hitCount;
        }
    }

    public void ResetGetHit()
    {
        _hitsInCover.Reset();
    }

    public CoverPointBotDataClass(BotComponent bot, CoverPointClass coverPoint, PathData pathData)
    {
        Bot = bot;
        CoverPoint = coverPoint;
        PathData = pathData;
    }

    public PathData PathData { get; }

    private CoverHitCounts _hitsInCover { get; } = new CoverHitCounts();

    private static CoverStatus CheckStatus(float distance)
    {
        if (distance <= DIST_COVER_INCOVER)
            return CoverStatus.InCover;

        if (distance <= DIST_COVER_CLOSE)
            return CoverStatus.CloseToCover;

        if (distance <= DIST_COVER_MID)
            return CoverStatus.MidRangeToCover;

        return CoverStatus.FarFromCover;
    }

    private bool CheckSpotted()
    {
        var hits = _hitsInCover;
        int total = hits.Total;
        if (total == 0) return false;

        if (total >= SPOTTED_HITINCOVER_COUNT_TOTAL)
            return true;

        if (hits.CantSee >= SPOTTED_HITINCOVER_COUNT_CANTSEE)
            return true;

        if (hits.Unknown >= SPOTTED_HITINCOVER_COUNT_UNKNOWN)
            return true;

        if (hits.ThirdParty >= SPOTTED_HITINCOVER_COUNT_THIRDPARTY)
            return true;

        if (hits.Legs >= SPOTTED_HITINCOVER_COUNT_LEGS)
            return true;

        return false;
    }

    private static int CalcHitCount(DamageInfoStruct DamageInfoStruct)
    {
        float received = DamageInfoStruct.Damage;
        float max = HITINCOVER_MAX_DAMAGE;
        float maxCoef = HITINCOVER_DAMAGE_COEF;
        if (received >= max)
        {
            return Mathf.RoundToInt(maxCoef);
        }
        float min = HITINCOVER_MIN_DAMAGE;
        float minCoef = 1f;
        if (received <= min)
        {
            return Mathf.RoundToInt(minCoef);
        }

        float num = max - min;
        float diff = received - min;
        float result = Mathf.Lerp(min, max, diff / num);
        return Mathf.RoundToInt(result);
    }

    private readonly BotComponent Bot;
}

public class CoverPoint
{
    public event Action<Vector3> OnPositionUpdated;

    private const int SPOTTED_HITINCOVER_COUNT_TOTAL = 3;
    private const int SPOTTED_HITINCOVER_COUNT_THIRDPARTY = 1;
    private const int SPOTTED_HITINCOVER_COUNT_CANTSEE = 2;
    private const int SPOTTED_HITINCOVER_COUNT_LEGS = 2;
    private const int SPOTTED_HITINCOVER_COUNT_UNKNOWN = 1;
    private const float HITINCOVER_MAX_DAMAGE = 120f;
    private const float HITINCOVER_MIN_DAMAGE = 40f;
    private const float HITINCOVER_DAMAGE_COEF = 3f;
    private const float CHECKDIST_MAX_DIST = 50;
    private const float CHECKDIST_MIN_DIST = 10;
    private const float CHECKDIST_MAX_DELAY = 1f;
    private const float CHECKDIST_MIN_DELAY = 0.1f;
    private const float DIST_COVER_INCOVER = 1.25f;
    private const float DIST_COVER_INCOVER_STAY = 1.5f;
    private const float DIST_COVER_CLOSE = 10f;
    private const float DIST_COVER_MID = 20f;

    public CoverData CoverData { get; } = new CoverData();

    public Vector3 Position {
        get
        {
            return CoverData.Position;
        }
        set
        {
            var current = CoverData.Position;
            if ((value - current).sqrMagnitude < 0.001)
            {
                //Logger.LogWarning($"new Pos is the same as old pos!");
                return;
            }
            updateDirAndPos(value);
            OnPositionUpdated?.Invoke(value);
        }
    }

    public DirectionData DirectionData => CoverData.DirectionData;

    public float DistanceToBot { get; set; }

    public float GetDistance(Vector3 from)
    {
        return (Position - from).magnitude;
    }

    public float GetDistanceSqr(Vector3 from)
    {
        return (Position - from).sqrMagnitude;
    }

    public bool Spotted {
        get
        {
            // are we already spotted? check if it has expired
            var hits = _hitsInCover;
            if (hits.Spotted)
            {
                if (Time.time - hits.TimeSpotted > SpottedCoverPoint.SPOTTED_PERIOD)
                    ResetGetHit();

                return hits.Spotted;
            }

            // we aren't currently spotted, check to make sure we weren't hit too many times
            hits.Spotted = checkSpotted();

            if (hits.Spotted)
                hits.TimeSpotted = Time.time;

            return hits.Spotted;
        }
    }

    public CoverStatus StraightDistanceStatus {
        get
        {
            float distance = UpdateDist(Bot.Transform.NavData.Position);
            return GetStraightDistanceStatus(distance);
        }
    }

    public CoverStatus GetStraightDistanceStatus(float distance)
    {
        _nextGetDistTime = Time.time + 0.25f;
        DistanceToBot = distance;
        if (CoverData.StraightLengthStatus == CoverStatus.InCover && distance <= DIST_COVER_INCOVER_STAY)
        {
            CoverData.StraightLengthStatus = CoverStatus.InCover;
            return CoverStatus.InCover;
        }
        CoverData.StraightLengthStatus = checkStatus(distance);
        return CoverData.StraightLengthStatus;
    }

    private float UpdateDist(Vector3 from)
    {
        if (_nextGetDistTime < Time.time)
        {
            _nextGetDistTime = Time.time + 0.25f;
            DistanceToBot = GetDistance(from);
        }
        return DistanceToBot;
    }

    public CoverStatus PathDistanceStatus {
        get
        {
            float pathLength = PathData.PathLength;
            if (CoverData.PathLengthStatus == CoverStatus.InCover &&
                pathLength <= DIST_COVER_INCOVER_STAY)
            {
                CoverData.PathLengthStatus = CoverStatus.InCover;
                return CoverStatus.InCover;
            }
            CoverData.PathLengthStatus = checkStatus(pathLength);
            return CoverData.PathLengthStatus;
        }
    }

    public NavMeshPath PathToPoint => PathData.Path;
    public float CoverHeight => HardData.Height;
    public Collider Collider { get; }
    public float LastHitInCoverTime { get; private set; }
    public bool IsCurrent => Bot.Cover.CoverInUse == this;

    public bool ShallUpdate(string targetProfileId)
    {
        float timeSinceUpdated = CoverData.TimeSinceUpdated;
        if (_lastCheckedProfileId != targetProfileId)
        {
            if (timeSinceUpdated >= 0.1f)
            {
                _lastCheckedProfileId = targetProfileId;
                return true;
            }
            return false;
        }
        if (Bot.Cover.CoverInUse == this)
        {
            return timeSinceUpdated >= 0.25f;
        }
        return timeSinceUpdated >= 1f;
    }

    private string _lastCheckedProfileId;
    public int RoundedPathLength => PathData.RoundedPathLength;
    public bool BotInThisCover => IsCurrent && (StraightDistanceStatus == CoverStatus.InCover || PathDistanceStatus == CoverStatus.InCover);

    public void GetHit(DamageInfoStruct DamageInfoStruct, EBodyPart partHit, Enemy currentEnemy)
    {
        int hitCount = calcHitCount(DamageInfoStruct);
        bool islegs = partHit.IsLegs();

        var hits = _hitsInCover;
        LastHitInCoverTime = Time.time;
        hits.Total += hitCount;

        IPlayer hitFrom = DamageInfoStruct.Player?.iPlayer;
        if (currentEnemy == null || hitFrom == null)
        {
            hits.Unknown += hitCount;
            return;
        }

        bool sameEnemy = currentEnemy.EnemyPlayer.ProfileId == hitFrom.ProfileId;
        if (!sameEnemy)
        {
            Enemy thirdParty = Bot.EnemyController.GetEnemy(hitFrom.ProfileId, false);
            if (thirdParty == null)
            {
                hits.Unknown += hitCount;
                return;
            }

            // Did I get shot in the legs and can't see them?
            if (islegs && thirdParty.IsVisible == false)
                hits.Legs += hitCount;

            // Did the player who shot me shoot me from a direction that this cover doesn't protect from?
            if (Vector3.Dot(thirdParty.EnemyDirectionNormal, CoverData.ProtectionDirection) < 0.25f)
                hits.ThirdParty += hitCount;

            return;
        }

        if (!currentEnemy.IsVisible)
        {
            if (islegs)
                hits.Legs += hitCount;

            hits.CantSee += hitCount;
        }
    }

    public void ResetGetHit()
    {
        _hitsInCover.Reset();
    }

    public CoverPoint(BotComponent bot, Collider collider, PathData pathData, Vector3 coverPosition)
    {
        Bot = bot;
        Collider = collider;
        PathData = pathData;
        Vector3 size = collider.bounds.size;
        HardData = new SAINHardCoverData {
            Id = _count,
            Height = size.y,
            Value = (size.x + size.y + size.z).Round10(),
        };
        updateDirAndPos(coverPosition);
        _count++;
    }

    public SAINHardCoverData HardData { get; }

    public PathData PathData { get; }

    private CoverHitCounts _hitsInCover { get; } = new CoverHitCounts();

    private void updateDirAndPos(Vector3 coverPosition)
    {
        CoverData.Position = coverPosition;
        Vector3 dir = Collider.transform.position - coverPosition;
        dir.y = 0;
        CoverData.ProtectionDirection = dir.normalized;
        CoverData.TimeLastUpdated = Time.time;
    }

    private CoverStatus checkStatus(float distance)
    {
        if (distance <= DIST_COVER_INCOVER)
            return CoverStatus.InCover;

        if (distance <= DIST_COVER_CLOSE)
            return CoverStatus.CloseToCover;

        if (distance <= DIST_COVER_MID)
            return CoverStatus.MidRangeToCover;

        return CoverStatus.FarFromCover;
    }

    private bool checkSpotted()
    {
        var hits = _hitsInCover;
        int total = hits.Total;
        if (total == 0) return false;

        if (total >= SPOTTED_HITINCOVER_COUNT_TOTAL)
            return true;

        if (hits.CantSee >= SPOTTED_HITINCOVER_COUNT_CANTSEE)
            return true;

        if (hits.Unknown >= SPOTTED_HITINCOVER_COUNT_UNKNOWN)
            return true;

        if (hits.ThirdParty >= SPOTTED_HITINCOVER_COUNT_THIRDPARTY)
            return true;

        if (hits.Legs >= SPOTTED_HITINCOVER_COUNT_LEGS)
            return true;

        return false;
    }

    private int calcHitCount(DamageInfoStruct DamageInfoStruct)
    {
        float received = DamageInfoStruct.Damage;
        float max = HITINCOVER_MAX_DAMAGE;
        float maxCoef = HITINCOVER_DAMAGE_COEF;
        if (received >= max)
        {
            return Mathf.RoundToInt(maxCoef);
        }
        float min = HITINCOVER_MIN_DAMAGE;
        float minCoef = 1f;
        if (received <= min)
        {
            return Mathf.RoundToInt(minCoef);
        }

        float num = max - min;
        float diff = received - min;
        float result = Mathf.Lerp(min, max, diff / num);
        return Mathf.RoundToInt(result);
    }

    private float calcMagnitudeDelay(float dist)
    {
        float maxDelay = CHECKDIST_MAX_DELAY;
        float maxDist = CHECKDIST_MAX_DIST;
        if (dist >= maxDist)
        {
            return maxDelay;
        }
        float minDelay = CHECKDIST_MIN_DELAY;
        float minDist = CHECKDIST_MIN_DIST;
        if (dist <= minDist)
        {
            return minDelay;
        }
        float num = maxDist - minDist;
        float diff = dist - minDist;
        float result = Mathf.Lerp(minDelay, maxDelay, diff / num);
        return result;
    }

    private float _nextGetDistTime;
    private static int _count;
    private readonly BotComponent Bot;
}
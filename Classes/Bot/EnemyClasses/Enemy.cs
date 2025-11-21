using EFT;
using SAIN.Classes.Transform;
using SAIN.Components;
using SAIN.Components.BotComponentSpace.Classes.EnemyClasses;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Preset.GlobalSettings;
using SAIN.Types.PlayerSmoothing;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class Enemy : BotBase, ISPlayer
{
    public PredictivePositionSmoother PositionSmoother { get; } = new PredictivePositionSmoother();

    public bool ShallCheckLook(float currentTime, out float deltaTime)
    {
        if (_nextCheckLookTime < currentTime)
        {
            if (!CheckValid())
            {
                deltaTime = 0f;
                return false;
            }
            deltaTime = currentTime - _nextCheckLookTime;
            _nextCheckLookTime = currentTime + LookUpdateInterval();
            return true;
        }
        deltaTime = 0f;
        return false;
    }

    public bool ShallCheckLoS(float currentTime)
    {
        if (_nextCheckLoSTime < currentTime)
        {
            if (!CheckValid()) return false;
            _nextCheckLoSTime = currentTime + LookUpdateInterval();
            return true;
        }
        return false;
    }

    private float LookUpdateInterval()
    {
        float interval;
        if (!Bot.BotActive || !IsEnemyActive(this))
        {
            interval = 0.5f;
        }
        else if (IsCurrentEnemy)
        {
            interval = IsAI ? 1f / 15f : 1f / 30f;
        }
        else if (RealDistance > 200f)
        {
            interval = IsAI ? 1f / 4f : 1f / 12f;
        }
        else if (RealDistance > 100f)
        {
            interval = IsAI ? 1f / 6f : 1f / 12f;
        }
        else if (RealDistance > 50f)
        {
            interval = IsAI ? 1f / 8f : 1f / 12f;
        }
        else
        {
            interval = IsAI ? 1f / 12f : 1f / 16f;
        }
        return interval * UnityEngine.Random.Range(0.75f, 1.25f);
    }

    private float _nextCheckLookTime = 0f;
    private float _nextCheckLoSTime = 0f;

    public Vector3 NavMeshPosition => EnemyTransform.NavData.Position;

    /// <summary>
    /// Enemy player to ProfileId player
    /// </summary>
    public float GetDistanceToPlayer(string ProfileId)
    {
        return EnemyPlayerComponent.GetDistanceToPlayer(ProfileId);
    }

    /// <summary>
    /// Enemy player to ProfileId player
    /// </summary>
    public bool IsPlayerInRange(string ProfileId, float maxDistance, out float playerDistance)
    {
        playerDistance = GetDistanceToPlayer(ProfileId);
        return playerDistance <= maxDistance;
    }

    public void TickEnemy(float currentTime, float forgetEnemyTime, bool botSearching)
    {
        UpdateDistAndDirection();
        if (IsSniper) IsSniper = RealDistance < Bot.Info.PersonalitySettings.General.ENEMYSNIPER_DISTANCE_END;
        KnownChecker.TickEnemy(currentTime, forgetEnemyTime, botSearching);
        Vision.TickEnemy(currentTime);
        Hearing.TickEnemy(currentTime);
        KnownPlaces.TickEnemy(currentTime);
        Status.TickEnemy(currentTime);

#if DEBUG
        if (IsCurrentEnemy && GetVisibilePathPoint(out Vector3 point))
        {
            DebugGizmos.DrawSphere(point, 0.06f, Color.red, 0.02f);
            DebugGizmos.DrawLine(point, Bot.Transform.EyePosition, Color.red, 0.015f, 0.02f);
        }
#endif
    }

    public event Action OnEnemyDisposed;

    public string EnemyName { get; }
    public string EnemyProfileId { get; }
    public PlayerComponent EnemyPlayerComponent { get; }
    public Player EnemyPlayer { get; private set; }
    public PlayerTransformClass EnemyTransform { get; }
    public OtherPlayerData EnemyPlayerData { get; }
    public bool IsAI => EnemyPlayer.IsAI;
    public bool IsZombie => EnemyPlayer.UsedSimplifiedSkeleton;

    /// <summary>
    /// Does this enemy have a usable firearm
    /// </summary>
    /// <returns></returns>
    public bool IsShooter()
    {
        return !IsZombie && EnemyPlayer.HandsController is Player.FirearmController;
    }

    public bool IsSamePlayer(string profileId)
    {
        return EnemyProfileId == profileId;
    }

    public EnemyEvents Events { get; }
    public EnemyKnownPlaces KnownPlaces { get; private set; }
    public SAINEnemyStatus Status { get; }
    public EnemyVisionClass Vision { get; }
    public SAINEnemyPath Path { get; }
    public EnemyInfo EnemyInfo { get; }
    public EnemyAim Aim { get; }
    public EnemyHearing Hearing { get; }

    public bool IsCurrentEnemy => Bot.EnemyController.GoalEnemy == this;

    public float RealDistance { get; private set; }
    public bool IsSniper { get; private set; }
    public Vector3? VisiblePathPoint { get; private set; }
    public float VisiblePathPointDistanceToBot { get; private set; }
    public float VisiblePathPointDistanceToEnemyLastKnown { get; private set; }
    public float? VisiblePathPointSignedAngle { get; private set; }

    public Vector3? SuppressionTarget {
        get
        {
            if (!GlobalSettingsClass.Instance.Mind.TARGET_SUPPRESS_TOGGLE)
            {
                return null;
            }
            Vector3? enemyLastKnown = KnownPlaces.LastKnownPosition;
            if (enemyLastKnown == null)
            {
                return null;
            }
            if (GetVisibilePathPoint(out Vector3 point) && IsTargetInSuppRange(enemyLastKnown.Value, point))
            {
                return point;
            }
            return null;
        }
    }

    public bool EnemyKnown => Events.OnEnemyKnownChanged.Value;
    public bool EnemyNotLooking => IsVisible && !Status.EnemyLookAtMe && !Status.ShotAtMeRecently;
    public bool WasValid { get; private set; } = true;

    public Vector3 CenterMass => FindCenterMass(EnemyPlayerComponent);

    public bool FirstContactOccured => Vision.FirstContactOccured;

    public bool FirstContactReported { get; set; }

    public EPathDistance EPathDistance => Path.EPathDistance;
    public Vector3? LastKnownPosition => KnownPlaces.LastKnownPosition;

    public Vector3 EnemyPosition => EnemyTransform.Position;
    public Vector3 EnemyDirection { get; private set; }
    public Vector3 EnemyDirectionNormal { get; private set; }

    public float TimeSinceLastKnownUpdated => KnownPlaces.TimeSinceLastKnownUpdated;
    public bool InLineOfSight => Vision.InLineOfSight;
    public bool IsVisible => Vision.IsVisible;
    public bool CanShoot => Vision.CanShoot;
    public bool Seen => Vision.Seen;
    public bool Heard => Hearing.Heard;

    public bool EnemyLookingAtMe => Status.EnemyLookAtMe;
    public float TimeSinceSeen => Vision.TimeSinceSeen;
    public float TimeSinceHeard => Hearing.TimeSinceHeard;

    private const float SQUADREPORT_SIGHT_INTERVAL = 0.25f;

    public static bool IsEnemyActive(Enemy enemy)
    {
        if (enemy == null) return false;
        if (!enemy.PlayerComponent.IsActive) return false;
        if (enemy.IsAI)
        {
            BotOwner enemyBotOwner = enemy.EnemyPlayerComponent.BotOwner;
            if (enemyBotOwner == null) return false;
            if (enemyBotOwner.BotState != EBotState.Active) return false;
            if (enemyBotOwner.StandBy.StandByType != BotStandByType.active) return false;
        }
        return true;
    }

    public Enemy(BotComponent bot, PlayerComponent enemyComponent, EnemyInfo enemyInfo) : base(bot)
    {
        EnemyPlayerComponent = enemyComponent;
        EnemyPlayer = enemyComponent.Player;
        EnemyTransform = enemyComponent.Transform;
        EnemyName = $"{enemyComponent.Name} ({enemyComponent.Player.Profile.Nickname})";
        EnemyInfo = enemyInfo;
        EnemyProfileId = enemyComponent.ProfileId;
        OwnerProfileId = bot.ProfileId;

        EnemyPlayerData = bot.PlayerComponent.OtherPlayersData.DataDictionary[enemyComponent.ProfileId];
        UpdateDistAndDirection();

        var _enemyData = new EnemyData(this);
        Events = new EnemyEvents(_enemyData);
        Events.OnEnemyKnownChanged.OnToggle += OnEnemyKnownChanged;
        KnownChecker = new EnemyKnownChecker(_enemyData);
        Status = new SAINEnemyStatus(_enemyData);
        Vision = new EnemyVisionClass(_enemyData);
        Path = new SAINEnemyPath(_enemyData);
        KnownPlaces = new EnemyKnownPlaces(_enemyData);
        Aim = new EnemyAim(_enemyData);
        Hearing = new EnemyHearing(_enemyData);

        _nextCheckLookTime = Time.time + UnityEngine.Random.Range(0, 1f);
    }

    private readonly string OwnerProfileId;

    public override void Init()
    {
        Events.Init(EnemyPlayer);
        KnownChecker.Init(Bot);
        KnownPlaces.Init();
        Vision.Init();
        Path.Init();
        Hearing.Init();
        Status.Init();
        base.Init();
    }

    public override void Dispose()
    {
        OnEnemyDisposed?.Invoke();
        Events.Dispose(EnemyPlayer);
        Events.OnEnemyKnownChanged.OnToggle -= OnEnemyKnownChanged;
        KnownChecker.Dispose(Bot);
        KnownPlaces.Dispose();
        Vision.Dispose();
        Path.Dispose();
        Hearing.Dispose();
        Status.Dispose();
        base.Dispose();
    }

    private void OnEnemyKnownChanged(bool value, Enemy enemy)
    {
        if (!value)
        {
            ClearVisiblePathPoint();
            IsSniper = false;
            FirstContactReported = false;
        }
    }

    private void UpdateVisiblePathPointDist(Vector3 headPosition)
    {
        TryUpdateVisPathDist(headPosition, VisiblePathPoint, LastKnownPosition, out float distToBot, out float distToEnemy);
        VisiblePathPointDistanceToBot = distToBot;
        VisiblePathPointDistanceToEnemyLastKnown = distToEnemy;
    }

    private static void TryUpdateVisPathDist(Vector3 headPosition, Vector3? visPathPoint, Vector3? lastKnown, out float distToBot, out float distToEnemy)
    {
        distToBot = float.MaxValue;
        distToEnemy = float.MaxValue;
        if (visPathPoint == null)
        {
            return;
        }
        if (lastKnown == null)
        {
            return;
        }
        distToBot = (visPathPoint.Value - headPosition).magnitude;
        distToEnemy = (visPathPoint.Value - lastKnown.Value).magnitude;
    }

    public bool GetVisibilePathPoint(out Vector3 pathPoint)
    {
        if (VisiblePathPoint != null)
        {
            pathPoint = VisiblePathPoint.Value;
            if (_visPathPointIsCorner)
            {
                pathPoint += Bot.Steering.WeaponRootOffset;
            }
            return true;
        }
        pathPoint = Vector3.zero;
        return false;
    }

    public void ClearVisiblePathPoint()
    {
        _visPathPointIsCorner = false;
        VisiblePathPoint = null;
        VisiblePathPointSignedAngle = null;
        VisiblePathPointDistanceToBot = float.MaxValue;
        VisiblePathPointDistanceToEnemyLastKnown = float.MaxValue;
    }

    public void SetLastVisiblePathPoint(BotVisiblePathNode node)
    {
        _visPathPointIsCorner = false;
        VisiblePathPoint = node.Point;
        UpdateVisiblePathPointDist(Bot.Transform.EyePosition);
        if (node.CornerStartIndex == node.CornerEndIndex)
        {
            VisiblePathPointSignedAngle = null;
            return;
        }
        Vector3? LastKnown = LastKnownPosition;
        if (LastKnown != null)
        {
            Vector3 botPosition = Bot.Position;
            Vector3 botEyePosition = Bot.Transform.EyePosition;
            botPosition.y = botEyePosition.y;

            //Vector3 otherPoint = point.Point + node.DirectionToCornerNormal * VisiblePathPointDistanceToBot;
            Vector3 cornerA = Path.PathCorners[node.CornerStartIndex];
            Vector3 cornerB = Path.PathCorners[node.CornerEndIndex];
            DebugGizmos.DrawLine(cornerA, cornerB, Color.yellow, 0.05f, 1f);
            VisiblePathPointSignedAngle = Vector.FindFlatSignedAngle(cornerA, cornerB, botPosition);
        }
    }

    public void SetLastCornerAsVisiblePathPoint(Vector3 LastCorner, int CornerIndex)
    {
        _visPathPointIsCorner = true;
        VisiblePathPoint = LastCorner;
        VisiblePathPointSignedAngle = null;
        UpdateVisiblePathPointDist(Bot.Transform.EyePosition);
    }

    public bool FindLookPoint(out Vector3 Position, out EEnemySteerDir EnemySteerDir)
    {
        if (IsVisible)
        {
            EnemySteerDir = EEnemySteerDir.VisibleEnemyPos;
            Position = EnemyPosition + Bot.Steering.WeaponRootOffset;
            return true;
        }
        if (GetVisibilePathPoint(out Position))
        {
            EnemySteerDir = EEnemySteerDir.PathNode;
            return true;
        }
        EnemyKnownPlaces Places = KnownPlaces;
        EnemyPlace lastKnown = Places.LastKnownPlace;
        if (lastKnown == null)
        {
            EnemySteerDir = EEnemySteerDir.NullLastKnown_ERROR;
            return false;
        }
        var lastSeen = Places.LastSeenPlace;
        if (lastSeen != null &&
            (lastSeen == lastKnown || (lastSeen.Position - lastKnown.Position).sqrMagnitude < GlobalSettingsClass.Instance.Steering.STEER_LASTSEEN_TO_LASTKNOWN_DISTANCE.Sqr()))
        {
            EnemySteerDir = EEnemySteerDir.LastSeenPos;
            Position = lastSeen.Position + Bot.Steering.WeaponRootOffset;
            return true;
        }

        EnemySteerDir = EEnemySteerDir.LastKnownPos;
        Position = lastKnown.Position + Bot.Steering.WeaponRootOffset;
        return true;
    }

    public bool CheckValid()
    {
        if (!WasValid) return false;
        WasValid = IsEnemyValid(OwnerProfileId, EnemyPlayerComponent);
        return WasValid;
    }

    public static bool IsEnemyValid(string botProfileId, PlayerComponent enemyPlayerComp)
    {
        if (enemyPlayerComp == null)
        {
            //Logger.LogError($"Enemy {Enemy.EnemyName} PlayerComponent is Null");
            return false;
        }
        var Player = enemyPlayerComp.Player;
        if (Player == null)
        {
            //Logger.LogError($"Enemy {Enemy.EnemyName} PlayerComponent is Null");
            return false;
        }
        if (!Player.HealthController.IsAlive)
        {
            //Logger.LogDebug("Enemy Player Is Dead");
            return false;
        }
        // Checks specific to bots
        BotOwner enemyBotOwner = enemyPlayerComp.BotOwner;
        if (enemyPlayerComp.IsAI && enemyBotOwner == null)
        {
            if (enemyBotOwner == null)
            {
#if DEBUG
                Logger.LogDebug("Enemy is AI, but BotOwner is null");
#endif
                return false;
            }
            if (enemyBotOwner.ProfileId == botProfileId)
            {
#if DEBUG
                Logger.LogWarning("Enemy has same profile id as Bot?");
#endif
                return false;
            }
        }
        return true;
    }

    private bool IsTargetInSuppRange(Vector3 target, Vector3 suppressPoint)
    {
        var settings = Bot.Info.PersonalitySettings.General;

        float distSqr = (target - suppressPoint).sqrMagnitude;
        if (distSqr <= settings.TARGET_SUPPRESS_DIST.Sqr())
        {
            return true;
        }
        if (distSqr > settings.TARGET_SUPPRESS_DIST_MAX.Sqr())
        {
            return false;
        }
        Vector3 directionToSuppPoint = suppressPoint - Bot.Position;
        Vector3 directionToTarget = target - Bot.Position;
        float angle = Vector3.Angle(directionToSuppPoint.normalized, directionToTarget.normalized);
        if (angle < settings.MAX_TARGET_SUPPRESS_ANGLE)
        {
            return true;
        }
        return false;
    }

    private void UpdateDistAndDirection()
    {
        var data = EnemyPlayerData.DistanceData;
        RealDistance = data.Distance;
        EnemyDirection = data.Direction;
        EnemyDirectionNormal = data.DirectionNormal;
        try
        {
            EnemyInfo.Direction = EnemyDirection;
            EnemyInfo.Distance = RealDistance;
        }
        catch
        { // EFT code loves throwing random errors
        }
    }

    private static Vector3 FindCenterMass(PlayerComponent playerComp)
    {
        Vector3 headPos = playerComp.Player.MainParts[BodyPartType.head].Position;
        Vector3 floorPos = playerComp.Position;
        Vector3 centerMass = Vector3.Lerp(headPos, floorPos, SAINPlugin.LoadedPreset.GlobalSettings.Aiming.CenterMassVal);
        return centerMass;
    }

    public void UpdateLastSeenPosition(Vector3 position, float currentTime)
    {
        var place = KnownPlaces.UpdateSeenPlace(position, currentTime);
        if (place == null)
        {
#if DEBUG
            Logger.LogError($"Failed to update last seen position for {EnemyName} at {position}");
#endif
            return;
        }
        Bot.Squad.SquadInfo?.ReportEnemyPosition(this, place, true, currentTime);
    }

    public void UpdateCurrentEnemyPos(Vector3 position, float currentTime)
    {
        var place = KnownPlaces.UpdateSeenPlace(position, currentTime);
        if (_nextReportSightTime < currentTime)
        {
            _nextReportSightTime = currentTime + SQUADREPORT_SIGHT_INTERVAL;
            Bot.Squad.SquadInfo?.ReportEnemyPosition(this, place, true, currentTime);
        }
    }

    public void EnemyPositionReported(EnemyPlace place, bool seen, float currentTime)
    {
        if (seen)
        {
            KnownPlaces.UpdateSquadSeenPlace(place, currentTime);
        }
        else
        {
            KnownPlaces.UpdateSquadHeardPlace(place, currentTime);
        }
    }

    public void SetEnemyAsSniper(bool isSniper)
    {
        IsSniper = isSniper;
        if (isSniper && Bot.Squad.BotInGroup && Bot.Talk.GroupTalk.FriendIsClose)
        {
            Bot.Talk.TalkAfterDelay(EPhraseTrigger.SniperPhrase, ETagStatus.Combat, UnityEngine.Random.Range(0.33f, 0.66f));
        }
    }

    private EnemyKnownChecker KnownChecker { get; }

    private bool _visPathPointIsCorner;
    public float NextCheckFlashLightTime;
    private float _nextReportSightTime;
}
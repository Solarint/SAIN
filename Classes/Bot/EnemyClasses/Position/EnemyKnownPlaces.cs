using EFT;
using SAIN.Helpers;
using SAIN.Models.Structs;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class EnemyKnownPlaces
{
    public EnemyPlace LastKnownPlace { get; private set; }
    public EnemyPlace LastSeenPlace { get; private set; }
    public EnemyPlace LastHeardPlace { get; private set; }
    public EnemyPlace LastSquadSeenPlace { get; private set; }
    public EnemyPlace LastSquadHeardPlace { get; private set; }
    public float TimeSinceLastKnownUpdated => LastKnownPlace == null ? float.MaxValue : Time.time - TimeLastKnownUpdated;

    public Vector3? LastKnownPosition => LastKnownPlace?.Position;

    public float EnemyDistanceFromLastKnown {
        get
        {
            if (LastKnownPlace == null)
            {
                return float.MaxValue;
            }
            return LastKnownPlace.DistanceToEnemyRealPosition;
        }
    }

    public float BotDistanceFromLastKnown {
        get
        {
            if (LastKnownPlace == null)
            {
                return float.MaxValue;
            }
            return LastKnownPlace.DistanceToBot;
        }
    }

    public float EnemyDistanceFromLastSeen {
        get
        {
            if (LastSeenPlace == null)
            {
                return float.MaxValue;
            }
            return LastSeenPlace.DistanceToEnemyRealPosition;
        }
    }

    public float EnemyDistanceFromLastHeard {
        get
        {
            if (LastHeardPlace == null)
            {
                return float.MaxValue;
            }
            return LastHeardPlace.DistanceToEnemyRealPosition;
        }
    }

    public bool SearchedAllKnownLocations { get; private set; }

    private void checkSearched(float currentTime)
    {
        if (_nextCheckSearchTime > currentTime)
        {
            return;
        }
        _nextCheckSearchTime = currentTime + 0.25f;

        bool allSearched = true;
        if (LastKnownPlace != null && !LastKnownPlace.HasArrivedPersonal && !LastKnownPlace.HasArrivedSquad)
        {
            allSearched = false;
        }

        if (allSearched
            && !SearchedAllKnownLocations)
        {
            Enemy.Events.EnemyLocationsSearched();
        }

        SearchedAllKnownLocations = allSearched;
    }

    public List<EnemyPlace> AllEnemyPlaces { get; } = new List<EnemyPlace>();

    public EnemyKnownPlaces(EnemyData enemyData)
    {
        Enemy = enemyData.Enemy;
        _placeData = new PlaceData {
            OwnerEnemy = enemyData.Enemy,
            Owner = enemyData.Enemy.Bot,
            IsAI = enemyData.Enemy.IsAI,
            OwnerID = enemyData.Enemy.Bot.ProfileId
        };
    }

    private readonly Enemy Enemy;

    public void Init()
    {
        Enemy.Events.OnEnemyKnownChanged.OnToggle += OnEnemyKnownChanged;
    }

    public void TickEnemy(float currentTime)
    {
        //UpdatePlaces();
        if (Enemy.EnemyKnown)
        {
            checkSearched(currentTime);
            if (Enemy.IsCurrentEnemy)
            {
                createDebug();
            }
        }
    }

    public void Dispose()
    {
        clearAllPlaces();

        Enemy.Events.OnEnemyKnownChanged.OnToggle -= OnEnemyKnownChanged;

        foreach (var obj in _guiObjects)
        {
            DebugGizmos.DestroyLabel(obj.Value);
        }
        _guiObjects?.Clear();
    }

    public void OnEnemyKnownChanged(bool known, Enemy enemy)
    {
        if (!known)
        {
            clearAllPlaces();
        }
    }

    private void clearAllPlaces()
    {
        AllEnemyPlaces.Clear();
        LastSeenPlace = null;
        LastHeardPlace = null;
        LastSquadSeenPlace = null;
        LastSquadHeardPlace = null;
        LastKnownPlace = null;
        TimeLastKnownUpdated = -1000f;
    }

    private void createDebug()
    {
#if DEBUG
        if (SAINPlugin.DebugMode)
        {
            EnemyPlace lastKnown = LastKnownPlace;
            if (lastKnown != null)
            {
                if (debugLastKnown == null)
                {
                    debugLastKnown = DebugGizmos.CreateLabel(lastKnown.Position, string.Empty);
                    _guiObjects.Add(lastKnown, debugLastKnown);
                }
                updateDebugString(lastKnown, debugLastKnown);
            }
        }
        else if (debugLastKnown != null)
        {
            DebugGizmos.DestroyLabel(debugLastKnown);
            debugLastKnown = null;
        }
#endif
    }

    private void tryTalk()
    {
        if (_nextTalkClearTime < Time.time
            && Enemy.Bot.Talk.GroupSay(EFTMath.RandomBool(75) ? EPhraseTrigger.Clear : EPhraseTrigger.LostVisual, null, true, 75))
        {
            _nextTalkClearTime = Time.time + 10f;
        }
    }

    public void SetPlaceAsSearched(EnemyPlace place)
    {
        tryTalk();
        if (place.PlaceData.OwnerID == Enemy.Bot.ProfileId)
        {
            place.HasArrivedPersonal = true;
        }
        else
        {
            place.HasArrivedSquad = true;
        }
    }

    private void updateDebugString(EnemyPlace place, DebugLabel obj)
    {
        obj.WorldPos = place.Position;

        StringBuilder stringBuilder = obj.StringBuilder;
        stringBuilder.Clear();

        stringBuilder.AppendLine($"Bot: {Enemy.Bot.name}");
        stringBuilder.AppendLine($"Known Location of {Enemy.EnemyPlayer.name}");

        if (LastKnownPlace == place)
        {
            stringBuilder.AppendLine($"Last Known Location.");
        }

        stringBuilder.AppendLine($"Time Since Position Updated: {place.TimeSincePositionUpdated}");

        stringBuilder.AppendLine($"Arrived? [{place.HasArrivedPersonal}]"
            + (place.HasArrivedPersonal ? $"Time Since Arrived: [{Time.time - place._timeArrivedPers}]" : string.Empty));

        stringBuilder.AppendLine($"Seen? [{place.HasSeenPersonal}]"
            + (place.HasSeenPersonal ? $"Time Since Seen: [{Time.time - place._timeSeenPers}]" : string.Empty));
    }

    public EnemyPlace UpdateSeenPlace(Vector3 position, float currentTime)
    {
        if (LastSeenPlace == null)
        {
            LastSeenPlace = new EnemyPlace(_placeData, position, true, EEnemyPlaceType.Vision, null) {
                HasSeenPersonal = true
            };
            AllEnemyPlaces.Add(LastSeenPlace);
        }
        else
        {
            LastSeenPlace.UpdatePosition(position);
        }
        SetLastKnown(LastSeenPlace, currentTime);
        return LastSeenPlace;
    }

    public void UpdateSquadSeenPlace(EnemyPlace memberPlace, float currentTime)
    {
        if (Enemy.IsVisible)
        {
            return;
        }
        if (LastSquadSeenPlace == null)
        {
            LastSquadSeenPlace = new EnemyPlace(_placeData, memberPlace.Position, true, EEnemyPlaceType.Vision, null) {
                HasSeenSquad = true
            };
            AllEnemyPlaces.Add(LastSquadSeenPlace);
        }
        else
        {
            LastSquadSeenPlace.UpdatePosition(memberPlace.Position);
        }
        SetLastKnown(LastSquadSeenPlace, currentTime);
    }

    public EnemyPlace UpdatePersonalHeardPosition(SAINHearingReport report, float currentTime)
    {
        if (LastHeardPlace != null)
        {
            LastHeardPlace.IsDanger = report.isDanger;
            LastHeardPlace.SoundType = report.soundType;
            LastHeardPlace.UpdatePosition(report.position);
        }
        else 
        {                
            LastHeardPlace = new EnemyPlace(_placeData, report);
            AllEnemyPlaces.Add(LastHeardPlace);
        }
        SetLastKnown(LastHeardPlace, currentTime);
        return LastHeardPlace;
    }

    public void UpdateSquadHeardPlace(EnemyPlace memberPlace, float currentTime)
    {
        if (Enemy.IsVisible)
        {
            return;
        }

        if (LastSquadHeardPlace != null)
        {
            LastSquadHeardPlace.IsDanger = memberPlace.IsDanger;
            LastSquadHeardPlace.SoundType = memberPlace.SoundType;
            LastSquadHeardPlace.UpdatePosition(memberPlace.Position);
        }
        else
        {
            LastSquadHeardPlace = new EnemyPlace(_placeData, memberPlace.Position, memberPlace.IsDanger, memberPlace.PlaceType, memberPlace.SoundType);
            AllEnemyPlaces.Add(LastSquadHeardPlace);
        }
        SetLastKnown(LastSquadHeardPlace, currentTime);
    }

    private void SetLastKnown(EnemyPlace place, float currentTime)
    {
        if (place == null)
        {
#if DEBUG
            Logger.LogWarning("SetLastKnown called with null place");
#endif
            return;
        }
        SearchedAllKnownLocations = false;
        TimeLastKnownUpdated = currentTime;
        LastKnownPlace = place;
        Enemy.Events.LastKnownUpdated(place, currentTime);
    }

    public float TimeLastKnownUpdated { get; private set; } = -1000f;

    private readonly PlaceData _placeData;
    private float _nextTalkClearTime;
    private float _nextCheckSearchTime;
#if DEBUG
    private DebugLabel debugLastKnown;
#endif
    private readonly Dictionary<EnemyPlace, DebugLabel> _guiObjects = new();
}
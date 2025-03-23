using EFT;
using SAIN.Components.BotComponentSpace.Classes.EnemyClasses;
using SAIN.Helpers;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class EnemyKnownPlaces : EnemyBase, IBotEnemyClass
    {
        public EnemyPlace LastKnownPlace { get; private set; }
        public EnemyPlace LastSeenPlace { get; private set; }
        public EnemyPlace LastHeardPlace { get; private set; }
        public EnemyPlace LastSquadSeenPlace { get; private set; }
        public EnemyPlace LastSquadHeardPlace { get; private set; }
        public float TimeSinceLastKnownUpdated => LastKnownPlace == null ? float.MaxValue : Time.time - TimeLastKnownUpdated;

        public Vector3? LastKnownPosition => LastKnownPlace?.Position;
        public Vector3? LastSeenPosition => LastSeenPlace?.Position;
        public Vector3? LastHeardPosition => LastHeardPlace?.Position;

        public float EnemyDistanceFromLastKnown {
            get
            {
                if (LastKnownPlace == null) {
                    return float.MaxValue;
                }
                return LastKnownPlace.DistanceToEnemyRealPosition;
            }
        }

        public float BotDistanceFromLastKnown {
            get
            {
                if (LastKnownPlace == null) {
                    return float.MaxValue;
                }
                return LastKnownPlace.DistanceToBot;
            }
        }

        public float EnemyDistanceFromLastSeen {
            get
            {
                if (LastSeenPlace == null) {
                    return float.MaxValue;
                }
                return LastSeenPlace.DistanceToEnemyRealPosition;
            }
        }

        public float EnemyDistanceFromLastHeard {
            get
            {
                if (LastHeardPlace == null) {
                    return float.MaxValue;
                }
                return LastHeardPlace.DistanceToEnemyRealPosition;
            }
        }

        public bool SearchedAllKnownLocations { get; private set; }

        private void checkSearched()
        {
            if (_nextCheckSearchTime > Time.time) {
                return;
            }
            _nextCheckSearchTime = Time.time + 0.25f;

            bool allSearched = true;
            if (LastKnownPlace != null && !LastKnownPlace.HasArrivedPersonal && !LastKnownPlace.HasArrivedSquad) {
                allSearched = false;
            }

            if (allSearched
                && !SearchedAllKnownLocations) {
                Enemy.Events.EnemyLocationsSearched();
            }

            SearchedAllKnownLocations = allSearched;
        }

        public List<EnemyPlace> AllEnemyPlaces { get; } = new List<EnemyPlace>();

        public EnemyKnownPlaces(Enemy enemy) : base(enemy)
        {
            _placeData = new PlaceData {
                Enemy = enemy,
                Owner = enemy.Bot,
                IsAI = enemy.IsAI,
                OwnerID = enemy.Bot.ProfileId
            };
        }

        public void Init()
        {
            Enemy.Events.OnEnemyKnownChanged.OnToggle += OnEnemyKnownChanged;
        }

        public void Update()
        {
            updatePlaces();
            if (Enemy.EnemyKnown) {
                //checkIfArrived();
                checkSearched();

                if (Enemy.IsCurrentEnemy) {
                    //checkIfSeen();
                    createDebug();
                }
            }
        }

        public void Dispose()
        {
            clearAllPlaces();

            Enemy.Events.OnEnemyKnownChanged.OnToggle -= OnEnemyKnownChanged;

            foreach (var obj in _guiObjects) {
                DebugGizmos.DestroyLabel(obj.Value);
            }
            _guiObjects?.Clear();
        }

        public void OnEnemyKnownChanged(bool known, Enemy enemy)
        {
            if (known) {
                return;
            }
            clearAllPlaces();
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
            if (SAINPlugin.DebugMode) {
                EnemyPlace lastKnown = LastKnownPlace;
                if (lastKnown != null) {
                    if (debugLastKnown == null) {
                        debugLastKnown = DebugGizmos.CreateLabel(lastKnown.Position, string.Empty);
                    }
                    updateDebugString(lastKnown, debugLastKnown);
                }
            }
            else if (debugLastKnown != null) {
                DebugGizmos.DestroyLabel(debugLastKnown);
                debugLastKnown = null;
            }
        }

        private void checkIfSeen()
        {
            if (!Enemy.IsCurrentEnemy) {
                return;
            }
            EnemyPlace lastKnown = LastKnownPlace;
            if (lastKnown == null) {
                return;
            }
            lastKnown.CheckLineOfSight(Bot.Transform.EyePosition, LayerMaskClass.HighPolyWithTerrainMaskAI);
        }

        private void tryTalk()
        {
            if (_nextTalkClearTime < Time.time
                && Bot.Talk.GroupSay(EFTMath.RandomBool(75) ? EPhraseTrigger.Clear : EPhraseTrigger.LostVisual, null, true, 75)) {
                _nextTalkClearTime = Time.time + 10f;
            }
        }

        public void SetPlaceAsSearched(EnemyPlace place)
        {
            tryTalk();
            if (place.PlaceData.OwnerID == Bot.ProfileId) {
                place.HasArrivedPersonal = true;
            }
            else {
                place.HasArrivedSquad = true;
            }
        }

        private void updateDebugString(EnemyPlace place, GUIObject obj)
        {
            obj.WorldPos = place.Position;

            StringBuilder stringBuilder = obj.StringBuilder;
            stringBuilder.Clear();

            stringBuilder.AppendLine($"Bot: {BotOwner.name}");
            stringBuilder.AppendLine($"Known Location of {EnemyPlayer.Profile.Nickname}");

            if (LastKnownPlace == place) {
                stringBuilder.AppendLine($"Last Known Location.");
            }

            stringBuilder.AppendLine($"Time Since Position Updated: {place.TimeSincePositionUpdated}");

            stringBuilder.AppendLine($"Arrived? [{place.HasArrivedPersonal}]"
                + (place.HasArrivedPersonal ? $"Time Since Arrived: [{Time.time - place._timeArrivedPers}]" : string.Empty));

            stringBuilder.AppendLine($"Seen? [{place.HasSeenPersonal}]"
                + (place.HasSeenPersonal ? $"Time Since Seen: [{Time.time - place._timeSeenPers}]" : string.Empty));
        }

        public EnemyPlace UpdateSeenPlace(Vector3 position)
        {
            if (LastSeenPlace == null) {
                LastSeenPlace = new EnemyPlace(_placeData, position, true, EEnemyPlaceType.Vision, null);
                LastSeenPlace.HasSeenPersonal = true;
                addPlace(LastSeenPlace);
            }
            else {
                LastSeenPlace.Position = position;
            }
            return LastSeenPlace;
        }

        private void addPlace(EnemyPlace place)
        {
            if (place != null) {
                SearchedAllKnownLocations = false;
                place.OnPositionUpdated += lastKnownPosUpdated;
                lastKnownPosUpdated(place);
                AllEnemyPlaces.Add(place);
            }
        }

        public void UpdateSquadSeenPlace(EnemyPlace place)
        {
            if (place == null) {
                return;
            }
            if (LastSquadSeenPlace == place) {
                return;
            }
            removePlace(LastSquadSeenPlace);
            LastSquadSeenPlace = place;
            addPlace(place);
        }

        public EnemyPlace UpdatePersonalHeardPosition(HearingReport report)
        {
            if (Enemy.IsVisible) {
                //return null;
            }

            var lastHeard = LastHeardPlace;
            if (lastHeard != null) {
                lastHeard.Position = report.position;
                lastHeard.IsDanger = report.isDanger;
                lastHeard.SoundType = report.soundType;
                lastHeard.HasArrivedPersonal = false;
                lastHeard.HasArrivedSquad = false;
                lastHeard.HasSeenPersonal = false;
                lastHeard.HasSeenSquad = false;
                return lastHeard;
            }

            LastHeardPlace = new EnemyPlace(_placeData, report);
            addPlace(LastHeardPlace);
            return LastHeardPlace;
        }

        public void UpdateSquadHeardPlace(EnemyPlace place)
        {
            if (place == null) {
                return;
            }
            if (LastSquadHeardPlace == place) {
                return;
            }

            removePlace(LastSquadHeardPlace);
            LastSquadHeardPlace = place;
            addPlace(place);
        }

        private void updatePlaces(bool force = false)
        {
            LastSeenPlace?.Update();
            LastHeardPlace?.Update();
            if (_nextSortPlacesTime < Time.time) {
                _nextSortPlacesTime = Time.time + 0.5f;
                sortAndClearPlaces();
            }
        }

        private void removePlace(EnemyPlace place)
        {
            if (place != null) {
                place.OnPositionUpdated -= lastKnownPosUpdated;
                AllEnemyPlaces.Remove(place);
                if (LastKnownPlace != null && LastKnownPlace == place) {
                    LastKnownPlace = null;
                }
            }
        }

        private void sortAndClearPlaces()
        {
            if (LastSeenPlace?.ShallClear == true) {
                removePlace(LastSeenPlace);
                LastSeenPlace = null;
            }
            if (LastHeardPlace?.ShallClear == true) {
                removePlace(LastHeardPlace);
                LastHeardPlace = null;
            }
            if (LastSquadHeardPlace?.ShallClear == true) {
                removePlace(LastSquadHeardPlace);
                LastSquadHeardPlace = null;
            }
            if (LastSquadSeenPlace?.ShallClear == true) {
                removePlace(LastSquadSeenPlace);
                LastSquadSeenPlace = null;
            }

            if (AllEnemyPlaces.Count > 0) {
                AllEnemyPlaces.RemoveAll(x => x == null);
                AllEnemyPlaces.Sort((x, y) => x.TimeSincePositionUpdated.CompareTo(y.TimeSincePositionUpdated));
            }
        }

        private void lastKnownPosUpdated(EnemyPlace place)
        {
            if (place == null) return;

            SearchedAllKnownLocations = false;
            TimeLastKnownUpdated = Time.time;
            LastKnownPlace = place;
            Enemy.Events.LastKnownUpdated(place);
        }

        public float TimeLastKnownUpdated { get; private set; } = -1000f;

        private readonly PlaceData _placeData;
        private float _nextTalkClearTime;
        private float _nextCheckSearchTime;
        private float _nextSortPlacesTime;
        private GUIObject debugLastKnown;
        private readonly Dictionary<EnemyPlace, GUIObject> _guiObjects = new Dictionary<EnemyPlace, GUIObject>();
    }
}
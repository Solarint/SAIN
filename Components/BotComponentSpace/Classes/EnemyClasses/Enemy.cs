using EFT;
using SAIN.Components.BotComponentSpace.Classes.EnemyClasses;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Components.PlayerComponentSpace.PersonClasses;
using SAIN.Helpers;
using SAIN.Preset;
using System;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class Enemy : BotBase, IBotClass
    {
        public string EnemyName { get; }
        public string EnemyProfileId { get; }
        public PlayerComponent EnemyPlayerComponent { get; }
        public PersonClass EnemyPerson { get; }
        public PersonTransformClass EnemyTransform { get; }
        public bool IsAI => EnemyPlayer.IsAI;

        public bool IsCurrentEnemy { get; private set; }
        public float RealDistance => EnemyPlayerData.DistanceData.Distance;
        public bool IsSniper { get; private set; }

        public EnemyEvents Events { get; }
        public EnemyKnownPlaces KnownPlaces { get; private set; }
        public SAINEnemyStatus Status { get; }
        public EnemyVisionClass Vision { get; }
        public SAINEnemyPath Path { get; }
        public EnemyInfo EnemyInfo { get; }
        public EnemyAim Aim { get; }
        public EnemyHearing Hearing { get; }

        private EnemyKnownChecker _knownChecker { get; }
        private EnemyActiveThreatChecker _activeThreatChecker { get; }
        private EnemyValidChecker _validChecker { get; }

        public event Action OnEnemyDisposed;

        public OtherPlayerData EnemyPlayerData { get; }

        public Enemy(BotComponent bot, PlayerComponent enemyComponent, EnemyInfo enemyInfo) : base(bot)
        {
            EnemyPlayerComponent = enemyComponent;
            EnemyPerson = enemyComponent.Person;
            EnemyTransform = enemyComponent.Transform;
            EnemyName = $"{enemyComponent.Name} ({enemyComponent.Person.Nickname})";
            EnemyInfo = enemyInfo;
            EnemyProfileId = enemyComponent.ProfileId;

            EnemyPlayerData = bot.PlayerComponent.OtherPlayersData.Datas[enemyComponent.ProfileId];

            Events = new EnemyEvents(this);
            _activeThreatChecker = new EnemyActiveThreatChecker(this);
            _validChecker = new EnemyValidChecker(this);
            _knownChecker = new EnemyKnownChecker(this);
            Status = new SAINEnemyStatus(this);
            Vision = new EnemyVisionClass(this);
            Path = new SAINEnemyPath(this);
            KnownPlaces = new EnemyKnownPlaces(this);
            Aim = new EnemyAim(this);
            Hearing = new EnemyHearing(this);

            updateDistAndDirection(true);
        }

        public void Init()
        {
            base.SubscribeToPreset(updatePresetSettings);

            Events.Init();
            _validChecker.Init();
            _knownChecker.Init();
            _activeThreatChecker.Init();
            KnownPlaces.Init();
            Vision.Init();
            Path.Init();
            Hearing.Init();
            Status.Init();
        }

        public void Update()
        {
            IsCurrentEnemy = Bot.EnemyController.ActiveEnemy?.EnemyProfileId == EnemyProfileId;

            calcFrequencyCoef();
            updateDistAndDirection();
            updateSniperStatus();

            Events.Update();
            _validChecker.Update();
            _knownChecker.Update();
            _activeThreatChecker.Update();
            updateActiveState();
            Vision.Update();
            KnownPlaces.Update();
            Path.Update();
            Status.Update();
        }

        public void Dispose()
        {
            OnEnemyDisposed?.Invoke();
            Events.Dispose();
            _validChecker.Dispose();
            _knownChecker.Dispose();
            _activeThreatChecker.Dispose();
            KnownPlaces.Dispose();
            Vision.Dispose();
            Path.Dispose();
            Hearing.Dispose();
            Status.Dispose();
        }

        public bool EnemyKnown => Events.OnEnemyKnownChanged.Value;
        public bool EnemyNotLooking => IsVisible && !Status.EnemyLookingAtMe && !Status.ShotAtMeRecently;

        public bool CheckValid()
        {
            return _validChecker.CheckValid();
        }

        public bool WasValid => _validChecker.WasValid;

        public float TimeSinceCurrentEnemy => _hasBeenActive ? Time.time - _timeLastActive : float.MaxValue;

        public Collider HidingBehindObject {
            get
            {
                float time = Time.time;
                if (_nextCheckHidingTime < time) {
                    _nextCheckHidingTime = time + _checkHidingFreq;
                    _hidingBehindObject = null;
                    Vector3? lastKnown = LastKnownPosition;
                    if (lastKnown != null
                        && Physics.Raycast(lastKnown.Value + Vector3.up, Bot.Position + Vector3.up, out RaycastHit hit, _checkHidingRayDist, LayerMaskClass.HighPolyCollider)) {
                        _hidingBehindObject = hit.collider;
                    }
                }
                return _hidingBehindObject;
            }
        }

        public Vector3? SuppressionTarget {
            get
            {
                Vector3? enemyLastKnown = KnownPlaces.LastKnownPosition;
                if (enemyLastKnown == null) {
                    return null;
                }

                EnemyCornerDictionary corners = Path.EnemyCorners;

                Vector3? blindCorner = corners.EyeLevelPosition(ECornerType.Blind);
                if (blindCorner != null &&
                    isTargetInSuppRange(enemyLastKnown.Value, blindCorner.Value)) {
                    return blindCorner;
                }

                Vector3? lastCorner = corners.EyeLevelPosition(ECornerType.Last);
                if (lastCorner != null &&
                    Path.CanSeeLastCornerToEnemy &&
                    isTargetInSuppRange(enemyLastKnown.Value, lastCorner.Value)) {
                    return lastCorner;
                }

                if (HidingBehindObject != null) {
                    Vector3 pos = HidingBehindObject.transform.position + HidingBehindObject.bounds.size.z * Vector3.up;
                    if (isTargetInSuppRange(enemyLastKnown.Value, pos)) {
                        return pos;
                    }
                }
                return null;
            }
        }

        private bool isTargetInSuppRange(Vector3 target, Vector3 suppressPoint)
        {
            if ((target - suppressPoint).sqrMagnitude <= MAX_TARGET_SUPPRESS_DIST) {
                return true;
            }
            Vector3 directionToSuppPoint = suppressPoint - Bot.Position;
            Vector3 directionToTarget = target - Bot.Position;
            float angle = Vector3.Angle(directionToSuppPoint.normalized, directionToTarget.normalized);
            if (angle < MAX_TARGET_SUPPRESS_ANGLE) {
                return true;
            }
            return false;
        }

        private const float MAX_TARGET_SUPPRESS_DIST = 5f * 5f;
        private const float MAX_TARGET_SUPPRESS_ANGLE = 20f;

        public Vector3? CenterMass {
            get
            {
                if (EnemyIPlayer == null) {
                    return null;
                }
                if (_nextGetCenterTime < Time.time) {
                    _nextGetCenterTime = Time.time + 0.05f;
                    _centerMass = new Vector3?(findCenterMass());
                }
                return _centerMass;
            }
        }

        public bool FirstContactOccured => Vision.FirstContactOccured;

        public bool FirstContactReported = false;

        public EPathDistance EPathDistance => Path.EPathDistance;
        public IPlayer EnemyIPlayer => EnemyPlayerComponent.IPlayer;
        public Player EnemyPlayer => EnemyPlayerComponent.Player;

        public Vector3? LastKnownPosition => KnownPlaces.LastKnownPosition;

        public Vector3 EnemyMoveDirection {
            get
            {
                if (_nextCalcMoveDirTime < Time.time) {
                    _nextCalcMoveDirTime = Time.time + 0.1f;
                    Vector2 moveDirV2 = EnemyPlayer.MovementContext.MovementDirection;
                    Vector3 moveDirection = new Vector3(moveDirV2.x, 0, moveDirV2.y);
                    if (EnemyTransform.VelocityMagnitudeNormal > 0.01f) {
                        LastMoveDirection = moveDirection;
                        if (EnemyPlayer.IsSprintEnabled) {
                            LastSprintDirection = moveDirection;
                        }
                    }
                    _moveDirection = moveDirection;
                }
                return _moveDirection;
            }
        }

        private Vector3 _moveDirection;
        private float _nextCalcMoveDirTime;

        public Vector3 LastMoveDirection { get; private set; }
        public Vector3 LastSprintDirection { get; private set; }
        public Vector3 EnemyPosition => EnemyTransform.Position;
        public Vector3 EnemyDirection => EnemyPlayerData.DistanceData.Direction;
        public Vector3 EnemyDirectionNormal => EnemyPlayerData.DistanceData.DirectionNormal;
        public Vector3 EnemyHeadPosition => EnemyTransform.HeadPosition;

        public float TimeSinceLastKnownUpdated => KnownPlaces.TimeSinceLastKnownUpdated;
        public bool InLineOfSight => Vision.InLineOfSight;
        public bool IsVisible => Vision.IsVisible;
        public bool CanShoot => Vision.CanShoot;
        public bool Seen => Vision.Seen;
        public bool Heard => Hearing.Heard;

        public bool EnemyLookingAtMe => Status.EnemyLookingAtMe;
        public float TimeSinceSeen => Vision.TimeSinceSeen;
        public float TimeSinceHeard => Hearing.TimeSinceHeard;

        private void updateDistAndDirection(bool force = false)
        {
            try {
                EnemyInfo.Direction = EnemyDirection;
                EnemyInfo.Distance = RealDistance;
            }
            catch {
            }
        }

        private void updateSniperStatus()
        {
            if (IsSniper) {
                if (!EnemyKnown) {
                    IsSniper = false;
                    return;
                }
                if (RealDistance < GlobalSettings.Mind.ENEMYSNIPER_DISTANCE_END) {
                    IsSniper = false;
                    return;
                }
            }
        }

        public float UpdateFrequencyCoef { get; private set; }

        private void calcFrequencyCoef()
        {
            if (_nextUpdateCoefTime < Time.time) {
                _nextUpdateCoefTime = Time.time + 0.1f;
                UpdateFrequencyCoef = calcUpdateFrequencyCoef(out float normal);
                UpdateFrequencyCoefNormal = normal;
            }
        }

        public float UpdateFrequencyCoefNormal { get; private set; }

        private float calcUpdateFrequencyCoef(out float normal)
        {
            float enemyDist = RealDistance;
            float min = ENEMY_UPDATEFREQUENCY_MIN_DIST;
            if (enemyDist <= min) {
                normal = 0;
                return 1f;
            }
            float max = ENEMY_UPDATEFREQUENCY_MAX_DIST;
            if (enemyDist >= max) {
                normal = 1f;
                return max;
            }

            float num = max - min;
            float num2 = enemyDist - min;
            normal = num2 / num;
            float result = Mathf.Lerp(1f, ENEMY_UPDATEFREQUENCY_MAX_SCALE, normal);
            return result;
        }

        private const float ENEMY_UPDATEFREQUENCY_MAX_SCALE = 5f;
        private const float ENEMY_UPDATEFREQUENCY_MAX_DIST = 500f;
        private const float ENEMY_UPDATEFREQUENCY_MIN_DIST = 50f;

        private void updateActiveState()
        {
            if (IsCurrentEnemy &&
                !_hasBeenActive) {
                _hasBeenActive = true;
            }

            if (IsCurrentEnemy || IsVisible || Status.HeardRecently) {
                _timeLastActive = Time.time;
            }
        }

        private Vector3 findCenterMass()
        {
            PlayerComponent enemy = EnemyPlayerComponent;
            Vector3 headPos = enemy.Player.MainParts[BodyPartType.head].Position;
            Vector3 floorPos = enemy.Position;
            Vector3 centerMass = Vector3.Lerp(headPos, floorPos, SAINPlugin.LoadedPreset.GlobalSettings.Aiming.CenterMassVal);

            if (enemy.Player.IsYourPlayer && SAINPlugin.DebugMode && _debugCenterMassTime < Time.time) {
                _debugCenterMassTime = Time.time + 1f;
                DebugGizmos.Sphere(centerMass, 0.1f, 5f);
            }

            return centerMass;
        }

        public void UpdateLastSeenPosition(Vector3 position)
        {
            var place = KnownPlaces.UpdateSeenPlace(position);
            Bot.Squad.SquadInfo?.ReportEnemyPosition(this, place, true);
        }

        public void UpdateCurrentEnemyPos(Vector3 position)
        {
            var place = KnownPlaces.UpdateSeenPlace(position);
            if (_nextReportSightTime < Time.time) {
                _nextReportSightTime = Time.time + _reportSightFreq;
                Bot.Squad.SquadInfo?.ReportEnemyPosition(this, place, true);
            }
        }

        public void EnemyPositionReported(EnemyPlace place, bool seen)
        {
            if (seen) {
                KnownPlaces.UpdateSquadSeenPlace(place);
            }
            else {
                KnownPlaces.UpdateSquadHeardPlace(place);
            }
        }

        public void SetEnemyAsSniper(bool isSniper)
        {
            IsSniper = isSniper;
            if (isSniper && Bot.Squad.BotInGroup && Bot.Talk.GroupTalk.FriendIsClose) {
                Bot.Talk.TalkAfterDelay(EPhraseTrigger.SniperPhrase, ETagStatus.Combat, UnityEngine.Random.Range(0.33f, 0.66f));
            }
        }

        private void updatePresetSettings(SAINPresetClass preset)
        {
        }

        public float NextCheckFlashLightTime;

        private float _nextUpdateCoefTime;
        private bool _hasBeenActive;
        private Vector3? _centerMass;
        private float _nextGetCenterTime;
        private static float _debugCenterMassTime;
        private Collider _hidingBehindObject;
        private const float _checkHidingRayDist = 3f;
        private const float _checkHidingFreq = 1f;
        private float _nextCheckHidingTime;
        private float _nextReportSightTime;
        private const float _reportSightFreq = 0.5f;
        private float _timeLastActive;
    }
}
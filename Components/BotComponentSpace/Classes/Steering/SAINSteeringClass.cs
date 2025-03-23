using EFT;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace SAIN.SAINComponent.Classes.Mover
{
    public class SAINSteeringClass : BotBase, IBotClass
    {
        private static SteeringSettings _steerSettings => GlobalSettingsClass.Instance.Steering;
        private float STEER_LASTSEEN_TO_LASTKNOWN_DISTANCE_SQR => _steerSettings.STEER_LASTSEEN_TO_LASTKNOWN_DISTANCE.Sqr();
        public ESteerPriority CurrentSteerPriority => _steerPriorityClass.CurrentSteerPriority;
        public ESteerPriority LastSteerPriority => _steerPriorityClass.LastSteerPriority;
        public EEnemySteerDir EnemySteerDir { get; private set; }
        public Vector3 WeaponRootOffset => BotOwner.WeaponRoot.position - Bot.Position + (Vector3.down * 0.1f);

        public bool SteerByPriority(Enemy enemy = null, bool lookRandom = true, bool ignoreRunningPath = false)
        {
            if (enemy == null)
                enemy = Bot.Enemy;

            switch (_steerPriorityClass.GetCurrentSteerPriority(lookRandom, ignoreRunningPath)) {
                case ESteerPriority.RunningPath:
                case ESteerPriority.Aiming:
                    return true;

                case ESteerPriority.ManualShooting:
                    LookToPoint(Bot.ManualShoot.ShootPosition + Bot.Info.WeaponInfo.Recoil.CurrentRecoilOffset);
                    return true;

                case ESteerPriority.EnemyVisible:
                    LookToEnemy(Bot.Enemy);
                    return true;

                case ESteerPriority.UnderFire:
                    lookToUnderFirePos();
                    return true;

                case ESteerPriority.LastHit:
                    lookToLastHitPos();
                    return true;

                case ESteerPriority.EnemyLastKnownLong:
                case ESteerPriority.EnemyLastKnown:
                    if (!LookToLastKnownEnemyPosition(enemy)) {
                        LookToRandomPosition();
                    }
                    return true;

                case ESteerPriority.HeardThreat:
                    HeardSoundSteering.LookToHeardPosition();
                    return true;

                case ESteerPriority.Sprinting:
                    LookToMovingDirection(400);
                    return true;

                case ESteerPriority.RandomLook:
                    LookToRandomPosition();
                    return true;

                default:
                    return false;
            }
        }

        public bool LookToLastKnownEnemyPosition(Enemy enemy, Vector3? lastKnown = null)
        {
            Vector3? place = lastKnown ?? FindLastKnownTarget(enemy);
            if (place != null) {
                LookToPoint(place.Value);
                return true;
            }
            return false;
        }

        public void LookToMovingDirection(float rotateSpeed = 150f, bool sprint = false)
        {
            if (sprint || Player.IsSprintEnabled) {
                BotOwner.Steering.LookToMovingDirection(500f);
            }
            else {
                BotOwner.Steering.LookToMovingDirection(rotateSpeed);
            }
        }

        public void LookToPoint(Vector3 point, float minTurnSpeed = -1, float maxTurnSpeed = -1f)
        {
            Vector3 direction = point - BotOwner.WeaponRoot.position;
            if (direction.sqrMagnitude < 1f) {
                direction = direction.normalized;
            }
            float turnSpeed;
            if (_steerSettings.SMOOTH_TURN_TOGGLE) {
                turnSpeed = calcTurnSpeed(direction, minTurnSpeed, maxTurnSpeed);
            }
            else {
                turnSpeed = maxTurnSpeed;
            }
            BotOwner.Steering.LookToDirection(direction, turnSpeed);
        }

        private float calcTurnSpeed(Vector3 targetDirection, float minTurnSpeed, float maxTurnSpeed)
        {
            float minSpeed = minTurnSpeed > 0 ? minTurnSpeed : _steerSettings.SteerSpeed_MinSpeed;
            float maxSpeed = maxTurnSpeed > 0 ? maxTurnSpeed : _steerSettings.SteerSpeed_MaxSpeed;
            if (minSpeed >= maxSpeed) {
                return minSpeed;
            }

            float maxAngle = _steerSettings.SteerSpeed_MaxAngle;
            Vector3 currentDir = _lookDirection;
            float angle = Vector3.Angle(currentDir, targetDirection.normalized);

            if (angle >= maxAngle) {
                return maxSpeed;
            }
            float minAngle = _steerSettings.SteerSpeed_MinAngle;
            if (angle <= minAngle) {
                return minSpeed;
            }

            float angleDiff = maxAngle - minAngle;
            float targetDiff = angle - minAngle;
            float ratio = targetDiff / angleDiff;
            float result = Mathf.Lerp(minSpeed, maxSpeed, ratio);
            //Logger.LogDebug($"Steer Speed Calc: Result: [{result}] Angle: [{angle}]");
            return result;
        }

        public void LookToDirection(Vector3 direction, bool flat, float rotateSpeed = -1f)
        {
            if (flat) {
                direction.y = 0f;
            }
            Vector3 pos = BotOwner.WeaponRoot.position + direction.normalized;
            LookToPoint(pos, rotateSpeed);
        }

        public void LookToDirection(Vector2 direction, float rotateSpeed = -1f)
        {
            Vector3 vector = new Vector3(direction.x, 0, direction.y);
            Vector3 pos = BotOwner.WeaponRoot.position + vector.normalized;
            LookToPoint(pos, rotateSpeed);
        }

        public void LookToEnemy(Enemy enemy)
        {
            if (enemy != null) {
                LookToPoint(enemy.EnemyPosition + WeaponRootOffset);
            }
        }

        public void LookToRandomPosition()
        {
            Vector3? point = _randomLook.UpdateRandomLook();
            if (point != null) {
                float random = Random.Range(_steerSettings.STEER_RANDOMLOOK_SPEED_MIN, _steerSettings.STEER_RANDOMLOOK_SPEED_MAX);
                LookToPoint(point.Value, random, random * _steerSettings.STEER_RANDOMLOOK_SPEED_MAX_COEF);
            }
        }

        public SAINSteeringClass(BotComponent sain) : base(sain)
        {
            _randomLook = new RandomLookClass(this);
            _steerPriorityClass = new SteerPriorityClass(this);
            HeardSoundSteering = new HeardSoundSteeringClass(this);
        }

        public float AngleToPointFromLookDir(Vector3 point)
        {
            Vector3 direction = (point - BotOwner.WeaponRoot.position).normalized;
            return Vector3.Angle(_lookDirection, direction);
        }

        public float AngleToDirectionFromLookDir(Vector3 direction)
        {
            return Vector3.Angle(_lookDirection, direction);
        }

        public void Init()
        {
            base.SubscribeToPreset(UpdatePresetSettings);
            HeardSoundSteering.Init();
        }

        public void Update()
        {
            HeardSoundSteering.Update();
            if (!Bot.SAINLayersActive) {
                BotOwner.Settings.FileSettings.Move.BASE_ROTATE_SPEED = _steerSettings.STEER_BASE_ROTATE_SPEED_PEACE;
            }
            else {
                BotOwner.Settings.FileSettings.Move.BASE_ROTATE_SPEED = _steerSettings.STEER_BASE_ROTATE_SPEED_COMBAT;
            }
        }

        public void Dispose()
        {
            HeardSoundSteering.Dispose();
        }

        public Vector3? EnemyLastKnown(Enemy enemy, out bool visible)
        {
            visible = false;
            EnemyPlace lastKnownPlace = enemy?.KnownPlaces.LastKnownPlace;
            if (lastKnownPlace == null) {
                return null;
            }
            visible = lastKnownPlace.CheckLineOfSight(Bot.Transform.EyePosition, LayerMaskClass.HighPolyWithTerrainMask);
            return lastKnownPlace.GroundedPosition();
        }

        public Vector3? FindLastKnownTarget(Enemy enemy)
        {
            EnemySteerDir = EEnemySteerDir.None;
            if (enemy == null) {
                EnemySteerDir = EEnemySteerDir.NullEnemy_ERROR;
                return null;
            }
            if (enemy.IsVisible) {
                EnemySteerDir = EEnemySteerDir.VisibleEnemyPos;
                return enemy.EnemyPosition + WeaponRootOffset;
            }
            var lastKnown = enemy.KnownPlaces.LastKnownPlace;
            if (lastKnown == null) {
                EnemySteerDir = EEnemySteerDir.NullLastKnown_ERROR;
                return null;
            }
            var lastSeen = enemy.KnownPlaces.LastSeenPlace;
            if (lastSeen != null) {
                if (lastSeen == lastKnown) {
                    EnemySteerDir = EEnemySteerDir.LastSeenPos;
                    return lastSeen.Position + WeaponRootOffset;
                }
                if ((lastSeen.Position - lastKnown.Position).sqrMagnitude < STEER_LASTSEEN_TO_LASTKNOWN_DISTANCE_SQR) {
                    EnemySteerDir = EEnemySteerDir.LastSeenPos;
                    return lastSeen.Position + WeaponRootOffset;
                }
            }

            EnemySteerDir = EEnemySteerDir.LastKnownPos;
            return lastKnown.Position + WeaponRootOffset;
        }

        private void lookToUnderFirePos()
        {
            LookToPoint(Bot.Memory.UnderFireFromPosition + WeaponRootOffset);
        }

        private void lookToLastHitPos()
        {
            var enemyWhoShotMe = _steerPriorityClass.EnemyWhoLastShotMe;
            if (enemyWhoShotMe != null) {
                Vector3? lastKnown = FindLastKnownTarget(enemyWhoShotMe);
                if (lastKnown != null) {
                    LookToPoint(lastKnown.Value);
                    return;
                }
                var lastShotPos = enemyWhoShotMe.Status.LastShotPosition;
                if (lastShotPos != null) {
                    LookToPoint(lastShotPos.Value + WeaponRootOffset);
                    return;
                }
            }
            LookToRandomPosition();
        }

        public HeardSoundSteeringClass HeardSoundSteering { get; }
        private readonly RandomLookClass _randomLook;
        private readonly SteerPriorityClass _steerPriorityClass;

        private Vector3 _lookDirection => Bot.LookDirection;

        protected void UpdatePresetSettings(SAINPresetClass preset)
        {
        }
    }
}
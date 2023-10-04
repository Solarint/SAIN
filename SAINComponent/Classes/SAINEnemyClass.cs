using EFT;
using SAIN.Helpers;
using SAIN.SAINComponent.BaseClasses;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace SAIN.SAINComponent.Classes
{
    public class SAINEnemyClass : SAINBase, ISAINClass
    {
        public SAINEnemyClass(SAINComponentClass bot, SAINPersonClass person) : base(bot)
        {
            TimeEnemyCreated = Time.time;
            EnemyPerson = person;

            EnemyStatus = new SAINEnemyStatus(this);
            Vision = new SAINEnemyVision(this);
            Path = new SAINEnemyPath(this);
        }

        public SAINPersonClass EnemyPerson { get; private set; }
        public SAINPersonTransformClass EnemyTransform => EnemyPerson.Transform;
        public bool IsCurrentEnemy => SAIN.HasEnemy && SAIN.EnemyController.ActiveEnemy == this;

        public void Init()
        {
        }

        public void Update()
        {
            if (!SAIN.HasEnemy)
            {
                SAIN.EnemyController.ClearEnemy();
                return;
            }

            bool isCurrent = IsCurrentEnemy;
            Vision.Update(isCurrent);
            Path.Update(isCurrent);
        }

        public void Dispose()
        {
        }

        public EnemyPathDistance CheckPathDistance() => Path.CheckPathDistance();

        // ActiveEnemy Properties
        public IPlayer EnemyIPlayer => EnemyPerson.IPlayer;

        public Player EnemyPlayer => EnemyPerson.Player;

        public float TimeEnemyCreated { get; private set; }

        public float TimeSinceEnemyCreated => Time.time - TimeEnemyCreated;

        public Vector3 EnemyPosition => EnemyTransform.Position;

        public Vector3 EnemyDirection => EnemyTransform.Direction(SAIN.Transform.Position);

        public Vector3 EnemyHeadPosition => EnemyTransform.Head;

        public Vector3 EnemyChestPosition => EnemyTransform.Chest;

        // Look Properties
        public bool InLineOfSight => Vision.InLineOfSight;

        public bool IsVisible => Vision.IsVisible;
        public bool CanShoot => Vision.CanShoot;
        public bool Seen => Vision.Seen;
        public Vector3? LastCornerToEnemy => Vision.LastSeenPosition;
        public float LastChangeVisionTime => Vision.VisibleStartTime;
        public bool EnemyLookingAtMe => EnemyStatus.EnemyLookingAtMe;
        public Vector3? LastSeenPosition => Vision.LastSeenPosition;
        public float VisibleStartTime => Vision.VisibleStartTime;
        public float TimeSinceSeen => Vision.TimeSinceSeen;

        // PathToEnemy Properties
        public bool ArrivedAtLastSeenPosition => Path.HasArrivedAtLastSeen;

        public float RealDistance => Path.EnemyDistance;
        public bool CanSeeLastCornerToEnemy => Path.CanSeeLastCornerToEnemy;
        public float PathDistance => Path.PathDistance;
        public NavMeshPath NavMeshPath => Path.PathToEnemy;

        public SAINEnemyStatus EnemyStatus { get; private set; }
        public SAINEnemyVision Vision { get; private set; }
        public SAINEnemyPath Path { get; private set; }
    }

    public class SAINEnemyVision : EnemyBase
    {
        public SAINEnemyVision(SAINEnemyClass enemy) : base(enemy)
        {
        }

        public void Update(bool isCurrentEnemy)
        {
            if (!isCurrentEnemy)
            {
                InLineOfSight = false;
                UpdateVisible(false);
                UpdateCanShoot(false);
                return;
            }

            bool visible = false;
            bool canshoot = false;
            bool usingLight = EnemyPlayer?.AIData?.UsingLight == true && Enemy?.Path != null && Enemy.EnemyTransform?.TransformNull == false && Enemy.Path.EnemyDistance < 50f;

            if (CheckLosTimer < Time.time)
            {
                CheckLosTimer = Time.time + 0.075f;
                InLineOfSight = CheckLineOfSight(usingLight);
            }

            var goalenemy = BotOwner.Memory.GoalEnemy;
            if ((goalenemy?.IsVisible == true || usingLight) && InLineOfSight)
            {
                visible = true;
            }
            if (goalenemy?.CanShoot == true)
            {
                canshoot = true;
            }

            UpdateVisible(visible);
            UpdateCanShoot(canshoot);
        }

        private bool CheckLineOfSight(bool usingLight)
        {
            if (Enemy.Path.EnemyDistance <= BotOwner.Settings.Current.CurrentVisibleDistance)
            {
                if (CheckInVisionCone() || usingLight)
                {
                    foreach (var part in EnemyPlayer.MainParts.Values)
                    {
                        Vector3 headPos = BotOwner.LookSensor._headPoint;
                        Vector3 direction = part.Position - headPos;
                        if (!Physics.Raycast(headPos, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool CheckInVisionCone()
        {
            Vector3 enemyDir = EnemyPosition - BotOwner.Position;
            Vector3 lookDir = BotOwner.LookDirection;
            float angle = Vector3.Angle(lookDir, enemyDir);
            float maxVisionCone = BotOwner.Settings.FileSettings.Core.VisibleAngle / 2f;
            return angle <= maxVisionCone;
        }

        public void UpdateVisible(bool visible)
        {
            bool wasVisible = IsVisible;
            IsVisible = visible;

            if (IsVisible)
            {
                if (!wasVisible)
                {
                    VisibleStartTime = Time.time;
                }
                if (!Seen)
                {
                    TimeFirstSeen = Time.time;
                    Seen = true;
                }
            }
            if (!IsVisible)
            {
                VisibleStartTime = -1f;
                if (wasVisible)
                {
                    TimeLastSeen = Time.time;
                    LastSeenPosition = EnemyPerson.Position;
                }
            }

            if (IsVisible != wasVisible)
            {
                LastChangeVisionTime = Time.time;
            }
        }

        public void UpdateCanShoot(bool value)
        {
            CanShoot = value;
        }

        public bool InLineOfSight { get; private set; }
        public bool IsVisible { get; private set; }
        public bool CanShoot { get; private set; }
        public Vector3? LastSeenPosition { get; set; }
        public float VisibleStartTime { get; private set; }
        public float TimeSinceSeen => Seen ? Time.time - TimeLastSeen : -1f;
        public bool Seen { get; private set; }
        public float TimeFirstSeen { get; private set; }
        public float TimeLastSeen { get; private set; }
        public float LastChangeVisionTime { get; private set; }

        private float CheckLosTimer;
    }

    public class SAINEnemyPath : EnemyBase
    {
        public SAINEnemyPath(SAINEnemyClass enemy) : base(enemy)
        {
        }

        public void Update(bool isCurrentEnemy)
        {
            if (!isCurrentEnemy)
            {
                LastCornerToEnemy = null;
                CanSeeLastCornerToEnemy = false;
                return;
            }

            if (CheckPathTimer < Time.time)
            {
                CheckPathTimer = Time.time + 0.35f;
                CalcPath();
            }
        }

        public NavMeshPathStatus PathToEnemyStatus { get; private set; }

        private void CalcPath()
        {
            bool hasLastCorner = false;
            float pathDistance = Mathf.Infinity;
            PathToEnemy.ClearCorners();
            if (NavMesh.CalculatePath(SAIN.Position, EnemyPosition, -1, PathToEnemy))
            {
                pathDistance = PathToEnemy.CalculatePathLength();
                if (PathToEnemy.corners.Length > 2)
                {
                    hasLastCorner = true;
                }
            }

            PathDistance = pathDistance;
            PathToEnemyStatus = PathToEnemy.status;

            if (hasLastCorner && PathToEnemyStatus == NavMeshPathStatus.PathComplete)
            {
                Vector3 lastCorner = PathToEnemy.corners[PathToEnemy.corners.Length - 2];
                LastCornerToEnemy = lastCorner;

                Vector3 cornerRay = lastCorner + Vector3.up * 1f;
                Vector3 headPos = SAIN.Transform.Head;
                Vector3 direction = cornerRay - headPos;

                CanSeeLastCornerToEnemy = !Physics.Raycast(headPos, direction.normalized, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask);
            }
            else
            {
                LastCornerToEnemy = null;
            }
        }

        private void UpdateDistance()
        {
        }

        public EnemyPathDistance CheckPathDistance()
        {
            const float VeryCloseDist = 8f;
            const float CloseDist = 35f;
            const float FarDist = 100f;
            const float VeryFarDist = 150f;

            EnemyPathDistance pathDistance;
            float distance = PathDistance;

            if (distance <= VeryCloseDist)
            {
                pathDistance = EnemyPathDistance.VeryClose;
            }
            else if (distance <= CloseDist)
            {
                pathDistance = EnemyPathDistance.Close;
            }
            else if (distance <= FarDist)
            {
                pathDistance = EnemyPathDistance.Mid;
            }
            else if (distance <= VeryFarDist)
            {
                pathDistance = EnemyPathDistance.Far;
            }
            else
            {
                pathDistance = EnemyPathDistance.VeryFar;
            }

            return pathDistance;
        }

        public float DistanceToEnemy(Vector3 point) => Vector.DistanceBetween(Enemy.EnemyPosition, point);

        public float DistanceToMe(Vector3 point) => Vector.DistanceBetween(SAIN.Transform.Position, point);

        public bool HasArrivedAtLastSeen => !Enemy.EnemyPerson.PlayerNull && Enemy.Seen && !Enemy.IsVisible && (MyDistanceFromLastSeen < 2f || VisitedLastSeenPosition);
        private bool VisitedLastSeenPosition => !Enemy.EnemyPerson.PlayerNull && Enemy.Seen && !Enemy.IsVisible && (MyDistanceFromLastSeen < 2f || VisitedLastSeenPosition);
        public float EnemyDistanceFromLastSeen => Enemy.LastSeenPosition != null ? DistanceToEnemy(Enemy.LastSeenPosition.Value) : 0f;
        public float EnemyDistance => DistanceToMe(Enemy.EnemyPosition);
        public float MyDistanceFromLastSeen => Enemy.LastSeenPosition != null ? DistanceToMe(Enemy.LastSeenPosition.Value) : EnemyDistance;
        public float PathDistance { get; private set; }
        public Vector3? LastCornerToEnemy { get; private set; }
        public bool CanSeeLastCornerToEnemy { get; private set; }

        public readonly NavMeshPath PathToEnemy = new NavMeshPath();

        private float CheckPathTimer = 0f;
    }

    public class SAINEnemyStatus : EnemyBase
    {
        public SAINEnemyStatus(SAINEnemyClass enemy) : base(enemy)
        {
        }

        public bool EnemyLookingAtMe
        {
            get
            {
                Vector3 dirToEnemy = Vector.NormalizeFastSelf(BotOwner.LookSensor._headPoint - EnemyPosition);
                return Vector.IsAngLessNormalized(dirToEnemy, EnemyPerson.Transform.LookDirection, 0.9659258f);
            }
        }

        public bool EnemyIsReloading
        {
            get
            {
                if (_soundResetTimer < Time.time)
                {
                    _enemyIsReloading = false;
                }
                return _enemyIsReloading;
            }
            set
            {
                if (value == true)
                {
                    _enemyIsReloading = true;
                    _soundResetTimer = Time.time + 3f * Random.Range(0.75f, 1.5f);
                }
            }
        }

        public bool EnemyHasGrenadeOut
        {
            get
            {
                if (_grenadeResetTimer < Time.time)
                {
                    _enemyHasGrenade = false;
                }
                return _enemyHasGrenade;
            }
            set
            {
                if (value == true)
                {
                    _enemyHasGrenade = true;
                    _grenadeResetTimer = Time.time + 3f * Random.Range(0.75f, 1.5f);
                }
            }
        }

        public bool EnemyIsHealing
        {
            get
            {
                if (_healResetTimer < Time.time)
                {
                    _enemyIsHeal = false;
                }
                return _enemyIsHeal;
            }
            set
            {
                if (value == true)
                {
                    _enemyIsHeal = true;
                    _healResetTimer = Time.time + 4f * Random.Range(0.75f, 1.25f);
                }
            }
        }

        private bool _enemyIsReloading;
        private float _soundResetTimer;
        private bool _enemyHasGrenade;
        private float _grenadeResetTimer;
        private bool _enemyIsHeal;
        private float _healResetTimer;
    }

    public abstract class EnemyBase
    {
        public EnemyBase(SAINEnemyClass enemy)
        {
            Enemy = enemy;
        }

        public SAINComponentClass SAIN => Enemy.SAIN;
        public Player EnemyPlayer => Enemy.EnemyPlayer;
        public IPlayer EnemyIPlayer => Enemy.EnemyPerson.IPlayer;
        public BotOwner BotOwner => Enemy.BotOwner;
        public SAINEnemyClass Enemy { get; private set; }
        public SAINPersonClass EnemyPerson => Enemy.EnemyPerson;
        public SAINPersonTransformClass EnemyTransform => Enemy.EnemyTransform;
        public Vector3 EnemyPosition => Enemy.EnemyPosition;
        public Vector3 EnemyDirection => Enemy.EnemyDirection;
        public bool IsCurrentEnemy => Enemy.IsCurrentEnemy;
    }
}
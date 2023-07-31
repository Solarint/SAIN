using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace SAIN.Classes
{
    public class SAINEnemy : SAINBot
    {
        static SAINEnemy()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(SAINEnemy));
        }

        public SAINEnemy(SAINComponent bot, IAIDetails person) : base(bot)
        {
            Person = person;
            EnemyProfileID = person.ProfileId;
            EnemyPlayer = EFTInfo.GetPlayer(person);

            EnemyStatus = new SAINEnemyStatus(this);
            EnemyVision = new SAINEnemyVision(this);
            EnemyPath = new SAINEnemyPath(this);

            TimeEnemyCreated = Time.time;
        }

        public void Update()
        {
            if (!SAIN.HasEnemy)
            {
                SAIN.EnemyController.ClearEnemy();
                return;
            }

            EnemyVision.Update();
            EnemyPath.Update();
        }
        public void UpdateCanShoot(bool value) => EnemyVision.UpdateCanShoot(value);
        public void UpdateVisible(bool inLineOfSight) => EnemyVision.UpdateVisible(inLineOfSight);
        public SAINEnemyPathEnum CheckPathDistance() => EnemyPath.CheckPathDistance();

        // Enemy Properties
        public IAIDetails Person { get; private set; }
        public string EnemyProfileID { get; private set; }
        public Player EnemyPlayer { get; private set; }
        public float TimeEnemyCreated { get; private set; }
        public float TimeSinceEnemyCreated => Time.time - TimeEnemyCreated;
        public Vector3 CurrPosition => (Person != null ? Person.Position : Vector3.zero);
        public Vector3 LookDirection => (Person != null ? Person.LookDirection : Vector3.zero);
        public Vector3 Direction => (Person != null ? CurrPosition - BotPosition : Vector3.zero);
        public Vector3 EnemyHeadPosition => (Person != null ? Person.MainParts[BodyPartType.head].Position : Vector3.zero);
        public Vector3 EnemyChestPosition => (Person != null ? Person.MainParts[BodyPartType.body].Position : Vector3.zero);

        // EnemyVision Properties
        public bool InLineOfSight => EnemyVision.InLineOfSight;
        public bool IsVisible => EnemyVision.IsVisible;
        public bool CanShoot => EnemyVision.CanShoot;
        public bool Seen => EnemyVision.Seen;
        public Vector3? LastCornerToEnemy => EnemyVision.LastSeenPosition;
        public float LastChangeVisionTime => EnemyVision.VisibleStartTime;
        public bool EnemyLookingAtMe => EnemyStatus.EnemyLookingAtMe;
        public Vector3 LastSeenPosition => EnemyVision.LastSeenPosition;
        public float VisibleStartTime => EnemyVision.VisibleStartTime;
        public float TimeFirstSeen => EnemyVision.TimeFirstSeen;
        public float TimeLastSeen => EnemyVision.TimeLastSeen;
        public float TimeSinceSeen => EnemyVision.TimeSinceSeen;

        // EnemyPath Properties
        public bool ArrivedAtLastSeenPosition => EnemyPath.ArrivedAtLastSeenPosition;
        public float RealDistance => EnemyPath.RealDistance;
        public bool CanSeeLastCornerToEnemy => EnemyPath.CanSeeLastCornerToEnemy;
        public float PathDistance => EnemyPath.PathDistance;
        public NavMeshPath Path => EnemyPath.Path;

        public SAINEnemyStatus EnemyStatus { get; private set; }
        public SAINEnemyVision EnemyVision { get; private set; }
        public SAINEnemyPath EnemyPath { get; private set; }

        private static readonly ManualLogSource Logger;
    }

    public class SAINEnemyVision : SAINEnemyAbstract
    {
        public SAINEnemyVision(SAINEnemy enemy) : base(enemy) { }

        public void Update()
        {
            bool visible = false;
            bool canshoot = false;

            if (CheckLosTimer < Time.time)
            {
                CheckLosTimer = Time.time + 0.075f;
                InLineOfSight = CheckLineOfSight();
            }

            var goalenemy = BotOwner.Memory.GoalEnemy;
            if (goalenemy?.IsVisible == true && InLineOfSight)
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

        private bool CheckLineOfSight()
        {
            if (CheckInVisionCone())
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
            return false;
        }

        private bool CheckInVisionCone()
        {
            Vector3 enemyDir = CurrPosition - BotOwner.Position;
            Vector3 lookDir = BotOwner.LookDirection;
            float angle = Vector3.Angle(lookDir, enemyDir);
            float maxVisionCone = BotOwner.Settings.FileSettings.Core.VisibleAngle;
            return angle <= maxVisionCone;
        }

        public void UpdateVisible(bool inLineOfSight)
        {
            bool wasVisible = IsVisible;
            IsVisible = inLineOfSight;

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

        public void LoseSight()
        {
            CanShoot = false;
            InLineOfSight = false;
            IsVisible = false;
        }

        public bool InLineOfSight { get; private set; }
        public bool IsVisible { get; private set; }
        public bool CanShoot { get; private set; }
        public Vector3 LastSeenPosition { get; private set; }
        public float VisibleStartTime { get; private set; }
        public float TimeSinceSeen => Seen ? Time.time - TimeLastSeen : -1f;
        public bool Seen { get; private set; }
        public float TimeFirstSeen { get; private set; }
        public float TimeLastSeen { get; private set; }
        public float LastChangeVisionTime { get; private set; }

        private float CheckLosTimer;
    }

    public class SAINEnemyPath : SAINEnemyAbstract
    {
        public SAINEnemyPath(SAINEnemy enemy) : base(enemy) { }

        public void Update()
        {
            if (CheckPathTimer < Time.time)
            {
                CheckPathTimer = Time.time + 0.35f;
                CalcPath();
            }

            if (DistanceTimer < Time.time)
            {
                DistanceTimer = Time.time + 0.1f;
                UpdateDistance();
            }
        }

        private void CalcPath()
        {
            Path.ClearCorners();
            if (NavMesh.CalculatePath(BotOwner.Transform.position, CurrPosition, -1, Path))
            {
                PathDistance = Path.CalculatePathLength();
                if (Path.corners.Length > 2)
                {
                    LastCornerToEnemy = Path.corners[Path.corners.Length - 2];
                    Vector3 cornerRay = LastCornerToEnemy.Value;
                    cornerRay += Vector3.up * 1f;
                    Vector3 headPos = BotOwner.LookSensor._headPoint;
                    Vector3 direction = cornerRay - headPos;
                    CanSeeLastCornerToEnemy = !Physics.Raycast(headPos, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask);
                }
                else
                {
                    LastCornerToEnemy = null;
                }
            }
        }

        private void UpdateDistance()
        {
            if (EnemyPerson != null)
            {
                bool isVisible = Enemy.IsVisible;
                Vector3 lastSeenPos = Enemy.LastSeenPosition;
                float distance = GetMagnitudeToBot(CurrPosition);
                RealDistance = distance;
                LastSeenDistance = isVisible ? distance : GetMagnitudeToBot(lastSeenPos);
                DistanceFromLastSeen = isVisible ? 0f : (lastSeenPos - CurrPosition).magnitude;

                if (isVisible)
                {
                    ArrivedAtLastSeenPosition = false;
                }
                else if ((lastSeenPos - BotOwner.Position).sqrMagnitude < 2f)
                {
                    ArrivedAtLastSeenPosition = true;
                }
            }
            else
            {
                RealDistance = 0;
                LastSeenDistance = 0;
                DistanceFromLastSeen = 0;
                ArrivedAtLastSeenPosition = false;
            }
        }

        public SAINEnemyPathEnum CheckPathDistance()
        {
            const float VeryCloseDist = 5f;
            const float CloseDist = 20f;
            const float FarDist = 80f;
            const float VeryFarDist = 120f;

            SAINEnemyPathEnum pathDistance;
            float distance = PathDistance;

            if (distance <= VeryCloseDist)
            {
                pathDistance = SAINEnemyPathEnum.VeryClose;
            }
            else if (distance <= CloseDist)
            {
                pathDistance = SAINEnemyPathEnum.Close;
            }
            else if (distance <= FarDist)
            {
                pathDistance = SAINEnemyPathEnum.Mid;
            }
            else if (distance <= VeryFarDist)
            {
                pathDistance = SAINEnemyPathEnum.Far;
            }
            else
            {
                pathDistance = SAINEnemyPathEnum.VeryFar;
            }

            return pathDistance;
        }

        private float GetMagnitudeToBot(Vector3 point) => (BotOwner.Position - point).magnitude;

        public bool ArrivedAtLastSeenPosition { get; private set; }
        public float DistanceFromLastSeen { get; private set; }
        public float PathDistance { get; private set; }
        public float RealDistance { get; private set; }
        public float LastSeenDistance { get; private set; }
        public Vector3? LastCornerToEnemy { get; private set; }
        public bool CanSeeLastCornerToEnemy { get; private set; }
        public NavMeshPath Path { get; private set; } = new NavMeshPath();

        private float DistanceTimer = 0f;
        private float CheckPathTimer = 0f;
    }

    public class SAINEnemyStatus : SAINEnemyAbstract
    {
        public SAINEnemyStatus(SAINEnemy enemy) : base(enemy) { }

        public bool EnemyLookingAtMe
        {
            get
            {
                Vector3 dirToEnemy = VectorHelpers.NormalizeFastSelf(BotOwner.LookSensor._headPoint - CurrPosition);
                return VectorHelpers.IsAngLessNormalized(dirToEnemy, EnemyPerson.LookDirection, 0.9659258f);
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

    public abstract class SAINEnemyAbstract
    {
        public SAINEnemyAbstract(SAINEnemy enemy)
        {
            Enemy = enemy;
        }

        public readonly SAINEnemy Enemy;
        public BotOwner BotOwner => Enemy.BotOwner;
        public Player EnemyPlayer => Enemy.EnemyPlayer;
        public IAIDetails EnemyPerson => Enemy.Person;
        public Vector3 CurrPosition => Enemy.CurrPosition;
    }
}
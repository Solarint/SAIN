using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace SAIN.Classes
{
    public class SAINEnemy : SAINBot
    {
        public EnemyBodyPart[] BodyParts { get; private set; } = new EnemyBodyPart[6];
        public EnemyBodyPart ChestPart { get; private set; }
        public EnemyBodyPart HeadPart { get; private set; }
        public Player EnemyPlayer { get; private set; }

        private readonly float BotDifficultyModifier;

        public SAINEnemy(BotOwner bot, IAIDetails person, float BotDifficultyMod) : base(bot)
        {
            Person = person;
            EnemyPlayer = person.GetPlayer;
            BotDifficultyModifier = BotDifficultyMod;
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            TimeEnemyCreated = Time.time;

            //DefaultLogger.LogInfo($"New Enemy [{person.GetPlayer.name}] for {BotOwner.name}");

            //BifacialTransform bottransform = bot.Transform;
            //BodyPartType type;
            //EnemyBodyPart part;
            //for (int i = 0; i < PartTypes.Count; i++)
            //{
            //    type = PartTypes[i];
            //    part = new EnemyBodyPart(bottransform, person, type);
            //
            //    if (type == BodyPartType.head)
            //    {
            //        HeadPart = part;
            //    }
            //    if (type == BodyPartType.body)
            //    {
            //        ChestPart = part;
            //    }
            //
            //    BodyParts[i] = part;
            //}
        }

        public float TimeEnemyCreated { get; private set; }
        public float TimeSinceEnemyCreated => Time.time - TimeEnemyCreated;

        public readonly List<BodyPartType> PartTypes = new List<BodyPartType> { BodyPartType.leftLeg, BodyPartType.rightLeg, BodyPartType.body, BodyPartType.leftArm, BodyPartType.rightArm, BodyPartType.head };

        private readonly ManualLogSource Logger;

        public void Update()
        {
            UpdateDistance();
            UpdatePath();

            bool visible = false;
            bool canshoot = false;
            //foreach (var part in BodyParts)
            //{
            //    if (part.Visible)
            //    {
            //        visible = true;
            //    }
            //    if (part.CanShoot)
            //    {
            //        canshoot = true;
            //    }
            //    if (canshoot && visible)
            //    {
            //        break;
            //    }
            //}
            if (CheckLosTimer < Time.time)
            {
                CheckLosTimer = Time.time + 0.075f;
                InLineOfSight = false;
                foreach (var part in EnemyPlayer.MainParts.Values)
                {
                    Vector3 direction = part.Position - SAIN.HeadPosition;
                    if (!Physics.Raycast(SAIN.HeadPosition, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        InLineOfSight = true; break;
                    }
                }
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

        private void UpdateDistance()
        {
            if (DistanceTimer < Time.time)
            {
                DistanceTimer = Time.time + 0.1f;
                if (Person != null)
                {
                    float distance = GetMagnitudeToBot(Position);
                    RealDistance = distance;
                    LastSeenDistance = IsVisible ? distance : GetMagnitudeToBot(PositionLastSeen);
                    DistanceFromLastSeen = IsVisible ? 0f : (PositionLastSeen - Position).magnitude;
                }
                else
                {
                    RealDistance = 0;
                    LastSeenDistance = 0;
                    DistanceFromLastSeen = 0;
                }
            }
        }

        public float DistanceFromLastSeen { get; private set; }
        public Vector3 Position => (Person != null ? Person.Position : Vector3.zero);

        private float GetMagnitudeToBot(Vector3 point)
        {
            return (BotOwner.Position - point).magnitude;
        }

        private bool _enemyIsReloading;
        private float _soundResetTimer;

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

        private bool _enemyHasGrenade;
        private float _grenadeResetTimer;

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

        private bool _enemyIsHeal;
        private float _healResetTimer;

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

        public float RealDistance { get; private set; }
        public float LastSeenDistance { get; private set; }
        public Vector3 PositionLastSeen { get; private set; }
        public float VisibleStartTime { get; private set; }
        public float TimeSinceSeen => Seen ? Time.time - TimeLastSeen : -1f;
        public bool Seen { get; private set; }
        public float TimeFirstSeen { get; private set; }
        public float TimeLastSeen { get; private set; }
        public bool CanHearCloseVisible { get; private set; }
        public bool EnemyClose { get; private set; }
        
        private float DistanceTimer = 0f;

        public void UpdateCanShoot(bool value)
        {
            CanShoot = value;
        }

        public Vector3 Direction => Position - BotPosition;
        private float CheckLosTimer;

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
                    PositionLastSeen = Person.Position;
                }
            }

            if (IsVisible != wasVisible)
            {
                LastChangeVisionTime = Time.time;
            }
        }

        public Vector3? LastCornerToEnemy { get; private set; }

        public float LastChangeVisionTime { get; private set; }

        public void UpdatePath()
        {
            if (CheckPathTimer < Time.time)
            {
                CheckPathTimer = Time.time + 0.5f;

                CalcPath(Person.Position);
            }
        }

        private void CalcPath(Vector3 pos)
        {
            Path.ClearCorners();
            if (NavMesh.CalculatePath(BotOwner.Transform.position, pos, -1, Path))
            {
                PathDistance = Path.CalculatePathLength();
                if (Path.corners.Length > 2)
                {
                    LastCornerToEnemy = Path.corners[Path.corners.Length - 2];
                    Vector3 cornerRay = LastCornerToEnemy.Value;
                    cornerRay += Vector3.up * 1f;
                    Vector3 direction = cornerRay - SAIN.HeadPosition;
                    CanSeeLastCornerToEnemy = !Physics.Raycast(SAIN.HeadPosition, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask);
                }
                else
                {
                    LastCornerToEnemy = null;
                }
            }
        }

        public bool CanSeeLastCornerToEnemy { get; private set; }

        public SAINEnemyPath CheckPathDistance()
        {
            const float VeryCloseDist = 5f;
            const float CloseDist = 20f;
            const float FarDist = 80f;
            const float VeryFarDist = 120f;

            SAINEnemyPath pathDistance;
            float distance = PathDistance;

            if (distance <= VeryCloseDist)
            {
                pathDistance = SAINEnemyPath.VeryClose;
            }
            else if (distance <= CloseDist)
            {
                pathDistance = SAINEnemyPath.Close;
            }
            else if (distance <= FarDist)
            {
                pathDistance = SAINEnemyPath.Mid;
            }
            else if (distance <= VeryFarDist)
            {
                pathDistance = SAINEnemyPath.Far;
            }
            else
            {
                pathDistance = SAINEnemyPath.VeryFar;
            }

            return pathDistance;
        }

        public NavMeshPath Path = new NavMeshPath();
        public float PathDistance { get; private set; }

        private float CheckPathTimer = 0f;

        public bool EnemyLookingAtMe
        {
            get
            {
                Vector3 EnemyLookDirection = VectorHelpers.NormalizeFastSelf(BotOwner.LookSensor._headPoint - Person.Transform.position);
                return VectorHelpers.IsAngLessNormalized(EnemyLookDirection, Person.LookDirection, 0.9659258f);
            }
        }

        public Vector3 EnemyHeadPosition => Person.MainParts[BodyPartType.head].Position;

        public Vector3 EnemyChestPosition => Person.MainParts[BodyPartType.body].Position;

        public IAIDetails Person { get; private set; }

        public bool InLineOfSight { get; private set; }

        public bool IsVisible { get; private set; }

        public bool CanShoot { get; private set; }
    }
}
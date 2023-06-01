using EFT;
using SAIN.Components;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace SAIN.Classes
{
    public class EnemiesComponent : MonoBehaviour
    {
        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
        }

        private void Update()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                return;
            }

            if (UpdateVisionTimer < Time.time)
            {
                UpdateVisionTimer = Time.time + 0.25f;

                UpdateEnemyList();

                UpdateVisionOnEnemies();

                FindPriorityEnemy();
            }
        }

        private float UpdateListTimer = 0f;
        private float UpdateVisionTimer = 0f;
        private float PriorityEnemyTimer = 0f;

        private void UpdateEnemyList()
        {
            if (BotOwner.EnemiesController.EnemyInfos.Count > 0)
            {
                var dictionary = new Dictionary<IAIDetails, SAINEnemy>();

                foreach (var enemy in BotOwner.BotsGroup.Enemies)
                {
                    if (enemy.Key != null && enemy.Key.HealthController.IsAlive)
                    {
                        dictionary.Add(enemy.Key, new SAINEnemy(BotOwner, enemy.Key));
                    }
                }

                ActiveEnemies = dictionary;
            }
        }

        private void UpdateVisionOnEnemies()
        {
            if (ActiveEnemies.Count > 0)
            {
                List<Vector3> enemyPositions = new List<Vector3>();

                foreach (var enemy in ActiveEnemies)
                {
                    if (enemy.Key != null && enemy.Value != null)
                    {
                        enemy.Value.ManualUpdate();

                        if (enemy.Value.IsVisible)
                        {
                            enemyPositions.Add(enemy.Key.Position);
                        }
                        else
                        {
                            enemyPositions.Add(enemy.Value.LastSeen.LastSeenPos);
                        }
                    }
                }

                EnemyPositions = enemyPositions.ToArray();
            }
        }

        public bool IsPersonLookAtMe(IAIDetails person)
        {
            if (person != null)
            {
                Vector3 EnemyLookDirection = VectorHelpers.NormalizeFastSelf(BotOwner.LookSensor._headPoint - person.Transform.position);
                return VectorHelpers.IsAngLessNormalized(EnemyLookDirection, person.LookDirection, 0.9659258f);
            }
            return false;
        }

        public SAINEnemy PriorityEnemy { get; private set; }

        private void FindPriorityEnemy()
        {
            SAINEnemy priorityEnemy = null;

            if (ActiveEnemies.Count > 0)
            {
                float distance = 999f;

                foreach (var enemy in ActiveEnemies)
                {
                    if (enemy.Key != null && enemy.Value != null)
                    {
                        if (enemy.Value.IsVisible)
                        {
                            float enemydist = Vector3.Distance(enemy.Key.Position, BotOwner.Position);
                            if (enemydist < distance)
                            {
                                distance = enemydist;
                                priorityEnemy = enemy.Value;
                            }
                        }
                    }
                }
            }

            PriorityEnemy = priorityEnemy;
        }

        public Vector3[] EnemyPositions { get; private set; }

        public SAINEnemy ClosestEnemy
        {
            get
            {
                SAINEnemy closestEnemy = null;

                if (ActiveEnemies.Count > 0)
                {
                    float distance = 999f;
                    foreach (var enemy in ActiveEnemies)
                    {
                        if (enemy.Key != null && enemy.Value != null)
                        {
                            float enemydist = Vector3.Distance(enemy.Key.Position, BotOwner.Position);

                            if (enemydist < distance)
                            {
                                distance = enemydist;
                                closestEnemy = enemy.Value;
                            }
                        }
                    }
                }

                return closestEnemy;
            }
        }

        public Dictionary<IAIDetails, SAINEnemy> ActiveEnemies { get; private set; } = new Dictionary<IAIDetails, SAINEnemy>();

        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN.BotOwner;

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }
    }

    public class SAINEnemy : SAINBot
    {
        public SAINEnemy(BotOwner bot, IAIDetails person) : base(bot)
        {
            Person = person;
            Path = new EnemyPath(bot, this);
            LastSeen = new EnemyLastSeen(bot, this);
        }

        public void ManualUpdate()
        {
            if (NextRayCastTime < Time.time)
            {
                NextRayCastTime = Time.time + 0.25f;

                if (BotOwner.LookSensor.IsPointInVisibleSector(Person.Position))
                {
                    IsVisible = RaycastHelpers.CheckVisible(BotOwner.LookSensor._headPoint, Person, LayerMaskClass.HighPolyWithTerrainMask);

                    //CanShoot = RaycastHelpers.CheckVisible(BotOwner.WeaponRoot.position, Person, LayerMaskClass.HighPolyWithTerrainMask);

                    LastSeen.UpdateSeen(IsVisible);
                }
            }

            if (CheckPathTimer < Time.time)
            {
                CheckPathTimer = Time.time + 0.5f;

                Path.ManualUpdate();
            }
        }

        public bool EnemyLookingAtMe
        {
            get
            {
                if (Person != null)
                {
                    Vector3 EnemyLookDirection = VectorHelpers.NormalizeFastSelf(BotOwner.LookSensor._headPoint - Person.Transform.position);
                    return VectorHelpers.IsAngLessNormalized(EnemyLookDirection, Person.LookDirection, 0.9659258f);
                }
                return false;
            }
        }

        public float TimeFirstVisible = 0f;

        public float TimeVisibleReal = 0f;

        public Vector3? EnemyHeadPosition => Person.MainParts[BodyPartType.head].Position;

        public Vector3? EnemyChestPosition => Person.MainParts[BodyPartType.body].Position;

        public EnemyPath Path { get; private set; }

        public EnemyLastSeen LastSeen { get; private set; }

        public IAIDetails Person { get; private set; }

        public bool IsVisible { get; private set; }

        public bool CanShoot { get; private set; }

        private float CheckPathTimer = 0f;

        private float NextRayCastTime = 0f;
    }

    public class EnemyPath : SAINBot
    {
        private readonly SAINEnemy Enemy;

        public EnemyPath(BotOwner bot, SAINEnemy enemy) : base(bot)
        {
            Enemy = enemy;
        }

        public void ManualUpdate()
        {
            if (Enemy == null)
            {
                return;
            }

            Vector3 pos = Vector3.zero;
            if (Enemy.IsVisible)
            {
                pos = Enemy.Person.Position;
            }
            else if (Enemy.LastSeen.HasSeenEnemy)
            {
                pos = Enemy.LastSeen.LastSeenPos;
            }

            if (Enemy.IsVisible ||  Enemy.LastSeen.HasSeenEnemy)
            {
                Path = new NavMeshPath();
                NavMesh.CalculatePath(BotOwner.Transform.position, pos, -1, Path);
                PathDistance = Path.CalculatePathLength();
            }
        }

        public bool RangeVeryClose => PathDistance <= VeryCloseDist;

        public bool RangeClose => !RangeVeryClose && PathDistance <= CloseDist;

        public bool RangeMid => !RangeClose && PathDistance <= FarDist;

        public bool RangeFar => !RangeMid && PathDistance <= VeryFarDist;

        public bool RangeVeryFar => PathDistance > VeryFarDist;

        public NavMeshPath Path = new NavMeshPath();

        public float PathDistance { get; private set; } = 999f;

        public const float VeryCloseDist = 8f;

        public const float CloseDist = 30f;

        public const float FarDist = 80f;

        public const float VeryFarDist = 120f;
    }

    public class EnemyLastSeen : SAINBot
    {
        private readonly SAINEnemy Enemy;

        public EnemyLastSeen(BotOwner bot, SAINEnemy enemy) : base(bot)
        {
            Enemy = enemy;
        }

        public void UpdateSeen(bool CanSee)
        {
            if (CanSee)
            {
                SeenTime = Time.time;

                if (!HasSeenEnemy)
                {
                    HasSeenEnemy = true;
                }
            }
            else if (CouldSeeEnemy)
            {
                TimeSinceSeen = Time.time - SeenTime;

                LastSeenPos = Enemy.Person.Position;
            }

            CouldSeeEnemy = CanSee;
        }

        public bool HasSeenEnemy { get; private set; }

        private bool CouldSeeEnemy = false;

        public Vector3 LastSeenPos { get; private set; }

        public bool NotSeenEnemyRecent => TimeSinceSeen > 3f;

        public bool NotSeenEnemyMid => TimeSinceSeen > 10f;

        public bool NotSeenEnemyLong => TimeSinceSeen > 20f;

        public bool NotSeenEnemyVeryLong => TimeSinceSeen > 60f;

        public float SeenTime { get; private set; }

        public float TimeSinceSeen { get; private set; }
    }
}
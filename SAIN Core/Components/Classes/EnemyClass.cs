using EFT;
using SAIN.Components;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Classes
{
    public class EnemyClass : SAINBot
    {
        public EnemyClass(BotOwner bot) : base(bot)
        {
            LastSeen = new EnemyLastSeen(bot);
            Path = new EnemyPath(bot);
        }

        public void Update(IAIDetails person)
        {
            if (person == null)
            {
                return;
            }

            if (Timers.CheckPathTimer < Time.time)
            {
                Timers.CheckPathTimer = Time.time + Timers.CheckPathFreq;
                Path.Update(person.Transform.position);
            }

            if (Timers.VisionRaycastTimer < Time.time)
            {
                Timers.VisionRaycastTimer = Time.time + Timers.VisionRaycastFreq;
                UpdateVision(person);
            }
        }

        private void UpdateVision(IAIDetails person)
        {
            if (BotOwner.Memory.GoalEnemy.IsVisible && BotOwner.Memory.GoalEnemy.CanShoot)
            {
                EnemyLookingAtMe = IsPersonLookAtMe(person);

                person.MainParts.TryGetValue(BodyPartType.head, out BodyPartClass EnemyHead);
                EnemyHeadPosition = EnemyHead.Position;
                person.MainParts.TryGetValue(BodyPartType.body, out BodyPartClass EnemyBody);
                EnemyChestPosition = EnemyBody.Position;

                LastSeen.UpdateSeen(person);
            }
            else
            {
                LastSeen.UpdateTimeSinceSeen();
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

        public bool EnemyLookingAtMe { get; private set; } = false;

        //public bool CanShoot = false;
        public float TimeFirstVisible = 0f;
        public float TimeVisibleReal = 0f;
        //public bool CanSee = false;

        public Vector3? EnemyHeadPosition { get; private set; }
        public Vector3? EnemyChestPosition { get; private set; }

        public EnemyPath Path { get; private set; }
        public EnemyLastSeen LastSeen { get; private set; }

        private readonly Timers Timers = new Timers();

        public class EnemyPath : SAINBot
        {
            public EnemyPath(BotOwner bot) : base(bot)
            {
            }

            public void Update(Vector3 enemyPosition)
            {
                Path = new NavMeshPath();
                NavMesh.CalculatePath(BotOwner.Transform.position, enemyPosition, -1, Path);
                PathLength = Path.CalculatePathLength();
            }

            /// <summary>
            /// Gets a value indicating whether the path length is very close. 
            /// </summary>
            public bool RangeVeryClose => PathLength <= VeryCloseDist;
            /// <summary>
            /// Checks if the path length is within the close distance and not within the very close distance. 
            /// </summary>
            public bool RangeClose => !RangeVeryClose && PathLength <= CloseDist;
            /// <summary>
            /// Checks if the path length is within the mid range and not close.
            /// </summary>
            public bool RangeMid => !RangeClose && PathLength <= FarDist;
            /// <summary>
            /// Checks if the path length is within the very far distance and not within the mid range. 
            /// </summary>
            public bool RangeFar => !RangeMid && PathLength <= VeryFarDist;
            /// <summary>
            /// Checks if the path length is greater than the VeryFarDist. 
            /// </summary>
            public bool RangeVeryFar => PathLength > VeryFarDist;

            public NavMeshPath Path = new NavMeshPath();
            public float PathLength { get; private set; } = 0f;

            public const float VeryCloseDist = 8f;
            public const float CloseDist = 30f;
            public const float FarDist = 80f;
            public const float VeryFarDist = 120f;
        }

        public class EnemyLastSeen : SAINBot
        {
            public EnemyLastSeen(BotOwner bot) : base(bot)
            {
            }

            public void UpdateSeen(IAIDetails person)
            {
                Time = UnityEngine.Time.time;
            }

            public void UpdateTimeSinceSeen()
            {
                TimeSinceSeen = UnityEngine.Time.time - Time;
            }


            /// <summary>
            /// Gets a value indicating whether the enemy has not been seen in the last 3 seconds.
            /// </summary>
            public bool NotSeenEnemyRecent => TimeSinceSeen > 3f;
            /// <summary>
            /// Gets a value indicating whether the enemy has not been seen in the middle for more than 10 seconds.
            /// </summary>
            public bool NotSeenEnemyMid => TimeSinceSeen > 10f;
            /// <summary>
            /// Gets a value indicating whether the enemy has not been seen for more than 20 seconds.
            /// </summary>
            public bool NotSeenEnemyLong => TimeSinceSeen > 20f;
            /// <summary>
            /// Gets a value indicating whether the enemy has not been seen for more than 60 seconds.
            /// </summary>
            public bool NotSeenEnemyVeryLong => TimeSinceSeen > 60f;

            public float Time { get; private set; }
            public float TimeSinceSeen { get; private set; }

            public float EnemyStraightDistance { get; private set; }
        }
    }
}
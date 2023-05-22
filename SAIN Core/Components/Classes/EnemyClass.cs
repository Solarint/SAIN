using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN_Helpers;
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

            UpdateGrenade();

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

            if (Timers.ShootRaycastTimer < Time.time)
            {
                Timers.ShootRaycastTimer = Time.time + Timers.ShootRaycastFreq;
                UpdateShoot(person);
            }
        }

        private void UpdateVision(IAIDetails person)
        {
            bool CouldSeeEnemy = CanSee;
            CanSee = RaycastHelpers.CheckVisible(BotOwner.MyHead.position, person, SAINCoreComponent.SightMask);

            if (CanSee)
            {
                if (!CouldSeeEnemy)
                {
                    TimeFirstVisible = Time.time;
                }
                else
                {
                    TimeVisibleReal = Time.time - TimeFirstVisible;
                }

                EnemyLookingAtMe = IsPersonLookAtMe(person);

                person.MainParts.TryGetValue(BodyPartType.head, out BodyPartClass EnemyHead);
                EnemyHeadPosition = EnemyHead.Position;
                person.MainParts.TryGetValue(BodyPartType.body, out BodyPartClass EnemyBody);
                EnemyChestPosition = EnemyBody.Position;

                LastSeen.UpdateSeen(person);

                NewEnemy = person != LastPerson;
                if (NewEnemy)
                {
                    DebugGizmos(Color.magenta, 3f);

                    Path.Update(person.Transform.position);
                    LastPerson = CurrentPerson;
                    CurrentPerson = person;
                }
                else
                {
                    if (EnemyLookingAtMe)
                    {
                        DebugGizmos(Color.yellow);
                    }
                    else
                    {
                        DebugGizmos(Color.green);
                    }
                }
            }
            else
            {
                LastSeen.UpdateTimeSinceSeen();
            }
        }

        private void UpdateShoot(IAIDetails person)
        {
            CanShoot = RaycastHelpers.CheckVisible(BotOwner.WeaponRoot.position, person, SAINCoreComponent.ShootMask);

            if (CanShoot)
            {
                LastSeen.UpdateShoot(person);

                DebugGizmos(BotOwner.WeaponRoot.position, Color.red);
            }
            else
            {
                LastSeen.UpdateTimeSinceShoot();
            }
        }

        private float GrenadeCheckTimer = 0f;

        private void UpdateGrenade()
        {
            if (BotOwner.BewareGrenade?.GrenadeDangerPoint != null)
            {
                Grenade = BotOwner.BewareGrenade.GrenadeDangerPoint.Grenade;

                if (GrenadeCheckTimer < Time.time)
                {
                    GrenadeCheckTimer = Time.time + 0.05f;

                    CanSeeGrenade = RaycastHelpers.CheckVisible(Core.HeadPosition, Grenade.transform.position, SAINCoreComponent.SightMask);

                    if (CanSeeGrenade)
                    {
                        LastSeen.UpdateGrenade(Grenade.transform.position, BotOwner.BewareGrenade.GrenadeDangerPoint.DangerPoint);
                    }
                    else
                    {
                        LastSeen.UpdateTimeSinceGrenade();
                    }
                }
            }
            else
            {
                if (Grenade != null)
                {
                    Grenade = null;
                    LastSeen.DisposeGrenade();
                }
            }
        }

        private void DebugGizmos(Color color, float width = 0.025f)
        {
            if (Plugin.DebugDrawVision.Value)
            {
                DebugDrawer.Line(EnemyChestPosition.Value, Core.HeadPosition, width, color, Plugin.VisionRaycast.Value);
            }
        }

        private void DebugGizmos(Vector3 start, Color color, float width = 0.025f)
        {
            if (Plugin.DebugDrawVision.Value)
            {
                DebugDrawer.Line(start, EnemyChestPosition.Value, width, color, Plugin.VisionRaycast.Value);
            }
        }

        private void DebugGizmos(Vector3 start, Vector3 end, Color color, float width = 0.025f)
        {
            if (Plugin.DebugDrawVision.Value)
            {
                DebugDrawer.Line(start, end, width, color, Plugin.VisionRaycast.Value);
            }
        }

        public bool IsPersonLookAtMe(IAIDetails person)
        {
            if (person != null)
            {
                Vector3 EnemyLookDirection = SAIN_Math.NormalizeFastSelf(BotOwner.LookSensor._headPoint - person.Transform.position);
                return SAIN_Math.IsAngLessNormalized(EnemyLookDirection, person.LookDirection, 0.9659258f);
            }
            return false;
        }

        public Grenade Grenade { get; private set; }
        public bool NewEnemy { get; private set; } = false;
        public bool EnemyLookingAtMe { get; private set; } = false;
        public IAIDetails CurrentPerson { get; private set; } = null;
        public IAIDetails LastPerson { get; private set; } = null;

        public bool CanShoot = false;
        public float TimeFirstVisible = 0f;
        public float TimeVisibleReal = 0f;
        public bool CanSee = false;
        public bool CanSeeGrenade { get; private set; }

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
                EnemyPosition = person.Transform.position;
                EnemyDirection = person.Transform.position - BotOwner.Transform.position;
                EnemyStraightDistance = Vector3.Distance(person.Transform.position, BotOwner.Transform.position);
                BotPosition = BotOwner.Transform.position;
            }

            public void UpdateTimeSinceSeen()
            {
                TimeSinceSeen = UnityEngine.Time.time - Time;
            }

            public void UpdateShoot(IAIDetails person)
            {
                ShootTime = UnityEngine.Time.time;
                BotShootPosition = BotOwner.Transform.position;
            }

            public void UpdateTimeSinceShoot()
            {
                TimeSinceShoot = UnityEngine.Time.time - ShootTime;
            }

            public void UpdateGrenade(Vector3 grenadePos, Vector3 dangerPoint)
            {
                GrenadeTime = UnityEngine.Time.time;
                GrenadePosition = grenadePos;
                GrenadeDangerPoint = dangerPoint;
            }

            public void UpdateTimeSinceGrenade()
            {
                GrenadeTimeSinceSeen = UnityEngine.Time.time - GrenadeTime;
            }

            public void DisposeGrenade()
            {
                GrenadeTimeSinceSeen = 999f;
                GrenadePosition = null;
                GrenadeDangerPoint = null;
                GrenadeTime = 0f;
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

            public Vector3 BotPosition { get; private set; }
            public float ShootTime { get; private set; }
            public float TimeSinceShoot { get; private set; }
            public Vector3 BotShootPosition { get; private set; }

            public Vector3 EnemyPosition { get; private set; }
            public Vector3 EnemyDirection { get; private set; }
            public float EnemyStraightDistance { get; private set; }

            public Vector3? GrenadeDangerPoint { get; private set; }
            public Vector3? GrenadePosition { get; private set; }
            public float GrenadeTime { get; private set; }
            public float GrenadeTimeSinceSeen { get; private set; }
        }
    }
}
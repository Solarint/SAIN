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

                LastSeen.Update(person);

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
                LastSeen.TimeSinceSeen = Time.time - LastSeen.Time;
            }
        }

        private void UpdateShoot(IAIDetails person)
        {
            CanShoot = RaycastHelpers.CheckVisible(BotOwner.WeaponRoot.position, person, SAINCoreComponent.ShootMask);

            if (CanShoot)
            {
                LastSeen.BotShootTime = Time.time;

                LastSeen.BotShootPosition = BotOwner.Transform.position;

                DebugGizmos(BotOwner.WeaponRoot.position, Color.red);
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
                    GrenadeCheckTimer = Time.time + 0.1f;

                    CanSeeGrenade = RaycastHelpers.CheckVisible(Core.HeadPosition, Grenade.transform.position, SAINCoreComponent.SightMask);

                    if (CanSeeGrenade)
                    {
                        LastSeen.GrenadePosition = Grenade.transform.position;
                        LastSeen.GrenadeTime = Time.time;
                    }
                    else
                    {
                        LastSeen.GrenadeTimeSinceSeen = Time.time - LastSeen.GrenadeTime;
                    }
                }
            }
            else
            {
                if (Grenade != null) Grenade = null;
                if (LastSeen.GrenadePosition != Vector3.zero) LastSeen.GrenadePosition = Vector3.zero;
                if (LastSeen.GrenadeTime != 0f) LastSeen.GrenadeTime = 0f;
                if (LastSeen.GrenadeTimeSinceSeen != 0f) LastSeen.GrenadeTimeSinceSeen = 0f;
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
        public bool CanSeeGrenade = false;

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
                Length = Path.CalculatePathLength();
            }

            public bool RangeVeryClose => Length <= VeryCloseDist;
            public bool RangeClose => Length <= CloseDist;
            public bool RangeMid => Length <= FarDist;
            public bool RangeFar => Length > FarDist;

            public NavMeshPath Path = new NavMeshPath();
            public float Length = 999f;

            private const float VeryCloseDist = 3f;
            private const float CloseDist = 10f;
            private const float FarDist = 100f;
        }

        public class EnemyLastSeen : SAINBot
        {
            public EnemyLastSeen(BotOwner bot) : base(bot)
            {
            }

            public void Update(IAIDetails person)
            {
                Time = UnityEngine.Time.time;
                EnemyPosition = person.Transform.position;
                EnemyDirection = person.Transform.position - BotOwner.Transform.position;
                EnemyStraightDistance = Vector3.Distance(person.Transform.position, BotOwner.Transform.position);
                BotPosition = BotOwner.Transform.position;
            }

            public float Time = 0f;
            public float TimeSinceSeen = 0f;

            public Vector3 BotPosition = Vector3.zero;
            public float BotShootTime = 0f;
            public Vector3 BotShootPosition = Vector3.zero;

            public Vector3 EnemyPosition = Vector3.zero;
            public Vector3 EnemyDirection = Vector3.zero;
            public float EnemyStraightDistance = 0f;

            public Vector3 GrenadePosition = Vector3.zero;
            public float GrenadeTime = 0f;
            public float GrenadeTimeSinceSeen = 0f;
        }
    }
}
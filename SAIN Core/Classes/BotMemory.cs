using EFT;
using UnityEngine;
using UnityEngine.AI;
using SAIN.Components;
using SAIN.Helpers;

namespace SAIN.Classes
{
    public class Enemy : SAINBot
    {
        public Enemy(BotOwner bot) : base(bot)
        {
            LastSeen = new EnemyLastSeen(bot);
            Path = new EnemyPath(bot);
        }

        public void Update(Vector3 botPosition, IAIDetails person)
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
                UpdateVision(botPosition, person);
            }

            if (Timers.ShootRaycastTimer < Time.time)
            {
                Timers.ShootRaycastTimer = Time.time + Timers.ShootRaycastFreq;
                UpdateShoot(botPosition, person);
            }
        }

        private void UpdateVision(Vector3 botPosition, IAIDetails person)
        {
            if (RaycastHelpers.CheckVisible(botPosition, person, SAINCoreComponent.SightMask))
            {
                CanSee = true;

                LastSeen.Time = Time.time;
                LastSeen.EnemyPosition = person.Transform.position;
                LastSeen.EnemyDirection = person.Transform.position - botPosition;
                LastSeen.EnemyStraightDistance = Vector3.Distance(person.Transform.position, botPosition);
                LastSeen.BotPosition = botPosition;

                bool newEnemy = person != LastPerson;
                if (newEnemy)
                {
                    Path.Update(person.Transform.position);
                    LastPerson = CurrentPerson;
                    CurrentPerson = person;
                }
            }
            else
            {
                CanSee = false;
                LastSeen.TimeSinceSeen = Time.time - LastSeen.Time;
            }
        }

        private void UpdateShoot(Vector3 botPosition, IAIDetails person)
        {
            if (RaycastHelpers.CheckVisible(botPosition, person, SAINCoreComponent.ShootMask))
            {
                CanShoot = true;

                LastSeen.BotShootTime = Time.time;

                LastSeen.BotShootPosition = botPosition;
            }
            else
            {
                CanShoot = false;
            }
        }

        public IAIDetails CurrentPerson = null;
        public IAIDetails LastPerson = null;

        public bool CanShoot = false;
        public bool CanSee = false;

        public EnemyPath Path {  get; private set; }
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
        }
    }

    public class Medical : SAINBot
    {
        public Medical(BotOwner bot) : base(bot)
        {
        }

        public void Update()
        {
            BotOwner.Medecine?.Stimulators?.Refresh();
            BotOwner.Medecine?.FirstAid?.Refresh();
            CanHeal = BotOwner.Medecine.FirstAid.ShallStartUse();
        }

        public bool CanHeal = false;
        public bool HasStims => BotOwner.Medecine.Stimulators.HaveSmt;
        public bool Bleeding => BotOwner.Medecine.FirstAid.IsBleeding;
        public bool HasFirstAid => BotOwner.Medecine.FirstAid.HaveSmth2Use;
    }

    public class BotStatus : SAINBot
    {
        public BotStatus(BotOwner bot) : base(bot)
        {
        }

        public void Update(ETagStatus status)
        {
            HealthStatus = status;
        }

        public bool Healthy => HealthStatus == ETagStatus.Healthy;
        public bool Injured => HealthStatus == ETagStatus.Injured;
        public bool BadlyInjured => HealthStatus == ETagStatus.BadlyInjured;
        public bool Dying => HealthStatus == ETagStatus.Dying;

        private ETagStatus HealthStatus = ETagStatus.Healthy;
    }

    public class Debug
    {
        public static string Reason(bool canSee)
        {
            string reason;
            if (canSee)
            {
                reason = "Can See";
            }
            else
            {
                reason = "Can't See";
            }
            return reason;
        }

        public static string Reason(Enemy enemy)
        {
            string reason;
            if (enemy.Path.RangeFar)
            {
                reason = "Far";
            }
            else if (enemy.Path.RangeMid)
            {
                reason = "MidRange";
            }
            else if (enemy.Path.RangeClose)
            {
                reason = "Close";
            }
            else
            {
                reason = "Very Close";
            }
            return reason;
        }

        public static string Reason(BotStatus status)
        {
            string reason;
            if (status.Injured)
            {
                reason = "Injured";
            }
            else if (status.BadlyInjured)
            {
                reason = "Badly Injured";
            }
            else if (status.Dying)
            {
                reason = "Dying";
            }
            else
            {
                reason = "Healthy";
            }
            return reason;
        }

        public static string Reason(Medical medical)
        {
            string reason = "Some Reason";

            if (medical.CanHeal)
            {
                reason = "Can Heal";
            }
            if (medical.HasStims)
            {
                reason += " and Have Stims";
            }
            if (medical.Bleeding)
            {
                reason += " and Bleeding";
            }
            if (medical.HasFirstAid)
            {
                reason += " and Have First Aid";
            }

            return reason;
        }
    }
}
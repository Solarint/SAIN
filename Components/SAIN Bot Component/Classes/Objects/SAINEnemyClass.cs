using EFT;
using SAIN.Components;
using SAIN.Helpers;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Classes
{
    public class SAINEnemy
    {
        public BotOwner BotOwner { get; private set; }
        public GClass475 GoalEnemy => BotOwner.Memory.GoalEnemy;

        public SAINEnemy(BotOwner bot, IAIDetails person)
        {
            BotOwner = bot;
            Person = person;
        }

        public void Update()
        {
            UpdateVisible();
            UpdatePath();
        }

        private void UpdateVisible()
        {
            if (CheckVisibleTimer < Time.time)
            {
                CheckVisibleTimer = Time.time + 0.15f;

                bool lineOfSight = CheckEnemyVisible();

                bool visible = false;
                if (lineOfSight == true && GoalEnemy?.IsVisible == true)
                {
                    visible = BotOwner.LookSensor.IsPointInVisibleSector(Person.Position);
                }

                IsVisible = visible;
                InLineOfSight = lineOfSight;
            }
        }

        private bool CheckEnemyVisible()
        {
            Vector3 HeadPosition = BotOwner.LookSensor._headPoint;
            float distance = (Person.Position - BotOwner.Position).magnitude;

            LayerMask Mask;

            if (distance < 10f)
            {
                Mask = LayerMaskClass.HighPolyWithTerrainMask;
            }
            else
            {
                Mask = LayerMaskClass.HighPolyWithTerrainMaskAI;
            }

            foreach (var part in Person.MainParts.Values)
            {
                Vector3 partPos = part.Position;
                Vector3 Direction = partPos - HeadPosition;
                if (!Physics.Raycast(HeadPosition, Direction, Direction.magnitude, Mask))
                {
                    var weapon = BotOwner.WeaponRoot.position;
                    Direction = partPos - weapon;
                    CanShoot = !Physics.Raycast(weapon, Direction, Direction.magnitude, Mask);
                    return true;
                }
            }
            return false;
        }

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
            Path = new NavMeshPath();
            NavMesh.CalculatePath(BotOwner.Transform.position, pos, -1, Path);
            PathDistance = Path.CalculatePathLength();
        }

        public SAINPathDistance CheckPathDistance()
        {
            const float VeryCloseDist = 5f;
            const float CloseDist = 20f;
            const float FarDist = 80f;
            const float VeryFarDist = 120f;

            SAINPathDistance pathDistance;
            float distance = PathDistance;

            if (distance <= VeryCloseDist)
            {
                pathDistance = SAINPathDistance.VeryClose;
            }
            else if (distance <= CloseDist)
            {
                pathDistance = SAINPathDistance.Close;
            }
            else if (distance <= FarDist)
            {
                pathDistance = SAINPathDistance.Mid;
            }
            else if (distance <= VeryFarDist)
            {
                pathDistance = SAINPathDistance.Far;
            }
            else
            {
                pathDistance = SAINPathDistance.VeryFar;
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

        private float CheckVisibleTimer = 0f;
    }

    public enum SAINPathDistance
    {
        VeryClose = 0,
        Close = 1,
        Mid = 2,
        Far = 3,
        VeryFar = 4
    }
}
using BepInEx.Logging;
using EFT;
using SAIN.Components;
using static SAIN.UserSettings.VisionConfig;
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
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private readonly ManualLogSource Logger;

        public void Update()
        {
            UpdateDistance();
            UpdateVisible();
            UpdatePath();
        }

        private float RayCastFreq
        {
            get
            {
                const float CloseCap = 10f;
                const float FarCap = 90f;
                const float baseTime = 0.1f;
                const float baseTimeAdd = 0.2f;
                const float maxTime = 0.5f;

                float distance = DistanceFromLastSeen;

                float clampedClose = Mathf.Clamp(distance, 0f, CloseCap);
                float scaled = clampedClose / CloseCap;
                scaled *= baseTimeAdd;
                scaled += baseTime;

                float result = scaled;

                if (distance > CloseCap)
                {
                    float clampedFar = Mathf.Clamp(distance - CloseCap, 0f, FarCap);
                    float farScale = clampedFar / FarCap;
                    farScale *= baseTimeAdd;
                    result += farScale;
                }

                float clampedResult = Mathf.Clamp(result, 0f, maxTime);

                if (DebugTimer < Time.time && DebugVision.Value && Person.GetPlayer.ProfileId == Plugin.MainPlayer.ProfileId)
                {
                    DebugTimer = Time.time + 1f;
                    Logger.LogDebug($"RayCast Frequency result: [{clampedResult}]");
                }
                return clampedResult;
            }
        }

        private float DebugTimer = 0f;

        private void UpdateDistance()
        {
            if (DistanceTimer < Time.time)
            {
                DistanceTimer = Time.time + 0.25f;
                float distance = GetMagnitudeToBot(Position);
                RealDistance = distance;
                LastSeenDistance = IsVisible ? distance : GetMagnitudeToBot(PositionLastSeen);
                DistanceFromLastSeen = IsVisible ? 0f : (PositionLastSeen - Position).magnitude;
            }
        }

        public float DistanceFromLastSeen { get; private set; }
        public Vector3 Position => Person.Position;

        private float GetMagnitudeToBot(Vector3 point)
        {
            return (BotOwner.Position - point).magnitude;
        }

        public float RealDistance { get; private set; }
        public float LastSeenDistance { get; private set; }
        public Vector3 PositionLastSeen { get; private set; }
        public float TimeSinceSeen { get; private set; }
        public float TimeSeen { get; private set; }
        public bool Seen { get; private set; }
        public float TimeFirstSeen { get; private set; }
        public float TimeLastSeen { get; private set; }
        
        private float DistanceTimer = 0f;
        private bool Reacting = false;
        private float ReactionTimer = 0f;

        private void UpdateVisible()
        {
            if (CheckVisibleTimer < Time.time)
            {
                CheckVisibleTimer = Time.time + RayCastFreq;

                bool lineOfSight = false;
                bool visible = false;
                bool wasVisible = IsVisible;

                if (CheckPartsVisible())
                {
                    lineOfSight = true;
                    if (GoalEnemy?.IsVisible == true && BotOwner.LookSensor.IsPointInVisibleSector(Person.Position))
                    {
                        if (!Reacting)
                        {
                            Reacting = true;
                            ReactionTimer = Time.time + 0.2f;
                        }
                        else
                        {
                            if (ReactionTimer < Time.time)
                            {
                                visible = true;
                            }
                        }
                    }
                }
                if (visible == true)
                {
                    TimeSeen = Time.time;
                    if (!Seen)
                    {
                        TimeFirstSeen = Time.time;
                        Seen = true;
                    }
                }
                if (visible == false)
                {
                    if (wasVisible)
                    {
                        TimeLastSeen = Time.time;
                        PositionLastSeen = Person.Position;
                    }
                    if (Seen)
                    {
                        TimeSinceSeen = Time.time - TimeLastSeen;
                    }
                    Reacting = false;
                }

                InLineOfSight = lineOfSight;
                IsVisible = visible;
            }
        }

        private bool CheckPartsVisible()
        {
            Vector3 HeadPosition = BotOwner.LookSensor._headPoint;
            float distance = (Person.Position - BotOwner.Position).magnitude;
            float maxDistance = BotOwner.Settings.Current.CurrentVisibleDistance;

            if (distance >= maxDistance)
            {
                return false;
            }

            var NoFoliageMask = LayerMaskClass.HighPolyWithTerrainMask;
            var FoliageMask = LayerMaskClass.HighPolyWithTerrainMaskAI;
            LayerMask Mask = distance < 5f ? NoFoliageMask : FoliageMask;

            foreach (var part in Person.MainParts.Values)
            {
                Vector3 partPos = part.Position;
                Vector3 Direction = partPos - HeadPosition;
                float visionCap = Mathf.Clamp(Direction.magnitude, 0f, BotOwner.Settings.Current.CurrentVisibleDistance);
                if (!Physics.Raycast(HeadPosition, Direction, out var hit, visionCap, Mask))
                {
                    var weapon = BotOwner.WeaponRoot.position;
                    Direction = partPos - weapon;
                    CanShoot = !Physics.Raycast(weapon, Direction, Direction.magnitude, Mask);

                    if (DebugVision.Value && Mask == NoFoliageMask)
                    {
                        DebugGizmos.SingleObjects.Line(HeadPosition, EnemyChestPosition, Color.red, 0.01f, true, 1f, true);
                        Logger.LogDebug($"[{BotOwner.name}] can see through foliage because distance < 5f");
                    }

                    return true;
                }
                else
                {
                    if (Mask == LayerMaskClass.HighPolyWithTerrainMaskAI)
                    {
                        Vector3 hitPos = hit.point;
                        Vector3 hitToTarget = part.Position - hitPos;
                        if (hitToTarget.magnitude < 1f)
                        {
                            Vector3 directionAwayFromTarget = -hitToTarget.normalized * 0.1f;
                            Vector3 newRayPos = hitPos + directionAwayFromTarget;
                            Direction = part.Position - newRayPos;
                            if (!Physics.Raycast(newRayPos, Direction.normalized, Direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                            {
                                if (DebugVision.Value)
                                {
                                    DebugGizmos.SingleObjects.Line(newRayPos, EnemyChestPosition, Color.red, 0.01f, true, 1f, true);
                                    Logger.LogDebug($"[{BotOwner.name}] can see through foliage because distance < 1f");
                                }

                                return true;
                            }
                        }

                        //Vector3 StartToHit = hitPos - HeadPosition;
                    }
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
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

        private readonly float BotDifficultyModifier;

        public SAINEnemy(BotOwner bot, IAIDetails person, float BotDifficultyMod)
        {
            BotOwner = bot;
            Person = person;
            BotDifficultyModifier = BotDifficultyMod;
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private readonly ManualLogSource Logger;

        public void Update()
        {
            UpdateDistance();
            CheckEnemyVisible();
            UpdatePath();
        }

        private float RayCastFreq
        {
            get
            {
                const float CloseCap = 10f;
                const float FarCap = 90f;
                const float baseTime = 0.1f;
                const float baseTimeAdd = 0.1f;
                const float maxTime = 0.33f;

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
        public bool Seen { get; private set; }
        public float TimeFirstSeen { get; private set; }
        public float TimeLastSeen { get; private set; }

        private ReactionTimeChecker ReactionTime;
        
        private float DistanceTimer = 0f;

        private void CheckEnemyVisible()
        {
            if (CheckVisibleTimer < Time.time)
            {
                CheckVisibleTimer = Time.time + 0.1f;

                bool lineOfSight = RayCastBodyParts();
                bool visible = false;
                if (lineOfSight)
                {
                    if (BotOwner.LookSensor.IsPointInVisibleSector(Person.Position))
                    {
                        if (GoalEnemy?.IsVisible == true)
                        {
                            visible = true;
                        }
                    }
                }

                UpdateVisible(visible, lineOfSight);
            }
        }

        private void UpdateVisible(bool isVisible, bool inLineOfSight)
        {
            InLineOfSight = inLineOfSight;
            bool wasVisible = IsVisible;
            IsVisible = isVisible;

            if (isVisible)
            {
                if (!Seen)
                {
                    TimeFirstSeen = Time.time;
                    Seen = true;
                }
            }
            if (!isVisible)
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
            }
        }

        private bool RayCastBodyParts()
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
                            if (!Physics.Raycast(HeadPosition, Direction, Direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                            {
                                if (DebugVision.Value)
                                {
                                    DebugGizmos.SingleObjects.Line(HeadPosition, EnemyChestPosition, Color.red, 0.01f, true, 1f, true);
                                    Logger.LogDebug($"[{BotOwner.name}] can see through foliage because distance < 1f");
                                }

                                return true;
                            }
                        }
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

        private float CheckVisibleTimer = 0f;
    }

    public class ReactionTimeChecker
    {
        public ReactionTimeChecker(float reactionTime)
        {
            ReactionTime = reactionTime;
            startTime = Time.time;
        }

        private readonly float ReactionTime;
        private readonly float startTime;

        public bool Update()
        {
            return Time.time - startTime >= ReactionTime;
        }
    }
}
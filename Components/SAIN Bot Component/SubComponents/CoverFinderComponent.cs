using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static SAIN.UserSettings.CoverConfig;

namespace SAIN.Components
{
    public class CoverFinderComponent : MonoBehaviour
    {
        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();

            CollidersArray = new ColliderArray(BotOwner, 150);

            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        public void LookForCover(Vector3 targetPosition, Vector3 originPoint)
        {
            TargetPosition = targetPosition;
            OriginPoint = originPoint;

            if (TakeCoverCoroutine == null)
            {
                TakeCoverCoroutine = StartCoroutine(FindCover());
            }
        }

        public void StopLooking()
        {
            if (TakeCoverCoroutine != null)
            {
                StopCoroutine(TakeCoverCoroutine);
                TakeCoverCoroutine = null;
                //CurrentCover = null;
                //CurrentFallBackPoint = null;
            }
        }

        private IEnumerator FindCover()
        {
            while (true)
            {
                float minDist = Vector3.Distance(TargetPosition, OriginPoint) / 2f;
                MinTargetDist = Mathf.Clamp(minDist, CoverMinEnemyDistance.Value, 999f);

                if (!RecheckFoundPoints())
                {
                    var colliders = CollidersArray.GetColliders();

                    if (colliders.Length == 0)
                    {
                        //Logger.LogError("FindCover: hits is 0!");
                    }
                    else
                    {
                        for (int i = 0; i < colliders.Length; i++)
                        {
                            if (CheckColliderForCover(colliders[i].NavMeshHit.position, colliders[i].Collider.transform.position, colliders[i].Collider.bounds.size.y, out var newPoint))
                            {
                                SetCoverPoint(newPoint);
                                break;
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(CoverUpdateFrequency.Value);
            }
        }

        private bool CheckColliderForCover(Vector3 position, Vector3 colliderPos, float colliderHeight, out CoverPoint newPoint)
        {
            newPoint = new CoverPoint(position, colliderPos, colliderHeight);

            if (CheckSides(newPoint))
            {
                if (CheckPositionVsOtherBots(newPoint.Position))
                {
                    return true;
                }
            }

            newPoint = null;
            return false;
        }

        private bool RecheckFoundPoints()
        {
            var newCover = CheckOldPoint(CurrentCover);
            var newFallBack = CheckOldPoint(CurrentFallBackPoint);

            if (newCover != null)
            {
                CurrentCover = newCover;
            }

            if (newFallBack != null)
            {
                CurrentFallBackPoint = newFallBack;

                if (newCover == null)
                {
                    CurrentCover = newFallBack;
                }
            }

            return newCover != null && newFallBack != null;
        }

        private CoverPoint CheckOldPoint(CoverPoint oldPoint)
        {
            CoverPoint cover = null;
            if (oldPoint != null && Vector3.Distance(TargetPosition, oldPoint.Position) > MinTargetDist)
            {
                CheckColliderForCover(oldPoint.Position, oldPoint.ColliderPosition, oldPoint.Height, out cover);
            }
            return cover;
        }

        private void SetCoverPoint(CoverPoint newPoint)
        {
            CurrentCover = newPoint;

            if (newPoint.Height >= 1.55f)
            {
                CurrentFallBackPoint = newPoint;
            }
        }

        public bool CheckPositionVsOtherBots(Vector3 position)
        {
            foreach (var memberPos in SAIN.BotSquad.SquadLocations)
            {
                if (memberPos != null)
                {
                    if (Vector3.Distance(position, memberPos) < 0.25f)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool CheckRayCast(Vector3 point)
        {
            point.y += 0.5f;
            var trgRayPoint = TargetPosition;
            trgRayPoint.y += 0.5f;

            var direction = trgRayPoint - point;

            return Physics.Raycast(point, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask);
        }

        private bool CheckSides(CoverPoint point)
        {
            float HideSens = CoverHideSensitivity.Value;
            if (CheckRayCast(point.Position) && Vector3.Dot(point.DirectionToCollider.normalized, (TargetPosition - point.Position).normalized) > HideSens)
            {
                return true;
            }
            else
            {
                int i = 0;
                while (i < 3)
                {
                    Vector3 randomPos = Random.onUnitSphere * 2f;
                    randomPos.y = 0;

                    var newPos = point.Position + randomPos;

                    if (NavMesh.SamplePosition(newPos, out var hit, CoverNavSampleDistance.Value, -1))
                    {
                        if (CheckRayCast(hit.position))
                        {
                            point.Position = hit.position;
                            point.DirectionToCollider = point.ColliderPosition - hit.position;

                            return true;
                        }
                    }
                    i++;
                }
            }

            return false;
        }

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }

        public CoverPoint CurrentCover { get; private set; }

        public CoverPoint CurrentFallBackPoint { get; private set; }

        private ColliderArray CollidersArray;

        private BotOwner BotOwner => SAIN.BotOwner;

        private SAINComponent SAIN;

        protected ManualLogSource Logger;

        public float MinObstacleHeight;

        private Coroutine TakeCoverCoroutine;

        private Vector3 OriginPoint;

        private Vector3 TargetPosition;

        public const float MinObstacleXZ = 0.33f;

        public float MaxRange => CoverColliderRadius.Value;
        public float MinTargetDist { get; private set; }

        public class ColliderArray : SAINBot
        {
            private CoverFinderComponent Finder => SAIN.Cover.CoverFinder;

            private const float GetNewCollidersFreq = 30f;
            private const float GetNewCollidersDist = 5f;
            private const float CheckCollidersFreq = 0.25f;

            public ColliderWithPath[] colliderAndPathLengths;

            public readonly Collider[] Colliders;

            private readonly ManualLogSource Logger;

            public ColliderArray(BotOwner bot, int count) : base(bot)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
                Colliders = new Collider[count];
            }

            public ColliderWithPath[] GetColliders()
            {
                if (CheckColliderTimer < Time.time)
                {
                    CheckColliderTimer = Time.time + CheckCollidersFreq;

                    int hits = GetCollidersArray();

                    var collidersList = new List<ColliderWithPath>();

                    for (int i = 0; i < hits; i++)
                    {
                        float X = Colliders[i].bounds.size.x;
                        float Y = Colliders[i].bounds.size.y;
                        float Z = Colliders[i].bounds.size.z;

                        if (Y < Finder.MinObstacleHeight || (X < MinObstacleXZ && Z < MinObstacleXZ))
                        {
                            continue;
                        }

                        var colliderPos = Colliders[i].transform.position;

                        if (Vector3.Distance(colliderPos, Finder.TargetPosition) < Finder.MinTargetDist)
                        {
                            continue;
                        }
                        if (!CheckDirectionAndDist(colliderPos))
                        {
                            continue;
                        }

                        if (CheckPathLength(Colliders[i], out ColliderWithPath point))
                        {
                            collidersList.Add(point);
                        }
                    }

                    var collidersArray = collidersList.ToArray();

                    System.Array.Sort(collidersArray, ColliderPathSortComparer);

                    colliderAndPathLengths = collidersArray;

                    //Logger.LogInfo($"Final collider array length = [{collidersArray.Length}]");

                    return collidersArray;
                }

                return colliderAndPathLengths;
            }

            private bool CheckDirectionAndDist(Vector3 colliderPos)
            {
                var direction = colliderPos - Finder.OriginPoint;
                var enemyDirection = Finder.TargetPosition - Finder.OriginPoint;

                if (Vector3.Dot(direction, enemyDirection) > 0.25f && direction.magnitude > enemyDirection.magnitude)
                {
                    return false;
                }

                return true;
            }

            private bool CheckPathLength(Collider collider, out ColliderWithPath colliderPath)
            {
                var direction = (Finder.TargetPosition - collider.transform.position).normalized * 1.5f;
                direction.y = 0f;

                var testPos = collider.transform.position - direction;

                if (NavMesh.SamplePosition(testPos, out var hit, CoverNavSampleDistance.Value, -1))
                {
                    NavMeshPath Path = new NavMeshPath();
                    if (NavMesh.CalculatePath(BotOwner.Position, hit.position, -1, Path) && Path.status == NavMeshPathStatus.PathComplete)
                    {
                        float pathLength = Path.CalculatePathLength();
                        if (pathLength < Finder.MaxRange * 2f)
                        {
                            colliderPath = new ColliderWithPath(collider, hit, Path, pathLength);
                            return true;
                        }
                    }
                }

                colliderPath = null;
                return false;
            }

            private int GetCollidersArray()
            {
                if (GetNewCollidersTimer < Time.time || Vector3.Distance(BotOwner.Position, LastCheckPosition) > GetNewCollidersDist)
                {
                    GetNewCollidersTimer = Time.time + GetNewCollidersFreq;
                    LastCheckPosition = BotOwner.Position;

                    for (int i = 0; i < Colliders.Length; i++)
                    {
                        Colliders[i] = null;
                    }

                    LastColliderHits = Physics.OverlapSphereNonAlloc(Finder.OriginPoint, Finder.MaxRange, Colliders, SAINComponent.CoverMask);

                    System.Array.Sort(Colliders, ColliderArraySortComparer);
                }

                return LastColliderHits;
            }

            public int ColliderArraySortComparer(Collider A, Collider B)
            {
                if (A == null && B != null)
                {
                    return 1;
                }
                else if (A != null && B == null)
                {
                    return -1;
                }
                else if (A == null && B == null)
                {
                    return 0;
                }
                else
                {
                    return Vector3.Distance(SAIN.BotOwner.Transform.position, A.transform.position).CompareTo(Vector3.Distance(SAIN.BotOwner.Transform.position, B.transform.position));
                }
            }

            public int ColliderPathSortComparer(ColliderWithPath A, ColliderWithPath B)
            {
                if (A == null && B != null)
                {
                    return 1;
                }
                else if (A != null && B == null)
                {
                    return -1;
                }
                else if (A == null && B == null)
                {
                    return 0;
                }
                else
                {
                    return A.PathLength.CompareTo(B.PathLength);
                }
            }

            private float CheckColliderTimer = 0f;
            private float GetNewCollidersTimer = 0f;
            private int LastColliderHits = 0;
            private Vector3 LastCheckPosition = Vector3.zero;
        }
    }

    public class ColliderParams
    {
        public ColliderParams(
            float maxRange,
            float minPlayerDist,
            Vector3? originPoint,
            Vector3? enemyPosition,
            float minObstacleY,
            float minObstacleXZ,
            LayerMask coverLayer)
        {
            MaxRange = maxRange;
            MinEnemyDistance = minPlayerDist;
            OriginPoint = originPoint;
            TargetPosition = enemyPosition;
            MinObstacleHeightY = minObstacleY;
            MinObstacleWidthXZ = minObstacleXZ;
            CoverLayer = coverLayer;
        }

        public float MaxRange { get; private set; }
        public float MinEnemyDistance { get; private set; }
        public Vector3? OriginPoint { get; private set; }
        public Vector3? TargetPosition { get; private set; }
        public float MinObstacleHeightY { get; private set; }
        public float MinObstacleWidthXZ { get; private set; }
        public LayerMask CoverLayer { get; private set; }
    }

    public class ColliderWithPath
    {
        public ColliderWithPath(Collider collider, NavMeshHit hitPos, NavMeshPath Path, float pathLength)
        {
            Collider = collider;
            NavMeshHit = hitPos;
            NavMeshPath = Path;
            PathLength = pathLength;
        }

        public Collider Collider { get; private set; }

        public NavMeshPath NavMeshPath;
        public NavMeshHit NavMeshHit;
        public float PathLength;
        public Vector3 TargetPosition;
    }

    public class CoverPoint
    {
        public CoverPoint(Vector3 position, Vector3 colliderPos, float colliderHeight)
        {
            Position = position;
            ColliderPosition = colliderPos;
            DirectionToCollider = colliderPos - position;
            Height = colliderHeight;
        }

        public Vector3 Position { get; set; }
        public Vector3 DirectionToCollider { get; set; }
        public Vector3 ColliderPosition { get; private set; }
        public float Height { get; private set; }
    }
}
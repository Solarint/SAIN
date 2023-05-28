using BepInEx.Logging;
using EFT;
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

            CollidersArray = new ColliderArray(BotOwner, 100);

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
            }
        }

        private IEnumerator FindCover()
        {
            while (true)
            {
                Vector3 originPoint = OriginPoint;
                Vector3 targetPosition = TargetPosition;

                float minDist = Vector3.Distance(targetPosition, originPoint) / 2f;
                float maxDist = CoverColliderRadius.Value;

                if (!RecheckFoundPoints(targetPosition, minDist, maxDist))
                {
                    var colliderParams = new ColliderParams(
                        maxDist,
                        minDist,
                        originPoint,
                        targetPosition,
                        MinObstacleHeight,
                        0.1f,
                        SAINComponent.CoverMask);

                    var colliders = CollidersArray.GetColliders(colliderParams);

                    if (colliders.Length == 0)
                    {
                        Logger.LogError("FindCover: hits is 0!");
                    }
                    else
                    {
                        CoverPoint newPoint = null;

                        for (int i = 0; i < colliders.Length; i++)
                        {
                            if (CheckColliderForCover(colliders[i], out newPoint, maxDist))
                            {
                                break;
                            }
                        }

                        if (newPoint != null)
                        {
                            SetCoverPoint(newPoint);
                        }
                    }
                }

                yield return new WaitForSeconds(CoverUpdateFrequency.Value);
            }
        }

        private bool CheckColliderForCover(ColliderWithPath collider, out CoverPoint newPoint, float maxDistance)
        {
            newPoint = null;
            if (CheckSides(collider, CoverNavSampleDistance.Value, CoverHideSensitivity.Value, maxDistance))
            {
                if (CheckPositionVsOtherBots(collider.NavMeshHit.position))
                {
                    newPoint = new CoverPoint(collider);
                    return true;
                }
            }
            return false;
        }

        private bool RecheckFoundPoints(Vector3? targetPos, float minTargetDistance, float maxCoverDistance)
        {
            var newCover = CheckOldPoint(targetPos, CurrentCover, minTargetDistance, maxCoverDistance);
            var newFallBack = CheckOldPoint(targetPos, CurrentFallBackPoint, minTargetDistance, maxCoverDistance);

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

        private CoverPoint CheckOldPoint(Vector3? targetPos, CoverPoint oldPoint, float minTargetDistance, float maxCoverDistance)
        {
            CoverPoint cover = null;
            if (oldPoint != null && Vector3.Distance(targetPos.Value, oldPoint.Position) > minTargetDistance)
            {
                CheckColliderForCover(oldPoint.ColliderPack, out cover, maxCoverDistance);
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
                    if (Vector3.Distance(position, memberPos) < 1f)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool CheckSides(ColliderWithPath collider, float navSampleSize, float HideSens, float maxDistance)
        {
            if (Vector3.Dot((collider.Collider.transform.position - collider.NavMeshHit.position).normalized, (collider.TargetPosition - collider.NavMeshHit.position).normalized) > HideSens)
            {
                return true;
            }
            else
            {
                if (NavMesh.SamplePosition(collider.Collider.transform.position - (collider.TargetPosition - collider.NavMeshHit.position).normalized * 2, out NavMeshHit hit2, navSampleSize, NavMesh.AllAreas))
                {
                    NavMeshPath Path = new NavMeshPath();
                    if (NavMesh.CalculatePath(SAIN.BotOwner.Transform.position, hit2.position, NavMesh.AllAreas, Path) && Path.status == NavMeshPathStatus.PathComplete)
                    {
                        float pathLength = Path.CalculatePathLength();
                        if (pathLength <= maxDistance)
                        {
                            if (Vector3.Dot((collider.Collider.transform.position - hit2.position).normalized, (collider.TargetPosition - hit2.position).normalized) > HideSens)
                            {
                                collider.NavMeshPath = Path;
                                collider.NavMeshHit = hit2;
                                collider.PathLength = pathLength;
                                return true;
                            }
                        }
                    }
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

        private LayerMask HidableLayers = LayerMaskClass.LowPolyColliderLayerMask;

        public float MinObstacleHeight;

        private Coroutine TakeCoverCoroutine;

        private Vector3 OriginPoint;

        private Vector3 TargetPosition;
    }

    public class ColliderArray : SAINBot
    {
        private const float GetNewCollidersFreq = 5f;
        private const float GetNewCollidersDist = 2f;
        private const float CheckCollidersFreq = 0.5f;

        public ColliderWithPath[] colliderAndPathLengths;

        public readonly Collider[] Colliders;

        private readonly ManualLogSource Logger;

        public ColliderArray(BotOwner bot, int count) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            Colliders = new Collider[count];
        }

        public ColliderWithPath[] GetColliders(ColliderParams inputParams)
        {
            if (CheckColliderTimer < Time.time)
            {
                CheckColliderTimer = Time.time + CheckCollidersFreq;

                int hits = GetCollidersArray(inputParams.OriginPoint.Value, inputParams.MaxRange, inputParams.CoverLayer);

                var collidersList = new List<ColliderWithPath>();
                for (int i = 0; i < hits; i++)
                {
                    bool badCollider = false;

                    if (Colliders[i].bounds.size.y < inputParams.MinObstacleHeightY)
                    {
                        badCollider = true;
                    }
                    else if (Colliders[i].bounds.size.x < inputParams.MinObstacleWidthXZ && Colliders[i].bounds.size.z < inputParams.MinObstacleWidthXZ)
                    {
                        badCollider = true;
                    }

                    if (!badCollider)
                    {
                        Vector3 colliderPos = Colliders[i].transform.position;
                        badCollider = Vector3.Distance(colliderPos, inputParams.TargetPosition.Value) < inputParams.MinEnemyDistance;
                    }

                    if (!badCollider)
                    {
                        if (CheckPathLength(inputParams.OriginPoint.Value, Colliders[i], inputParams.MaxRange, out ColliderWithPath point))
                        {
                            point.TargetPosition = inputParams.TargetPosition.Value;
                            collidersList.Add(point);
                        }
                    }
                }

                var collidersArray = collidersList.ToArray();

                System.Array.Sort(collidersArray, ColliderPathSortComparer);

                colliderAndPathLengths = collidersArray;

                Logger.LogInfo($"Final collider array length = [{collidersArray.Length}]");

                return collidersArray;
            }

            return colliderAndPathLengths;
        }

        public static bool CheckPathLength(Vector3 botPos, Collider collider, float maxRange, out ColliderWithPath colliderPath)
        {
            if (NavMesh.SamplePosition(collider.transform.position, out var hit, 5f, -1))
            {
                NavMeshPath Path = new NavMeshPath();
                if (NavMesh.CalculatePath(botPos, hit.position, -1, Path) && Path.status == NavMeshPathStatus.PathComplete)
                {
                    float pathLength = Path.CalculatePathLength();
                    if (pathLength < maxRange)
                    {
                        colliderPath = new ColliderWithPath(collider, hit, Path, pathLength);
                        return true;
                    }
                }
            }

            colliderPath = null;
            return false;
        }

        private int GetCollidersArray(Vector3 botPos, float maxRange, LayerMask coverLayer)
        {
            int hits = LastColliderHits;

            if (GetNewCollidersTimer < Time.time || Vector3.Distance(BotOwner.Transform.position, LastCheckPosition) > GetNewCollidersDist)
            {
                GetNewCollidersTimer = Time.time + GetNewCollidersFreq;

                for (int i = 0; i < Colliders.Length; i++)
                {
                    Colliders[i] = null;
                }

                hits = Physics.OverlapSphereNonAlloc(botPos, maxRange, Colliders, coverLayer);

                System.Array.Sort(Colliders, ColliderArraySortComparer);
            }

            if (hits == 0)
            {
                Logger.LogError("GetCollidersArray: Hits is 0!");
            }

            LastColliderHits = hits;
            return hits;
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
        public CoverPoint(ColliderWithPath objectWithPath)
        {
            ColliderPack = objectWithPath;

            //CheckDistanceToCover();
        }

        private void CheckDistanceToCover()
        {
            // Raise the y values so we have a clear line of sight and wont hit any tiny objects
            Vector3 rayPos = Position;
            rayPos.y += 0.25f;
            Vector3 rayEnemyPos = EnemyPosition;
            rayEnemyPos.y += 0.25f;

            Vector3 rayDirection = rayEnemyPos - rayPos;

            // Raycast 0.5 meters to see if the object providing cover is too close
            if (Physics.Raycast(rayPos, rayDirection, 0.5f, SAINComponent.CoverMask))
            {
                // Normalize the direction and multiply it by 0.5 to get a distance to shift the position
                Vector3 shiftPosition = (EnemyPosition - Position).normalized * 0.5f;
                // subtract the direction from the position to shift the coverPos away from the object
                Vector3 newPosition = Position - shiftPosition;

                // Make sure the new position is on the navmesh
                if (NavMesh.SamplePosition(newPosition, out var hit, 0.5f, NavMesh.AllAreas))
                {
                    // Assign it as our Position and navmesh hit
                    //Position = hit.position;
                    ColliderPack.NavMeshHit = hit;
                }
            }
        }

        public ColliderWithPath ColliderPack;

        public Vector3 EnemyPosition => ColliderPack.TargetPosition;
        public Vector3 Position => ColliderPack.NavMeshHit.position;
        public Vector3 DirectionToObject => ColliderPack.Collider.transform.position - ColliderPack.NavMeshHit.position;
        public Vector3 DirectionToTarget => ColliderPack.TargetPosition - ColliderPack.NavMeshHit.position;

        public float Height => ColliderPack.Collider.bounds.size.y;
    }

    public static class CoverColliders
    {
        public static List<Collider> CollidersUsed = new List<Collider>();
        public static List<Collider> KnownBadColliders = new List<Collider>();
    }
}
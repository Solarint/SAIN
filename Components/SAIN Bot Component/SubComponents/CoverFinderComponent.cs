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
                        else
                        {
                            Logger.LogWarning("No Point found");
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
                    var direction = collider.TargetPosition - collider.NavMeshHit.position;
                    if (Physics.Raycast(collider.NavMeshHit.position, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        newPoint = new CoverPoint(collider);
                        return true;
                    }
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
                    if (Vector3.Distance(position, memberPos) < 0.25f)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool CheckSides(ColliderWithPath collider, float navSampleSize, float HideSens, float maxDistance)
        {
            var rayPoint = collider.NavMeshHit.position;
            rayPoint.y += 0.5f;

            var trgRayPoint = collider.TargetPosition;
            trgRayPoint.y += 0.5f;

            var direction = trgRayPoint - rayPoint;
            if (!Physics.Raycast(rayPoint, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
            {
                return false;
            }


            if (Vector3.Dot((collider.Collider.transform.position - collider.NavMeshHit.position).normalized, (collider.TargetPosition - collider.NavMeshHit.position).normalized) > HideSens)
            {
                DebugGizmos.SingleObjects.Line(collider.NavMeshHit.position, BotOwner.MyHead.position, Color.blue, 0.1f, true, 1f, true);
                return true;
            }
            else
            {
                Vector3 randomPos = Random.onUnitSphere;
                randomPos.y = 0;
                var newPos = collider.NavMeshHit.position + randomPos;
                if (NavMesh.SamplePosition(newPos, out var hit, 1f, -1))
                {
                    if (Vector3.Dot((collider.Collider.transform.position - hit.position).normalized, (collider.TargetPosition - hit.position).normalized) > HideSens)
                    {
                        collider.NavMeshHit = hit;
                        DebugGizmos.SingleObjects.Line(collider.NavMeshHit.position, BotOwner.MyHead.position, Color.green, 0.1f, true, 1f, true);
                        return true;
                    }
                }
            }

            DebugGizmos.SingleObjects.Line(collider.NavMeshHit.position, BotOwner.MyHead.position, Color.red, 0.1f, true, 1f, true);
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

                var badColliders = new List<Collider>(KnownBadColliders);

                var collidersList = new List<ColliderWithPath>();

                for (int i = 0; i < hits; i++)
                {
                    if (badColliders.Contains(Colliders[i]))
                    {
                        continue;
                    }

                    if (Colliders[i].bounds.size.y < 0.4f || Colliders[i].bounds.size.x < inputParams.MinObstacleWidthXZ && Colliders[i].bounds.size.z < inputParams.MinObstacleWidthXZ)
                    {
                        badColliders.Add(Colliders[i]);
                        continue;
                    }

                    var direction = Colliders[i].transform.position - inputParams.OriginPoint.Value;
                    var enemyDirection = inputParams.TargetPosition.Value - inputParams.OriginPoint.Value;
                    if (Vector3.Dot(direction, enemyDirection) > 0f)
                    {
                        float distanceToTarget = Vector3.Distance(inputParams.OriginPoint.Value, inputParams.TargetPosition.Value);
                        float distanceToCollider = Vector3.Distance(inputParams.OriginPoint.Value, Colliders[i].transform.position);
                        if (distanceToCollider > distanceToTarget)
                        {
                            continue;
                        }

                    }

                    if (Colliders[i].bounds.size.y > inputParams.MinObstacleHeightY)
                    {
                        if (Vector3.Distance(Colliders[i].transform.position, inputParams.TargetPosition.Value) > inputParams.MinEnemyDistance)
                        {
                            if (CheckPathLength(inputParams.TargetPosition.Value, inputParams.OriginPoint.Value, Colliders[i], inputParams.MaxRange, out ColliderWithPath point))
                            {
                                collidersList.Add(point);
                            }
                        }
                    }
                }

                KnownBadColliders = badColliders;

                var collidersArray = collidersList.ToArray();

                System.Array.Sort(collidersArray, ColliderPathSortComparer);

                colliderAndPathLengths = collidersArray;

                Logger.LogInfo($"Final collider array length = [{collidersArray.Length}]");

                return collidersArray;
            }

            return colliderAndPathLengths;
        }

        public List<Collider> KnownBadColliders = new List<Collider>();

        public static bool CheckPathLength(Vector3 targetPos, Vector3 botPos, Collider collider, float maxRange, out ColliderWithPath colliderPath)
        {
            var direction = (targetPos - collider.transform.position).normalized * 1.5f;
            direction.y = 0f;

            var testPos = collider.transform.position - direction;

            if (NavMesh.SamplePosition(testPos, out var hit, 3f, -1))
            {
                NavMeshPath Path = new NavMeshPath();
                if (NavMesh.CalculatePath(botPos, hit.position, -1, Path) && Path.status == NavMeshPathStatus.PathComplete)
                {
                    float pathLength = Path.CalculatePathLength();
                    if (pathLength < maxRange * 2f)
                    {
                        colliderPath = new ColliderWithPath(collider, hit, Path, pathLength);
                        colliderPath.TargetPosition = targetPos;
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
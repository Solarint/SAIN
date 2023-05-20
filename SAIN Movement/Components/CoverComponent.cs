using BepInEx.Logging;
using EFT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.CoverConfig;

namespace SAIN.Components
{
    public class CoverComponent : MonoBehaviour
    {
        public CoverPoint CurrentCover { get; private set; }
        public CoverPoint CurrentFallBackPoint { get; private set; }

        private ColliderArray CollidersArray;
        private BotOwner BotOwner;

        public static readonly float MaxPathLengthMulti = 1.5f;
        private const float GetNewCollidersFreq = 5f;
        private const float GetNewCollidersDist = 2f;
        private const float CheckCollidersFreq = 0.5f;

        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = GetComponent<SAINComponent>();
            CollidersArray = new ColliderArray(BotOwner, 250);
        }

        public void LookForCover(Vector3? targetPosition)
        {
            if (targetPosition == null)
            {
                Logger.LogError("Target Position is null");
                return;
            }

            TargetPosition = targetPosition;

            if (TakeCoverCoroutine == null)
            {
                TakeCoverCoroutine = StartCoroutine(FindCover());
            }
        }

        private Vector3? TargetPosition;

        public void StopLooking()
        {
            if (TakeCoverCoroutine != null)
            {
                StopCoroutine(TakeCoverCoroutine);
                TakeCoverCoroutine = null;
                TargetPosition = null;
            }
        }

        private IEnumerator FindCover()
        {
            while (true)
            {
                Vector3 botPosition = BotOwner.Transform.position;
                Vector3? targetPosition = TargetPosition;
                Vector3? enemyHeadPos = SAIN.Core.Enemy.EnemyHeadPosition;

                float minDist = GetMinimumDistance(botPosition, targetPosition);
                float maxDist = CoverColliderRadius.Value;

                if (!RecheckFoundPoints(targetPosition, botPosition, minDist, maxDist))
                {
                    var colliderParams = new ColliderParams(
                        maxDist,
                        minDist,
                        botPosition,
                        targetPosition,
                        MinObstacleHeight,
                        0.15f,
                        HidableLayers);

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
                            if (CheckColliderForCover(colliders[i].Collider, targetPosition, enemyHeadPos, out newPoint, maxDist))
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

        private float GetMinimumDistance(Vector3 botPosition, Vector3? targetPosition)
        {
            if (targetPosition == null)
            {
                return 0f;
            }

            float result;
            float distance = Vector3.Distance(botPosition, targetPosition.Value);

            float min = CoverMinEnemyDistance.Value;

            const float max = 50f;

            float clamp = Mathf.Clamp(distance, min, max);

            float scaled = (clamp - min) / (max - min);
            scaled += min;

            Logger.LogDebug($"Min Distance Result: [{scaled}] because Distance: [{distance}]");

            result = Mathf.Clamp(scaled, min, max);

            return result;
        }

        private bool CheckColliderForCover(Collider collider, Vector3? targetPosition, Vector3? enemyHeadPos, out CoverPoint newPoint, float maxDistance)
        {
            newPoint = null;
            if (NavSampleChecks.CheckSides(collider, targetPosition, enemyHeadPos, out NavMeshHit navHit, CoverNavSampleDistance.Value, CoverHideSensitivity.Value))
            {
                if (CheckPositionVsOtherBots(navHit.position))
                {
                    NavMeshPath Path = new NavMeshPath();
                    if (NavMesh.CalculatePath(SAIN.BotOwner.Transform.position, navHit.position, NavMesh.AllAreas, Path))
                    {
                        if (Path.status == NavMeshPathStatus.PathComplete)
                        {
                            if (Path.CalculatePathLength() <= maxDistance * MaxPathLengthMulti)
                            {
                                newPoint = new CoverPoint(collider, navHit, Path, targetPosition.Value);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool RecheckFoundPoints(Vector3? targetPos, Vector3? enemyHeadPos, float minTargetDistance, float maxCoverDistance)
        {
            var newCover = CheckOldPoint(targetPos, enemyHeadPos, CurrentCover, minTargetDistance, maxCoverDistance);
            var newFallBack = CheckOldPoint(targetPos, enemyHeadPos, CurrentFallBackPoint, minTargetDistance, maxCoverDistance);

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

        private CoverPoint CheckOldPoint(Vector3? targetPos, Vector3? enemyHeadPos, CoverPoint oldPoint, float minTargetDistance, float maxCoverDistance)
        {
            CoverPoint cover = null;
            if (oldPoint != null && Vector3.Distance(targetPos.Value, oldPoint.Position) > minTargetDistance)
            {
                CheckColliderForCover(oldPoint.Collider, targetPos, enemyHeadPos, out cover, maxCoverDistance);
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
            foreach (var memberPos in SAIN.Core.BotSquad.SquadLocations)
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

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }

        private SAINComponent SAIN;
        protected ManualLogSource Logger;
        private LayerMask HidableLayers = LayerMaskClass.HighPolyWithTerrainMask;
        public float MinObstacleHeight;
        private Coroutine TakeCoverCoroutine;

        private class ColliderArray : SAINBotExt
        {
            public ColliderAndPathLength[] colliderAndPathLengths;

            public readonly Collider[] Colliders;

            private readonly ManualLogSource Logger;

            public ColliderArray(BotOwner bot, int count) : base(bot)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
                Colliders = new Collider[count];
            }

            public ColliderAndPathLength[] GetColliders(ColliderParams inputParams)
            {
                if (CheckColliderTimer < Time.time)
                {
                    CheckColliderTimer = Time.time + CheckCollidersFreq;

                    if (inputParams.BotPosition == null || inputParams.TargetPosition == null)
                    {
                        Logger.LogError("GetColliders: BotPos is null or TargetPos is null!");
                        return null;
                    }

                    int hits = GetCollidersArray(inputParams.BotPosition.Value, inputParams.MaxRange, inputParams.CoverLayer);

                    var collidersList = new List<ColliderAndPathLength>();
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
                            bool tooCloseEnemy = ColliderChecks.DistanceCheck(inputParams.TargetPosition.Value, colliderPos, inputParams.MinEnemyDistance);
                            bool behindEnemy = ColliderChecks.DotProductCheck(inputParams.BotPosition.Value, inputParams.TargetPosition.Value, colliderPos);

                            if (tooCloseEnemy || behindEnemy)
                            {
                                badCollider = true;
                            }
                        }

                        if (!badCollider)
                        {
                            if (ColliderChecks.CheckPathLength(inputParams.BotPosition.Value, Colliders[i], inputParams.MaxRange, out ColliderAndPathLength point))
                            {
                                collidersList.Add(point);
                            }
                        }
                    }

                    var collidersArray = collidersList.ToArray();

                    System.Array.Sort(collidersArray, ColliderPathSortComparer);

                    colliderAndPathLengths = collidersArray;
                }

                return colliderAndPathLengths;
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

            public int ColliderPathSortComparer(ColliderAndPathLength A, ColliderAndPathLength B)
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
                Vector3? botPosition,
                Vector3? enemyPosition,
                float minObstacleY,
                float minObstacleXZ,
                LayerMask coverLayer)
            {
                MaxRange = maxRange;
                MinEnemyDistance = minPlayerDist;
                BotPosition = botPosition;
                TargetPosition = enemyPosition;
                MinObstacleHeightY = minObstacleY;
                MinObstacleWidthXZ = minObstacleXZ;
                CoverLayer = coverLayer;
            }

            public float MaxRange { get; private set; }
            public float MinEnemyDistance { get; private set; }
            public Vector3? BotPosition { get; private set; }
            public Vector3? TargetPosition { get; private set; }
            public float MinObstacleHeightY { get; private set; }
            public float MinObstacleWidthXZ { get; private set; }
            public LayerMask CoverLayer { get; private set; }
        }

        private class NavSampleChecks
        {
            public static bool CheckSides(Collider collider, Vector3? targetPos, Vector3? enemyHeadPos, out NavMeshHit coverHit, float navSampleSize = 2f, float HideSens = 0f)
            {
                coverHit = new NavMeshHit();
                if (NavMesh.SamplePosition(collider.transform.position, out NavMeshHit hit, navSampleSize, NavMesh.AllAreas))
                {
                    if (!NavMesh.FindClosestEdge(hit.position, out hit, NavMesh.AllAreas))
                    {
                    }

                    Vector3 rayCastPos = GetRayCastPos(enemyHeadPos, targetPos.Value);

                    if (Vector3.Dot(hit.normal, (targetPos.Value - hit.position).normalized) < HideSens)
                    {
                        if (CheckRayCast(hit.position, rayCastPos))
                        {
                            coverHit = hit;
                            return true;
                        }
                    }
                    else
                    {
                        if (CheckOppositeSide(hit, collider, targetPos, rayCastPos, out NavMeshHit hit2, navSampleSize, HideSens))
                        {
                            coverHit = hit2;
                            return true;
                        }
                    }
                }

                return false;
            }

            private static Vector3 GetRayCastPos(Vector3? enemyHeadPos, Vector3 targetPos)
            {
                Vector3 rayCastPos;
                if (enemyHeadPos == null)
                {
                    rayCastPos = targetPos;
                }
                else
                {
                    rayCastPos = enemyHeadPos.Value;
                }
                return rayCastPos;
            }

            private static bool CheckOppositeSide(NavMeshHit hit, Collider collider, Vector3? targetPos, Vector3 rayCastPos, out NavMeshHit placeToGo, float navSampleSize = 2f, float HideSens = 0f)
            {
                if (NavMesh.SamplePosition(collider.transform.position - (targetPos.Value - hit.position).normalized * 2, out NavMeshHit hit2, navSampleSize, NavMesh.AllAreas))
                {
                    if (!NavMesh.FindClosestEdge(hit2.position, out hit2, NavMesh.AllAreas))
                    {
                    }

                    if (Vector3.Dot(hit2.normal, (targetPos.Value - hit2.position).normalized) < HideSens)
                    {
                        if (CheckRayCast(hit2.position, rayCastPos))
                        {
                            placeToGo = hit2;
                            return true;
                        }
                    }
                }

                placeToGo = hit;
                return false;
            }

            private static bool CheckRayCast(Vector3 coverPos, Vector3 targetPos)
            {
                Vector3 direction = coverPos - targetPos;
                return Physics.Raycast(targetPos, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask);
            }
        }

        private class ColliderChecks
        {
            public static bool DotProductCheck(Vector3 botPos, Vector3 enemyPos, Vector3 colliderPos)
            {
                Vector3 toCollider = colliderPos - enemyPos;
                Vector3 toBot = botPos - enemyPos;

                return Vector3.Dot(toCollider.normalized, toBot.normalized) < 0;
            }

            public static bool DistanceCheck(Vector3 enemyPos, Vector3 colliderPos, float minDistance)
            {
                return Vector3.Distance(colliderPos, enemyPos) < minDistance;
            }

            public static bool CheckPathLength(Vector3 botPos, Collider collider, float maxRange, out ColliderAndPathLength colliderPath)
            {
                colliderPath = null;

                NavMeshPath Path = new NavMeshPath();
                if (NavMesh.CalculatePath(botPos, collider.transform.position, NavMesh.AllAreas, Path))
                {
                    float pathLength = Path.CalculatePathLength();
                    if (pathLength < maxRange * MaxPathLengthMulti)
                    {
                        colliderPath = new ColliderAndPathLength(collider, pathLength);
                        return true;
                    }
                }

                return false;
            }
        }
    }

    public class ColliderAndPathLength
    {
        public ColliderAndPathLength(Collider collider, float pathLength)
        {
            Collider = collider;
            PathLength = pathLength;
        }
        public Collider Collider { get; private set; }
        public float PathLength { get; private set; }
    }

    public class CoverPoint
    {
        public CoverPoint(Collider collider, NavMeshHit hit, NavMeshPath pathToCover, Vector3 enemyPosition)
        {
            Collider = collider;
            Height = collider.bounds.size.y;

            MeshHit = hit;
            Position = hit.position;

            PathToCover = pathToCover;
            EnemyPosition = enemyPosition;

            PositionToCollider = (collider.transform.position - hit.position).normalized;
            PositionToEnemy = (enemyPosition - hit.position).normalized;

            Calculate();
        }

        private void Calculate()
        {
            CheckDistanceToCover();

            PathDistance = PathToCover.CalculatePathLength();

            // Corner 0 is always at the bot position
            StraightDist = Vector3.Distance(PathToCover.corners[0], Position);
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
            if (Physics.Raycast(rayPos, rayDirection, 0.5f, SAINCoreComponent.CoverMask))
            {
                // Normalize the direction and multiply it by 0.5 to get a distance to shift the position
                Vector3 shiftPosition = (EnemyPosition - Position).normalized * 0.5f;
                // subtract the direction from the position to shift the coverPos away from the object
                Vector3 newPosition = Position - shiftPosition;

                // Make sure the new position is on the navmesh
                if (NavMesh.SamplePosition(newPosition, out var hit, 0.5f, NavMesh.AllAreas))
                {
                    // Assign it as our Position and navmesh hit
                    Position = hit.position;
                    MeshHit = hit;
                }
            }
        }

        public Collider Collider { get; private set; }
        public NavMeshHit MeshHit { get; private set; }
        public NavMeshPath PathToCover { get; private set; }
        public Vector3 EnemyPosition { get; private set; }

        public Vector3 Position { get; private set; }
        public Vector3 PositionToCollider { get; private set; }
        public Vector3 PositionToEnemy { get; private set; }

        public float Height { get; private set; }
        public float PathDistance { get; private set; }
        public float StraightDist { get; private set; }
    }

    public static class CoverColliders
    {
        public static List<Collider> CollidersUsed = new List<Collider>();
        public static List<Collider> KnownBadColliders = new List<Collider>();
    }
}
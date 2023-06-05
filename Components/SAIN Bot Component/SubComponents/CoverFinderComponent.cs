using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.CoverConfig;

namespace SAIN.Components
{
    public class CoverFinderComponent : MonoBehaviour
    {
        private Collider[] Colliders;

        private void Awake()
        {
            Colliders = new Collider[ColliderArrayCount.Value];

            SAIN = GetComponent<SAINComponent>();
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
                tooclose = 0;
                baddist = 0;
                visible = 0;
                nopath = 0;
                totalCount = 0;

                MinTargetDist = CoverMinEnemyDistance.Value;

                if (!RecheckPoint())
                {
                    bool found = false;
                    var colliders = CheckToGetColliders(out int hits);

                    CoverPoint cover = null;
                    CoverPoint FallBackPoint = null;

                    int max = 150;
                    int pointsChecked = 0;

                    for (int i = 0; i < hits; i++)
                    {
                        if (colliders[i] != null)
                        {
                            if (CheckColliderForCover(colliders[i], out var newPoint))
                            {
                                if (cover == null)
                                {
                                    cover = newPoint;
                                }

                                if (newPoint.Collider.bounds.size.y > 1.6f)
                                {
                                    FallBackPoint = newPoint;
                                }

                                if (cover != null && FallBackPoint != null)
                                {
                                    found = true;
                                    CurrentCover = cover;
                                    CurrentFallBackPoint = FallBackPoint;

                                    if (DebugLogTimer < Time.time && DebugCoverFinder.Value)
                                    {
                                        DebugLogTimer = Time.time + 1f;
                                        Logger.LogInfo($"[{BotOwner.name}] - Found Cover!: Stats: Too Close To Friendly: [{tooclose}] Too Close to Enemy: [{baddist}] Visible: [{visible}] No Path: [{nopath}] Valid Colliders checked: [{totalCount}]");
                                    }

                                    break;
                                }
                            }

                            pointsChecked++;

                            if (pointsChecked >= max || nopath >= 50)
                            {
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        if (DebugLogTimer < Time.time && DebugCoverFinder.Value)
                        {
                            DebugLogTimer = Time.time + 1f;
                            Logger.LogWarning($"[{BotOwner.name}] - No Cover Found! Oh no!: Stats: Too Close To Friendly: [{tooclose}] Too Close to Enemy: [{baddist}] Visible: [{visible}] No Path: [{nopath}] Valid Colliders checked: [{totalCount}]");
                        }
                    }
                }

                if (DebugCoverFinder.Value)
                {
                    if (CurrentCover != null)
                    {
                        DebugGizmos.SingleObjects.Line(CurrentCover.Position, SAIN.HeadPosition, Color.magenta, 0.1f, true, 0.25f);
                    }
                    if (CurrentFallBackPoint != null)
                    {
                        DebugGizmos.SingleObjects.Line(CurrentFallBackPoint.Position, SAIN.HeadPosition, Color.yellow, 0.1f, true, 0.25f);
                    }
                }

                yield return new WaitForSeconds(CoverUpdateFrequency.Value);
            }
        }

        private bool CheckColliderForCover(Collider collider, out CoverPoint newPoint)
        {
            if (collider == null || collider.bounds.size.y < MinObstacleHeight)
            {
                newPoint = null;
                return false;
            }

            totalCount++;

            Vector3 colliderPos = collider.transform.position;

            // The direction from the target to the collider
            Vector3 colliderDir = (colliderPos - TargetPosition).normalized;

            // a farPoint on opposite side of the target
            Vector3 farPoint = colliderPos + colliderDir;

            // the closest edge to that farPoint
            if (NavMesh.FindClosestEdge(farPoint, out var hit, -1))
            {
                // Shift the farPoint away from the edge so its not too close
                Vector3 shift = (hit.position - TargetPosition).normalized / 2f;
                Vector3 coverPosition = hit.position + shift;

                if (CheckPosition(coverPosition))
                {
                    if (CheckPathToPosition(coverPosition))
                    {
                        if (DebugCoverFinder.Value)
                        {
                            DebugGizmos.SingleObjects.Sphere(coverPosition, 0.2f, Color.blue, true, 10f);
                        }

                        newPoint = new CoverPoint(BotOwner, coverPosition, collider);

                        return true;
                    }
                    else
                    {
                        nopath++;
                    }
                }

                if (DebugCoverFinder.Value)
                {
                    DebugGizmos.SingleObjects.Sphere(coverPosition, 0.1f, Color.red, true, 10f);
                }
            }

            newPoint = null;
            return false;
        }

        private bool CheckPosition(Vector3 position)
        {
            if (CheckPositionVsOtherBots(position))
            {
                if (Vector3.Distance(position, TargetPosition) > MinTargetDist)
                {
                    if (VisibilityCheck(position))
                    {
                        return true;
                    }
                    else
                    {
                        visible++;
                    }
                }
                else
                {
                    baddist++;
                }
            }
            else
            {
                tooclose++;
            }

            return false;
        }

        private bool CheckPathToPosition(Vector3 position)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(BotOwner.Position, position, -1, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    var corner = path.corners[i];
                    float cornerDifference = corner.y - TargetPosition.y;

                    if (cornerDifference < 1f && cornerDifference > -1f)
                    {
                        Vector3 cornerToTarget = TargetPosition - corner;
                        Vector3 botToTarget = TargetPosition - OriginPoint;

                        float cornerDistance = cornerToTarget.magnitude;
                        float targetDistance = botToTarget.magnitude;

                        if (cornerDistance < 0.25f)
                        {
                            if (DebugCoverFinder.Value)
                            {
                                DebugGizmos.SingleObjects.Ray(OriginPoint, corner - OriginPoint, Color.red, (corner - OriginPoint).magnitude, 0.05f, true, 30f);
                            }

                            return false;
                        }

                        if (cornerDistance < targetDistance - 1f)
                        {
                            if (i < path.corners.Length - 2)
                            {
                                Vector3 cornerB = path.corners[i + 1];
                                Vector3 pathBetweenCorners = cornerB - corner;

                                if (Vector3.Dot(cornerToTarget.normalized, pathBetweenCorners.normalized) > 0.33f)
                                {
                                    if (DebugCoverFinder.Value)
                                    {
                                        DebugGizmos.SingleObjects.Ray(corner, pathBetweenCorners, Color.red, pathBetweenCorners.magnitude, 0.05f, true, 30f);
                                    }

                                    return false;
                                }
                            }
                        }
                    }
                }
                return true;
            }

            return false;
        }

        private int totalCount = 0;
        private int baddist = 0;
        private int tooclose = 0;
        private int visible = 0;
        private int nopath = 0;
        private float DebugLogTimer = 0f;

        private bool RecheckPoint()
        {
            float distance;
            if (CurrentCover != null)
            {
                distance = Vector3.Distance(CurrentCover.Position, OriginPoint);

                if (distance > 30f || !CheckPosition(CurrentCover.Position))
                {
                    CurrentCover = null;
                }
            }

            if (CurrentFallBackPoint != null)
            {
                distance = Vector3.Distance(CurrentFallBackPoint.Position, OriginPoint);

                if (distance > 30f || !CheckPosition(CurrentFallBackPoint.Position))
                {
                    CurrentFallBackPoint = null;
                }
            }

            if (CurrentCover == null && CurrentFallBackPoint != null)
            {
                CurrentCover = CurrentFallBackPoint;
            }

            return CurrentCover != null && CurrentFallBackPoint != null;
        }

        public bool CheckPositionVsOtherBots(Vector3 position)
        {
            if (SAIN.BotSquad.SquadLocations == null || SAIN.BotSquad.SquadMembers == null || SAIN.BotSquad.SquadMembers.Count < 2)
            {
                return true;
            }

            const float DistanceToBotThresh = 1f;
            const float DistanceToBotCoverThresh = 1f;

            foreach (var memberPos in SAIN.BotSquad.SquadLocations)
            {
                if (memberPos != null && (memberPos - BotOwner.Position).magnitude > 0.1f && Vector3.Distance(position, memberPos) < DistanceToBotThresh)
                {
                    return false;
                }
            }

            foreach (var member in SAIN.BotSquad.SquadMembers.Values)
            {
                if (member != null && member.Cover.CurrentCoverPoint != null && member.BotOwner != BotOwner)
                {
                    if (Vector3.Distance(position, member.Cover.CurrentFallBackPoint.Position) < DistanceToBotCoverThresh)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool VisibilityCheck(Vector3 position)
        {
            const float offset = 0.15f;

            Vector3 target = TargetPosition;

            if (CheckRayCast(position, target))
            {
                Vector3 enemyDirection = target - position;
                enemyDirection = enemyDirection.normalized * offset;

                Quaternion right = Quaternion.Euler(0f, 90f, 0f);
                Vector3 rightPoint = right * enemyDirection;
                rightPoint += position;

                if (CheckRayCast(rightPoint, target))
                {
                    Quaternion left = Quaternion.Euler(0f, -90f, 0f);
                    Vector3 leftPoint = left * enemyDirection;
                    leftPoint += position;

                    if (CheckRayCast(leftPoint, target))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckRayCast(Vector3 point, Vector3 target, float distance = 3f)
        {
            point.y += 0.66f;
            target.y += 0.66f;
            Vector3 direction = target - point;
            return Physics.Raycast(point, direction, distance, LayerMaskClass.HighPolyWithTerrainMask);
        }

        private Collider[] CheckToGetColliders(out int hits)
        {
            const float CheckDistThresh = 10f;
            const float ColliderSortDistThresh = 5f;

            if (NewColliderTimer < Time.time)
            {
                NewColliderTimer = Time.time + 1f;

                float distance = Vector3.Distance(LastCheckPos, OriginPoint);

                if (distance > CheckDistThresh)
                {
                    Logger.LogWarning($"Getting new colliders because distance is over Threshold. Distance: [{distance}]. Threshold: [{CheckDistThresh}]");

                    LastCheckPos = OriginPoint;

                    GetColliders();

                    if (DebugCoverFinder.Value)
                    {
                        foreach (var collider in Colliders)
                        {
                            if (collider != null)
                            {
                                DebugGizmos.SingleObjects.Line(collider.transform.position, OriginPoint, Color.green, 0.025f, true, 10f, true);
                            }
                        }
                    }
                }

                if (distance > ColliderSortDistThresh)
                {
                    System.Array.Sort(Colliders, ColliderArraySortComparer);
                }
            }

            hits = LastHitCount;
            return Colliders;
        }

        private float NewColliderTimer = 0f;
        private Vector3 LastCheckPos = Vector3.zero;

        private void GetColliders()
        {
            var colliders = new Collider[ColliderArrayCount.Value];

            var mask = LayerMaskClass.HighPolyWithTerrainMask;

            int hits = Physics.OverlapSphereNonAlloc(OriginPoint, MaxRange, colliders, mask);
            Logger.LogDebug($"{mask}");

            for (int i = 0; i < hits; i++)
            {
                float yDiff = colliders[i].transform.position.y - OriginPoint.y;
                if (yDiff > 5f || yDiff < -5f || colliders[i].bounds.size.y < 0.5)
                {
                    colliders[i] = null;
                }
            }

            LastHitCount = hits;
            Colliders = colliders;
        }

        private int LastHitCount = 0;

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
                return Vector3.Distance(OriginPoint, A.transform.position).CompareTo(Vector3.Distance(OriginPoint, B.transform.position));
            }
        }

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }

        public CoverPoint CurrentCover { get; private set; }

        public CoverPoint CurrentFallBackPoint { get; private set; }

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
    }
}
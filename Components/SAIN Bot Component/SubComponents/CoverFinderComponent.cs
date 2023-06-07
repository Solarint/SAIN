using BepInEx.Logging;
using EFT;
using EFT.Interactive;
using SAIN.Helpers;
using System.Collections;
using System.Collections.Generic;
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
            interactiveLayer = LayerMask.NameToLayer("Interactive");
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
                        var collider = colliders[i];

                        if (nopath >= 35 && i < hits - 4)
                        {
                            collider = colliders[i + 4];
                        }
                        else if (nopath >= 30 && i < hits - 3)
                        {
                            collider = colliders[i + 3];
                        }
                        else if (nopath >= 20 && i < hits - 2)
                        {
                            collider = colliders[i + 2];
                        }
                        else if (nopath >= 10 && i < hits - 1)
                        {
                            collider = colliders[i + 1];
                        }

                        if (nopath >= 40)
                        {
                            break;
                        }

                        if (collider != null)
                        {
                            if (CheckColliderForCover(collider, out var newPoint))
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
                                        Logger.LogInfo($"[{BotOwner.name}] - Found Cover!: Stats: Too Close To Friendly: [{tooclose}] Too Close to Enemy: [{baddist}] Visible: [{visible}] No Path: [{nopath}] Valid Colliders checked: [{totalCount}] Collider Array Size = [{hits}]");
                                    }

                                    break;
                                }
                            }

                            pointsChecked++;

                            if (pointsChecked >= max)
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
                            Logger.LogWarning($"[{BotOwner.name}] - No Cover Found! Oh no!: Stats: Too Close To Friendly: [{tooclose}] Too Close to Enemy: [{baddist}] Visible: [{visible}] No Path: [{nopath}] Valid Colliders checked: [{totalCount}] Collider Array Size = [{hits}]");
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
            newPoint = null;

            if (collider == null || collider.bounds.size.y < MinObstacleHeight)
            {
                return false;
            }

            totalCount++;

            if (!CheckColliderPos(collider))
            {
                return false;
            }

            Vector3 colliderPos = collider.transform.position;

            // The botToCorner from the target to the collider
            Vector3 colliderDir = (colliderPos - TargetPosition).normalized;
            colliderDir.y = 0f;

            // a farPoint on opposite side of the target
            Vector3 farPoint = colliderPos + colliderDir;

            // the closest edge to that farPoint
            if (NavMesh.SamplePosition(farPoint, out var hit, 2f, -1))
            {
                // Shift the farPoint away from the edge so its not too close
                Vector3 shift = (hit.position - TargetPosition).normalized / 2f;
                Vector3 coverPosition = hit.position + shift;

                if (CheckPathToPosition(coverPosition, collider))
                {
                    if (CheckPosition(coverPosition))
                    {
                        if (DebugCoverFinder.Value)
                        {
                            DebugGizmos.SingleObjects.Sphere(coverPosition, 0.2f, Color.blue, true, 10f);
                        }

                        newPoint = new CoverPoint(BotOwner, coverPosition, collider);

                        return true;
                    }
                }
                else
                {
                    nopath++;
                }

                if (DebugCoverFinder.Value)
                {
                    DebugGizmos.SingleObjects.Sphere(coverPosition, 0.1f, Color.red, true, 10f);
                }
            }

            return false;
        }

        private bool CheckColliderPos(Collider collider)
        {
            Vector3 pos = collider.transform.position;
            Vector3 target = TargetPosition;
            Vector3 bot = BotOwner.Position;

            Vector3 directionToTarget = target - bot;
            float targetDist = directionToTarget.magnitude;

            Vector3 directionToCollider = pos - bot;
            float colliderDist = directionToCollider.magnitude;

            float dot = Vector3.Dot(directionToTarget.normalized, directionToCollider.normalized);

            if (dot <= 0f)
            {
                return true;
            }
            if (dot <= 0.33f)
            {
                return colliderDist < targetDist * 2f;
            }
            if (dot <= 0.5f)
            {
                return colliderDist < targetDist * 1.5f;
            }
            if (dot <= 0.75f)
            {
                return colliderDist < targetDist;
            }
            return true;
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

        private bool CheckPathToPosition(Vector3 position, Collider collider)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(BotOwner.Position, position, -1, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                if (CheckForPathToEnemy(path))
                {
                    if (CheckForInvalidDoors(path, collider))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckForPathToEnemy(NavMeshPath path)
        {
            for (int i = 1; i < path.corners.Length - 1; i++)
            {
                var corner = path.corners[i];
                Vector3 cornerToTarget = TargetPosition - corner;
                Vector3 botToTarget = TargetPosition - OriginPoint;
                Vector3 botToCorner = corner - OriginPoint;

                if (cornerToTarget.magnitude < 0.5f)
                {
                    if (DebugCoverFinder.Value)
                    {
                        DebugGizmos.SingleObjects.Ray(OriginPoint, corner - OriginPoint, Color.red, (corner - OriginPoint).magnitude, 0.05f, true, 30f);
                    }

                    return false;
                }

                if (i == 1)
                {
                    if (Vector3.Dot(botToCorner.normalized, botToTarget.normalized) > 0.75f)
                    {
                        if (DebugCoverFinder.Value)
                        {
                            DebugGizmos.SingleObjects.Ray(corner, cornerToTarget, Color.red, cornerToTarget.magnitude, 0.05f, true, 30f);
                        }
                        return false;
                    }
                }
                else if (i < path.corners.Length - 2)
                {
                    Vector3 cornerB = path.corners[i + 1];
                    Vector3 directionToNextCorner = cornerB - corner;

                    if (Vector3.Dot(cornerToTarget.normalized, directionToNextCorner.normalized) > 0.75f)
                    {
                        if (directionToNextCorner.magnitude > cornerToTarget.magnitude)
                        {
                            if (DebugCoverFinder.Value)
                            {
                                DebugGizmos.SingleObjects.Ray(corner, cornerToTarget, Color.red, cornerToTarget.magnitude, 0.05f, true, 30f);
                            }
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool CheckForInvalidDoors(NavMeshPath path, Collider collider)
        {
            for (int i = 0; i < path.corners.Length - 2; i++)
            {
                var direction = path.corners[i + 1] - path.corners[i];
                var start = path.corners[i];
                start.y += 0.25f;
                if (Physics.Raycast(start, direction, out var hit, direction.magnitude + 0.25f, LayerMaskClass.InteractiveMask))
                {
                    if (hit.transform.name.ToLower().Contains("door"))
                    {
                        Door door = hit.transform.GetComponent<Door>();
                        if (door != null)
                        {
                            if (DebugCoverFinder.Value)
                            {
                                Logger.LogWarning($"CheckStuck(): Found Door Component");
                            }
                            if (!door.Operatable || door.DoorState == EDoorState.Locked)
                            {
                                if (DebugCoverFinder.Value)
                                {
                                    Logger.LogWarning($"CheckStuck(): Found not operable or locked. Blocked.");
                                }
                                return false;
                            }
                        }
                        else
                        {
                            if (DebugCoverFinder.Value)
                            {
                                Logger.LogWarning($"CheckStuck(): No Door Component. Fake Door? Returned false");
                            }
                            return false;
                        }
                    }
                    else
                    {
                        if (DebugCoverFinder.Value)
                        {
                            Logger.LogWarning($"[{hit.transform.name}]");
                        }
                    }
                }
            }
            return true;
        }

        public static List<Collider> InvalidDoorColliders = new List<Collider>();
        public static List<Vector3> InvalidDoorSpots = new List<Vector3>();
        public static int interactiveLayer;

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

                if (distance > 30f)
                {
                    CurrentCover = null;
                }
                else
                {
                    if (CheckColliderForCover(CurrentCover.Collider, out var point1))
                    {
                        CurrentCover = point1;
                    }
                    else
                    {
                        CurrentCover = null;
                    }
                }
            }

            if (CurrentFallBackPoint != null)
            {
                distance = Vector3.Distance(CurrentFallBackPoint.Position, OriginPoint);

                if (distance > 30f)
                {
                    CurrentCover = null;
                }
                else
                {
                    if (CheckColliderForCover(CurrentFallBackPoint.Collider, out var point2))
                    {
                        CurrentFallBackPoint = point2;
                    }
                    else
                    {
                        CurrentFallBackPoint = null;
                    }
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

            const float DistanceToBotCoverThresh = 1f;

            foreach (var member in SAIN.BotSquad.SquadMembers.Values)
            {
                if (member != null && member.BotOwner != BotOwner)
                {
                    if (member.Cover.CurrentCoverPoint != null)
                    {
                        if (Vector3.Distance(position, member.Cover.CurrentCoverPoint.Position) < DistanceToBotCoverThresh)
                        {
                            return false;
                        }
                    }
                    if (member.Cover.CurrentFallBackPoint != null)
                    {
                        if (Vector3.Distance(position, member.Cover.CurrentFallBackPoint.Position) < DistanceToBotCoverThresh)
                        {
                            return false;
                        }
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

                if (distance > CheckDistThresh || !FirstCheck)
                {
                    FirstCheck = true;

                    //Logger.LogWarning($"Getting new colliders because distance is over Threshold. Distance: [{distance}]. Threshold: [{CheckDistThresh}]");

                    LastCheckPos = OriginPoint;

                    GetColliders();
                }
                else if (distance > ColliderSortDistThresh)
                {
                    System.Array.Sort(Colliders, ColliderArraySortComparer);
                }
            }

            hits = LastHitCount;
            return Colliders;
        }

        private bool FirstCheck = false;
        private float NewColliderTimer = 0f;
        private Vector3 LastCheckPos;

        private void GetColliders()
        {
            var colliders = new Collider[ColliderArrayCount.Value];

            var mask = LayerMaskClass.HighPolyWithTerrainMask;
            int hits = Physics.OverlapSphereNonAlloc(OriginPoint, MaxRange, colliders, mask);
            int hitReduction = 0;

            for (int i = 0; i < hits; i++)
            {
                if (InvalidDoorColliders.Contains(colliders[i]))
                {
                    colliders[i] = null;
                    hitReduction++;
                }
                else
                {
                    float yDiff = colliders[i].transform.position.y - OriginPoint.y;
                    if (yDiff > 5f || yDiff < -5f || colliders[i].bounds.size.y < 0.5)
                    {
                        colliders[i] = null;
                        hitReduction++;
                    }
                }
            }

            System.Array.Sort(colliders, ColliderArraySortComparer);

            hits -= hitReduction;
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
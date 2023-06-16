using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using SAIN.Classes;
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
        public List<CoverPoint> CoverPoints { get; private set; } = new List<CoverPoint>();

        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        private void Update()
        {
            if (DebugCoverFinder.Value)
            {
                if (CoverPoints.Count > 0)
                {
                    DebugGizmos.SingleObjects.Line(CoverPoints.PickRandom().Position, SAIN.HeadPosition, Color.yellow, 0.025f, true, 0.1f);
                }
            }
        }

        public void LookForCover(Vector3 targetPosition, Vector3 originPoint)
        {
            TargetPosition = targetPosition;
            OriginPoint = originPoint;

            if (SAIN.Decision.CurrentSelfDecision == SAINSelfDecision.RunAwayGrenade)
            {
                MinObstacleHeight = 1.5f;
                MinDistance = 10f;
            }
            else
            {
                MinObstacleHeight = CoverMinHeight.Value;
                MinDistance = CoverMinEnemyDistance.Value;
            }

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

                if (!RecheckPoints())
                {
                    GetColliders(out int hits);
                    int frameWait = 0;
                    int coverCount = 0;
                    int totalChecked = 0;

                    for (int i = 0; i < hits; i++)
                    {
                        if (CheckCollider(Colliders[i], out var newPoint))
                        {
                            CoverPoints.Add(newPoint);
                            coverCount++;
                        }
                        // Every 10 colliders checked, wait until the next frame before continuing.
                        if (frameWait == 10)
                        {
                            frameWait = 0;
                            yield return null;
                        }

                        frameWait++;
                        totalChecked++;

                        if (coverCount >= 5)
                        {
                            break;
                        }
                    }

                    if (coverCount > 0)
                    {
                        var pointsArray = CoverPoints.ToArray();
                        System.Array.Sort(pointsArray, CoverPointPathComparerer);
                        CoverPoints.Clear();
                        CoverPoints.AddRange(pointsArray);

                        if (DebugLogTimer < Time.time && DebugCoverFinder.Value)
                        {
                            DebugLogTimer = Time.time + 1f;
                            Logger.LogInfo($"[{BotOwner.name}] - Found [{CoverPoints.Count}] CoverPoints: Stats: Too Close To Friendly: [{tooclose}] Too Close to Enemy: [{baddist}] Visible: [{visible}] No Path: [{nopath}] Valid Colliders checked: [{totalCount}] Collider Array Size = [{hits}]");
                        }
                    }
                    else
                    {
                        if (DebugLogTimer < Time.time && DebugCoverFinder.Value)
                        {
                            DebugLogTimer = Time.time + 1f;
                            Logger.LogWarning($"[{BotOwner.name}] - No Cover Found! Oh no!: Stats: Too Close To Friendly: [{tooclose}] Too Close to Enemy: [{baddist}] Visible: [{visible}] No Path: [{nopath}] Valid Colliders checked: [{totalCount}] Collider Array Size = [{hits}]");
                        }
                    }
                }

                yield return new WaitForSeconds(CoverUpdateFrequency.Value + SAIN.AILimit.TimeAdd);
            }
        }

        private bool CheckCollider(Collider collider, out CoverPoint newPoint)
        {
            const float ExtendLengthThresh = 1.5f;

            newPoint = null;
            if (collider == null || collider.bounds.size.y < MinObstacleHeight || !ColliderDirection(collider))
            {
                return false;
            }

            Vector3 colliderPos = collider.transform.position;

            // The botToCorner from the target to the collider
            Vector3 colliderDir = (colliderPos - TargetPosition).normalized;
            colliderDir.y = 0f;

            if (collider.bounds.size.z > ExtendLengthThresh && collider.bounds.size.x > ExtendLengthThresh)
            {
                colliderDir *= ExtendLengthThresh;
            }

            // a farPoint on opposite side of the target
            Vector3 farPoint = colliderPos + colliderDir;

            // the closest edge to that farPoint
            if (NavMesh.SamplePosition(farPoint, out var hit, 1f, -1))
            {
                if (CheckPath(hit.position))
                {
                    if (CheckPosition(hit.position))
                    {
                        newPoint = new CoverPoint(BotOwner, hit.position, collider);
                    }
                }
                else
                {
                    nopath++;
                }
            }

            return newPoint != null;
        }

        private bool ColliderDirection(Collider collider)
        {
            Vector3 pos = collider.transform.position;
            Vector3 target = TargetPosition;
            Vector3 bot = BotOwner.Position;

            Vector3 directionToTarget = target - bot;
            float targetDist = directionToTarget.magnitude;

            Vector3 directionToCollider = pos - bot;
            float colliderDist = directionToCollider.magnitude;

            float dot = Vector3.Dot(directionToTarget.normalized, directionToCollider.normalized);

            if (dot <= 0.33f)
            {
                return true;
            }
            if (dot <= 0.6f)
            {
                return colliderDist < targetDist * 2f;
            }
            if (dot <= 0.8f)
            {
                return colliderDist < targetDist * 1.5f;
            }
            return colliderDist < targetDist;
        }


        private float MinDistance;

        private bool CheckPosition(Vector3 position)
        {
            if (CheckPositionVsOtherBots(position))
            {
                if ((position - TargetPosition).sqrMagnitude > MinDistance)
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

        private bool CheckPath(Vector3 position)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(BotOwner.Position, position, -1, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                if (PathToEnemy(path))
                {
                    return true;
                }
            }

            return false;
        }

        private bool PathToEnemy(NavMeshPath path)
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

        private int totalCount = 0;
        private int baddist = 0;
        private int tooclose = 0;
        private int visible = 0;
        private int nopath = 0;

        private float DebugLogTimer = 0f;

        private bool RecheckPoints()
        {
            List<CoverPoint> checkedPoints = new List<CoverPoint>();
            for (int i = 0; i < CoverPoints.Count; i++)
            {
                var point = CoverPoints[i];
                Vector3 coverPos = point.Collider.transform.position;
                BotOwner Leader = SAIN.Squad.Leader;
                if (Leader != null && !Leader.IsDead)
                {
                    float leadDist = (point.Position - Leader.Position).sqrMagnitude;
                    if (leadDist > 1600f)
                    {
                        continue;
                    }
                }

                float sqrMag = (point.Position - OriginPoint).sqrMagnitude;
                if (sqrMag <= 2500 && CheckCollider(point.Collider, out var point2))
                {
                    checkedPoints.Add(point2);
                }
            }

            CoverPoints.Clear();
            if (checkedPoints.Count > 0)
            {
                if (checkedPoints.Count > 1)
                {
                    var pointsArray = checkedPoints.ToArray();
                    System.Array.Sort(pointsArray, CoverPointPathComparerer);
                    CoverPoints.AddRange(pointsArray);
                    return true;
                }
                CoverPoints.AddRange(checkedPoints);
                return true;
            }
            return false;
        }

        public bool CheckPositionVsOtherBots(Vector3 position)
        {
            if (SAIN.Squad.SquadLocations == null || SAIN.Squad.SquadMembers == null || SAIN.Squad.SquadMembers.Count < 2)
            {
                return true;
            }

            BotOwner Leader = SAIN.Squad.Leader;
            if (Leader != null && !Leader.IsDead)
            {
                float leadDist = (position - Leader.Position).sqrMagnitude;
                if (leadDist > 1600f)
                {
                    return false;
                }
            }

            const float DistanceToBotCoverThresh = 1f;

            foreach (var member in SAIN.Squad.SquadMembers.Values)
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

        private void GetColliders(out int hits)
        {
            const float CheckDistThresh = 100f;
            const float ColliderSortDistThresh = 25f;

            float distance = (LastCheckPos - OriginPoint).sqrMagnitude;
            if (Colliders == null || distance > CheckDistThresh)
            {
                if (Colliders == null)
                {
                    Colliders = new Collider[ColliderArrayCount.Value];
                }
                LastCheckPos = OriginPoint;
                GetNewColliders(out hits);
                LastHitCount = hits;
                return;
            }
            else if (distance > ColliderSortDistThresh)
            {
                System.Array.Sort(Colliders, ColliderArraySortComparer);
            }

            hits = LastHitCount;
        }

        private Vector3 LastCheckPos;

        private void ClearColliders()
        {
            for (int i = 0; i < Colliders.Length; i++)
            {
                Colliders[i] = null;
            }
        }

        private void GetNewColliders(out int hits)
        {
            ClearColliders();

            var mask = LayerMaskClass.HighPolyWithTerrainMask;
            var orientation = Quaternion.identity;
            var origin = OriginPoint;

            const float widthAdd = 3f;
            const float widthBase = 10f;

            const float heightAdd = 1f;
            const float heightBase = 3f;

            float width = widthBase;
            float height = heightBase;
            hits = 0;

            for (int j = 0; j < 5; j++)
            {
                Vector3 box = new Vector3(width, height, width);
                hits = Physics.OverlapBoxNonAlloc(origin, box, Colliders, orientation, mask);
                hits = FilterColliders(hits);

                if (hits > 50)
                {
                    break;
                }
                else
                {
                    ClearColliders();
                    width += widthAdd;
                    height += heightAdd;
                }
            }

            System.Array.Sort(Colliders, ColliderArraySortComparer);
        }

        private int FilterColliders(int hits)
        {
            int hitReduction = 0;
            for (int i = 0; i < hits; i++)
            {
                if (Colliders[i].bounds.size.y < 0.66)
                {
                    Colliders[i] = null;
                    hitReduction++;
                }
                else if (Colliders[i].bounds.size.x < 0.1f && Colliders[i].bounds.size.z < 0.1f)
                {
                    Colliders[i] = null;
                    hitReduction++;
                }
            }
            hits -= hitReduction;
            return hits;
        }

        private int LastHitCount = 0;

        public int CoverPointPathComparerer(CoverPoint A, CoverPoint B)
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
                return A.PathLengthAtCreation.CompareTo(B.PathLengthAtCreation);
            }
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
                float AMag = (OriginPoint - A.transform.position).sqrMagnitude;
                float BMag = (OriginPoint - B.transform.position).sqrMagnitude;
                return AMag.CompareTo(BMag);
            }
        }

        public int ColliderArrayHeightSortComparer(Collider A, Collider B)
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
                float absDiffA = Mathf.Abs(A.transform.position.y - OriginPoint.y);
                float absDiffB = Mathf.Abs(B.transform.position.y - OriginPoint.y);
                return absDiffA.CompareTo(absDiffB);
            }
        }

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }

        private BotOwner BotOwner => SAIN.BotOwner;
        private SAINComponent SAIN;
        protected ManualLogSource Logger;
        private float MinObstacleHeight;
        private Coroutine TakeCoverCoroutine;
        private Vector3 OriginPoint;
        public Vector3 TargetPosition { get; private set; }
    }
}
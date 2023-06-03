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

        private Collider[] Colliders = new Collider[100];

        private void Awake()
        {
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

                float minDist = Vector3.Distance(TargetPosition, OriginPoint) / 2f;
                MinTargetDist = Mathf.Clamp(minDist, CoverMinEnemyDistance.Value, 999f);
                bool found = false;

                if (!RecheckFoundPoints())
                {
                    var colliders = CheckToGetColliders(out int hits);

                    for (int i = 0; i < hits; i++)
                    {
                        if (colliders[i] != null)
                        {
                            if (CheckColliderForCover(colliders[i], out var newPoint, iterationRange))
                            {
                                SetCoverPoint(newPoint);
                                found = true;
                                break;
                            }
                        }
                    }

                    if (DebugLogTimer < Time.time)
                    {
                        DebugLogTimer = Time.time + 3f;
                        //Logger.LogInfo($"Too Close: [{tooclose}] Too Close to Enemy: [{baddist}] Visible: [{visible}] No Path: [{nopath}] Total: [{LastHitCount}] Iteration Range: [{iterationRange}] Collider Array Length: [{colliders.Length}]");
                    }

                    if (!found)
                    {
                        iterationRange += 5f;
                    }
                    else
                    {
                        iterationRange -= 5f;
                    }

                    iterationRange = Mathf.Clamp(iterationRange, 5f, CoverColliderRadius.Value);
                }

                yield return new WaitForSeconds(CoverUpdateFrequency.Value);
            }
        }

        private float iterationRange = 5f;

        private bool CheckColliderForCover(Collider collider, out CoverPoint newPoint, float maxRange)
        {
            Vector3 colliderPos = collider.transform.position;

            // The direction from the target to the collider
            Vector3 colliderDir = (colliderPos - TargetPosition).normalized;

            // a point on opposite side of the target
            Vector3 point = colliderPos + colliderDir;

            // the closest edge to that point
            NavMesh.FindClosestEdge(point, out var hit, -1);

            // Shift the point away from the edge so its not too close
            Vector3 shift = (hit.position - TargetPosition).normalized / 2f;
            Vector3 Point = hit.position + shift;

            if (Vector3.Distance(BotOwner.Position, Point) < maxRange)
            {
                if (CheckPositionVsOtherBots(Point))
                {
                    if (Vector3.Distance(Point, TargetPosition) > MinTargetDist)
                    {
                        newPoint = new CoverPoint(Point, collider);

                        if (VisibilityCheck(newPoint))
                        {
                            NavMeshPath path = new NavMeshPath();
                            if (NavMesh.CalculatePath(BotOwner.Position, newPoint.Position, -1, path) && path.status == NavMeshPathStatus.PathComplete)
                            {
                                //DebugGizmos.SingleObjects.Ray(newPoint.Position, Vector3.up, Color.blue, 1f, 0.025f, true, 30f);
                                return true;
                            }
                            else
                            {
                                nopath++;
                            }
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

                //DebugGizmos.SingleObjects.Sphere(Point, 0.15f, Color.red, true, 30f);
            }


            newPoint = null;
            return false;
        }

        int baddist = 0;
        int tooclose = 0;
        int visible = 0;
        int nopath = 0;
        private float DebugLogTimer = 0f;

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
            if (oldPoint != null)
            {
                CheckColliderForCover(oldPoint.Collider, out cover, iterationRange);
            }
            return cover;
        }

        private void SetCoverPoint(CoverPoint newPoint)
        {
            CurrentCover = newPoint;

            if (newPoint.Collider.bounds.size.y >= 1.50f)
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

        private bool VisibilityCheck(CoverPoint point)
        {
            if (CheckRayCast(point.Position))
            {
                return true;
            }

            int i = 0;
            while (i < 3)
            {
                Vector3 newPos = point.Position + GetRandomPos();

                if (NavMesh.SamplePosition(newPos, out var hit, 0.25f, -1))
                {
                    if (CheckRayCast(hit.position))
                    {
                        point.Position = hit.position;
                        return true;
                    }
                }
                i++;
            }

            return false;
        }

        private Vector3 GetRandomPos()
        {
            Vector3 randomPos = Random.onUnitSphere;
            randomPos.y = 0;

            if (randomPos.x > 0f)
            {
                randomPos.x = Mathf.Clamp(randomPos.x, 0.5f, 1f);
            }
            else
            {
                randomPos.x = Mathf.Clamp(randomPos.x, -0.5f, -1f);
            }

            if (randomPos.z > 0f)
            {
                randomPos.z = Mathf.Clamp(randomPos.z, 0.5f, 1f);
            }
            else
            {
                randomPos.z = Mathf.Clamp(randomPos.z, -0.5f, -1f);
            }

            return randomPos;
        }

        private Collider[] CheckToGetColliders(out int hits)
        {
            if (Vector3.Distance(LastCheckPos, BotOwner.Position) > CheckDistThresh)
            {
                return GetColliders(out hits);
            }
            else
            {
                hits = LastHitCount;
                return Colliders;
            }
        }

        private Vector3 LastCheckPos = Vector3.zero;
        private const float CheckDistThresh = 10f;

        private Collider[] GetColliders(out int hits)
        {
            for (int i = 0; i < Colliders.Length; i++)
            {
                Colliders[i] = null;
            }

            hits = Physics.OverlapSphereNonAlloc(OriginPoint, MaxRange, Colliders, SAINComponent.CoverMask);
            int hitReduction = 0;

            for (int i = 0; i < hits; i++)
            {
                float X = Colliders[i].bounds.size.x;
                float Y = Colliders[i].bounds.size.y;
                float Z = Colliders[i].bounds.size.z;

                if (Y < MinObstacleHeight || (X < MinObstacleXZ && Z < MinObstacleXZ))
                {
                    Colliders[i] = null;
                    hitReduction++;
                }
            }

            System.Array.Sort(Colliders, ColliderArraySortComparer);

            hits -= hitReduction;

            LastHitCount = hits;

            return Colliders;
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
                return Vector3.Distance(BotOwner.Position, A.transform.position).CompareTo(Vector3.Distance(BotOwner.Position, B.transform.position));
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

        public const float MinObstacleXZ = 0.25f;

        public float MaxRange => CoverColliderRadius.Value;
        public float MinTargetDist { get; private set; }
    }

    public class CoverPoint
    {
        public CoverPoint(Vector3 point, Collider collider)
        {
            Position = point;
            Collider = collider;
        }

        public Collider Collider { get; private set; }
        public Vector3 Position { get; set; }
    }
}
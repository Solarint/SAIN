using BepInEx.Logging;
using EFT;
using SAIN.Classes;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Components
{
    public class CoverAnalyzer : SAINBotAbst
    {
        public CoverAnalyzer(SAINComponent botOwner, CoverFinderComponent coverFinder) : base(botOwner)
        {
            Path = new NavMeshPath();
            CoverFinder = coverFinder;
        }

        private readonly CoverFinderComponent CoverFinder; 

        public bool CheckCollider(Collider collider, out CoverPoint newPoint, float minHeight, Vector3 origin, Vector3 target, float minEnemyDist)
        {
            OriginPoint = origin;
            TargetPosition = target;
            MinObstacleHeight = minHeight;
            MinEnemyDist = minEnemyDist;

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
                        newPoint = new CoverPoint(SAIN, hit.position, collider);
                    }
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

        private float MinEnemyDist;

        private bool CheckPosition(Vector3 position)
        {
            if (CoverFinder.SpottedPoints.Count > 0)
            {
                foreach (var point in CoverFinder.SpottedPoints)
                {
                    if (!point.IsValidAgain && point.TooClose(position))
                    {
                        return false;
                    }
                }
            }
            if (CheckPositionVsOtherBots(position))
            {
                if ((position - TargetPosition).magnitude > MinEnemyDist)
                {
                    if (VisibilityCheck(position, TargetPosition))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckPath(Vector3 position)
        {
            Path.ClearCorners();
            if (NavMesh.CalculatePath(OriginPoint, position, -1, Path) && Path.status == NavMeshPathStatus.PathComplete)
            {
                if (PathToEnemy(Path))
                {
                    return true;
                }
            }

            return false;
        }

        private readonly NavMeshPath Path;

        static bool DebugCoverFinder => SAINPlugin.LoadedPreset.GlobalSettings.Cover.DebugCoverFinder;
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
                    if (DebugCoverFinder)
                    {
                        //DebugGizmos.SingleObjects.Ray(OriginPoint, corner - OriginPoint, Color.red, (corner - OriginPoint).magnitude, 0.05f, true, 30f);
                    }

                    return false;
                }

                if (i == 1)
                {
                    if (Vector3.Dot(botToCorner.normalized, botToTarget.normalized) > 0.75f)
                    {
                        if (DebugCoverFinder)
                        {
                            //DebugGizmos.SingleObjects.Ray(corner, cornerToTarget, Color.red, cornerToTarget.magnitude, 0.05f, true, 30f);
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
                            if (DebugCoverFinder)
                            {
                                //DebugGizmos.SingleObjects.Ray(corner, cornerToTarget, Color.red, cornerToTarget.magnitude, 0.05f, true, 30f);
                            }
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public bool CheckPositionVsOtherBots(Vector3 position)
        {
            if (SAIN.Squad.SquadLocations == null || SAIN.Squad.SquadMembers == null || SAIN.Squad.SquadMembers.Count < 2)
            {
                return true;
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
                    if (member.Cover.FallBackPoint != null)
                    {
                        if (Vector3.Distance(position, member.Cover.FallBackPoint.Position) < DistanceToBotCoverThresh)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static bool VisibilityCheck(Vector3 position, Vector3 target)
        {
            const float offset = 0.15f;

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

        private static bool CheckRayCast(Vector3 point, Vector3 target, float distance = 3f)
        {
            point.y += 0.66f;
            target.y += 0.66f;
            Vector3 direction = target - point;
            return Physics.Raycast(point, direction, distance, LayerMaskClass.HighPolyWithTerrainMask);
        }

        private float MinObstacleHeight;
        private Vector3 OriginPoint;
        private Vector3 TargetPosition;
    }
}
using Comfort.Common;
using EFT;
using SAIN.Components;
using SAIN.Components.CoverFinder;
using SAIN.Models.Structs;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.SubComponents.CoverFinder;

public class CoverAnalyzer(BotComponent bot, CoverFinderComponent coverFinder) : BotBase(bot)
{
    private readonly CoverFinderComponent CoverFinder = coverFinder;

    public bool CheckCreateNewCoverPoint(Collider collider, Vector3 targetPosition, Vector3 botPosition, Vector3 targetDirectionNormal, out CoverPoint coverPoint, out string reason)
    {
        if (!CheckColliderForCover(targetPosition, targetDirectionNormal, out reason, collider, out NavMeshHit navMeshHit))
        {
            coverPoint = null;
            return false;
        }
        PathData pathData = new();
        if (!CheckPath(navMeshHit.position, pathData, botPosition, targetPosition, true))
        {
            reason = "badPath";
            coverPoint = null;
            return false;
        }
        reason = string.Empty;
        coverPoint = new CoverPoint(Bot, collider, pathData, navMeshHit.position);
        return true;
    }

    public bool RecheckCoverPoint(CoverPoint coverPoint, Vector3 targetPosition, Vector3 targetDirectionNormal, Vector3 botPosition, out string reason)
    {
        if (!CheckColliderForCover(targetPosition, targetDirectionNormal, out reason, coverPoint.Collider, out NavMeshHit navMeshHit))
        {
            return false;
        }
        if (!CheckPath(navMeshHit.position, coverPoint.PathData, botPosition, targetPosition, true))
        {
            reason = "badPath";
            return false;
        }
        coverPoint.Position = navMeshHit.position;
        reason = string.Empty;
        return true;
    }

    private static bool FindNavMesh(
        Vector3 colliderSize, 
        out string reason, 
        Vector3 colliderPosition, 
        Vector3 targetPosition, 
        out NavMeshHit navMeshHit)
    {

        Vector3 target2ColliderNormal = (colliderPosition - targetPosition).normalized;
        Vector3 colliderWidth = new(colliderSize.x, 0, colliderSize.z);
        Vector3 oppositeDirectionToTarget = (target2ColliderNormal * (colliderWidth.magnitude + 0.5f));
        if (!NavMesh.SamplePosition(colliderPosition + oppositeDirectionToTarget, out navMeshHit, 0.25f, -1))
        {
            // Check for navmesh at point between collider's top and bottom
            float colliderHeight = colliderSize.y;
            Vector3 midPointColliderPosition = Vector3.Lerp(colliderPosition, colliderPosition + (Vector3.up * colliderHeight), 0.5f);
            if (!NavMesh.SamplePosition(midPointColliderPosition + oppositeDirectionToTarget, out navMeshHit, 0.25f, -1))
            {
                // Raycast from the middle point between the collider's top and bottom to find ground
                if (!Physics.Raycast(midPointColliderPosition + oppositeDirectionToTarget, Vector3.down, out RaycastHit raycastHit, colliderHeight * 0.55f, LayerMaskClass.HighPolyWithTerrainNoGrassMask))
                {
                    reason = "canFindGround";
                    return false;
                }
                // We found ground, now check if there's a NavMesh at that point
                if (!NavMesh.SamplePosition(raycastHit.point, out navMeshHit, 0.25f, -1))
                {
                    reason = "noNavMeshAtPosition";
                    return false;
                }
            }
        }
        reason = string.Empty;
        return true;
    }


    private bool CheckColliderForCover(Vector3 targetPosition, Vector3 targetDirectionNormal, out string reason, Collider collider, out NavMeshHit navMeshHit)
    {
        if (!FindNavMesh(collider.bounds.size, out reason, collider.transform.position, targetPosition, out navMeshHit))
        {
            return false;
        }
        if (!checkDistToTarget(navMeshHit.position, targetPosition))
        {
            reason = "tooCloseToTarget";
            return false;
        }
        if (isPositionSpotted(navMeshHit.position))
        {
            reason = "tooCloseToSpottedPoint";
            return false;
        }
        if (!CheckCoverDirectionvsTargetDirection(targetDirectionNormal, targetPosition, navMeshHit.position))
        {
            reason = "coverBehindTarget";
            return false;
        }
        if (!CheckLineOfSightToTarget(navMeshHit.position, targetPosition, collider, 5))
        {
            reason = "pointVisibleToTarget";
            return false;
        }
        if (!checkPositionVsOtherBots(navMeshHit.position))
        {
            reason = "tooCloseToAnotherBot";
            return false;
        }
        return true;
    }

    private static bool CheckCoverDirectionvsTargetDirection(Vector3 targetDirectionNormal, Vector3 targetPosition, Vector3 coverPosition)
    {
        Vector3 dirTargetToBot = -targetDirectionNormal;
        Vector3 dirTargetToCover = (coverPosition - targetPosition).normalized;
        if (Vector3.Dot(dirTargetToBot, dirTargetToCover) < 0)
        {
            return false; // Cover is behind target
        }
        return true;
    }

    private static bool checkDistToTarget(Vector3 coverPosition, Vector3 targetPosition)
    {
        return (coverPosition - targetPosition).sqrMagnitude > CoverFinderComponent.CoverMinEnemyDistSqr;
    }

    private bool isPositionSpotted(Vector3 position)
    {
        foreach (var point in CoverFinder.SpottedCoverPoints)
            if (!point.IsValidAgain && point.TooClose(position))
                return true;
        return false;
    }

    private static bool CheckPath(Vector3 position, PathData pathData, Vector3 botPosition, Vector3 targetPosition, bool checkEnemy = true)
    {
        pathData.CalcPath(botPosition, position);
        if (pathData.Path.status != NavMeshPathStatus.PathComplete) return false;
        if (pathData.PathLength > SAINPlugin.LoadedPreset.GlobalSettings.General.Cover.MaxCoverPathLength) return false;
        if (checkEnemy && !checkPathToEnemy(pathData.Path, targetPosition)) return false;
        return true;
    }

    private static bool checkPathToEnemy(NavMeshPath path, Vector3 targetPosition)
    {
        const float PATH_NODE_MIN_DIST_SQR = 0.5f;
        for (int i = 0; i < path.corners.Length - 1; i++)
            if (IsPositionNearLineSegment(targetPosition, path.corners[i], path.corners[i + 1], PATH_NODE_MIN_DIST_SQR))
                return false;
        return true;
    }

    public static bool IsPositionNearLineSegment(Vector3 position, Vector3 start, Vector3 end, float maxDistanceSqr)
    {
        // Direction vector from start to end
        Vector3 lineDir = end - start;
        float lineLength = lineDir.sqrMagnitude;
        if (lineLength < 0.01f) return (position - start).magnitude <= maxDistanceSqr;

        // Project position onto the line (clamped to segment)
        float t = Vector3.Dot(position - start, lineDir.normalized) / lineLength;
        t = Mathf.Clamp01(t); // Clamp between 0 and 1 for segment

        Vector3 closestPoint = start + t * lineDir;
        float dist = (position - closestPoint).sqrMagnitude;

        return dist <= maxDistanceSqr;
    }

    private bool checkPositionVsOtherBots(Vector3 position)
    {
        string profileID = Bot.ProfileId;

        if (checkIfPlayerCollidersNear(position, profileID, 0.5f))
        {
            return false;
        }

        var members = Bot.Squad.Members;
        if (members != null)
            foreach (var member in Bot.Squad.Members.Values)
                if (member != null && member.ProfileId != profileID)
                    foreach (var coverPoint in member.Cover.CoverPoints)
                        if (isDistanceTooClose(coverPoint, position))
                            return false;

        return true;
    }

    private static bool checkIfPlayerCollidersNear(Vector3 point, string botProfileId, float radius)
    {
        for (int i = 0; i < _playerColliderArray.Length; i++)
        {
            _playerColliderArray[i] = null;
        }

        Physics.OverlapSphereNonAlloc(point, radius, _playerColliderArray, LayerMaskClass.PlayerMask);

        int count = 0;
        Collider foundCollider = null;
        foreach (var collider in _playerColliderArray)
        {
            if (collider == null) continue;
            count++;
            if (count > 1)
            {
                return true;
            }
            foundCollider = collider;
        }
        if (count == 0)
        {
            return false;
        }

        var player = Singleton<GameWorld>.Instance?.GetPlayerByCollider(foundCollider);
        if (player == null)
        {
            return false;
        }
        if (player.ProfileId == botProfileId)
        {
            return false;
        }
        return true;
    }

    private static Collider[] _playerColliderArray = new Collider[5];

    private static bool isDistanceTooClose(CoverPoint point, Vector3 position)
    {
        const float DistanceToBotCoverThresh = 0.5f;
        const float distSqr = DistanceToBotCoverThresh * DistanceToBotCoverThresh;
        return point != null && (position - point.Position).sqrMagnitude < distSqr;
    }

    /// <returns> false if the collider does not block LoS to the target Position</returns>
    private static bool CheckLineOfSightToTarget(Vector3 position, Vector3 targetPosition, Collider collider, int maxIterations = 5)
    {
        const float RANDOM_SPHERE_RADIUS = 0.25f;
        const float RAY_MAX_DISTANCE = 3f;

        float colliderHeight = collider.bounds.size.y;
        float heightOffset = colliderHeight - RANDOM_SPHERE_RADIUS;

        Vector3 offset = Vector3.up * heightOffset;
        Vector3 raisedTargetPosition = targetPosition + Vector3.up;

        for (int i = 0; i < maxIterations; i++)
        {
            Vector3 rayOrigin = position + offset + (Random.onUnitSphere * RANDOM_SPHERE_RADIUS);
            Ray ray = new() {
                origin = rayOrigin,
                direction = raisedTargetPosition - rayOrigin
            };
            if (!collider.Raycast(ray, out _, RAY_MAX_DISTANCE))
                return false;
        }
        return true;
    }
}
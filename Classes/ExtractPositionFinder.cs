using EFT.Interactive;
using HarmonyLib;
using SAIN.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SAIN.Components.Extract;

public class ExtractPositionFinder
{
    public bool ValidPathFound { get; private set; } = false;
    public Vector3? ExtractPosition { get; private set; } = null;

    private static FieldInfo colliderField = AccessTools.Field(typeof(ExfiltrationPoint), "_collider");

    // Parameters to determine the range of how dense the 3D mesh in the extract collider will be. Denser meshes have a higher
    // chance of finding valid NavMesh points, but performance will be worse.
    private static float initialColliderTestPointDensityFactor = 1f;

    private static float minColliderTestPointDensityFactor = 0.1f;
    private static int maxColliderTestPoints = 25;

    // Parameters to determine how large the radii will be for each point in the 3D mesh. The radii will never be allowed to
    // exceed the bounds of the mesh, but we can limit the initial radii to improve performance.
    private static float defaultExtractNavMeshSearchRadius = 3f;

    private static float maxExtractNavMeshSearchRadius = 5f;

    // After the 3D mesh is generated, the search radius will be increased by this amount to search for the NavMesh.
    // If this is smaller than ~0.75m, no NavMesh points can be found for the Labs Vent extract. However, the larger it gets,
    // the more likely it is that a NavMesh point will be selected that's outside of the extract collider.
    private static float finalExtractNavMeshSearchRadiusAddition = 0.75f;

    // When searching for endpoints from which pathing will be testing to the extract, endpoints with different elevation from
    // the extract point need to be deprioritized. Otherwise, it will be more likely to select points that are on different floors in
    // a building, which are much less likely to result in complete paths being calculated. To address this, an additional factor
    // of this value multiplied by the difference in elevation (in meters) is added to the distance between the points.
    private static float pathEndpointHeightDeprioritizationFactor = 2;

    // The minimum distance between each endpoint used to check if a complete path exists to the extract
    private static float minDistanceBetweenPathEndpoints = 75;

    // The number of endpoints to use for checking if a complete path exists to the extract
    private static int maxPathEndpoints = 2;

    private ExfiltrationPoint ex = null;
    private readonly List<Vector3> pathEndpoints = new();
    private readonly List<Vector3> sortedNavMeshPoints = new();
    private readonly Stack<Vector3> navMeshTestPoints = new();

    public IReadOnlyCollection<Vector3> PathEndpoints => pathEndpoints.AsReadOnly();

    public ExtractPositionFinder(ExfiltrationPoint _ex)
    {
        ex = _ex;
    }

    public IEnumerator SearchForExfilPosition()
    {
        if (ValidPathFound)
        {
            yield break;
        }

        if (ex == null)
        {
#if DEBUG
            Logger.LogError($"Cannot find a position for a null exfil point");
#endif

            yield break;
        }

        // Select points on the NavMesh within the extract collider that will be used to check if a complete path exists
        FindExtractPositionsOnNavMesh();
        if (!navMeshTestPoints.Any())
        {
#if DEBUG
            if (ExtractFinderComponent.DebugMode)
                Logger.LogWarning($"Cannot find any NavMesh positions for {ex.Settings.Name}");
#endif

            yield break;
        }

        // Select the next point from the mesh generated above
        ExtractPosition = navMeshTestPoints.Pop();
#if DEBUG
        if (ExtractFinderComponent.DebugMode)
            Logger.LogInfo($"Testing point {ExtractPosition} for {ex.Settings.Name}. {navMeshTestPoints.Count} test points remaining.");
#endif

        // Choose endpoints from which pathing to the extract will be tested
        FindPathEndPoints(ExtractPosition.Value);
        if (pathEndpoints.Count == 0)
        {
#if DEBUG
            if (ExtractFinderComponent.DebugMode)
                Logger.LogWarning($"Could not find any path endpoints near {ex.Settings.Name}");
#endif

            yield break;
        }

        // Check if a complete path can be calculated between the extract position and each endpoint selected above
        foreach (Vector3 pathEndPoint in pathEndpoints)
        {
            if (NavMeshHelpers.DoesCompletePathExist(pathEndPoint, ExtractPosition.Value))
            {
                ValidPathFound = true;
                
#if DEBUG
                if (ExtractFinderComponent.DebugMode)
                    Logger.LogInfo($"Found complete path to {ex.Settings.Name}");
#endif

                yield break;
            }
            
#if DEBUG
            if (ExtractFinderComponent.DebugMode)
            {
                float distanceBetweenPoints = Vector3.Distance(ExtractPosition.Value, pathEndPoint);
                Logger.LogWarning($"Could not find a complete path to {ex.Settings.Name} from {pathEndPoint} ({distanceBetweenPoints}m away).");
            }
#endif

            // Wait one frame to reduce the performance impact
            yield return null;
        }
        
#if DEBUG
        if (ExtractFinderComponent.DebugMode)
            Logger.LogWarning($"Could not find a complete path to {ex.Settings.Name}");
#endif
    }

    private float GetColliderTestPointSearchRadius(BoxCollider collider)
    {
        // The search radius should be no larger than the extents of the collider (up to the maximum allowed radius)
        float searchRadius = Math.Min(Math.Min(collider.size.x, collider.size.y), collider.size.z) / 2;
        searchRadius = Math.Min(searchRadius, maxExtractNavMeshSearchRadius);

        // Failsafe for a junk extract collider
        if (searchRadius == 0)
        {
            searchRadius = defaultExtractNavMeshSearchRadius;
            
#if DEBUG
            if (ExtractFinderComponent.DebugMode)
                Logger.LogWarning($"Collider size of {ex.Settings.Name} is (0, 0, 0). Using {searchRadius}m to check accessibility.");
#endif
        }

        return searchRadius;
    }

    private void FindExtractPositionsOnNavMesh()
    {
        // If there are still remaining points to test, use them first
        if (navMeshTestPoints.Any())
        {
            return;
        }

        // If NavMesh points have already been seelected in the extract collider, use them
        if (sortedNavMeshPoints.Any())
        {
            CreateNavMeshTestPointStack();
            return;
        }

        BoxCollider collider = (BoxCollider)colliderField.GetValue(ex);
        if (collider == null)
        {
#if DEBUG
            if (ExtractFinderComponent.DebugMode)
                Logger.LogWarning($"Could not find collider for {ex.Settings.Name}");
#endif

            return;
        }

        // Generate the 3D mesh of test points and filter them to only include points that intersect with the NavMesh
        float searchRadius = GetColliderTestPointSearchRadius(collider);
        IEnumerable<Vector3> colliderTestPoints = GetColliderTestPoints(collider, searchRadius);
        IList<Vector3> navMeshPoints = GetColliderTestPointsOnNavMesh(colliderTestPoints, searchRadius + finalExtractNavMeshSearchRadiusAddition);

        // Sort the filtered points to first test the one closest to the center of the collider and then its extremities
        Vector3 referencePoint = ex.transform.position;
        bool chooseFirst = true;
        while (navMeshPoints.Count > 0)
        {
            IEnumerable<Vector3> tmpSortedNavMeshPoints = navMeshPoints.OrderBy(x => Vector3.Distance(x, referencePoint));

            referencePoint = chooseFirst ? tmpSortedNavMeshPoints.First() : tmpSortedNavMeshPoints.Last();
            chooseFirst = false;

            sortedNavMeshPoints.Add(referencePoint);
            navMeshPoints.Remove(referencePoint);
        }

        sortedNavMeshPoints.Reverse();
        CreateNavMeshTestPointStack();
    }

    private void CreateNavMeshTestPointStack()
    {
        foreach (Vector3 testPoint in sortedNavMeshPoints)
        {
            navMeshTestPoints.Push(testPoint);
        }
        
#if DEBUG
        if (ExtractFinderComponent.DebugMode)
            Logger.LogInfo($"Found {navMeshTestPoints.Count} extract postions for {ex.Settings.Name}");
#endif
    }

    private IEnumerable<Vector3> GetColliderTestPoints(BoxCollider collider, float searchRadius)
    {
        // Adjust the 3D mesh density to limit the number of points in it, but do not allow the density to drop below a certain point
        float navNeshTestPointDensityFactor = initialColliderTestPointDensityFactor;
        IEnumerable<Vector3> colliderTestPoints = Enumerable.Repeat(Vector3.positiveInfinity, maxColliderTestPoints + 1);
        int lastPointCount = colliderTestPoints.Count();
        while ((lastPointCount > maxColliderTestPoints) && (navNeshTestPointDensityFactor >= minColliderTestPointDensityFactor))
        {
            colliderTestPoints = collider.GetNavMeshTestPoints(searchRadius, navNeshTestPointDensityFactor);

            // If the number of points is the same as the previous iteration, give up
            if (colliderTestPoints.Count() == lastPointCount)
            {
#if DEBUG
                if (ExtractFinderComponent.DebugMode)
                    Logger.LogWarning($"Could not minimize collider test point count for {ex.Settings.Name}");
#endif

                break;
            }

            lastPointCount = colliderTestPoints.Count();
            navNeshTestPointDensityFactor /= 2;
        }
        navNeshTestPointDensityFactor *= 2;

        if (!colliderTestPoints.Any())
        {
            colliderTestPoints = Enumerable.Repeat(collider.transform.position, 1);
            
#if DEBUG
            if (ExtractFinderComponent.DebugMode)
                Logger.LogWarning($"Could not create test points. Using collider position instead");
#endif
        }
        else
        {
#if DEBUG
            if (ExtractFinderComponent.DebugMode)
            {
                Logger.LogInfo($"Generated {colliderTestPoints.Count()} collider test points using a density factor of {Math.Round(navNeshTestPointDensityFactor, 3)} and a search radius of {searchRadius}m");
                Logger.LogInfo($"Extract collider: center={collider.transform.position}, size={collider.size}.");
            }
#endif
        }

        return colliderTestPoints;
    }

    private IList<Vector3> GetColliderTestPointsOnNavMesh(IEnumerable<Vector3> colliderTestPoints, float searchRadius)
    {
        // For each point in the 3D mesh, try to find a point on the NavMesh within a certain radius
        List<Vector3> navMeshPoints = new();
        foreach (Vector3 testPoint in colliderTestPoints)
        {
            Vector3? navMeshPoint = NavMeshHelpers.GetNearbyNavMeshPoint(testPoint, searchRadius);
            if (navMeshPoint == null)
            {
                continue;
            }

            // Do not allow duplicate point to be added, which is possible depending on the mesh density
            if (navMeshPoints.Any(x => x == navMeshPoint))
            {
                continue;
            }

            navMeshPoints.Add(navMeshPoint.Value);
        }
        
#if DEBUG
        if (ExtractFinderComponent.DebugMode && !navMeshPoints.Any())
        {
            Logger.LogWarning($"Could not find any NavMesh points for {ex.Settings.Name} from {colliderTestPoints.Count()} test points using radius {searchRadius}m");
            Logger.LogWarning($"Test points: {string.Join(",", colliderTestPoints)}");
        }
#endif

        return navMeshPoints;
    }

    private void FindPathEndPoints(Vector3 testPoint)
    {
        // If endpoints have already been selected for the extract, use them
        if (pathEndpoints.Count > 0)
        {
            return;
        }

        List<Vector3> navMeshPoints = GameWorldComponent.Instance.GetAllSpawnPointPositionsOnNavMesh()?.ToList();
        if (navMeshPoints == null || navMeshPoints.Count == 0)
        {
            return;
        }

        // Create a dictionary of the distance between each spawn point and the extract test point, but deprioritize points with
        // much different elevation
        Dictionary<Vector3, float> navMeshPointDistances = new Dictionary<Vector3, float>(navMeshPoints.Count);
        foreach (var point in navMeshPoints)
        {
            float distance = Vector3.Distance(point, testPoint);
            float elevationPenalty = Math.Abs(point.y - testPoint.y) * pathEndpointHeightDeprioritizationFactor;
            navMeshPointDistances[point] = distance + elevationPenalty;
        }

        // Select the desired number of endpoints ensuring they are not too close together
        for (int i = 0; i < maxPathEndpoints; i++)
        {
            Vector3? bestPoint = null;
            float bestScore = float.MaxValue;

            foreach (var point in navMeshPoints)
            {
                bool tooClose = false;
                foreach (var existing in pathEndpoints)
                {
                    if (Vector3.Distance(point, existing) <= minDistanceBetweenPathEndpoints)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                    continue;

                float score = navMeshPointDistances[point];
                if (score < bestScore)
                {
                    bestScore = score;
                    bestPoint = point;
                }
            }

            if (bestPoint.HasValue)
            {
                pathEndpoints.Add(bestPoint.Value);
            }
            else
            {
                // No suitable point found
                break;
            }
        }
    }
}
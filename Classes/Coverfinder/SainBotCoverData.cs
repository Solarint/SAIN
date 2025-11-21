using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.SubComponents.CoverFinder;

public class SainBotCoverData()
{
    public List<CoverPoint> CoverPoints { get; } = [];
    public List<SainBotColliderData> ValidCollidersList { get; } = [];

    private const int PRELLOCATED_SIZE = 500;
    private readonly Collider[] _preAllocatedColliderArray = new Collider[PRELLOCATED_SIZE];
    private readonly HashSet<Collider> validCollidersHashSet = [];
    //private readonly HashSet<Collider> _preAllocColliderHashSet = [];

    /// <returns>Number of valid colliders found in query</returns>
    public int OverlapBoxAndFilter(BotColliderQueryParams parameters)
    {
        ClearColliderArray(_preAllocatedColliderArray);

        int hits = Physics.OverlapBoxNonAlloc(parameters.origin, parameters.halfExtents, _preAllocatedColliderArray, Quaternion.identity, parameters.mask);
        int validColliderCount = Filter(hits, _preAllocatedColliderArray, parameters.minColliderSize, parameters.maxColliderSize);

        foreach (Collider collider in _preAllocatedColliderArray)
        {
            if (collider != null && validCollidersHashSet.Add(collider))
            {
                ValidCollidersList.Add(new SainBotColliderData(collider));
            }
        }
        return ValidCollidersList.Count;
    }

    public void HandleLists(Vector3 origin)
    {
        const int MIN_COLLIDERS = 30;
        const float MAX_DISTANCE = 50f;

        UpdateColliderDistances(ValidCollidersList, origin);
        SortCollidersByDistance(ValidCollidersList, origin);
        ClearCollidersOverDistance(ValidCollidersList, validCollidersHashSet, MAX_DISTANCE, MIN_COLLIDERS);
        //Logger.LogDebug($"Currently {ValidCollidersList.Count} valid colliders");
    }

    private static void UpdateColliderDistances(List<SainBotColliderData> list, Vector3 origin)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            SainBotColliderData data = list[i];
            if (data.Collider == null)
            {
                list.RemoveAt(i);
            }
            else
            {
                data.SqrMagnitude = (data.Collider.transform.position - origin).sqrMagnitude;
                list[i] = data;
            }
        }
    }

    public static void ClearCollidersOverDistance(List<SainBotColliderData> list, HashSet<Collider> hashSet, float maxDistance, int min)
    {
        if (list.Count <= min)
            return;

        float distSqr = maxDistance * maxDistance;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            SainBotColliderData data = list[i];
            if (data.SqrMagnitude > distSqr)
            {
                hashSet.Remove(data.Collider);
                list.RemoveAt(i);
            }
        }
    }

    public static void SortCollidersByDistance(List<SainBotColliderData> list, Vector3 origin)
    {
        list.Sort((x, y) => x.SqrMagnitude.CompareTo(y.SqrMagnitude));
    }

    private static void ClearColliderArray(Collider[] array)
    {
        for (int i = 0; i < array.Length; i++)
            array[i] = null;
    }

    private static int Filter(int hits, Collider[] array, Vector3 minSize, Vector3 maxSize)
    {
        int validCount = 0;
        for (int i = 0; i < hits; i++)
        {
            Collider collider = array[i];
            if (collider == null)
                continue;

            Vector3 boundsSize = collider.bounds.size;
            if (boundsSize.x > maxSize.x || boundsSize.y > maxSize.y || boundsSize.z > maxSize.z)
            {
                array[i] = null;
                continue;
            }
            if (boundsSize.y < minSize.y || (boundsSize.x < minSize.x && boundsSize.z < minSize.z))
            {
                array[i] = null;
                continue;
            }
            if (_excludedColliderNames.Contains(collider.transform?.parent?.name))
            {
                array[i] = null;
                continue;
            }
            // Compact valid colliders to the front of the array
            if (validCount != i)
            {
                array[validCount] = collider;
                array[i] = null;
            }
            validCount++;
        }
        // Null out the rest of the array for safety
        for (int i = validCount; i < hits; i++)
        {
            array[i] = null;
        }
        return validCount;
    }

    /// <summary>
    /// Manual collection of collider names that are known to be problematic or not useful for cover finding.
    /// </summary>
    private static readonly HashSet<string> _excludedColliderNames =
    [
        "metall_fence_2",
        "metallstolb",
        "stolb",
        "fonar_stolb",
        "fence_grid",
        "metall_fence_new",
        "ladder_platform",
        "frame_L",
        "frame_small_collider",
        "bump2x_p3_set4x",
        "bytovka_ladder",
        "sign",
        "sign17_lod",
        "ograda1",
        "ladder_metal"
    ];

    public struct BotColliderQueryParams
    {
        public Vector3 halfExtents;
        public Vector3 origin;
        public LayerMask mask;
        public Vector3 minColliderSize;
        public Vector3 maxColliderSize;
    }
}
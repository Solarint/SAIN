using SAIN.Components.CoverFinder;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.SubComponents.CoverFinder;

public class ColliderFinder
{
    static ColliderFinder()
    {
        _orientation = Quaternion.identity;
    }

    public ColliderFinder(CoverFinderComponent component)
    {
        CoverFinderComponent = component;
    }

    private CoverFinderComponent CoverFinderComponent;

    /// <summary>
    /// Find colliders in a box that expands in size until there are enough colliders to begin cover analysis
    /// </summary>
    public static IEnumerator GetNewColliders(SainBotCoverData coverData, ColliderFinderParams config)
    {
        Vector3 boxOrigin = config.OriginPoint + Vector3.up * config.StartBoxHeight;
        Vector3 minColliderSize = new(0.25f, GlobalSettingsClass.Instance.General.Cover.CoverMinHeight, 0.25f);
        Vector3 maxColliderSize = new(50f, 50f, 50f);
        int layerCount = _masks.Count;
        for (int i = 0; i < layerCount; i++)
        {
            LayerMask mask = _masks[i];

            float boxLength = config.StartBoxWidth;
            float boxHeight = config.StartBoxHeight;
            for (int j = 0; j < config.MaxIterations; j++)
            {
                SainBotCoverData.BotColliderQueryParams queryParams = new() {
                    origin = boxOrigin,
                    halfExtents = new Vector3(boxLength, boxHeight, boxLength),
                    mask = mask,
                    minColliderSize = minColliderSize,
                    maxColliderSize = maxColliderSize
                };
                int hits = coverData.OverlapBoxAndFilter(queryParams);
                if (hits >= config.HitThreshold)
                {
#if DEBUG
                    Logger.LogDebug(hits + " colliders found in Layer: [" + mask.MaskToString() + "] after " + (j + 1) + " iterations");
#endif
                    yield break;
                }
                boxOrigin += Vector3.down * config.HeightDecreasePerIncrement;
                boxHeight += config.HeightIncreasePerIncrement + config.HeightDecreasePerIncrement;
                boxLength += config.LengthIncreasePerIncrement;
                yield return null;
            }
        }
    }

    /// <summary>
    /// Find colliders in a box that expands in size until there are enough colliders to being cover analysis
    /// </summary>
    public IEnumerator GetNewColliders(Collider[] preAllocArray, Vector3 originPoint, int iterationMax = 10, float startBoxWidth = 3f, int hitThreshold = 100)
    {
        const float StartBoxHeight = 0.25f;
        const float HeightIncreasePerIncrement = 0.5f;
        const float HeightDecreasePerIncrement = 0.5f;
        const float LengthIncreasePerIncrement = 5f;

        clearColliders(preAllocArray);

        float boxLength = startBoxWidth;
        float boxHeight = StartBoxHeight;
        Vector3 boxOrigin = originPoint + Vector3.up * StartBoxHeight;

        HitCount = 0;
        int hits = 0;
        int totalIterations = 0;
        bool foundEnough = false;
        int layerCount = _masks.Count;
        for (int l = 0; l < layerCount; l++)
        {
            var layer = _masks[l];

            for (int i = 0; i < iterationMax; i++)
            {
                totalIterations++;
                hits = GetCollidersInBox(boxLength, boxHeight, boxLength, boxOrigin, preAllocArray, layer);
                foundEnough = hits >= hitThreshold;
                if (foundEnough)
                {
                    break;
                }

                boxOrigin += Vector3.down * HeightDecreasePerIncrement;
                boxHeight += HeightIncreasePerIncrement + HeightDecreasePerIncrement;
                boxLength += LengthIncreasePerIncrement;
                yield return null;
            }
            if (foundEnough)
            {
                Logger.LogInfo($"Found [{hits}] colliders in Layer: [{layer.MaskToString()}] after [{totalIterations}] total iterations");
                break;
            }
        }

        HitCount = hits;
    }

    private static readonly List<LayerMask> _masks = new()
    {
        //LayerMaskClass.TerrainLayer,
        LayerMaskClass.HighPolyWithTerrainNoGrassMask,
        //LayerMaskClass.LowPolyColliderLayerMask,
    };

    private static int GetCollidersInBox(float x, float y, float z, Vector3 boxOrigin, Collider[] array, LayerMask colliderMask)
    {
        int rawHits = Physics.OverlapBoxNonAlloc(boxOrigin, new(x, y, z), array, _orientation, colliderMask);
        return FilterColliders(array, rawHits);
    }

    private static Quaternion _orientation;

    private void destroyDebug()
    {
        for (int i = 0; i < debugObjects.Count; i++)
        {
            GameObject.Destroy(debugObjects[i]);
        }
        debugObjects.Clear();
    }

    public int HitCount;

    private List<GameObject> debugObjects = new();

    private static void clearColliders(Collider[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = null;
        }
    }

    /// <summary>
    /// Sorts an array of Colliders based on their Distance from bot's Position.
    /// </summary>
    /// <param value="array">The array of Colliders to be sorted.</param>
    public void SortArrayBotDist(Collider[] array)
    {
        Array.Sort(array, ColliderArrayBotDistComparer);
    }

    private static int FilterColliders(Collider[] array, int hits)
    {
        float minY = CoverFinderComponent.CoverMinHeight;
        const float minX = 0.25f;
        const float minZ = 0.25f;

        Vector3 maxSize = new(50f, 50f, 50f);

        int hitReduction = 0;
        for (int i = 0; i < hits; i++)
        {
            Vector3 size = array[i].bounds.size;
            if (size.x > maxSize.x || size.y > maxSize.y || size.z > maxSize.z)
            {
                array[i] = null;
                hitReduction++;
                continue;
            }
            if (size.y < minY || (size.x < minX && size.z < minZ))
            {
                array[i] = null;
                hitReduction++;
            }
        }
        //UpdateDebugColliders(array);
        return hits - hitReduction;
    }

    public void UpdateDebugColliders(Collider[] array)
    {
#if DEBUG
        if (SAINPlugin.DebugMode
            && SAINPlugin.LoadedPreset.GlobalSettings.General.Cover.DebugCoverFinder
            && CoverFinderComponent.Bot.Cover.CurrentCoverFinderState == Classes.CoverFinderState.on)
        {
            foreach (Collider collider in array)
            {
                if (collider == null) continue;

                if (!debugGUIObjects.ContainsKey(collider))
                {
                    var obj = DebugGizmos.CreateLabel(collider.transform.position, collider.name);
                    if (obj != null)
                    {
                        debugGUIObjects.Add(collider, obj);
                    }
                }
                if (!debugColliders.ContainsKey(collider))
                {
                    var marker = DebugGizmos.DrawSphere(collider.transform.position, 0.1f, Color.white, 1f);
                    if (marker != null)
                    {
                        debugColliders.Add(collider, marker);
                    }
                }
            }
        }
        else if (debugGUIObjects.Count > 0
            || debugColliders.Count > 0)
        {
            foreach (var obj in debugGUIObjects)
            {
                DebugGizmos.DestroyLabel(obj.Value);
            }
            foreach (var obj in debugColliders)
            {
                GameObject.Destroy(obj.Value);
            }
            debugGUIObjects.Clear();
            debugColliders.Clear();
        }
#endif
    }

    private static Dictionary<Collider, DebugLabel> debugGUIObjects = new();
    private static Dictionary<Collider, GameObject> debugColliders = new();

    public int ColliderArrayBotDistComparer(Collider A, Collider B)
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
            Vector3 origin = CoverFinderComponent.Bot.NavMeshPosition;
            float AMag = (origin - A.transform.position).sqrMagnitude;
            float BMag = (origin - B.transform.position).sqrMagnitude;
            return AMag.CompareTo(BMag);
        }
    }
}
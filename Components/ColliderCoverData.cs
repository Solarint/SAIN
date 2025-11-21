
using SAIN.Components.BotControllerSpace.Classes.Raycasts;
using SAIN.Helpers;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Components.CoverFinder;

public class ColliderCoverManager : MonoBehaviour
{
    public ColliderCoverComponent CreateCover(Collider collider)
    {
        ColliderCoverComponent component = collider.gameObject.GetComponent<ColliderCoverComponent>();
        if (component == null)
        {
            component = collider.gameObject.AddComponent<ColliderCoverComponent>();
            component.Initialize(collider);
            CoverGenerationList.Insert(0, component);
        }
        return component;
    }

    private void Update()
    {
        GenerateCover();
    }

    private void GenerateCover(int maxPerFrame = 4)
    {
        int totalIndexes = CoverGenerationList.Count - 1;
        int stopIndex = Mathf.Max(totalIndexes - maxPerFrame, 0);
        for (int i = totalIndexes; i >= stopIndex; i--)
        {
            ColliderCoverComponent component = CoverGenerationList[i];
            CoverGenerationList.RemoveAt(i);
            component.Generate();
            Colliders.Add(component);
        }
    }

    private List<ColliderCoverComponent> CoverGenerationList { get; } = [];
    public HashSet<ColliderCoverComponent> Colliders { get; } = [];
}

public class ColliderCoverComponent : MonoBehaviour
{
    private const int DIRECTION_COUNT = 60;

    static ColliderCoverComponent()
    {
        List<Vector3> list = [];
        float angleStep = 360f / DIRECTION_COUNT;
        Vector3 forward = Vector3.forward;
        for (int i = 0; i < DIRECTION_COUNT; i++)
        {
            Quaternion rotation = Quaternion.Euler(0f, angleStep * i, 0f);
            list.Add(rotation * forward);
        }
        StaticDirections = [.. list];
    }

    public HashSet<CoverPointClass> CoverPoints { get; } = [];
    public Vector3 ColliderPosition { get; private set; }
    public Vector3 Size { get; private set; }
    public Collider Collider { get; private set; }

    public void Initialize(Collider collider)
    {
        Collider = collider;
        Size = collider.bounds.size;
        ColliderPosition = collider.transform.position;
    }

    private static readonly Vector3[] StaticDirections;

    private static void CheckCoverRealSize(Collider collider, out List<RaycastHit> Hits, Vector3 heightOffset)
    {
        Vector3 colliderPosition = collider.transform.position;
        Vector3 size = collider.bounds.size;
        float magnitude = size.magnitude;
        Hits = [];
        for (int i = 0; i < StaticDirections.Length; i++)
        {
            Vector3 staticDirNormal = StaticDirections[i];
            Vector3 direction = staticDirNormal * magnitude;
            Vector3 rawPosition = direction + colliderPosition;
            DebugGizmos.DrawSphere(rawPosition, 0.1f, Color.white);
            Ray ray = new() {
                direction = -direction,
                origin = rawPosition + heightOffset
            };
            if (collider.Raycast(ray, out RaycastHit hit, magnitude))
                Hits.Add(hit);
        }
    }

    public void Generate()
    {
        //if (_generated) return;

        float magnitude = Size.magnitude;
        Vector3 heightOffset = Vector3.up * Mathf.Min(Size.y * 0.5f, 0.5f);
        CheckCoverRealSize(Collider, out List<RaycastHit> hits, heightOffset);
        float extent = FindExtent(hits);
        if (extent > 0)
        {
            Logger.LogDebug($"Extent Found [{extent}]");
            if (extent > 0.2f)
            {
                //DebugGizmos.Ray(ColliderPosition, Vector3.up, Color.white, magnitude, 0.1f, false);

                List<NavMeshHit> navHits = [];
                for (int i = 0; i < hits.Count; i++)
                {
                    RaycastHit colliderHit = hits[i];
                    Vector3 wallPosition = colliderHit.point - heightOffset;
                    Vector3 hitNormal = colliderHit.normal;
                    hitNormal.y = 0f;
                    hitNormal.Normalize();
                    if (NavMesh.SamplePosition(wallPosition + hitNormal, out NavMeshHit navHit1, 0.5f, -1))
                    {
                        navHits.Add(navHit1);
                    }
                }

                for (int i = 0; i < navHits.Count; i++)
                {
                    bool pointGood = true;
                    for (int j = i + 1; j < navHits.Count; j++)
                    {
                        if ((navHits[i].position - navHits[j].position).sqrMagnitude < 0.1f)
                        {
                            pointGood = false;
                            break;
                        }
                    }
                    if (pointGood)
                    {
                        CoverPoints.Add(new(Collider, ColliderPosition, navHits[i].position));
                        DebugGizmos.DrawSphere(navHits[i].position, 0.25f, Color.red);
                        DebugGizmos.DrawLine(navHits[i].position, ColliderPosition, Color.yellow, 0.02f);
                    }
                }
            }
        }
        Logger.LogDebug($"Generated Points [{CoverPoints.Count}]");
    }

    private static float FindExtent(List<RaycastHit> hits)
    {
        float extent = 0;
        for (int i = hits.Count - 1; i >= 0; i--)
        {
            RaycastHit hit = hits[i];
            for (int j = hits.Count - 2; j >= 0; j--)
            {
                float dist = (hit.point - hits[j].point).sqrMagnitude;
                if (dist > extent)
                {
                    extent = dist;
                }
            }
        }

        return extent;
    }
}

public class ColliderCoverDataClass(Collider collider)
{
    public bool IsValid = false;
    public Collider Collider = collider;
    public Vector3 ColliderPosition = collider.transform.position;
    public HashSet<CoverPointClass> CoverPoints { get; } = [];
}

public struct ColliderCoverData(int index, Collider collider, Vector3 targetPos, Vector3 botPos, DirCalcData botToTargetData)
{
    public bool Analyzed = false;
    public bool IsValid = true;

    public int Index = index;
    public Collider Collider = collider;
    public Vector3 ColliderPosition = collider.transform.position;
    public Vector3 TargetPosition = targetPos;
    public Vector3 BotPosition = botPos;

    public DirCalcData BotToCoverDirectionData = new();
    public DirCalcData TargetToCoverDirectionData = new();
    public DirCalcData BotToTargetDirectionData = botToTargetData;

    public float DotFromBotToTargetToCollider;
    public float DotFromTargetToBotToCollider;
}
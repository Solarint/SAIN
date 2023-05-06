using BepInEx.Logging;
using EFT;
using Movement.Helpers;
using SAIN_Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static Movement.UserSettings.Debug;
using static Movement.UserSettings.DogFight;

namespace Movement.Components
{
    public class CoverFinderComponent : MonoBehaviour
    {
        private void Awake()
        {
            bot = GetComponent<BotOwner>();
            BotColor = RandomColor;
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(LeanComponent) + $": {bot.name}: ");
            DebugTimer = Time.time + 5f;

            StartCoroutine(CornerFinder());

            StartCoroutine(CheckNavMeshCorners());
            StartCoroutine(RayCastCorners());
            StartCoroutine(LerpCorners());
            StartCoroutine(CheckPointsForCover());
        }

        public Color BotColor;
        public List<Vector3> FinalPositions = new List<Vector3>();

        private bool DebugMode => DebugCoverComponent.Value;

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }

        private IEnumerator CornerFinder()
        {
            while (true)
            {
                FindAllCorners();
                yield return new WaitForSeconds(5f);
            }
        }

        private void FindAllCorners()
        {
            int iterations = 0;
            const int max = 5;

            while (iterations < max)
            {
                CheckCornersInRadius(iterations * 10f + 2f);
                iterations++;
            }

            if (DebugMode && FinalPositions.Count > 0)
            {
                Log();

                foreach (var corner in FinalPositions)
                {
                    DebugDrawer.Sphere(corner, 0.15f, BotColor, 5f);
                }
            }
        }

        private void Log()
        {
            Logger.LogDebug($"Total CoverPositions [{FinalPositions.Count}]. " +

                $"BackLog = Raycast[{RaycastBacklog.Count}] Lerp[{LerpBacklog.Count}] Checked[{FinalCheckBacklog.Count}] Process:[{ProcessBacklog.Count}] " +

                $"with [{RawCorners.Count}] Total corners found and [{ProcessedCorners.Count}] corners added to processing");
        }

        private void CheckCornersInRadius(float radius)
        {
            int iter = 0;
            const int max = 10;
            int finished = 0;

            List<Vector3> newCorners = new List<Vector3>();

            while (iter < max)
            {
                Vector3 randomPoint = Random.onUnitSphere * radius;
                randomPoint.y = Random.Range(-5, 5);
                randomPoint += bot.Transform.position;

                if (NavMesh.SamplePosition(randomPoint, out var hit, 2f, NavMesh.AllAreas))
                {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(bot.Transform.position, hit.position, -1, path))
                    {
                        newCorners.AddRange(path.corners);
                        finished++;
                    }
                }
                iter++;
            }

            CompareAndAdd(newCorners);
        }

        private void CompareAndAdd(List<Vector3> corners)
        {
            var distinctList = corners.Except(RawCorners, new Vector3PositionComparer());
            var filteredList = distinctList.Where(corner => Vector3.Distance(corner, bot.Transform.position) >= 0.5f).ToList();
            RawCorners.AddRange(filteredList);

            var distinctList2 = filteredList.Except(ProcessedCorners, new Vector3PositionComparer());
            var filteredList2 = distinctList2.ToList();
            ProcessBacklog.AddRange(filteredList2);
        }

        private IEnumerator CheckNavMeshCorners()
        {
            while (true)
            {
                if (ProcessBacklog.Count > 1)
                {
                    List<Vector3> list = new List<Vector3>();

                    for (int i = 0; i < ProcessBacklog.Count; i++)
                    {
                        for (int j = i + 1; j < ProcessBacklog.Count; j++)
                        {
                            if (Approximately(ProcessBacklog[i].y, ProcessBacklog[j].y, 0.05f))
                            {
                                if (Vector3.Distance(ProcessBacklog[i], ProcessBacklog[j]) < 5f)
                                {
                                    list.Add(ProcessBacklog[i]);
                                    list.Add(ProcessBacklog[j]);
                                }
                            }
                        }
                    }

                    RaycastBacklog = FilterDistance(list);
                    ProcessedCorners.AddRange(ProcessBacklog);
                    ProcessBacklog.Clear();

                    yield return new WaitForSeconds(0.25f);
                }

                yield return null;
            }
        }

        private IEnumerator RayCastCorners()
        {
            while (true)
            {
                if (RaycastBacklog.Count > 1)
                {
                    List<Vector3> list = new List<Vector3>();

                    for (int i = 0; i < RaycastBacklog.Count; i++)
                    {
                        for (int j = i + 1; j < RaycastBacklog.Count; j++)
                        {
                            Vector3 position = RaycastBacklog[i];
                            position.y += 0.2f;
                            Vector3 direction = RaycastBacklog[j] - position;
                            direction.y += 0.2f;

                            if (!Physics.Raycast(position, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                            {
                                list.Add(RaycastBacklog[i]);
                                list.Add(RaycastBacklog[j]);
                            }
                        }
                    }

                    RaycastBacklog.Clear();
                    LerpBacklog = FilterDistance(list);
                    yield return new WaitForSeconds(0.25f);
                }

                yield return null;
            }
        }

        private IEnumerator LerpCorners()
        {
            while (true)
            {
                if (LerpBacklog.Count > 1)
                {
                    List<Vector3> list = new List<Vector3>();

                    for (int i = 0; i < LerpBacklog.Count; i++)
                    {
                        for (int j = i + 1; j < LerpBacklog.Count; j++)
                        {
                            if (Vector3.Distance(LerpBacklog[i], LerpBacklog[j]) < 5f)
                            {
                                Vector3 lerpedCorner = Vector3.Lerp(LerpBacklog[i], LerpBacklog[j], 0.5f);
                                list.Add(lerpedCorner);
                            }
                        }
                    }

                    LerpBacklog.Clear();
                    FinalCheckBacklog = FilterDistance(list);
                    yield return new WaitForSeconds(0.25f);
                }

                yield return null;
            }
        }

        private IEnumerator CheckPointsForCover()
        {
            while (true)
            {
                if (FinalCheckBacklog.Count > 1)
                {
                    Vector3 point = FinalCheckBacklog.PickRandom();

                    if (CheckForObjects(point))
                    {
                        FinalCheckBacklog.Remove(point);
                        FinalPositions.Add(point);
                    }

                    FinalPositions = FilterDistance(FinalPositions);
                    yield return new WaitForEndOfFrame();
                }
                yield return null;
            }
        }

        private bool CheckForObjects(Vector3 point)
        {
            point.y += 0.25f;
            float raycastDistance = 2f;

            foreach (Vector3 direction in Directions)
            {
                if (Physics.Raycast(point, direction, raycastDistance, LayerMaskClass.LowPolyColliderLayerMask))
                {
                    return true;
                }
            }
            return false;
        }

        private readonly Vector3[] Directions = {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            (Vector3.forward + Vector3.left).normalized,
            (Vector3.forward + Vector3.right).normalized,
            (Vector3.back + Vector3.left).normalized,
            (Vector3.back + Vector3.right).normalized
        };

        private List<Vector3> FilterDistance(List<Vector3> corners, float min = 0.5f)
        {
            for (int i = 0; i < corners.Count; i++)
            {
                for (int j = i + 1; j < corners.Count; j++)
                {
                    if (Vector3.Distance(corners[i], corners[j]) < min)
                    {
                        corners.RemoveAt(j);
                        j--;
                    }
                }
            }

            return corners;
        }

        private static bool Approximately(float a, float b, float tolerance)
        {
            return Mathf.Abs(a - b) < tolerance;
        }

        private static Vector3 FindArcPoint(Vector3 botPos, Vector3 targetPos, float arcRadius, float angle)
        {
            Vector3 direction = (botPos - targetPos).normalized;
            direction.y = 0f;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 arcPoint = arcRadius * (rotation * direction);

            return botPos + arcPoint;
        }

        public static Color RandomColor => new Color(Random.value, Random.value, Random.value);

        private float DebugTimer = 0f;
        private BotOwner bot;
        protected ManualLogSource Logger;

        private List<Vector3> RawCorners = new List<Vector3>();
        private List<Vector3> ProcessedCorners = new List<Vector3>();
        private List<Vector3> ProcessBacklog = new List<Vector3>();
        private List<Vector3> RaycastBacklog = new List<Vector3>();
        private List<Vector3> LerpBacklog = new List<Vector3>();
        private List<Vector3> FinalCheckBacklog = new List<Vector3>();

        // Custom IEqualityComparer that compares Vector3 objects based on their position
        private class Vector3PositionComparer : IEqualityComparer<Vector3>
        {
            public bool Equals(Vector3 v1, Vector3 v2)
            {
                return v1.Equals(v2);
            }

            public int GetHashCode(Vector3 v)
            {
                return v.GetHashCode();
            }
        }
    }
}
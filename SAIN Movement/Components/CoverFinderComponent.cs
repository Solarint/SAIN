using BepInEx.Logging;
using EFT;
using HarmonyLib;
using Movement.Helpers;
using SAIN_Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static Movement.UserSettings.Debug;
using static Movement.UserSettings.DogFight;
using static UnityEngine.UI.GridLayoutGroup;

namespace Movement.Components
{
    public class CoverFinderComponent : MonoBehaviour
    {
        private bool DebugMode => DebugCoverComponent.Value;
        private void Awake()
        {
            bot = GetComponent<BotOwner>();
            BotColor = RandomColor;
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(LeanComponent) + $": {bot.name}: ");

            StartCoroutine(PathGenerator());
            StartCoroutine(GeneratePoints());
            StartCoroutine(VerifyGenPoints());
            StartCoroutine(PossibleCoverCheck());
        }

        private const float MovementThreshold = 1.0f;

        private void Log()
        {
            Logger.LogDebug($"Final PossibleCoverPoints [{PossibleCoverPoints.Count}]. " +

                $"BackLog: VerifiedPoints = [{VerifiedPoints.Count}] GeneratedPoints = [{GeneratedPoints.Count}]. " +

                $"Data: [{RandomNavPoints.Count}] Total NavPoints found and [{ProcessedRandomPoints.Count}] Points added to processing");
        }

        private IEnumerator PathGenerator()
        {
            while (true)
            {
                const int maxCover = 1000;

                // If the bot hasn't moved a certain distance, stop calculating new cover positions
                float distance = Vector3.Distance(LastPositionForCheck, bot.Transform.position);
                if (distance < MovementThreshold && PossibleCoverPoints.Count > maxCover)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                // Save the bot's current position for reference later above in the next loop
                LastPositionForCheck = bot.Transform.position;

                // Set the steps to increase the range for each iteration
                const float rangeBase = 3f;
                const float rangeMod = 5f;

                // Calculate Paths in increasing radius based on the number of i
                const int max = 20;
                int i = 0;
                while (i < max)
                {
                    StartCoroutine(CheckPaths(i * rangeMod + rangeBase));
                    yield return new WaitForEndOfFrame();
                    i++;
                }

                if (DebugMode && PossibleCoverPoints.Count > 0 && DebugTimer < Time.time)
                {
                    DebugTimer = Time.time + 30f;

                    Log();

                    foreach (var corner in PossibleCoverPoints)
                    {
                        DebugDrawer.Ray(corner, Vector3.up, 0.5f, 0.05f, BotColor, 30f);
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }

        private IEnumerator CheckPaths(float radius)
        {
            int i = 0;
            const int max = 5;

            List<Vector3> Points = new List<Vector3>();

            while (i < max)
            {
                Vector3 randomPoint = Random.onUnitSphere * radius;
                randomPoint.y = Random.Range(-5, 5);
                randomPoint += bot.Transform.position;

                if (NavMesh.SamplePosition(randomPoint, out var hit, 2f, NavMesh.AllAreas))
                {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(bot.Transform.position, hit.position, -1, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        Points.Add(hit.position);
                    }
                }

                yield return new WaitForEndOfFrame();
                i++;
            }

            // Checks the randomly generate nav hits to make sure they aren't too close together, then make sure we haven't already processed them, and that they aren't awaiting processing.
            var newNavPoints = FilterDistance(Points, 10f).Except(ProcessedRandomPoints, new Vector3PositionComparer()).ToList().Except(RandomNavPoints, new Vector3PositionComparer());

            // If Any points remain, add them to our cache
            RandomNavPoints.AddRange(newNavPoints.ToList());

            yield return null;
        }

        private IEnumerator GeneratePoints()
        {
            while (true)
            {
                if (RandomNavPoints.Count > 0)
                {
                    Vector3 point = RandomNavPoints.PickRandom();
                    ProcessedRandomPoints.Add(point);
                    RandomNavPoints.Remove(point);

                    Generate(point);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private void Generate(Vector3 startPoint)
        {
            int i = 0;
            const int max = 10;

            List<Vector3> points = new List<Vector3>();

            while (i < max)
            {
                Vector3 randomPoint = Random.insideUnitCircle * 30f;
                randomPoint += startPoint;

                if (NavMesh.SamplePosition(randomPoint, out var hit, 1f, NavMesh.AllAreas))
                {
                    points.Add(hit.position);
                }
                i++;
            }

            points = FilterDistance(points);

            var distinctList = points.Except(GeneratedPoints, new Vector3PositionComparer());

            GeneratedPoints.AddRange(distinctList.ToList());

            GeneratedPoints = FilterDistance(GeneratedPoints);
        }

        private IEnumerator VerifyGenPoints()
        {
            while (true)
            {
                if (GeneratedPoints.Count > 0)
                {
                    // Grab a random point from the list cache
                    Vector3 point = GeneratedPoints.PickRandom();
                    // remove that point from the list cache
                    GeneratedPoints.Remove(point);

                    const float floatingCheck = 0.25f;
                    const float heightCheck = 1.5f;

                    // Check to make sure the point isn't floating.
                    if (Physics.Raycast(point, Vector3.down, floatingCheck, Mask))
                    {
                        // Check to make sure the point isn't under something
                        if (!Physics.Raycast(point, Vector3.up, heightCheck, Mask))
                        {
                            // Add point to list
                            VerifiedPoints.Add(point);
                        }
                        yield return new WaitForEndOfFrame();
                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator PossibleCoverCheck()
        {
            while (true)
            {
                if (VerifiedPoints.Count > 0)
                {
                    yield return new WaitForEndOfFrame();

                    // Grab a random point from the list cache
                    Vector3 point = VerifiedPoints.PickRandom();
                    // remove that point from the list cache
                    VerifiedPoints.Remove(point);

                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(bot.Transform.position, point, -1, path) && path.status != NavMeshPathStatus.PathComplete)
                    {
                        continue;
                    }

                    // max number of hits a point can have before its considered possible cover
                    const int threshold = 1;

                    // Copies the point and then raises it so its not blocked by small ground objects
                    Vector3 rayPoint = point;
                    rayPoint.y += 0.5f;

                    // Max distance to check and cover score int
                    float raycastDistance = 1f;
                    int cover = 0;

                    // Checks 8 cardinal directions around the point
                    foreach (Vector3 direction in Directions)
                    {
                        // is cover score over our limit?
                        if (cover > threshold)
                        {
                            // Then add it to our new list
                            PossibleCoverPoints.Add(point);
                            coverPoints.AddItem(new CoverPoint(point, direction, cover));
                            break;
                        }

                        // Does it intersect a collider in this direction?
                        if (Physics.Raycast(rayPoint, direction, raycastDistance, Mask))
                        {
                            // Add 1 point to its coverscore
                            cover++;
                        }
                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private CoverPoint[] coverPoints;

        private static bool Approximately(float a, float b, float tolerance)
        {
            return Mathf.Abs(a - b) < tolerance;
        }

        private static List<Vector3> FilterDistance(List<Vector3> corners, float min = 0.5f)
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

        public static Color RandomColor => new Color(Random.value, Random.value, Random.value);

        private List<Vector3> ProcessedRandomPoints = new List<Vector3>();
        private List<Vector3> RandomNavPoints = new List<Vector3>();
        private List<Vector3> GeneratedPoints = new List<Vector3>();
        private List<Vector3> VerifiedPoints = new List<Vector3>();
        private List<Vector3> PossibleCoverPoints = new List<Vector3>();

        private Vector3 LastPositionForCheck;
        public Color BotColor;
        private LayerMask Mask = LayerMaskClass.LowPolyColliderLayerMask;
        private float DebugTimer = 0f;
        private BotOwner bot;
        protected ManualLogSource Logger;

        private class CoverPoint
        {
            public CoverPoint(Vector3 point, Vector3 direction, int coverLevel)
            {
                Position = point;
                Direction = direction;
                CoverLevel = coverLevel;
            }

            public Vector3 Position { get; private set; }
            public Vector3 Direction { get; private set; }
            public int CoverLevel { get; private set; }
        }

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

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }
    }
}
using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN_Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using static Movement.Components.ConstantValues;
using static Movement.UserSettings.Debug;

namespace Movement.Components
{
    [BepInPlugin("com.solarint.unicover", "A Universal Cover System", "0.1")]
    public class Plugin : BaseUnityPlugin
    {
        private bool DebugMode => DebugCoverComponent.Value;

        //private List<CoverFinderComponent> CoverFinders = new List<CoverFinderComponent>();
        private List<Player> Players = new List<Player>();

        private GameObject CoverObject;
        private CoverCentralComponent Central;
        private List<string> PlayerIDs = new List<string>();

        private void Awake()
        {
            Logger.LogDebug($"Universal Cover System Loaded");
        }

        private void Update()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld == null || gameWorld.RegisteredPlayers == null)
            {
                return;
            }

            if (Frequency < Time.time)
            {
                Frequency = Time.time + 1.0f;

                if (CoverObject == null)
                {
                    CoverObject = new GameObject("CoverCentralObject");
                }

                if (Central == null)
                {
                    Central = CoverObject.AddComponent<CoverCentralComponent>();
                }
            }
        }

        private float Frequency = 0f;
    }

    public class CoverCentralComponent : MonoBehaviour
    {
        private bool DebugMode => DebugCoverComponent.Value;

        public List<CoverPoint> FinalCoverPoints = new List<CoverPoint>();
        private List<Vector3> PointsToCheck = new List<Vector3>();
        public List<Vector3> InputPoints = new List<Vector3>();

        private void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);

            Logger.LogDebug($"Central Controller Loaded");

            StartCoroutine(SendFinalPoints());
            StartCoroutine(CheckPointsForCover());
        }

        private List<CoverFinderComponent> CoverFinders = new List<CoverFinderComponent>();

        private void Update()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld == null || gameWorld.RegisteredPlayers == null)
            {
                if (CoverFinders.Count != 0)
                {
                    CoverFinders.Clear();
                }
                return;
            }

            if (RecheckTimer < Time.time)
            {
                RecheckTimer = Time.time + 1f;
                CheckForNewPlayers();
                DrawDebug();
            }
        }

        private float RecheckTimer = 0f;

        private void CheckForNewPlayers()
        {
            List<Player> currentPlayers = Singleton<GameWorld>.Instance.RegisteredPlayers;
            foreach (Player player in currentPlayers)
            {
                if (player.HealthController.IsAlive)
                {
                    bool lockTaken = false;
                    try
                    {
                        // Check if lock is not held before accessing CoverFinders
                        lockTaken = Monitor.TryEnter(_coverFindersLock);

                        var component = player.GetComponent<CoverFinderComponent>();

                        if (component == null)
                        {
                            CoverFinders.Add(player.gameObject.AddComponent<CoverFinderComponent>());
                        }
                    }
                    finally
                    {
                        // Release the lock if it was taken by this thread
                        if (lockTaken)
                        {
                            Monitor.Exit(_coverFindersLock);
                        }
                    }
                }
            }
        }

        public const float CoverRatio = 0.33f;

        /// <summary>
        /// Checks for possible cover points around the Player and adds them to the RawCoverPoints list.
        /// </summary>
        private IEnumerator SendFinalPoints()
        {
            while (true)
            {
                if (CoverFinders.Count > 0)
                {
                    List<Vector3> points = new List<Vector3>();

                    bool lockTaken = false;
                    try
                    {
                        // Check if lock is not held before accessing CoverFinders
                        lockTaken = Monitor.TryEnter(_coverFindersLock);

                        if (lockTaken)
                        {
                            foreach (var finder in CoverFinders.ToList())
                            {
                                if (finder != null)
                                {
                                    finder.FinalCoverPoints = FinalCoverPoints;
                                    points.AddRange(finder.OutputPoints);
                                    finder.OutputPoints.Clear();
                                }
                                else
                                {
                                    CoverFinders.Remove(finder);
                                }
                            }
                        }
                        else
                        {
                            yield return new WaitForSeconds(0.1f);
                            continue;
                        }
                    }
                    finally
                    {
                        // Release the lock if it was taken by this thread
                        if (lockTaken)
                        {
                            Monitor.Exit(_coverFindersLock);
                        }
                    }

                    if (points.Count > 0)
                    {
                        var filtered = FilterDistance(points, 2f);

                        Logger.LogDebug($"Checking [{filtered.Count}] points for cover");

                        PointsToCheck.AddRange(filtered);
                    }
                    yield return new WaitForSeconds(1f);
                }
                yield return null;
            }
        }

        private IEnumerator CheckPointsForCover()
        {
            while (true)
            {
                if (PointsToCheck.Count > 0)
                {
                    List<Vector3> points = new List<Vector3>();
                    points.AddRange(PointsToCheck);
                    PointsToCheck.Clear();

                    List<CoverPoint> coverPoints = new List<CoverPoint>();
                    int i = 0;
                    while (i < points.Count)
                    {
                        if (FinalCoverPoints.Count > 0 && CheckDistanceToExisting(FinalCoverPoints, points[i], 2f))
                        {
                            i++;
                            yield return new WaitForEndOfFrame();
                            continue;
                        }

                        // Copies the points and then raises it so its not blocked by small ground objects
                        Vector3 rayPoint = points[i];
                        rayPoint.y += RayYOffset;

                        yield return new WaitForEndOfFrame();

                        // Checks 8 even directions around the points
                        foreach (Vector3 direction in Directions)
                        {
                            // Does it intersect a collider in this direction?
                            if (Physics.Raycast(rayPoint, direction, out var hit, RayCastDistance, Mask))
                            {
                                Vector3 hitDirection = hit.point - rayPoint;

                                // If a cover point hits, but is too close to the objects that may provide cover, shift its position away from the object.
                                if (hitDirection.magnitude < HitDistanceThreshold)
                                {
                                    points[i] -= hitDirection.normalized / PointDistanceDivideBy;
                                }

                                var cover = new CoverPoint(points[i], direction);

                                yield return new WaitForEndOfFrame();

                                if (CheckHeightAndPercent(cover, out CoverPoint goodCover))
                                {
                                    coverPoints.Add(goodCover);
                                }
                                break;
                            }
                        }
                        i++;
                    }

                    FinalCoverPoints.AddRange(coverPoints);
                }

                yield return null;
            }
        }

        private bool CheckHeightAndPercent(CoverPoint point, out CoverPoint goodPoint, float minCoverRatio = 0.45f)
        {
            // Assign our constant values to check in the loop
            const int max = 30;
            const float height = 1.5f;
            const float heightStep = height / max;

            // Start the loop
            float heightHit = 0;
            int coverHit = 0;
            int i = 0;
            while (i < max)
            {
                // Take the position of the cover point, then raise it up to our character height. Then lower that in even steps to find the height of the cover.
                Vector3 rayPoint = point.Position;
                rayPoint.y += height - (i * heightStep);

                if (Physics.Raycast(rayPoint, point.Direction, 1f, Mask))
                {
                    // Store the first hit height as our cover height
                    if (coverHit == 0)
                    {
                        heightHit = rayPoint.y - point.Position.y;
                    }
                    // Count up the amount of hits
                    coverHit++;
                }
                i++;
            }

            // How many cover hits were there compared to the number of iterations in the loop?
            float coverAmount = (float)coverHit / i;

            // Assign the cover height and amount we found to the CoverPoint object
            point.CoverAmount = coverAmount;
            point.CoverHeight = heightHit;
            goodPoint = point;

            //Logger.LogDebug($"Final CheckForCalcPath for this point [{coverAmount > minCoverRatio}] because [{coverHit}] and [{i}]. CoverAmount [{coverAmount}] Height [{heightHit}]");

            // Return true if the cover amount is higher than the minimum input amount
            return coverAmount > minCoverRatio;
        }

        private void CheckCoverWidth(CoverPoint point)
        {
            const int max = 30;
            const float height = 1.5f;
            const float heightStep = max / height;
            const float angleStep = max / 360f;

            int i = 0;
            while (i < max)
            {
                Vector3 rayPoint = point.Position;
                rayPoint.y += height - (i * heightStep);
                if (Physics.Raycast(rayPoint, point.Direction, 1f, Mask))
                {
                    point.CoverHeight = rayPoint.y - point.Position.y;
                    break;
                }
                i++;
            }
        }

        private void DrawDebug()
        {
            if (DebugMode && DebugTimer < Time.time)
            {
                DebugTimer = Time.time + 10f;

                Logger.LogDebug($"Final Points = [{FinalCoverPoints.Count}]");

                if (FinalCoverPoints.Count > 0)
                {
                    foreach (var point in FinalCoverPoints)
                    {
                        DebugDrawer.Sphere(point.Position, 0.3f, Color.blue, 10f);
                    }
                }
            }
        }

        private static bool CheckDistanceToExisting(List<CoverPoint> points, Vector3 newPosition, float min = 0.5f)
        {
            foreach (var pos in points)
            {
                if (Vector3.Distance(pos.Position, newPosition) < min)
                {
                    return true;
                }
            }

            return false;
        }

        private List<Vector3> FilterDistance(List<Vector3> points, float min = 0.5f)
        {
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    if (Vector3.Distance(points[i], points[j]) < min)
                    {
                        points.RemoveAt(j);
                        j--;
                    }
                }
            }

            return points;
        }

        private readonly object _coverFindersLock = new object();
        private readonly object _inputPointsLock = new object();
        private int Filtered = 0;
        public List<CoverPoint> DistanceChecked = new List<CoverPoint>();
        private LayerMask Mask = LayerMaskClass.HighPolyWithTerrainMask;
        private float DebugTimer = 0f;
        protected ManualLogSource Logger;

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }
    }

    public class CoverPoint
    {
        public CoverPoint(Vector3 coverPosition, Vector3 coverDirection)
        {
            Position = coverPosition;
            Direction = coverDirection;
        }

        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public int CoverWidth { get; set; }
        public float CoverHeight { get; set; }
        public float CoverAmount { get; set; }
    }

    /// <summary>
    /// Compares CoverPoint objects.
    /// </summary>
    public class CoverPointComparer : IEqualityComparer<CoverPoint>
    {
        public bool Equals(CoverPoint v1, CoverPoint v2)
        {
            return v1.Equals(v2);
        }

        public int GetHashCode(CoverPoint v)
        {
            return v.GetHashCode();
        }
    }

    /// <summary>
    /// Compares Vector3 objects based on their newPosition.
    /// </summary>
    public class Vector3PositionComparer : IEqualityComparer<Vector3>
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

    public class CoverPointDistanceComparer : IComparer<CoverPoint>
    {
        private CoverPoint target;

        public CoverPointDistanceComparer(CoverPoint distanceToTarget)
        {
            target = distanceToTarget;
        }

        public int Compare(CoverPoint a, CoverPoint b)
        {
            var targetPosition = target.Position;
            return Vector3.Distance(a.Position, targetPosition).CompareTo(Vector3.Distance(b.Position, targetPosition));
        }
    }

    public static class CoverPointSorter
    {
        public static List<CoverPoint> SortByDistance(List<CoverPoint> coverPoints, Vector3 targetPosition)
        {
            return coverPoints.OrderBy(cp => Vector3.Distance(cp.Position, targetPosition)).ToList();
        }
    }
}
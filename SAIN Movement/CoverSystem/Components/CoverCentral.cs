using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Movement.Classes;
using SAIN_Helpers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Movement.Classes.ConstantValues;
using static Movement.Classes.HelperClasses;
using static Movement.UserSettings.Debug;

namespace Movement.Components
{
    [BepInPlugin("com.solarint.unicover", "A Universal Cover System", "0.1")]
    public class Plugin : BaseUnityPlugin
    {
        private bool DebugMode => DebugCoverComponent.Value;
        private GameObject CoverObject;
        private CoverCentralComponent Central;

        private void Awake()
        {
            Directory.CreateDirectory(CoverFolder);
            Logger.LogDebug($"Universal Cover System Loaded");
        }

        public static string PluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string CoverFolder = Path.Combine(PluginFolder, "CoverPoints");

        private void Update()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld == null || gameWorld.RegisteredPlayers == null)
            {
                if (Central != null)
                {
                    if (Central.FinalCoverPoints.Count > 0)
                    {
                        SaveFinalCoverPoints();

                        Central.FinalCoverPoints.Clear();
                    }

                    if (Central.CoverFinderDictionary.Count > 0)
                    {
                        Central.CoverFinderDictionary.Clear();
                    }

                    if (Central.PointGeneratorDictionary.Count > 0)
                    {
                        Central.PointGeneratorDictionary.Clear();
                    }
                }
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

        private void SaveFinalCoverPoints()
        {
            Logger.LogDebug($"Starting Save Process");

            // Convert the list of CoverPoint objects to JSON
            string json = Central.FinalCoverPoints.ToJson();

            string LevelName = Singleton<GameWorld>.Instance.LocationId.ToString();

            // Set the file path where you want to save the JSON
            string filePath = CoverFolder + LevelName + "_CoverPoints.json";

            // Write the JSON string to the file
            File.WriteAllText(filePath, json);

            Logger.LogWarning($"Saved CoverPoints to JSON");
        }

        private float Frequency = 0f;
    }

    public class CoverCentralComponent : MonoBehaviour
    {
        private bool DebugMode => DebugCoverComponent.Value;

        public List<CoverPoint> FinalCoverPoints = new List<CoverPoint>();
        private List<Vector3> PointsToCheck = new List<Vector3>();
        public List<Vector3> InputPoints = new List<Vector3>();
        public Dictionary<Player, PointGenerator> PointGeneratorDictionary = new Dictionary<Player, PointGenerator>();
        public Dictionary<Player, CoverFinderComponent> CoverFinderDictionary = new Dictionary<Player, CoverFinderComponent>();

        private void Start()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);

            Logger.LogDebug($"Central Controller Loaded");

            StartCoroutine(CheckPointsForCover());
        }

        float GeneratorTickTimer = 0f;
        private void Update()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.RegisteredPlayers == null)
            {
                return;
            }

            if (UpdateTimer < Time.time)
            {
                UpdateTimer = Time.time + 1f;

                foreach (Player player in gameWorld.RegisteredPlayers)
                {
                    if (player.HealthController.IsAlive)
                    {
                        if (!PointGeneratorDictionary.Keys.Contains(player))
                        {
                            Logger.LogWarning($"Added Point Generator for {player.name}");
                            PointGeneratorDictionary.Add(player, new PointGenerator(player));
                        }

                        if (!player.IsYourPlayer)
                        {
                            if (!CoverFinderDictionary.Keys.Contains(player))
                            {
                                Logger.LogWarning($"Added CoverFinder for {player.name}");
                                CoverFinderDictionary.Add(player, player.gameObject.AddComponent<CoverFinderComponent>());
                            }

                            if (CoverFinderDictionary.TryGetValue(player, out var component))
                            {
                                component.FinalCoverPoints = FinalCoverPoints;
                            }
                            else
                            {
                                Logger.LogError($"Cant get Value for CoverFinder Dictionary");
                            }
                        }
                    }
                }
            }

            if (FinalCoverPoints.Count > 0)
            {
                var point = FinalCoverPoints.PickRandom();
                DebugDrawer.Sphere(point.Position, 0.3f, Color.blue, 1f);
            }

            if (PointGeneratorDictionary.Count > 0 && GeneratorTickTimer < Time.time)
            {
                GeneratorTickTimer = Time.time + 0.25f;

                List<Vector3> points = new List<Vector3>();

                foreach (var generator in PointGeneratorDictionary.Values)
                {
                    if (generator == null)
                    {
                        Logger.LogError($"Point Generator is null");
                        continue;
                    }

                    generator.ManualUpdate();
                    points.AddRange(generator.OutputPoints);
                    generator.OutputPoints.Clear();
                }

                StartChecks(points);
            }

            DrawDebug();
        }

        private float UpdateTimer = 0;
        public const float CoverRatio = 0.33f;

        private void StartChecks(List<Vector3> points)
        {
            if (points.Count > 0)
            {
                var filtered = FilterDistance(points, 1f);

                Logger.LogDebug($"Checking [{filtered.Count}] points for cover");

                PointsToCheck.AddRange(filtered);
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
                        yield return new WaitForEndOfFrame();

                        if (FinalCoverPoints.Count > 0 && CheckDistanceToExisting(FinalCoverPoints, points[i], 2f))
                        {
                            i++;
                            continue;
                        }

                        if (CheckForCover(points[i], out CoverPoint goodCover))
                        {
                            coverPoints.Add(goodCover);
                        }

                        i++;
                    }

                    FinalCoverPoints.AddRange(coverPoints);
                }

                yield return new WaitForSeconds(1f);
            }
        }

        private bool CheckForCover(Vector3 point, out CoverPoint cover)
        {
            cover = null;

            // Copies the points and then raises it so its not blocked by small ground objects
            Vector3 rayPoint = point;
            rayPoint.y += RayYOffset;

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
                        point -= hitDirection.normalized / PointDistanceDivideBy;
                    }

                    if (CheckHeightAndPercent(new CoverPoint(point, direction), out CoverPoint goodCover))
                    {
                        cover = goodCover;
                        return true;
                    }
                }
            }

            return false;
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
}
using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Movement.Classes;
using Newtonsoft.Json;
using SAIN_Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Movement.Classes.ConstantValues;
using static Movement.UserSettings.DebugConfig;
using static Movement.UserSettings.CoverSystemConfig;
using BepInEx.Configuration;

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
                LoadTimer = Time.time + 10f;
                Loaded = false;
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

            if (LoadTimer < Time.time)
            {
                if (LoadedPoints.Count == 0 && Singleton<GameWorld>.Instance?.MainPlayer?.Location != null && !Loaded)
                {
                    LoadPoints(CurrentMap);

                    Logger.LogInfo($"Loaded {LoadedPoints.Count} CoverPoints");
                }

                if (Central != null && !Loaded && LoadedPoints.Count > 0)
                {
                    SaveTimer = Time.time + 5f;

                    Loaded = true;

                    Central.FinalCoverPoints.AddRange(LoadedPoints);

                    Logger.LogInfo($"Added {Central.FinalCoverPoints.Count} CoverPoints to Central List");

                    foreach (var point in LoadedPoints)
                    {
                        Color color;

                        if (point.percent > 0.75)
                        {
                            color = Color.blue;
                        }
                        else if (point.percent > 0.5)
                        {
                            color = Color.yellow;
                        }
                        else
                        {
                            color = Color.red;
                        }

                        Ray(point.position, Vector3.up, point.height, 0.1f, color);
                    }

                    LoadedPoints.Clear();
                }

                if (Central != null && Central.FinalCoverPoints.Count > 0 && SaveTimer < Time.time && SaveCoverPoints.Value)
                {
                    SaveTimer = Time.time + 10f;
                    SaveFinalCoverPoints();
                }
            }
        }

        public static GameObject Ray(Vector3 startPoint, Vector3 direction, float length, float lineWidth, Color color)
        {
            var rayObject = new GameObject();
            var lineRenderer = rayObject.AddComponent<LineRenderer>();

            // Set the color and width of the line
            lineRenderer.material.color = color;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;

            // Set the start and end points of the line to draw a ray
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, startPoint + direction.normalized * length);

            return rayObject;
        }

        private float LoadTimer = 0;
        private bool Loaded = false;
        public List<CoverPoint> LoadedPoints = new List<CoverPoint>();

        private void LoadPoints(string mapName)
        {
            // Construct the filename based on the map name
            string fileName = mapName + "_CoverPoints.json";
            string filePath = Path.Combine(CoverFolder, fileName);

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                //Logger.LogWarning($"File {filePath} does not exist");
                return;
            }

            // Read the file and deserialize the points
            string json = File.ReadAllText(filePath);
            LoadedPoints = JsonConvert.DeserializeObject<List<CoverPoint>>(json);
        }

        private float SaveTimer = 0f;

        private void SaveFinalCoverPoints()
        {
            // Convert the list of CoverPoint objects to JSON
            string json = Central.FinalCoverPoints.ToJson();

            var gameWorld = Singleton<GameWorld>.Instance;
            string mapName = gameWorld.MainPlayer.Location.ToLower();

            // Set the file name where you want to save the JSON
            string name = mapName + "_CoverPoints2.json";

            string path = Path.Combine(CoverFolder, name);

            // Write the JSON string to the file
            //File.WriteAllText(path, json);
            File.WriteAllText(path, json);

            Logger.LogWarning($"Saved CoverPoints to JSON");
        }

        private float Frequency = 0f;

        private string CurrentMap
        {
            get
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                return gameWorld.MainPlayer.Location.ToLower();
            }
        }
    }

    public class CoverCentralComponent : MonoBehaviour
    {
        private void Start()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

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
                            PointGeneratorDictionary.Add(player, new PointGenerator(player));
                        }

                        if (!player.IsYourPlayer)
                        {
                            if (!CoverFinderDictionary.Keys.Contains(player))
                            {
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

            if (PointGeneratorDictionary.Count > 0 && GeneratorTickTimer < Time.time)
            {
                GeneratorTickTimer = Time.time + 0.1f;

                List<Vector3> points = new List<Vector3>();

                foreach (var generator in PointGeneratorDictionary.Values)
                {
                    if (generator == null)
                    {
                        Logger.LogError($"Point Generator is null");
                        continue;
                    }

                    generator.ManualUpdate(MaxNavPaths, MaxNavCorners, MaxGenIter, GeneratorTimer, FilterDistance);

                    points.AddRange(generator.OutputPoints);
                    generator.OutputPoints.Clear();
                }

                InputPoints.AddRange(points);
            }

            CoverChecker.ProcessPoints(out List<CoverPoint> coverpoints, InputPoints, MaxBatchSize, Accuracy, FilterDistance);
            InputPoints.Clear();

            if (coverpoints != null && coverpoints.Count > 0)
            {
                foreach (var point in coverpoints)
                {
                    Color color;

                    if (point.percent > 0.75)
                    {
                        color = Color.blue;
                    }
                    else if (point.percent > 0.5)
                    {
                        color = Color.yellow;
                    }
                    else
                    {
                        color = Color.red;
                    }

                    Plugin.Ray(point.position, Vector3.up, point.height, 0.1f, color);

                    Vector3 pointpos = point.position;
                    pointpos.y += point.height;

                    Vector3 left = point.leftEdgeDirection;
                    Vector3 right = point.rightEdgeDirection;

                    Plugin.Ray(pointpos, left, left.magnitude, 0.05f, color);
                    Plugin.Ray(pointpos, right, right.magnitude, 0.05f, color);
                }

                FinalCoverPoints.AddRange(coverpoints);
            }

            DrawDebug();
        }

        private void DrawDebug()
        {
            if (DebugMode && DebugTimer < Time.time)
            {
                DebugTimer = Time.time + 10f;

                Logger.LogDebug($"Final Points = [{FinalCoverPoints.Count}]");
            }
        }

        private bool DebugMode => DebugCoverComponent.Value;
        private int Accuracy => RayCastAccuracy.Value;
        private int MaxNavCorners => MaxCorners.Value;
        private float GeneratorTimer => PointGeneratorTimer.Value;
        private int MaxBatchSize => CoverPointBatchSize.Value;
        private int MaxNavPaths => MaxPaths.Value;
        private int MaxGenIter => MaxRandomGenIterations.Value;
        private float FilterDistance => UserSettings.CoverSystemConfig.FilterDistance.Value;

        public List<CoverPoint> FinalCoverPoints = new List<CoverPoint>();
        public List<Vector3> InputPoints = new List<Vector3>();

        public Dictionary<Player, PointGenerator> PointGeneratorDictionary = new Dictionary<Player, PointGenerator>();
        public Dictionary<Player, CoverFinderComponent> CoverFinderDictionary = new Dictionary<Player, CoverFinderComponent>();
        private readonly CoverChecker CoverChecker = new CoverChecker();

        private float GeneratorTickTimer = 0f;
        private float UpdateTimer = 0;
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
        public CoverPoint(Vector3 coverPosition, Vector3 centerDirection)
        {
            position = coverPosition;
            this.centerDirection = centerDirection;
        }

        public Vector3 position { get; set; }
        public Vector3 centerDirection { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        public float percent { get; set; }
        public Vector3 leftEdgeDirection { get; set; }
        public Vector3 rightEdgeDirection { get; set; }
    }
}
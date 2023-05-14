using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Classes;
using SAIN.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.Classes.HelperClasses;
using static SAIN.UserSettings.CoverSystemConfig;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Components
{
    public class CoverCentralComponent : MonoBehaviour
    {
        private Player MainPlayer;
        public static readonly string FileName = "coverpoints";
        public static readonly string FolderName = "CoverPoints";

        private void Awake()
        {
            MainPlayer = GetComponent<Player>();
            PointGenerator = MainPlayer.gameObject.AddComponent<PointGenerator>();

            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);

            Draw = new DrawCoverPoints(Color.green, Color.blue, this.GetType().Name, true);
        }

        public static List<CoverPoint> FinalCoverPoints = new List<CoverPoint>();
        public static List<Vector3> InputPoints = new List<Vector3>();

        private PointGenerator PointGenerator;
        private DrawCoverPoints Draw;

        private void Update()
        {
            if (MainPlayer == null)
            {
                return;
            }

            if (!Loaded)
            {
                LoadFinalPoints();
                return;
            }

            FilterFinalPoints();

            Draw.DrawList(FinalCoverPoints, !ToggleDrawCoverPoints.Value);

            SavePointsToJson();
        }

        private int Count = 0;

        private void LoadFinalPoints()
        {
            Loaded = true;
            if (JsonUtility.LoadFromJson.GetSingle(out List<CoverPoint> coverPoints, FolderName, FileName, true))
            {
                Logger.LogInfo($"Loaded {coverPoints.Count} CoverPoints");

                FinalCoverPoints.AddRange(coverPoints);
            }
            else
            {
                Logger.LogDebug($"No Cover Points to load for this map");
            }
        }

        private void FilterFinalPoints()
        {
            if (StartFiltering.Value && Count != FinalCoverPoints.Count)
            {
                FinalCoverPoints = FilterCoverPointsByDistance(FinalCoverPoints, 0.5f);
                FinalCoverPoints = FilterNearbyPointsByDirection(FinalCoverPoints, 2f, 5f);
                Count = FinalCoverPoints.Count;
            }
        }

        private void SavePointsToJson()
        {
            if (FinalCoverPoints.Count > 0 && SaveTimer < Time.time && SaveCoverPoints.Value)
            {
                SaveTimer = Time.time + 120f;
                JsonUtility.SaveToJson.List(FinalCoverPoints, FolderName, FileName, true, false);
            }
        }

        public List<CoverPoint> FilterCoverPointsByDistance(List<CoverPoint> points, float minDistance)
        {
            List<CoverPoint> filteredPoints = new List<CoverPoint>();

            foreach (CoverPoint point in points)
            {
                bool isTooClose = false;

                foreach (CoverPoint otherPoint in filteredPoints)
                {
                    if (Vector3.Distance(point.Position, otherPoint.Position) < minDistance)
                    {
                        isTooClose = true;
                        break;
                    }
                }

                if (!isTooClose)
                {
                    filteredPoints.Add(point);
                }
            }

            return filteredPoints;
        }

        public List<CoverPoint> FilterNearbyPointsByDirection(List<CoverPoint> points, float minDistance, float minAngle)
        {
            List<CoverPoint> filteredPoints = new List<CoverPoint>();

            foreach (CoverPoint point in points)
            {
                bool isTooClose = false;

                foreach (CoverPoint otherPoint in filteredPoints)
                {
                    if (Vector3.Distance(point.Position, otherPoint.Position) < minDistance)
                    {
                        float angle = Vector3.Angle(point.CenterDirection, otherPoint.CenterDirection);
                        if (angle <= minAngle)
                        {
                            isTooClose = true;
                            break;
                        }
                    }
                }

                if (!isTooClose)
                {
                    filteredPoints.Add(point);
                }
            }

            return filteredPoints;
        }

        public List<CoverPoint> FilterCoverPointsByDistanceOct(List<CoverPoint> points, float minDistance)
        {
            GetNavMeshBounds();

            Vector3 size = maxBounds - minBounds;
            Octree<CoverPoint> octree = new Octree<CoverPoint>(minBounds.x, minBounds.y, minBounds.z, Mathf.Max(size.x, size.y, size.z));

            foreach (CoverPoint point in points)
            {
                Vector3 position = point.Position;

                List<CoverPoint> nearbyPoints = octree.Query(position.x - minDistance, position.y - minDistance, position.z - minDistance, minDistance * 2);

                if (nearbyPoints.All(nearbyPoint => Vector3.Distance(nearbyPoint.Position, position) >= minDistance))
                {
                    octree.Insert(point, coverPoint => coverPoint.Position);
                }
            }

            return octree.Query(minBounds.x, minBounds.y, minBounds.z, Mathf.Max(size.x, size.y, size.z));
        }

        private void GetNavMeshBounds()
        {
            NavMeshTriangulation navMeshTriangulation = NavMesh.CalculateTriangulation();

            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float minZ = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            float maxZ = float.NegativeInfinity;

            foreach (Vector3 vertex in navMeshTriangulation.vertices)
            {
                minX = Mathf.Min(minX, vertex.x);
                minY = Mathf.Min(minY, vertex.y);
                minZ = Mathf.Min(minZ, vertex.z);
                maxX = Mathf.Max(maxX, vertex.x);
                maxY = Mathf.Max(maxY, vertex.y);
                maxZ = Mathf.Max(maxZ, vertex.z);
            }

            minBounds = new Vector3(minX, minY, minZ);
            maxBounds = new Vector3(maxX, maxY, maxZ);
        }

        private void SaveBounds()
        {
            string bounds = GetPlayerBounds(30).ToJson();
            string boundsname = "PlayerBounds.json";
        }

        private static List<Vector3> GetPlayerBounds(int amount)
        {
            List<Vector3> bounds = new List<Vector3>();

            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld?.MainPlayer == null)
            {
                return null;
            }

            var player = gameWorld.MainPlayer;

            Bounds playerBounds = player.gameObject.GetComponent<Collider>().bounds;
            Vector3 size = playerBounds.size;
            Vector3 min = playerBounds.min;

            int subdivisions = Mathf.CeilToInt(Mathf.Pow(amount, 1f / 3f));

            for (int x = 0; x < subdivisions; x++)
            {
                for (int y = 0; y < subdivisions; y++)
                {
                    for (int z = 0; z < subdivisions; z++)
                    {
                        float lerpX = (float)x / (subdivisions - 1);
                        float lerpY = (float)y / (subdivisions - 1);
                        float lerpZ = (float)z / (subdivisions - 1);

                        Vector3 targetPoint = new Vector3(
                            min.x + size.x * lerpX,
                            min.y + size.y * lerpY,
                            min.z + size.z * lerpZ
                        );

                        bounds.Add(targetPoint);
                    }
                }
            }

            return bounds;
        }

        private bool DebugMode => DebugCoverComponent.Value;

        private float FilterDistance => UserSettings.CoverSystemConfig.FilterDistance.Value;

        private bool Loaded = false;

        private List<Vector3> PlayerBounds = new List<Vector3>();

        public Dictionary<BotOwner, CoverFinderComponent> CoverFinderDictionary = new Dictionary<BotOwner, CoverFinderComponent>();

        private readonly CoverPointGenerator CoverChecker = new CoverPointGenerator();

        private float SaveTimer = 0f;
        private Vector3 minBounds;
        private Vector3 maxBounds;
        protected ManualLogSource Logger;

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }
    }

    public class CoverPoint
    {
        public CoverPoint(Vector3 coverPosition, Vector3 direction)
        {
            Position = coverPosition;
            CenterDirection = direction;
        }

        public Vector3 Position { get; set; }
        public Vector3 CenterDirection { get; set; }
        public Vector3 LeftEdgeDirection { get; set; }
        public Vector3 RightEdgeDirection { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Ratio { get; set; }
    }
}
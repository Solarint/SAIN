using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Classes;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.CoverSystemConfig;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Components
{
    public static class CoverCentral
    {
        public static List<Vector3> ValidVertices = new List<Vector3>();
        public static List<CoverPoint> FinalCoverPoints = new List<CoverPoint>();
        public static List<Vector3> InputPoints = new List<Vector3>();
        public static Vector3[] NavmeshVertices;

        public static readonly string FileName = "coverpoints";
        public static readonly string FolderName = "CoverPoints";

        private static readonly string NavMeshFolder = "NavMesh";
        private static readonly string generatedPointsFile = "generatedpoints";

        private static bool NavMeshLoaded = false;
        public static void Update()
        {
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource("CoverCentral");
            }
            if (Draw == null)
            {
                Draw = new DrawCoverPoints(Color.green, Color.blue);
            }
            if (!NavMeshLoaded)
            {
                NavMeshLoaded = true;
                NavmeshVertices = NavMesh.CalculateTriangulation().vertices;
                Logger.LogWarning($"Total Vertices for this map: {NavmeshVertices.Length}");
            }

            if (!Loaded)
            {
                Loaded = true;
                FinalPoints.LoadFinalPoints();
                return;
            }

            FinalPoints.FilterFinalPoints();

            Draw.DrawList(FinalCoverPoints, !ToggleDrawCoverPoints.Value);

            FinalPoints.SavePointsToJson();
        }

        private static bool Loaded = false;
        private static DrawCoverPoints Draw;
        private static ManualLogSource Logger;

        public static class FinalPoints
        {
            public static void LoadFinalPoints()
            {
                if (JsonUtility.LoadFromJson.GetSingle(out List<CoverPoint> coverPoints, FolderName, FileName, true))
                {
                    System.Console.WriteLine($"Loaded {coverPoints.Count} CoverPoints");

                    FinalCoverPoints.AddRange(coverPoints);
                }
            }

            public static void FilterFinalPoints()
            {
                if (StartFiltering.Value && FilterPointCount != FinalCoverPoints.Count)
                {
                    FinalCoverPoints = FilterCoverPointsByDistance(FinalCoverPoints, 0.5f);
                    FinalCoverPoints = FilterNearbyPointsByDirection(FinalCoverPoints, 2f, 5f);
                    FilterPointCount = FinalCoverPoints.Count;
                }
            }

            private static int FilterPointCount = 0;

            public static void SavePointsToJson()
            {
                if (FinalCoverPoints.Count > 0 && SaveCoverPoints.Value && PointCount != FinalCoverPoints.Count)
                {
                    JsonUtility.SaveToJson.List(FinalCoverPoints, FolderName, FileName, true, false);
                    PointCount = FinalCoverPoints.Count;
                }
            }

            private static int PointCount = 0;

            public static List<CoverPoint> FilterCoverPointsByDistance(List<CoverPoint> points, float minDistance)
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

            public static List<CoverPoint> FilterNearbyPointsByDirection(List<CoverPoint> points, float minDistance, float minAngle)
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
        }
    }

    public class CoverPoint
    {
        public CoverPoint(Vector3 coverPosition, Vector3 direction)
        {
            Position = coverPosition;
            CenterDirection = direction;
        }

        public Vector3 Position { get; set; } = Vector3.zero;
        public Vector3 CenterDirection { get; set; } = Vector3.zero;
        public Vector3 LeftEdgeDirection { get; set; } = Vector3.zero;
        public Vector3 RightEdgeDirection { get; set; } = Vector3.zero;
        public float Width { get; set; } = 0f;
        public float Height { get; set; } = 0f;
        public float CoverLevel { get; set; } = 0f;
        public float CoverAngle { get; set; } = 0f;
    }
}
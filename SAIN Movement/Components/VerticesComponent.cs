using BepInEx.Logging;
using Comfort.Common;
using EFT;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.CoverSystemConfig;

namespace SAIN.Classes
{
    public class PointGenerator : MonoBehaviour
    {
        private void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        private readonly string NavMeshFolder = "NavMesh";
        private readonly string VerticesFile = "vertices";
        private readonly string generatedPointsFile = "generatedpoints";

        public const float FloatingCheck = 0.25f;
        public const float HeightCheck = 1.5f;

        private void CheckForExistingPoints()
        {

            if (JsonUtility.LoadFromJson.GetSingle(out List<Vector3> generatedPoints, NavMeshFolder, generatedPointsFile, true))
            {
                Logger.LogDebug($"Loaded Saved Generated Points. Found [{generatedPoints.Count}] for this map.");

                OutputPoints.AddRange(generatedPoints);
            }
            else
            {
                Logger.LogDebug($"No Saved Points for this Map.");

                CheckForExistingVertices();

                StartGeneration = true;
            }
        }

        private void CheckForExistingVertices()
        {
            if (JsonUtility.LoadFromJson.GetSingle(out List<Vector3> vertices, NavMeshFolder, VerticesFile, true))
            {
                Logger.LogDebug($"Loaded Saved Vertices. Found [{vertices.Count}] for this map.");

                NavMeshVertices.AddRange(vertices);
            }
            else
            {
                var triang = NavMesh.CalculateTriangulation();

                RawVertices = triang.vertices.ToList();

                Logger.LogDebug($"Found [{RawVertices.Count}] Raw Vertices for this map.");
            }
        }

        private bool StartGeneration = false;

        private List<Vector3> RawVertices = new List<Vector3>();

        private bool Started = false;
        private bool Resetting = false;

        private void Update()
        {
            if (!Singleton<IBotGame>.Instantiated)
            {
                return;
            }

            if (ResetGenerator.Value)
            {
                Resetting = true;
                StartGeneration = true;

                RawVertices = NavMesh.CalculateTriangulation().vertices.ToList();

                Logger.LogDebug($"Found [{RawVertices.Count}] Raw Vertices for this map.");

                return;
            }

            if (!Started && TogglePointGenerator.Value && !Resetting)
            {
                CheckForExistingPoints();
                Started = true;
            }

            if (StartGeneration && TogglePointGenerator.Value && Started)
            {
                // Go through the list of navmesh Vertices from triangulation and make sure they can all be moved to by bots.
                if (CheckVertices(CheckVertBatchSize.Value))
                {
                    // Generate Points until the list is empty, then it will return true
                    if (GeneratePoints(MaxGeneratorBatchSize.Value, NavMeshSampleRange.Value, MaxRandomGenIterations.Value, 0.25f))
                    {
                        // Filter the points in batches until max number of times has passed, then it will return true
                        if (FilterGenPoints.RelativeDistanceFilter(GenPoints))
                        {
                            // Verify Points until the list is empty, then it will reutrn true;
                            if (VerifyPoints(MaxVerificationBatch.Value) && !FinalSave)
                            {
                                FinalSave = true;
                                OutputPoints.AddRange(VerifiedPoints);

                                // Save our final list
                                SaveList(VerifiedPoints, NavMeshFolder, generatedPointsFile);

                                VerifiedPoints.Clear();
                            }
                        }
                    }
                    if (DebugTimer < Time.time && !FinalSave)
                    {
                        DebugTimer = Time.time + DebugFrequency;
                        Logger.LogDebug($"SaveVerified [{VerifiedPoints.Count}]. GenPoints [{GenPoints.Count}]. NavMeshVertices [{NavMeshVertices.Count}].");
                    }
                }
            }
        }

        private FilterVector3 FilterGenPoints = new FilterVector3(100, 500);
        private FilterVector3 FilterVertices = new FilterVector3(100, 500);

        private bool Saved = false;
        private bool FinalSave = false;

        private void SaveList(List<Vector3> list, string folder, string filename)
        {
            if (list.Count > 0)
            {
                JsonUtility.SaveToJson.List(list, folder, filename, true);
                Logger.LogWarning($"Saved [{list.Count}] Points!");
            }
        }

        private List<Vector3> SaveVerts = new List<Vector3>();
        public List<Vector3> NavMeshVertices = new List<Vector3>();
        private List<Vector3> GenPoints = new List<Vector3>();
        public List<Vector3> OutputPoints = new List<Vector3>();
        private List<Vector3> VerifiedPoints = new List<Vector3>();

        private bool CheckVertices(int maxBatch)
        {
            if (RawVertices.Count > 0)
            {
                if (!FilterVertices.RelativeDistanceFilter(RawVertices, 2f))
                {
                    return false;
                }

                var batch = GetBatchFromList(RawVertices, maxBatch);

                List<Vector3> ValidVerts = new List<Vector3>();

                foreach (var v in batch)
                {
                    foreach (var player in Singleton<GameWorld>.Instance.RegisteredPlayers)
                    {
                        NavMeshPath path = new NavMeshPath();
                        if (NavMesh.CalculatePath(player.Transform.position, v, -1, path) && path.status == NavMeshPathStatus.PathComplete)
                        {
                            ValidVerts.Add(v);
                            break;
                        }
                    }
                }

                if (DebugTimer < Time.time)
                {
                    Logger.LogDebug($"Valid NavMesh Vertices Count: [{NavMeshVertices.Count}] with Remaing: [{RawVertices.Count}].");
                }

                NavMeshVertices.AddRange(ValidVerts);
                SaveVerts.AddRange(ValidVerts);

                return false;
            }
            else if (!Saved && SaveVerts.Count > 0)
            {
                Saved = true;

                Logger.LogDebug($"Total Vertices Output [{SaveVerts.Count}]");

                JsonUtility.SaveToJson.List(SaveVerts, NavMeshFolder, VerticesFile, true);

                SaveVerts.Clear();
            }

            return true;
        }

        private bool GeneratePoints(int maxBatch = 20, float sampleRange = 2f, int maxPoints = 10, float minDistance = 0.25f)
        {
            if (NavMeshVertices.Count > 0)
            {
                var batch = GetBatchFromList(NavMeshVertices, maxBatch);
                List<Vector3> outputPoints = new List<Vector3>();
                foreach (var point in batch)
                {
                    List<Vector3> genPoints = new List<Vector3>();
                    int i = 0;
                    while (i < maxPoints)
                    {
                        // Generate a random point
                        Vector3 randomPoint = Random.insideUnitSphere * 10f;
                        randomPoint.y = Random.Range(-1f, 1f);
                        // Add that point to the start point
                        randomPoint += point;

                        // Sample that point to see if its on navmesh
                        if (NavMesh.SamplePosition(randomPoint, out var hit, sampleRange, NavMesh.AllAreas))
                        {
                            foreach (var player in Singleton<GameWorld>.Instance.RegisteredPlayers)
                            {
                                NavMeshPath path = new NavMeshPath();
                                if (NavMesh.CalculatePath(player.Transform.position, hit.position, -1, path) && path.status == NavMeshPathStatus.PathComplete)
                                {
                                    genPoints.Add(hit.position);
                                    break;
                                }
                            }
                        }
                        i++;
                    }
                    outputPoints.AddRange(genPoints);
                }

                FilterList(outputPoints, minDistance);

                if (DebugTimer < Time.time) Logger.LogDebug($"Found {outputPoints.Count} generated points in batch");

                GenPoints.AddRange(outputPoints);

                return false;
            }

            return true;
        }

        private bool VerifyPoints(int maxGenerationBatch)
        {
            if (GenPoints.Count > 0 && NavMeshVertices.Count == 0)
            {
                var batch = GetBatchFromList(GenPoints, maxGenerationBatch);

                List<Vector3> verified = new List<Vector3>();
                foreach (var point in batch)
                {
                    // CheckForCalcPath to make sure the points isn't floating.
                    if (Physics.Raycast(point, Vector3.down, FloatingCheck, Mask))
                    {
                        // CheckForCalcPath to make sure the points isn't under something
                        if (!Physics.Raycast(point, Vector3.up, HeightCheck, Mask))
                        {
                            verified.Add(point);
                        }
                    }
                }

                if (DebugTimer < Time.time) Logger.LogDebug($"Found {verified.Count} verified points in batch");

                VerifiedPoints.AddRange(verified);
                return false;
            }
            return true;
        }

        private void FilterList(List<Vector3> positions, float min = 0.25f)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                for (int j = i + 1; j < positions.Count; j++)
                {
                    if (Vector3.Distance(positions[i], positions[j]) < min)
                    {
                        positions.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        private List<Vector3> GetBatchFromList(List<Vector3> list, int size)
        {
            int max = Mathf.Clamp(list.Count, 0, size);

            var batch = list.GetRange(0, max);

            list.RemoveRange(0, max);

            return batch;
        }

        private LayerMask Mask = LayerMaskClass.HighPolyWithTerrainNoGrassMask;
        protected ManualLogSource Logger;
        private float DebugTimer = 0f;
        private float DebugFrequency = 5f;
    }

    public class FilterVector3
    {
        public FilterVector3(int maxBatch, int maxIterations)
        {
            MaxIterations = maxIterations;
            MaxBatches = maxBatch;
        }

        public bool RelativeDistanceFilter(List<Vector3> list, float minDistance = 0.25f)
        {
            if (filterTimes >= MaxIterations)
            {
                return true;
            }

            if (list.Count > 0 && filterTimes < MaxIterations)
            {
                filterTimes++;

                if (startCount == 0)
                {
                    startCount = list.Count;
                }

                list.Randomize();
                int number = startCount / MaxBatches;
                var batch = GetBatchFromList(list, number);
                FilterList(batch, minDistance);
                list.AddRange(batch);
            }

            return false;
        }

        private void FilterList(List<Vector3> positions, float min = 0.25f)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                for (int j = i + 1; j < positions.Count; j++)
                {
                    if (Vector3.Distance(positions[i], positions[j]) < min)
                    {
                        positions.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        private List<Vector3> GetBatchFromList(List<Vector3> list, int size)
        {
            int max = Mathf.Clamp(list.Count, 0, size);

            var batch = list.GetRange(0, max);

            list.RemoveRange(0, max);

            return batch;
        }

        private int filterTimes = 0;
        private int MaxIterations;
        private int MaxBatches;
        private int startCount = 0;
    }
}
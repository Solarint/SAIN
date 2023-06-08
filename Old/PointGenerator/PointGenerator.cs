using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.UI.Map;
using SAIN.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.Classes.HelperClasses;
using static SAIN.UserSettings.CoverSystemConfig;

namespace SAIN.Classes
{
    public class PointGenerator : MonoBehaviour
    {
        private void Awake()
        {
            Player = GetComponent<Player>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        private Player Player;

        private readonly string NavMeshFolder = "NavMesh";
        private readonly string generatedPointsFile = "generatedpoints";

        public const float FloatingCheck = 0.1f;
        public const float HeightCheck = 1.5f;

        private void Update()
        {
            if (!Singleton<IBotGame>.Instantiated)
            {
                return;
            }

            if (!ListCreated && AddNewVertices.Value && CoverCentral.NavmeshVertices.Length > 0)
            {
                ListCreated = true;
                PersonalVertices = CoverCentral.NavmeshVertices.ToList();
                PersonalVertices.Randomize();
            }

            if (PersonalVertices.Count > 0)
            {
                CheckVertices(CheckVertBatchSize.Value);

                if (TogglePointGenerator.Value)
                {
                    GeneratePoints(MaxGeneratorBatchSize.Value, MaxRandomGenIterations.Value);

                    VerifyPoints(MaxVerificationBatch.Value);

                    if (GenPoints.Count == 0 && VerifiedPoints.Count > 0)
                    {
                        OutputPoints.AddRange(VerifiedPoints);

                        SaveList(VerifiedPoints, NavMeshFolder, generatedPointsFile);

                        Logger.LogWarning($"Finished Generation!");

                        return;
                    }
                }
                if (DebugTimer < Time.time)
                {
                    DebugTimer = Time.time + 1f;
                    Logger.LogDebug($"SaveVerified [{VerifiedPoints.Count}]. GenPoints [{GenPoints.Count}]. CoverCentral.ValidVertices [{CoverCentral.ValidVertices.Count}].");
                }
            }
        }

        private void SaveList(List<Vector3> list, string folder, string filename)
        {
            if (list.Count > 0)
            {
                JsonUtility.SaveToJson.List(list, folder, filename, true);
                Logger.LogWarning($"Saved [{list.Count}] Points!");
            }
        }

        private List<Vector3> PersonalVertices = new List<Vector3>();

        private List<Vector3> GenPoints = new List<Vector3>();
        public List<Vector3> OutputPoints = new List<Vector3>();
        private List<Vector3> VerifiedPoints = new List<Vector3>();
        private bool ListCreated = false;

        private void CheckVertices(int maxBatch)
        {
            if (PersonalVertices.Count > 0)
            {
                List<Vector3> ValidVerts = new List<Vector3>();
                var batch = GetBatchFromList(PersonalVertices, maxBatch);

                foreach (var point in batch)
                {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(Player.Transform.position, point, -1, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        ValidVerts.Add(point);
                    }
                }

                var distinct = ValidVerts.Except(CoverCentral.ValidVertices, new Vector3PositionComparer());
                CoverCentral.ValidVertices.AddRange(distinct);
            }
        }

        private void GeneratePoints(int maxBatch = 20, int maxPoints = 10)
        {
            if (CoverCentral.ValidVertices.Count > 0)
            {
                var batch = GetBatchFromList(CoverCentral.ValidVertices, maxBatch);

                var genPoints = Generate(batch, maxPoints);

                FilterList(genPoints, 1f);

                var NavChecked = CheckPaths(genPoints);

                if (DebugTimer < Time.time) Logger.LogDebug($"Found {NavChecked.Count} generated points in batch");

                GenPoints.AddRange(NavChecked);
            }
        }

        private List<Vector3> Generate(List<Vector3> points, int maxPoints, float maxRange = 10f, float minDistance = 1f, float maxY = 0f)
        {
            List<Vector3> randomPoints = new List<Vector3>();
            foreach (var point in points)
            {
                List<Vector3> genPoints = new List<Vector3>();
                int i = 0;
                while (i < maxPoints)
                {
                    // Generate a random point
                    Vector3 randomPoint = Random.insideUnitSphere * maxRange;
                    randomPoint.y = maxY;

                    // Add that point to the start point
                    randomPoint += point;

                    genPoints.Add(randomPoint);

                    i++;
                }

                FilterList(genPoints, minDistance);
                randomPoints.AddRange(genPoints);
            }

            return randomPoints;
        }

        private List<Vector3> CheckPaths(List<Vector3> points)
        {
            List<Vector3> checkedPoints = new List<Vector3>();
            foreach (var point in points)
            {
                // Sample that point to see if its on navmesh
                if (NavMesh.SamplePosition(point, out var hit, 5f, NavMesh.AllAreas))
                {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(Player.Transform.position, hit.position, -1, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        checkedPoints.Add(hit.position);
                    }
                }
            }
            return checkedPoints;
        }

        private bool VerifyPoints(int maxGenerationBatch)
        {
            if (GenPoints.Count > 0)
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
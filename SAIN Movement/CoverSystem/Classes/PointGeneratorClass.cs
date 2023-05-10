using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static Movement.Classes.ConstantValues;
using static Movement.Classes.HelperClasses;
using static Movement.UserSettings.Debug;

namespace Movement.Classes
{
    public class PointGenerator
    {
        public PointGenerator(Player player)
        {
            Player = player;

            CheckStationary = new CheckIfPlayerStationary(player, 5);

            PlayerColor = RandomColor;

            string name = player.IsYourPlayer ? player.Profile.Nickname : player.name;
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name + $": {name}: ");
        }

        private float RandomTimer => Random.Range(1.25f, 2.25f);

        /// <summary>
        /// Generates paths, Find Points from those paths, generating random points around those start points, and then verifying points to make sure they are valid places a player can walk to.
        /// </summary>
        public void ManualUpdate()
        {
            if (!Player.HealthController.IsAlive)
            {
                return;
            }

            if (PathGenTimer < Time.time && CheckStationary.CheckForCalcPath())
            {
                PathGenTimer = Time.time + RandomTimer;

                Paths.AddRange(GeneratePaths(Player.Transform.position));
            }

            if (StartPointTimer < Time.time && Paths.Count > 0)
            {
                StartPointTimer = Time.time + RandomTimer;

                var Points = FindPointsFromPaths(Paths);

                Paths.Clear();

                StartPoints.AddRange(Points);
            }

            if (PointGenTimer < Time.time && StartPoints.Count > 0)
            {
                PointGenTimer = Time.time + RandomTimer;

                foreach (Vector3 point in StartPoints)
                {
                    GenPoints.AddRange(GeneratePoints(point));
                }

                StartPoints.Clear();

                GenPoints = FilterDistance(GenPoints);
            }

            if (VerifyPointTimer < Time.time && GenPoints.Count > 0)
            {
                VerifyPointTimer = Time.time + RandomTimer;

                List<Vector3> verifiedPoints = new List<Vector3>();

                foreach (var point in GenPoints)
                {
                    if (VerifyPoint(point))
                    {
                        verifiedPoints.Add(point);
                    }
                }

                GenPoints.Clear();

                OutputPoints.AddRange(verifiedPoints);
            }
        }

        private float PathGenTimer = 0;
        private List<NavMeshPath> Paths = new List<NavMeshPath>();

        private float StartPointTimer = 0;
        private List<Vector3> StartPoints = new List<Vector3>();

        private float PointGenTimer = 0;
        private List<Vector3> GenPoints = new List<Vector3>();

        private float VerifyPointTimer = 0;
        public List<Vector3> OutputPoints = new List<Vector3>();

        /// <summary>
        /// Generates a list of random points around the Player position and checks them against the NavMesh for valid newPoints.
        /// </summary>
        /// <param name="radius">The radius around the Player position to generate random points.</param>
        /// <param name="playerPosition">The Player position to generate random points around.</param>
        private List<NavMeshPath> GeneratePaths(Vector3 startPosition)
        {
            int i = 0;
            List<NavMeshPath> Paths = new List<NavMeshPath>();
            while (i < MaxRangeIterations)
            {
                // Find a random points in a sphere around the Player position.
                Vector3 randomPoint = Random.onUnitSphere * IncreaseRange(i);
                randomPoint.y = RandomAngle;
                // Add the Player position to our random points, so that is it centered around them.
                randomPoint += startPosition;

                // Sample the position on the navmesh to find a hit.
                if (NavMesh.SamplePosition(randomPoint, out var hit, SamplePositionRange, NavMesh.AllAreas))
                {
                    // Calculate a botPath to that hit, and make sure there a complete botPath from the Player position.
                    NavMeshPath newPath = new NavMeshPath();
                    if (NavMesh.CalculatePath(startPosition, hit.position, -1, newPath) && newPath.status == NavMeshPathStatus.PathComplete)
                    {
                        // Add this path to our list
                        Paths.Add(newPath);
                    }
                    if (Paths.Count >= MaxPaths)
                    {
                        // Break the loop if we've reached our max paths per batch
                        break;
                    }
                }
                i++;
            }

            return Paths;
        }

        /// <summary>
        /// Finds a list of Vector3 points from a list of NavMeshPaths, filtering them by distance.
        /// </summary>
        /// <param name="paths">The list of NavMeshPaths to find points from.</param>
        /// <param name="maxCornerBatch">The maximum number of points to return.</param>
        /// <returns>A list of Vector3 points from the NavMeshPaths.</returns>
        private List<Vector3> FindPointsFromPaths(List<NavMeshPath> paths, int maxCornerBatch = 20)
        {
            List<Vector3> corners = new List<Vector3>();

            // Add all the corners from the NavMeshPaths to a single list.
            foreach (NavMeshPath path in paths)
            {
                corners.AddRange(path.corners);
            }

            int i = 0;
            const int max = 10;

            // Loop through the list to filter the vector3's by distance until we've reached our max input size, or we've hit 10 iterations of the loop.
            int cornersCount = corners.Count;
            while (cornersCount > maxCornerBatch)
            {
                // If We've reached out iteration limit, break the loop
                if (i >= max)
                {
                    break;
                }

                // Increase the minimum distance between corners until we've reached out input maxCornerBatch size;
                corners = FilterDistance(corners, 1f + (i * 0.5f));
                cornersCount = corners.Count;

                i++;
            }

            // If we're still over the max batch size after the loop. Sort the corners by their distance to the player position, and remove anything over 20;
            if (corners.Count > maxCornerBatch)
            {
                corners = Vector3Sorter.SortByDistance(corners, Player.Transform.position);
                corners.RemoveRange(maxCornerBatch, corners.Count - maxCornerBatch);
            }

            return corners;
        }

        /// <summary>
        /// Generates random points within a given range and filters out points that are too closePoints to one another.
        /// </summary>
        private List<Vector3> GeneratePoints(Vector3 navPoint)
        {
            List<Vector3> genPoints = new List<Vector3>();
            int i = 0;
            while (i < MaxRandomGen)
            {
                // Generate a random point
                Vector3 randomPoint = Random.insideUnitCircle * Random.Range(RandomGenMin, RandomGenMax);
                // Add that point to the start point
                randomPoint += navPoint;

                // Sample that point to see if its on navmesh
                if (NavMesh.SamplePosition(randomPoint, out var hit, SamplePositionRange, NavMesh.AllAreas))
                {
                    // Add it to our list
                    genPoints.Add(hit.position);
                }
                i++;
            }

            // Filter out any points that are too closePoints to one another
            return FilterDistance(genPoints, FilterDistancePoints);
        }

        /// <summary>
        /// Verifies generated points by checking if they are floating or under something.
        /// </summary>
        private bool VerifyPoint(Vector3 point)
        {
            // CheckForCalcPath that the point has a complete botPath to the Player position.
            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(Player.Transform.position, point, -1, path) || path.status != NavMeshPathStatus.PathComplete)
            {
                return false;
            }

            // CheckForCalcPath to make sure the points isn't floating.
            if (Physics.Raycast(point, Vector3.down, FloatingCheck, Mask))
            {
                // CheckForCalcPath to make sure the points isn't under something
                if (!Physics.Raycast(point, Vector3.up, HeightCheck, Mask))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Filters a list of Vector3 positions by removing any positions that are within a certain distance of each other.
        /// </summary>
        /// <param name="oldPositions">The list of Vector3 positions to filter.</param>
        /// <param name="min">The minimum distance between two Vector3 positions.</param>
        /// <returns>A list of Vector3 positions with any positions that are within a certain distance of each other removed.</returns>
        private List<Vector3> FilterDistance(List<Vector3> oldPositions, float min = 0.5f)
        {
            List<Vector3> positions = oldPositions;
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

            return positions;
        }

        /// <summary>
        /// Increases the range for the botPath finder.
        /// </summary>
        /// <param name="i">The iteration number.</param>
        /// <returns>The increased range.</returns>
        private float IncreaseRange(int i)
        {
            // Set our range for the botPath finder.
            float range = i * i + RangeBase;
            float iterRatio = (float)i / MaxRangeIterations;
            if (iterRatio > RangeThreshB)
            {
                range *= 2.5f;
            }
            else if (iterRatio > RangeThreshA)
            {
                range *= 1.5f;
            }
            return range;
        }

        /// <summary>
        /// Generates a random angle between the negative and positive maximum Y angle.
        /// </summary>
        private float RandomAngle => Random.Range(-MaxYAngle, MaxYAngle);
        public static Color RandomColor => new Color(Random.value, Random.value, Random.value);
        public Color PlayerColor;
        private LayerMask Mask = LayerMaskClass.HighPolyWithTerrainMask;
        private Player Player;
        private CheckIfPlayerStationary CheckStationary;
        protected ManualLogSource Logger;
    }
}
using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Movement.Components.ConstantValues;
using static Movement.Components.HelperClasses;
using static Movement.UserSettings.Debug;

namespace Movement.Components
{
    public class CoverFinderComponent : MonoBehaviour
    {
        private bool DebugMode => DebugCoverComponent.Value;
        public List<CoverPoint> FinalCoverPoints = new List<CoverPoint>();
        public List<CoverPoint> CloseCoverPoints = new List<CoverPoint>();
        public List<Vector3> PointsToSend = new List<Vector3>();

        private void Start()
        {
            player = GetComponent<Player>();
            PlayerColor = RandomColor;

            string name = player.IsYourPlayer ? player.Profile.Nickname : player.name;
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name + $": {name}: ");

            StartCoroutine(PathGenerator());
            StartCoroutine(CoverFinder());

            CheckStationary = new CheckIfPlayerStationary(player);
        }

        private CoverCentralComponent CoverCentralComponent;
        private CheckIfPlayerStationary CheckStationary;

        private void Update()
        {
            if (CheckStationary == null)
            {
                Logger.LogError($"Check Stationary Component is null!");
                CheckStationary = new CheckIfPlayerStationary(player);
            }

            if (CoverCentralComponent == null)
            {
                var central = GameObject.Find("CoverCentralObject");
                if (central != null)
                {
                    var centralComponent = central.GetComponent<CoverCentralComponent>();
                    if (centralComponent != null)
                    {
                        CoverCentralComponent = centralComponent;
                    }
                    else
                    {
                        Logger.LogError($"Central Component Null");
                    }
                }
                else
                {
                    Logger.LogError($"Central GameObject Null");
                }
            }

            if (CoverCentralComponent == null)
            {
                Logger.LogError($"Central Component is STILL Null! oh no!");
            }
            else
            {
            }

            if (PointsToSend.Count > 0)
            {
                return;
            }

            if (!player.HealthController.IsAlive)
            {
                StopAllCoroutines();
                Destroy(this);
            }
        }

        private IEnumerator CoverFinder()
        {
            while (true)
            {
                if (FinalCoverPoints.Count > 0)
                {
                    float StartTime = Time.time;

                    List<CoverPoint> finalPoints = new List<CoverPoint>();
                    List<CoverPoint> closePoints = new List<CoverPoint>();

                    finalPoints.AddRange(FinalCoverPoints);
                    finalPoints = CoverPointSorter.SortByDistance(finalPoints, player.Transform.position);

                    yield return new WaitForEndOfFrame();

                    if (finalPoints.Count > 150)
                    {
                        finalPoints.RemoveRange(150, finalPoints.Count - 150);
                    }

                    //Logger.LogDebug($"First 150 Count = {finalPoints.Count}");

                    finalPoints = FilterCoverSpacing(finalPoints, 3f);

                    yield return new WaitForEndOfFrame();

                    //Logger.LogDebug($"Filtered 150 Count = {finalPoints.Count} starting raycasting at {Time.time}");

                    int max = Mathf.Clamp(finalPoints.Count, 0, 30);

                    for (int i = 0; i < max; i++)
                    {
                        if (IsVector3Close(finalPoints[i].Position, out var path))
                        {
                            if (player.AIData?.BotOwner?.Memory?.GoalEnemy == null)
                            {
                                closePoints.Add(finalPoints[i]);
                            }
                            else
                            {
                                yield return new WaitForEndOfFrame();

                                if (CheckPointForHidden(finalPoints[i], path))
                                {
                                    closePoints.Add(finalPoints[i]);
                                }
                            }
                        }
                    }

                    //Logger.LogDebug($"Final Count = {closePoints.Count} which took {Time.time - StartTime} seconds");
                    CloseCoverPoints.Clear();
                    CloseCoverPoints.AddRange(closePoints);
                }

                DebugDrawPoints(CloseCoverPoints);

                yield return new WaitForSeconds(0.1f);
            }
        }

        private bool CheckPointForHidden(CoverPoint point, NavMeshPath path)
        {
            bool hidden = false;

            if (player.AIData?.BotOwner?.Memory?.GoalEnemy != null)
            {
                if (IsVector3Hidden(point.Position))
                {
                    if (AreCornersVisible(path))
                    {
                        hidden = true;
                    }
                }
            }
            return hidden;
        }

        private bool IsVector3Close(Vector3 point, out NavMeshPath path)
        {
            path = new NavMeshPath();
            if (Vector3.Distance(point, player.Transform.position) < 30f)
            {
                if (NavMesh.CalculatePath(player.Transform.position, point, -1, path) && path.CalculatePathLength() < 30f)
                {
                    return true;
                }
            }
            return false;
        }

        private bool AreCornersVisible(NavMeshPath path)
        {
            if (player.AIData?.BotOwner?.Memory?.GoalEnemy != null)
            {
                int hit = 0;
                foreach (var corner in path.corners)
                {
                    if (hit > 2)
                    {
                        return false;
                    }

                    if (!IsVector3Hidden(corner))
                    {
                        hit++;
                    }
                }
            }
            return true;
        }

        private bool IsVector3Hidden(Vector3 point)
        {
            var enemy = player.AIData.BotOwner.Memory.GoalEnemy;
            if (Vector3.Distance(point, enemy.CurrPosition) < 8f)
            {
                return false;
            }

            point.y += 0.25f;
            var direction = point - enemy.CurrPosition;

            if (Physics.Raycast(enemy.CurrPosition, direction, direction.magnitude, Mask))
            {
                return true;
            }
            return false;
        }

        private void DebugDrawPoints(List<CoverPoint> points)
        {
            if (points.Count > 0)
            {
                if (player.IsYourPlayer)
                {
                    return;
                }

                int i = 0;
                while (i < points.Count)
                {
                    Vector3 pos = player.Transform.position;
                    pos.y += 1.4f;
                    DebugDrawer.Line(points[i].Position, pos, 0.015f, PlayerColor, 0.15f);
                    DebugDrawer.Sphere(points[i].Position, 0.1f, PlayerColor, 0.15f);
                    i++;
                }
            }
        }


        /// <summary>
        /// Generates NavMesh newPoints in random directions at increasing ranges and checks to see if the bot is stationary before cutting off the generator.
        /// </summary>
        private IEnumerator PathGenerator()
        {
            while (true)
            {
                if (CheckStationary == null)
                {
                    yield return new WaitForSeconds(0.25f);
                    continue;
                }
                else if (CheckStationary.CheckForCalcPath())
                {
                    //yield return new WaitForSeconds(0.5f);
                    //continue;
                }

                var newPoints = FindStartPoints(player.Transform.position);

                yield return new WaitForEndOfFrame();

                List<Vector3> genPoints = new List<Vector3>();

                foreach (Vector3 point in newPoints)
                {
                    genPoints.AddRange(GeneratePoints(point));
                }

                yield return new WaitForEndOfFrame();

                var filteredPoints = FilterDistance(genPoints);

                List<Vector3> verifiedPoints = new List<Vector3>();

                foreach (var point in filteredPoints)
                {
                    if (VerifyPoint(point))
                    {
                        verifiedPoints.Add(point);
                    }
                }

                PointsToSend.AddRange(verifiedPoints);

                yield return new WaitForSeconds(2f);
            }
        }

        /// <summary>
        /// Generates a list of random points around the player position and checks them against the NavMesh for valid newPoints.
        /// </summary>
        /// <param name="radius">The radius around the player position to generate random points.</param>
        /// <param name="playerPosition">The player position to generate random points around.</param>
        private List<Vector3> FindStartPoints(Vector3 playerPosition)
        {
            int i = 0;
            List<Vector3> newPoints = new List<Vector3>();
            while (i < MaxRangeIterations)
            {
                // Find a random points in a sphere around the player position.
                Vector3 randomPoint = Random.onUnitSphere * IncreaseRange(i);
                randomPoint.y = RandomAngle;
                // Add the player position to our random points, so that is it centered around them.
                randomPoint += playerPosition;

                // Sample the position on the navmesh to find a hit.
                if (NavMesh.SamplePosition(randomPoint, out var hit, SamplePositionRange, NavMesh.AllAreas))
                {
                    // Calculate a path to that hit, and make sure there a complete path from the player position.
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(playerPosition, hit.position, -1, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        newPoints.Add(hit.position);
                    }
                    i++;
                }
            }

            return newPoints;
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
            // CheckForCalcPath that the point has a complete path to the player position.
            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(player.Transform.position, point, -1, path) || path.status != NavMeshPathStatus.PathComplete)
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

        private List<CoverPoint> FilterCoverSpacing(List<CoverPoint> points, float min = 1f)
        {
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    if (Vector3.Distance(points[i].Position, points[j].Position) < min)
                    {
                        points.RemoveAt(j);
                        j--;
                    }
                }
            }
            return points;
        }

        private float IncreaseRange(int i)
        {
            // Set our range for the path finder.
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

        private float RandomAngle => Random.Range(-MaxYAngle, MaxYAngle);
        public static Color RandomColor => new Color(Random.value, Random.value, Random.value);
        public Color PlayerColor;

        private LayerMask Mask = LayerMaskClass.HighPolyWithTerrainMask;
        private float DebugTimer = 0f;
        private Player player;
        protected ManualLogSource Logger;
    }

    public class HelperClasses
    {
        /// <summary>
        /// Checks if the player is stationary by comparing the distance between the last calculated position and the current position.
        /// </summary>
        /// <param name="playerPos">The current position of the player.</param>
        /// <returns>True if the player is stationary, false otherwise.</returns>
        public class CheckIfPlayerStationary
        {
            public CheckIfPlayerStationary(Player player, int maxStationaryCalc = 10, float tolerance = 1f)
            {
                Player = player;
                Tolerance = tolerance;
                Max = maxStationaryCalc;
            }

            public bool CheckForCalcPath()
            {
                if (!Movement)
                {
                    // If we have calculated X new newPoints at the same position, stop running until that changes.
                    if (PathCount > Max)
                    {
                        return true;
                    }
                    // Count up the number of newPoints calulated at a stationary position.
                    PathCount++;
                }
                // If we aren't within the same distance, count down the cover check limit down to 0
                else if (PathCount > 0)
                {
                    PathCount--;
                }

                // Save the player's current position for reference later above in the next loop
                LastPosition = Player.Transform.position;

                return false;
            }

            private bool Movement => PathDistance > Tolerance;
            private float PathDistance => Vector3.Distance(LastPosition, Player.Transform.position);

            private Vector3 LastPosition;
            private Player Player;
            private readonly float Tolerance;
            private readonly int Max;
            private int PathCount = 0;
        }
    }
}
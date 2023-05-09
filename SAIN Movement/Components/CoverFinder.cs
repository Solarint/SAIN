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
        public List<Vector3> OutputPoints = new List<Vector3>();

        private void Start()
        {
            Player = GetComponent<Player>();
            CheckStationary = new CheckIfPlayerStationary(Player);
            PlayerColor = RandomColor;

            string name = Player.IsYourPlayer ? Player.Profile.Nickname : Player.name;
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name + $": {name}: ");

            StartCoroutine(PathGenerator());
            StartCoroutine(CoverFinder());
        }

        private CheckIfPlayerStationary CheckStationary;

        private void Update()
        {
            DebugDrawPoints();
        }

        /// <summary>
        /// Coroutine to find cover points for the player.
        /// </summary>
        private IEnumerator CoverFinder()
        {
            while (true)
            {
                if (!Player.HealthController.IsAlive)
                {
                    if (OutputPoints.Count == 0)
                    {
                        StopAllCoroutines();
                        yield break;
                    }
                    yield return new WaitForSeconds(0.25f);
                    continue;
                }

                if (FinalCoverPoints.Count > 0)
                {
                    List<CoverPoint> finalPoints = new List<CoverPoint>();
                    List<CoverPoint> closePoints = new List<CoverPoint>();

                    finalPoints.AddRange(FinalCoverPoints);
                    finalPoints = CoverPointSorter.SortByDistance(finalPoints, Player.Transform.position);

                    yield return new WaitForEndOfFrame();

                    if (finalPoints.Count > 300)
                    {
                        finalPoints.RemoveRange(300, finalPoints.Count - 300);
                    }

                    finalPoints = FilterCoverSpacing(finalPoints, 2f);

                    int max = Mathf.Clamp(finalPoints.Count, 0, 100);

                    for (int i = 0; i < max; i++)
                    {
                        if (IsVector3Close(finalPoints[i].Position, out var path))
                        {
                            if (Player.AIData?.BotOwner?.Memory?.GoalEnemy == null)
                            {
                                closePoints.Add(finalPoints[i]);
                            }
                            else
                            {
                                Vector3 enemyPos = Player.AIData.BotOwner.Memory.GoalEnemy.CurrPosition;
                                if (CheckPointForCover(finalPoints[i], path, enemyPos))
                                {
                                    closePoints.Add(finalPoints[i]);
                                }

                                yield return new WaitForEndOfFrame();
                            }
                        }
                    }

                    closePoints = CoverPointSorter.SortByDistance(closePoints, Player.Transform.position);

                    CloseCoverPoints.Clear();
                    CloseCoverPoints.AddRange(closePoints);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        /// <summary>
        /// Checks if a given CoverPoint is facing the enemy and if it is hidden from the enemy's view.
        /// </summary>
        /// <param name="point">The CoverPoint to check.</param>
        /// <param name="botPath">The path of the bot.</param>
        /// <param name="enemyPosition">The position of the enemy.</param>
        /// <returns>True if the CoverPoint is facing the enemy and hidden from the enemy's view, false otherwise.</returns>
        private bool CheckPointForCover(CoverPoint point, NavMeshPath botPath, Vector3 enemyPosition)
        {
            bool hidden = false;

            if (IsCoverFacingEnemyDirection(point, enemyPosition))
            {
                if (IsPointVisible(point.Position))
                {
                    NavMeshPath enemyPath = new NavMeshPath();
                    NavMesh.CalculatePath(enemyPosition, point.Position, -1, enemyPath);
                    if (enemyPath.CalculatePathLength() > botPath.CalculatePathLength())
                    {
                        hidden = true;
                    }
                    else if (DoesPathLeadToEnemy(botPath, enemyPosition) == false)
                    {
                        hidden = true;
                    }
                }
            }

            return hidden;
        }

        /// <summary>
        /// Checks if a Vector3 point is close to the player, and if so, calculates a NavMeshPath between them, and checks the distance of that path.
        /// </summary>
        /// <param name="point">The Vector3 point to check.</param>
        /// <param name="path">The NavMeshPath between the player and the point.</param>
        /// <param name="minStraightDist">The minimum straight-line distance between the player and the point.</param>
        /// <param name="minPathDist">The minimum path distance between the player and the point.</param>
        /// <returns>True if the point is close to the player, false otherwise.</returns>
        private bool IsVector3Close(Vector3 point, out NavMeshPath path, float minStraightDist = 30f, float minPathDist = 30f)
        {
            path = new NavMeshPath();
            if (Vector3.Distance(point, Player.Transform.position) < minStraightDist)
            {
                if (NavMesh.CalculatePath(Player.Transform.position, point, -1, path) && path.CalculatePathLength() < minPathDist)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a given navmesh path leads to a given enemy position.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <param name="enemyPosition">The enemy position to check against.</param>
        /// <returns>True if the path leads to the enemy, false otherwise.</returns>
        private bool DoesPathLeadToEnemy(NavMeshPath path, Vector3 enemyPosition)
        {
            var corners = path.corners;

            if (corners.Length < 2)
            {
                return false; // Return false if there is no second corner in the botPath
            }

            // Check if the second corner (index 1) is towards the enemy
            return IsPointTowardsEnemy(corners[1], enemyPosition);
        }

        /// <summary>
        /// Checks if a point is pointing towards an enemy in a 90 degree arc.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="enemyPosition">The enemy's position.</param>
        /// <returns>True if the point is pointing towards the enemy, false otherwise.</returns>
        private bool IsPointTowardsEnemy(Vector3 point, Vector3 enemyPosition)
        {
            Vector3 pointDirection = (point - Player.Transform.position).normalized;
            Vector3 enemyDirection = (enemyPosition - Player.Transform.position).normalized;

            float dotProduct = Vector3.Dot(pointDirection, enemyDirection);

            // If the dot product is greater than the cosine of 45 degrees (approx. 0.7071),
            // it means the angle between these vectors is less than 45 degrees
            return dotProduct > 0.7071f;
        }

        /// <summary>
        /// Checks if the stored direction in a cover point object is facing the enemy's direction.
        /// </summary>
        /// <param name="point">The cover point to check.</param>
        /// <param name="enemyPosition">The enemy's position.</param>
        /// <returns>True if the cover point is facing the enemy's direction, false otherwise.</returns>
        private bool IsCoverFacingEnemyDirection(CoverPoint point, Vector3 enemyPosition)
        {
            Vector3 coverDirection = point.Direction.normalized;
            Vector3 enemyDirection = (enemyPosition - point.Position).normalized;

            float dotProduct = Vector3.Dot(coverDirection, enemyDirection);

            return dotProduct > 0;
        }

        /// <summary>
        /// Checks if a point is visible from the enemy's head.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>True if the point is not visible, false otherwise.</returns>
        private bool IsPointVisible(Vector3 point)
        {
            Player.AIData.BotOwner.Memory.GoalEnemy.Person.MainParts.TryGetValue(BodyPartType.head, out BodyPartClass EnemyHead);

            point.y += 0.25f;

            var direction = point - EnemyHead.Position;

            return Physics.Raycast(EnemyHead.Position, direction, direction.magnitude, LayerMaskClass.HighPolyCollider);
        }

        private void DebugDrawPoints()
        {
            if (CloseCoverPoints.Count > 0 && !Player.IsYourPlayer && Player.AIData?.BotOwner?.LookSensor != null)
            {
                var point = CloseCoverPoints.PickRandom();

                Vector3 coverPoint = point.Position;
                coverPoint.y += point.CoverHeight;

                DebugDrawPath(point);
                DebugDrawer.Line(point.Position, coverPoint, 0.03f, PlayerColor, 0.33f);
            }
        }

        private void DebugDrawPath(CoverPoint point)
        {
            NavMeshPath Path = new NavMeshPath();
            NavMesh.CalculatePath(Player.Transform.position, point.Position, -1, Path);
            for (int i = 0; i < Path.corners.Length - 1; i++)
            {
                Vector3 corner1 = Path.corners[i];
                Vector3 corner2 = Path.corners[i + 1];
                DebugDrawer.Line(corner1, corner2, 0.05f, Color.red, 1f);
            }
        }

        /// <summary>
        /// Generates NavMesh newPoints in random directions at increasing ranges and checks to see if the bot is stationary before cutting off the generator.
        /// </summary>
        private IEnumerator PathGenerator()
        {
            while (true)
            {
                if (!Player.HealthController.IsAlive)
                {
                    if (OutputPoints.Count == 0)
                    {
                        StopAllCoroutines();
                        yield break;
                    }
                    yield return new WaitForSeconds(1f);
                    continue;
                }

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

                var newPoints = FindStartPoints(Player.Transform.position);

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

                OutputPoints.AddRange(verifiedPoints);

                yield return new WaitForSeconds(2f);
            }
        }

        /// <summary>
        /// Generates a list of random points around the Player position and checks them against the NavMesh for valid newPoints.
        /// </summary>
        /// <param name="radius">The radius around the Player position to generate random points.</param>
        /// <param name="playerPosition">The Player position to generate random points around.</param>
        private List<Vector3> FindStartPoints(Vector3 playerPosition)
        {
            int i = 0;
            List<Vector3> newPoints = new List<Vector3>();
            while (i < MaxRangeIterations)
            {
                // Find a random points in a sphere around the Player position.
                Vector3 randomPoint = Random.onUnitSphere * IncreaseRange(i);
                randomPoint.y = RandomAngle;
                // Add the Player position to our random points, so that is it centered around them.
                randomPoint += playerPosition;

                // Sample the position on the navmesh to find a hit.
                if (NavMesh.SamplePosition(randomPoint, out var hit, SamplePositionRange, NavMesh.AllAreas))
                {
                    // Calculate a botPath to that hit, and make sure there a complete botPath from the Player position.
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
        /// Filters a list of CoverPoints based on a minimum spacing between them.
        /// </summary>
        /// <param name="points">The list of CoverPoints to filter.</param>
        /// <param name="min">The minimum spacing between CoverPoints.</param>
        /// <returns>The filtered list of CoverPoints.</returns>
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
        protected ManualLogSource Logger;
    }

    public class HelperClasses
    {
        /// <summary>
        /// Checks if the Player is stationary by comparing the distance between the last calculated position and the current position.
        /// </summary>
        /// <param name="playerPos">The current position of the Player.</param>
        /// <returns>True if the Player is stationary, false otherwise.</returns>
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

                // Save the Player's current position for reference later
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
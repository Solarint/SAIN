using Audio.DebugTools;
using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.Classes.HelperClasses;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Components
{
    public class CoverFinderComponent : MonoBehaviour
    {
        public CustomCoverPoint FallBackPoint { get; private set; }
        private bool DebugMode => DebugCoverComponent.Value;
        public List<CoverPoint> CloseCoverPoints = new List<CoverPoint>();
        public List<CoverPoint> SafeCoverPoints = new List<CoverPoint>();
        private SAINComponent SAIN;
        public CoverAnalyzer Analyzer { get; private set; }

        private void Start()
        {
            BotOwner = GetComponent<BotOwner>();
            SAIN = BotOwner.GetComponent<SAINComponent>();

            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name + $": {BotOwner.name}: ");

            StartCoroutine(FindClosePoints());
            StartCoroutine(FindSafePoints());
            StartCoroutine(FindNextFallBackPoint());
        }

        private void Update()
        {
            if (Analyzer == null && BotOwner.AIData?.BotOwner != null)
            {
                Analyzer = new CoverAnalyzer(BotOwner.AIData.BotOwner);
            }

            DebugDrawPoints();
        }

        private IEnumerator FindClosePoints()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                if (CloseCoverPoints.Count > 0)
                {
                    List<CoverPoint> closePoints = new List<CoverPoint>(CloseCoverPoints);
                    int closeCount = closePoints.Count;
                    for (int j = closeCount - 1; j >= 0; j--)
                    {
                        if (!IsVector3Close(closePoints[j].Position, out var path, 50f, 50f))
                        {
                            closePoints.RemoveAt(j);
                        }
                    }

                    CloseCoverPoints.Clear();
                    CloseCoverPoints.AddRange(closePoints);
                }

                if (CloseCoverPoints.Count < 50 && CoverCentral.FinalCoverPoints.Count > 0)
                {
                    int count = CoverCentral.FinalCoverPoints.Count;
                    int delay = Mathf.CeilToInt((float)count / 1000f); // Calculate the delay value

                    for (int i = 0; i < count; i++)
                    {
                        if (i % delay == 0)
                        {
                            yield return new WaitForEndOfFrame();
                        }

                        if (IsVector3Close(CoverCentral.FinalCoverPoints[i].Position, out var path, 50f, 50f))
                        {
                            CloseCoverPoints.Add(CoverCentral.FinalCoverPoints[i]);
                        }

                        if (CloseCoverPoints.Count >= 80)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private IEnumerator FindSafePoints()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                if (CloseCoverPoints.Count > 0 && BotOwner.AIData?.BotOwner?.Memory?.GoalEnemy != null)
                {
                    List<CoverPoint> closePoints = new List<CoverPoint>();
                    closePoints.AddRange(CloseCoverPoints);

                    SafeCoverPoints.Clear();
                    FullCover.Clear();

                    foreach (var point in closePoints)
                    {
                        yield return new WaitForEndOfFrame();

                        if (BotOwner.AIData?.BotOwner?.Memory?.GoalEnemy == null)
                        {
                            break;
                        }

                        Vector3 enemyPos = BotOwner.AIData.BotOwner.Memory.GoalEnemy.CurrPosition;

                        NavMeshPath path = new NavMeshPath();
                        NavMesh.CalculatePath(enemyPos, point.Position, -1, path);

                        if (CheckPointForCover(point, path, enemyPos))
                        {
                            SafeCoverPoints.Add(point);

                            if (point.CoverLevel >= 0.9f)
                            {
                                SAIN_Helpers.DebugDrawer.Line(BotOwner.LookSensor._headPoint, point.Position, 0.1f, Color.magenta, 0.1f);
                                FullCover.Add(point);
                            }
                        }
                    }

                    yield return new WaitForEndOfFrame();
                    SafeCoverPoints = SortPointsByDistance(SafeCoverPoints);
                    yield return new WaitForEndOfFrame();
                    FullCover = SortPointsByDistance(FullCover);
                }
            }
        }

        private List<CoverPoint> FullCover = new List<CoverPoint>();

        private IEnumerator FindNextFallBackPoint()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                if (BotOwner.AIData?.BotOwner?.Memory?.GoalEnemy != null)
                {
                    if (Analyzer == null)
                    {
                        yield return new WaitForSeconds(1f);
                        continue;
                    }

                    if (FullCover.Count > 0)
                    {
                        List<CoverPoint> fullCover = new List<CoverPoint>();
                        fullCover.AddRange(FullCover);

                        fullCover = SortPointsByDistance(fullCover);

                        int i = 0;
                        while (i < fullCover.Count)
                        {
                            if (BotOwner.AIData?.BotOwner?.Memory?.GoalEnemy == null)
                            {
                                break;
                            }

                            SAIN_Helpers.DebugDrawer.Line(FullCover[i].Position, BotOwner.MyHead.position, 0.1f, SAIN.Core.BotColor, 0.1f);

                            if (Analyzer.CheckPosition(fullCover[i].Position, out var custompoint, 1f))
                            {
                                FallBackPoint = custompoint;
                                break;
                            }

                            yield return new WaitForEndOfFrame();
                            i++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a given CoverPoint is facing the enemy and if it is hidden from the enemy's view.
        /// </summary>
        /// <param name="point">The CoverPoint to check.</param>
        /// <param name="botPath">The path of the BotOwner.</param>
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
            if (Vector3.Distance(point, BotOwner.Transform.position) < minStraightDist)
            {
                if (NavMesh.CalculatePath(BotOwner.Transform.position, point, -1, path) && path.CalculatePathLength() < minPathDist)
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
            Vector3 pointDirection = (point - BotOwner.Transform.position).normalized;
            Vector3 enemyDirection = (enemyPosition - BotOwner.Transform.position).normalized;

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
            Vector3 coverDirection = point.CenterDirection.normalized;
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
            BotOwner.AIData.BotOwner.Memory.GoalEnemy.Person.MainParts.TryGetValue(BodyPartType.head, out BodyPartClass EnemyHead);

            point.y += 0.25f;

            var direction = point - EnemyHead.Position;

            return Physics.Raycast(EnemyHead.Position, direction, direction.magnitude, Mask);
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
        /// Sorts a list of CoverPoints by distance from the player's position.
        /// </summary>
        /// <param name="points">The list of CoverPoints to sort.</param>
        /// <returns>The sorted list of CoverPoints.</returns>
        private List<CoverPoint> SortPointsByDistance(List<CoverPoint> points)
        {
            return CoverPointSorter.SortByDistance(points, BotOwner.Transform.position);
        }

        private void DebugDrawPoints()
        {
            if (BotOwner.AIData?.BotOwner != null)
            {
                if (FallBackPoint != null && BotOwner.AIData.BotOwner.Memory.GoalEnemy != null)
                {
                    if (BotOwner.AIData.BotOwner.Memory.GoalEnemy.Person.MainParts.TryGetValue(BodyPartType.head, out BodyPartClass EnemyHead))
                    {
                        //DebugDrawer.Line(BotOwner.AIData.BotOwner.LookSensor._headPoint, FallBackPoint.CoverPosition, 0.1f, SetColor, 0.05f);
                        //DebugDrawer.Line(EnemyHead.position, FallBackPoint.CoverPosition, 0.025f, SetColor, 0.05f);
                    }
                }

                if (SafeCoverPoints.Count > 0)
                {
                    var point = SafeCoverPoints.PickRandom();

                    Vector3 coverPoint = point.Position;
                    coverPoint.y += point.Height;

                    DebugDrawPath(point);
                    SAIN_Helpers.DebugDrawer.Line(point.Position, coverPoint, 0.03f, PlayerColor, 0.33f);
                }
            }
        }

        private void DebugDrawPath(CoverPoint point)
        {
            NavMeshPath Path = new NavMeshPath();
            NavMesh.CalculatePath(BotOwner.Transform.position, point.Position, -1, Path);
            for (int i = 0; i < Path.corners.Length - 1; i++)
            {
                Vector3 corner1 = Path.corners[i];
                Vector3 corner2 = Path.corners[i + 1];
                SAIN_Helpers.DebugDrawer.Line(corner1, corner2, 0.05f, Color.red, 1f);
            }
        }

        public static Color RandomColor => new Color(Random.value, Random.value, Random.value);
        private float SortTimer = 0f;
        public Color PlayerColor;
        private LayerMask Mask = LayerMaskClass.HighPolyCollider;
        private BotOwner BotOwner;
        protected ManualLogSource Logger;

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }
    }
}
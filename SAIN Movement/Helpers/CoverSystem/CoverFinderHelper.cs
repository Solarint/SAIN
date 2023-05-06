using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Movement.Helpers.Corners;
using static Movement.UserSettings.Debug;

namespace Movement.Helpers
{
    /// <summary>
    /// Finds Points that can be checked to see if they provide cover for a bot
    /// </summary>
    public class CoverFinder
    {
        public CoverFinder(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
            Analyzer = new CoverAnalyzer(bot);
        }

        private const int maxCoverIterations = 5;
        private const float coverReduceStep = -0.1f;

        private const float incrementRadiusBase = 10f;
        private const float radiusIncreaseStep = 10f;

        private const int maxLerpIterations = 10;
        private const float LerpStep = 1f / maxLerpIterations;

        private const float MinAngle = 45f;
        private const float MaxAngle = 175f;

        private void FindAllCorners(Vector3 enemyPosition)
        {
            TargetPosition = enemyPosition;
            float radius = 50f;
            int iterations = 0;
            const int limit = 30;
            const int max = 360 - limit * 2;
            List<Vector3> visitedCorners = new List<Vector3>();
            List<Vector3> newCorners = new List<Vector3>();
            int countedCorners = 0;
            while (iterations < max)
            {
                Vector3 randomPoint = FindArcPoint(BotPosition, TargetPosition, radius, (float)iterations + limit);

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 10f, -1))
                {
                    NavMeshPath path = new NavMeshPath();
                    NavMesh.CalculatePath(BotOwner.Transform.position, hit.position, -1, path);
                    List<Vector3> corners = new List<Vector3>(path.corners);
                    countedCorners += corners.Count;
                    corners.RemoveAll(c => visitedCorners.Contains(c));
                    newCorners.AddRange(corners);
                    visitedCorners.AddRange(corners);

                    if (DebugMode)
                    {
                        Logger.LogInfo($"");
                    }
                }

                iterations++;
            }

            Vector3[] NavCorners = newCorners.ToArray();

            Logger.LogInfo($"Total Corners found {NavCorners.Length}");
            Logger.LogInfo($"All Checked Corners {countedCorners}");
        }

        /// <summary>
        /// Finds a cover point for the AI to hide from the enemy.
        /// </summary>
        /// <param name="coverPoint">The cover point found.</param>
        /// <param name="enemyPosition">The position of the enemy.</param>
        /// <param name="minCoverLevel">The minimum cover level required.</param>
        /// <param name="minCoverReduce">Whether the minimum cover level should be reduced if no cover is found.</param>
        /// <returns>Whether a cover point was found.</returns>
        public bool FindCover(out CustomCoverPoint coverPoint, Vector3 enemyPosition, float minCoverLevel = 0.5f, bool minCoverReduce = true)
        {
            TargetPosition = enemyPosition;
            float currentRadius = incrementRadiusBase;
            int iterations = 0;
            coverPoint = null;

            while (iterations < maxCoverIterations)
            {
                currentRadius += radiusIncreaseStep * iterations;
                minCoverLevel += minCoverReduce ? coverReduceStep * iterations : 0f;

                if (FindRandomPoint(out NavMeshHit hit, currentRadius))
                {
                    if (FindCorners(hit, out coverPoint, minCoverLevel))
                    {
                        coverPoint = AddPath(coverPoint);

                        if (DebugMode)
                        {
                            Logger.LogInfo($"Found Cover after [{iterations}] iterations with Minimum Cover Level set to {minCoverLevel} and Radius = {currentRadius}");
                        }

                        return true;
                    }
                }

                iterations++;
            }

            if (DebugMode)
            {
                Logger.LogWarning($"Found No Cover after [{iterations}] iterations with Minimum Cover Level set to {minCoverLevel} and Radius = {currentRadius}");
            }

            return false;
        }

        private bool FindCorners(NavMeshHit hit, out CustomCoverPoint coverPoint, float minCover)
        {
            coverPoint = null;

            Vector3[] corners = Corners(hit.position);
            DebugCorners(corners);

            int i = 1;
            while (i < corners.Length)
            {
                if (CheckBetweenCorners(corners[i - 1], corners[i], out coverPoint, minCover))
                {
                    return true;
                }

                i++;
            }

            if (DebugMode)
            {
                Logger.LogWarning($"CheckCornersForCover: Found No Cover");
            }

            return false;
        }

        private bool CheckBetweenCorners(Vector3 cornerA, Vector3 cornerB, out CustomCoverPoint coverPoint, float minCover)
        {
            coverPoint = null;
            int iterations = 0;

            while (iterations < maxLerpIterations)
            {
                // Lerps between RawCorners to find possible cover points between them.
                Vector3 cornerLerped = LerpCorners(iterations, cornerA, cornerB);

                if (Analyzer.CheckPosition(TargetPosition, cornerLerped, out coverPoint, minCover))
                {
                    if (DebugMode)
                    {
                        Logger.LogInfo($"CheckCornersForCover: Found Cover after [{iterations}] iterations");
                        DebugDrawer.Ray(cornerLerped, Vector3.up, 1.5f, 0.1f, Color.green, 60f);
                    }

                    return true;
                }

                iterations++;
            }

            return false;
        }

        private bool FindRandomPoint(out NavMeshHit hit, float radius)
        {
            Vector3 randomPoint = FindArcPoint(BotPosition, TargetPosition, radius, RandomAngle);

            return NavMesh.SamplePosition(randomPoint, out hit, 10f, -1);
        }

        /// <summary>
        /// Calculates the position on an arc between two points with a given radius and angle.
        /// </summary>
        /// <param name="botPos">The position of the bot.</param>
        /// <param name="targetPos">The position of the target.</param>
        /// <param name="arcRadius">The radius of the arc.</param>
        /// <param name="angle">The angle of the arc.</param>
        /// <returns>The position on the arc.</returns>
        public static Vector3 FindArcPoint(Vector3 botPos, Vector3 targetPos, float arcRadius, float angle)
        {
            Vector3 direction = (botPos - targetPos).normalized;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 arcPoint = arcRadius * (rotation * direction);

            if (DebugCoverSystem.Value)
            {
                System.Console.WriteLine($"Arc Point Distance: {arcPoint.magnitude}");
                DebugDrawer.Line(botPos, botPos + arcPoint, 0.025f, Color.white, 2f);
                DebugDrawer.Sphere(botPos + arcPoint, 0.25f, Color.white, 2f);
            }

            return botPos + arcPoint;
        }

        private Vector3[] Corners(Vector3 point)
        {
            return Processing.GetCorners(BotPosition, point, BotOwner.LookSensor._headPoint, true, true, false, false, false, 1f);
        }

        private void DebugCorners(Vector3[] corners)
        {
            if (DebugMode && DebugTimer < Time.time)
            {
                Logger.LogDebug($"Corners Length = [{corners.Length}]");
                DebugTimer = Time.time + 10f;
                for (int i = 0; i < corners.Length - 1; i++)
                {
                    Vector3 corner1 = corners[i];
                    Vector3 corner2 = corners[i + 1];
                    corner1.y += 0.25f;
                    corner2.y += 0.25f;
                    DebugDrawer.Line(corner1, corner2, 0.075f, Color.green, 30f);
                }
            }
        }

        private Vector3 LerpCorners(float iterations, Vector3 A, Vector3 B)
        {
            return Vector3.Lerp(A, B, LerpStep + (LerpStep * iterations));
        }

        private CustomCoverPoint AddPath(CustomCoverPoint Cover)
        {
            NavMeshPath Path = new NavMeshPath();
            NavMesh.CalculatePath(BotPosition, Cover.CoverPosition, -1, Path);

            Cover.NavMeshPath = Path;
            Cover.CoverDistance = Path.CalculatePathLength();

            return Cover;
        }

        private Vector3 BotPosition => BotOwner.Transform.position;
        private float RandomAngle => SAIN_Math.RandomBool(50f) ? Random.Range(MinAngle, MaxAngle) : Random.Range(-MinAngle, -MaxAngle);
        private bool DebugMode => DebugCoverSystem.Value;
        public CoverAnalyzer Analyzer { get; private set; }

        private float DebugTimer = 0f;
        private Vector3 TargetPosition;
        protected ManualLogSource Logger;
        private readonly BotOwner BotOwner;
        private readonly CornerProcessing Processing = new CornerProcessing();
    }
}
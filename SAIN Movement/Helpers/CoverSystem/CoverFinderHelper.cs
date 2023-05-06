using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using static Movement.Helpers.Corners;
using static Movement.UserSettings.Debug;
using static UnityEngine.UI.GridLayoutGroup;

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
        private const int maxLerpIterations = 5;
        private const float LerpStep = 0.2f;
        private const float incrementRadiusBase = 10f;
        private const float radiusIncrementStep = 10f;
        private const float MinAngle = 30f;
        private const float MaxAngle = 175f;

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

                minCoverLevel = minCoverReduce ? DecreaseCoverLevel(minCoverLevel) : minCoverLevel;
                currentRadius = IncreaseRadius(iterations);
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
                // Lerps between corners to find possible cover points between them.
                Vector3 cornerLerped = LerpCorners(iterations, cornerA, cornerB, LerpStep);

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

            bool onNavMesh = false;

            if (NavMesh.SamplePosition(randomPoint, out hit, 10f, -1))
            {
                onNavMesh = true;
            }

            return onNavMesh;
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

            Vector3 rotatedDirection = rotation * direction;

            if (DebugCoverSystem.Value && DebugTimerArc < Time.time)
            {
                DebugTimerArc = Time.time + 1f;
                DebugDrawer.Line(botPos, botPos + arcRadius * rotatedDirection, 0.025f, Color.white, 10f);
                DebugDrawer.Sphere(botPos + arcRadius * rotatedDirection, 0.25f, Color.white, 10f);
            }

            return botPos + arcRadius * rotatedDirection;
        }

        private static float DebugTimerArc = 0f;

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

        private float DecreaseCoverLevel(float minCover)
        {
            return minCover > 0.2f ? minCover * 0.8f : minCover;
        }

        private float IncreaseRadius(float iterations)
        {
            return (incrementRadiusBase + (iterations * radiusIncrementStep));
        }

        private Vector3 LerpCorners(float iterations, Vector3 A, Vector3 B, float step = 0.1f)
        {
            return Vector3.Lerp(A, B, step + (step * iterations));
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
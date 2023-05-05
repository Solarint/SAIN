using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;
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

        public bool FindCover(out CustomCoverPoint coverPoint, Vector3 enemyPosition, float minCoverLevel = 0.5f, bool decreaseMinCoverLevelwithIterations = true, float rangeModifier = 1f)
        {
            TargetPosition = enemyPosition;

            const float incrementRadiusBase = 3f;
            const float radiusIncrementStep = 5f;
            float currentRadius = incrementRadiusBase;

            float initialCoverMin = minCoverLevel;

            const int maxIterations = 20;
            int iterations = 0;
            while (iterations < maxIterations)
            {
                // Generates a random point in a 200 degree arc away from the enemy
                float randomAngle = Random.Range(-90, 90);
                Vector3 randomPoint = FindArcPoint(BotPosition, enemyPosition, currentRadius, randomAngle);

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, -1))
                {
                    // Grabs Processing from our Corner Helper. We are trimming corners heavily here so that we are only checking viable corners with our raycast system.
                    Vector3[] corners = Corners.CornerProcessing.GetCorners(BotPosition, hit.position, true, false, true, true, true);

                    // Checks corners to see if a cover coverPosition is present along the path.
                    if (CheckCornersForCover(corners, out coverPoint, minCoverLevel))
                    {
                        if (DebugMode)
                        {
                            Logger.LogInfo($"Found Cover after [{iterations}] iterations with Minimum Cover Level set to {minCoverLevel} and Radius = {currentRadius}");
                        }
                        return true;
                    }
                }

                // Decrease the min cover level up to 2/3 its input value
                if (decreaseMinCoverLevelwithIterations && minCoverLevel > initialCoverMin / 1.5f)
                {
                    minCoverLevel *= 0.9f;
                }

                // Increment the radius size if no cover found and start again
                currentRadius += (incrementRadiusBase + (iterations * radiusIncrementStep)) * rangeModifier;
                iterations++;
            }

            // If still no suitable point is found, return false
            if (DebugMode)
            {
                Logger.LogWarning($"Found No Cover after [{iterations}] iterations with Minimum Cover Level set to {minCoverLevel} and Radius = {currentRadius}");
            }
            coverPoint = null;
            return false;
        }
        private bool CheckCornersForCover(Vector3[] corners, out CustomCoverPoint coverPoint, float minCoverLevel = 0.5f)
        {
            coverPoint = null;

            int i = 1;
            while (i < corners.Length)
            {
                const float lerpStep = 0.25f;
                const int maxIterations = 3;

                int iterations = 0;
                while (iterations < maxIterations)
                {
                    // Lerps between corners to find possible cover points between them.
                    float lerpValue = lerpStep + (lerpStep * iterations);
                    Vector3 cornerLerped = Vector3.Lerp(corners[i - 1], corners[i], lerpValue);

                    // Checks resulting point to see if its cover.
                    if (CheckCover(cornerLerped, out coverPoint, minCoverLevel))
                    {
                        // Calculate a new path to the cover point we found. Then get the distance and assign these values to our CustomCoverPoint.
                        NavMeshPath navMeshPath1 = new NavMeshPath();
                        NavMesh.CalculatePath(BotOwner.Transform.position, cornerLerped, -1, navMeshPath1);
                        float distance = navMeshPath1.CalculatePathLength();

                        coverPoint.CoverDistance = distance;
                        coverPoint.NavMeshPath = navMeshPath1;

                        if (DebugMode)
                        {
                            Logger.LogInfo($"CheckCornersForCover: Found Cover after [{iterations}] iterations. Cover Distance = {distance} for corner pair {i - 1}-{i}.");
                        }
                        return true;
                    }
                    iterations++;
                }

                i++;
            }

            if (DebugMode)
            {
                Logger.LogWarning($"CheckCornersForCover: Found No Cover");
            }
            return false;
        }

        public bool CheckCover(Vector3 coverPosition, out CustomCoverPoint coverPoint, float minCoverLevel = 0.5f)
        {
            // Check if the lerped value is cover or not
            if (Analyzer.AnalyseCoverPosition(TargetPosition, coverPosition, out coverPoint, minCoverLevel))
            {
                if (DebugMode)
                {
                    DebugDrawer.Ray(coverPosition, Vector3.up, 2f, 0.1f, Color.red, 30f);
                }
                return true;
            }
            coverPoint = null;
            return false;
        }

        private static bool CheckPathLength(NavMeshPath navMeshPath, float currentRadius, out float distance)
        {
            distance = navMeshPath.CalculatePathLength();
            return distance < currentRadius * 2f;
        }

        public static Vector3 FindArcPoint(Vector3 bot, Vector3 target, float arcRadius, float inputAngle)
        {
            Vector3 forward = bot - target;
            forward.y = 0.0f;
            forward.Normalize();

            Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);

            Vector3 direction = Quaternion.AngleAxis(inputAngle, Vector3.up) * right;
            Vector3 edgePosition = bot + direction * arcRadius;

            return edgePosition;
        }

        private bool DebugMode => DebugCoverSystem.Value;
        public CoverAnalyzer Analyzer { get; private set; }

        private Vector3 TargetPosition;
        private Vector3 BotPosition => BotOwner.Transform.position;
        protected ManualLogSource Logger;
        private readonly BotOwner BotOwner;

        // Shortcuts
        private bool GoalEnemyNull => BotOwner.Memory.GoalEnemy == null;
        private bool GoalTargetNull => BotOwner.Memory.GoalTarget == null;
    }
}
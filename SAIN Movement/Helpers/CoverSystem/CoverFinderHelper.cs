using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;

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

        public CoverAnalyzer Analyzer { get; private set; }

        public bool FindCover(out CustomCoverPoint coverPoint)
        {
            if (BotOwner.Memory.GoalEnemy == null)
            {
                coverPoint = null;
                return false;
            }

            const float incrementRadiusBase = 20f;
            const float radiusIncrementStep = 3f;
            float currentRadius = incrementRadiusBase;

            const int maxIterations = 10;

            int iterations = 0;
            while (iterations < maxIterations)
            {
                // Generates a random point in a 200 degree arc away from the enemy
                float randomAngle = Random.Range(-100, 100);
                Vector3 randomPoint = FindArcPoint(BotPosition, EnemyPosition, currentRadius, randomAngle);

                // Generates a path to the random point
                NavMeshPath navMeshPath = new NavMeshPath();
                NavMesh.CalculatePath(BotOwner.Transform.position, randomPoint, -1, navMeshPath);

                // Checks navMeshPath corners to see if a cover position is present along the path
                if (CheckCornersForCover(navMeshPath, currentRadius, out coverPoint))
                {
                    Logger.LogDebug($"Found Cover after [{iterations}] iterations");
                    return true;
                }

                // Increment the radius size if no cover found and start again
                currentRadius += incrementRadiusBase + (iterations * radiusIncrementStep);
                iterations++;
            }

            // If still no suitable point is found, return false
            coverPoint = null;
            return false;
        }

        private bool CheckCornersForCover(NavMeshPath navMeshPath, float currentRadius, out CustomCoverPoint coverPoint)
        {
            // Makes sure the length of the path is not too far from the bot, and that there are corners to check
            if (CheckPathLength(navMeshPath, currentRadius, out float distance) && navMeshPath.corners.Length > 2)
            {
                // Normalize corner 2 to check for cover points along its path to corner 3
                // Corner[0] is the bot position, so its not useful for us.
                Vector3 Corner1 = navMeshPath.corners[1];
                Vector3 Corner2 = navMeshPath.corners[2].normalized;

                const int maxCornerIterations = 5;
                const float lerpStep = 0.2f;

                int cornerIterations = 0;
                while (cornerIterations < maxCornerIterations)
                {
                    // Lerp from corner 1 to corner 2 normalized in even steps based on the iteration count to check for cover points every 0.2 meters
                    Vector3 cornerLerped = Vector3.Lerp(Corner1, Corner2, lerpStep * cornerIterations);

                    if (CheckCover(cornerLerped, out coverPoint))
                    {
                        coverPoint.CoverDistance = distance;
                        coverPoint.NavMeshPath = navMeshPath;
                        Logger.LogDebug($"CheckCornersForCover: Found Cover after [{cornerIterations}] Corner Iterations");
                        return true;
                    }
                    // if not, keep checking
                    cornerIterations++;
                }
            }
            // No cover found along this path.
            coverPoint = null;
            return false;
        }

        public bool CheckCover(Vector3 position, out CustomCoverPoint coverPoint)
        {
            if (BotOwner.Memory.GoalEnemy == null)
            {
                coverPoint = null;
                return false;
            }
            if (SimpleCheck(position))
            {
                DebugDrawer.Ray(position, Vector3.up, 1.5f, 0.1f, Color.red, 1f);
                // Check if the lerped value is cover or not
                coverPoint = Analyzer.AnalyseCoverPosition(position);
                // Is there any cover at this position?
                if (coverPoint != null)
                {
                    return true;
                }
            }
            coverPoint = null;
            return false;
        }

        private bool SimpleCheck(Vector3 pos)
        {
            Vector3 rayCheck = pos;
            rayCheck.y += 1f;
            Vector3 direction = BotOwner.Memory.GoalEnemy.CurrPosition - pos;
            float distance = Vector3.Distance(pos, BotOwner.Memory.GoalEnemy.CurrPosition);
            if (Physics.Raycast(rayCheck, direction, distance, LayerMaskClass.HighPolyWithTerrainMask))
            {
                return true;
            }
            return false;
        }

        private bool CheckPathLength(NavMeshPath navMeshPath, float currentRadius, out float distance)
        {
            distance = navMeshPath.CalculatePathLength();
            return distance < currentRadius * 1.2f;
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

        private bool CoverViability(float iterations)
        {
            return false;
        }
        private Vector3 EnemyPosition => BotOwner.Memory.GoalEnemy.CurrPosition;
        private Vector3 BotPosition => BotOwner.Transform.position;

        protected ManualLogSource Logger;
        private readonly BotOwner BotOwner;
    }
}
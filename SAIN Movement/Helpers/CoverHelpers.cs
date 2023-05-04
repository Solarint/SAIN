using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;
using static HairRenderer;

namespace Movement.Helpers
{
    public class CoverFinder
    {
        public CoverFinder(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
            Analyzer = new CoverAnalyzer(bot);
            Corners = new Corners.CornerProcessing(bot);
        }

        protected ManualLogSource Logger;
        private readonly BotOwner BotOwner;
        private readonly Corners.CornerProcessing Corners;

        public CoverAnalyzer Analyzer { get; private set; }
        public Vector3? CoverPosition { get; private set; }
        public Vector3[] SavedCoverPositions { get; private set; }

        /// <summary>
        /// Finds a fallback position for the bot to take cover in.
        /// </summary>
        /// <param name="coverPosition">The fallback position.</param>
        /// <param name="debugMode">Whether to enable debug mode.</param>
        /// <param name="debugDrawAll">Whether to draw all debug information.</param>
        /// <returns>Whether a fallback position was found.</returns>
        public bool FindFallbackPosition(out Vector3? coverPosition, bool debugMode = false, bool debugDrawAll = false)
        {
            if (BotOwner.Memory.GoalEnemy != null)
            {
                coverPosition = FindCover(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition, debugMode, debugDrawAll);

                if (coverPosition != null)
                {
                    //CoverPosition = coverPosition;
                    //SavedCoverPositions.AddToArray(coverPosition.Value);
                    return true;
                }
            }

            coverPosition = null;
            return false;
        }

        private float DebugTimer = 0f;

        /// <summary>
        /// Finds a suitable cover position for the bot to hide from the target.
        /// </summary>
        /// <param name="bot">The bot's current position.</param>
        /// <param name="target">The target's current position.</param>
        /// <param name="debugMode">Whether or not to enable debug mode.</param>
        /// <param name="debugDrawAll">Whether or not to draw all debug information.</param>
        /// <returns>
        /// Returns a Vector3 of the cover position if found, otherwise returns null.
        /// </returns>
        private Vector3? FindCover(Vector3 bot, Vector3 target, bool debugMode = false, bool debugDrawAll = false)
        {
            const float incrementRadiusBase = 20f;
            const float radiusIncrementStep = 10f;
            float currentRadius = incrementRadiusBase;

            const int maxIterations = 10;

            int iterations = 0;
            while (iterations < maxIterations)
            {
                // Generates a random point in a 240 degree arc away from the enemy
                Vector3 randomPoint = FindArcPoint(bot, target, currentRadius, Random.Range(-120, 120), debugDrawAll);

                // Generates a path to the random point and returns corners that have been trimmed and processed
                Vector3[] corners = Corners.GetCorners(randomPoint, true, true);

                // Checks those corners to see if a coverposition is present along the path
                Vector3? potentialCover = CheckCornersForCover(corners, currentRadius);

                // If Cover was found, potentialCover will not be null
                if (potentialCover != null)
                {
                    return potentialCover;
                }

                // Increment the radius size if no cover found and start again
                currentRadius += incrementRadiusBase + (iterations * radiusIncrementStep);
                iterations++;
            }

            // If still no suitable point is found, return null
            return null;
        }

        /// <summary>
        /// Checks the corners of a path for cover points.
        /// </summary>
        /// <param name="corners">The corners of the path.</param>
        /// <param name="currentRadius">The current radius of the path.</param>
        /// <returns>
        /// Returns a Vector3 if a cover point is found, otherwise returns null.
        /// </returns>
        private Vector3? CheckCornersForCover(Vector3[] corners, float currentRadius)
        {
            const int maxCornerIterations = 5;
            const float lerpStep = 0.2f;

            // Makes sure the length of the path is not too far from the bot, and that there are corners to check
            if (CheckPathLength(currentRadius) && corners.Length > 2)
            {
                // Normalize corner 2 to check for cover points along its path to corner 3
                Vector3 cornerNormalized = corners[2].normalized;

                int cornerIterations = 0;
                while (cornerIterations < maxCornerIterations)
                {
                    // Lerp from corner 1 to corner 2 normalized in even steps based on the iteration count to check for cover points every 0.2 meters
                    cornerNormalized = Vector3.Lerp(corners[1], cornerNormalized, lerpStep * cornerIterations);

                    // Draw a bunch of bullshit
                    DebugDraw(cornerNormalized, lerpStep, cornerIterations);

                    // Check if the lerped value is cover or not
                    if (CheckIfCoverGood(cornerNormalized, 1f))
                    {
                        return cornerNormalized;
                    }
                    // if not, keep checking
                    cornerIterations++;
                }
            }
            // No cover found along this path.
            return null;
        }

        /// <summary>
        /// Checks if there is good cover at a given position.
        /// </summary>
        /// <param name="cornerNormalized">The normalized corner position.</param>
        /// <param name="iterations">The number of iterations.</param>
        /// <param name="debugMode">Whether debug mode is enabled.</param>
        /// <returns>True if there is good cover at the given position, false otherwise.</returns>
        private bool CheckIfCoverGood(Vector3 cornerNormalized, float iterations, bool debugMode = false)
        {
            // Is there any cover at this position?
            if (Analyzer.AnalyseCoverPosition(cornerNormalized, debugMode))
            {
                // if so, how good is it?
                if (CoverViability(iterations))
                {
                    Logger.LogDebug("Found Cover!");
                    return true;
                }
            }
            // No cover here
            return false;
        }

        /// <summary>
        /// Checks if the path length is less than the current radius multiplied by 1.5f.
        /// </summary>
        /// <returns>True if the path length is less than the current radius multiplied by 1.5f, false otherwise.</returns>
        private bool CheckPathLength(float currentRadius)
        {
            return Corners.NavMeshPath.CalculatePathLength() < currentRadius * 1.5f;
        }

        /// <summary>
        /// Finds the point on an arc between two points with a given radius and angle.
        /// </summary>
        /// <param name="bot">The starting point of the arc.</param>
        /// <param name="target">The ending point of the arc.</param>
        /// <param name="arcRadius">The radius of the arc.</param>
        /// <param name="inputAngle">The angle of the arc.</param>
        /// <param name="debugDrawAll">Whether to draw the arc for debugging.</param>
        /// <returns>The point on the arc.</returns>
        public Vector3 FindArcPoint(Vector3 bot, Vector3 target, float arcRadius, float inputAngle, bool debugDrawAll = false)
        {
            Vector3 forward = bot - target;
            forward.y = 0.0f;
            forward.Normalize();

            Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);

            float angle = Mathf.Clamp(inputAngle, -120.0f, 120.0f);
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * right;
            Vector3 edgePosition = bot + direction * arcRadius;

            return edgePosition;
        }

        /// <summary>
        /// Draws a debug sphere at the given cornerNormalized position with a size, color, and expire time based on the lerpStep and cornerIterations.
        /// </summary>
        /// <param name="cornerNormalized">The normalized corner position.</param>
        /// <param name="lerpStep">The lerp step.</param>
        /// <param name="cornerIterations">The corner iterations.</param>
        private void DebugDraw(Vector3 cornerNormalized, float lerpStep, int cornerIterations)
        {
            float debugSphereSize = 0.1f + (lerpStep * cornerIterations * 0.1f);
            float debugExpireTime = 0.5f + (lerpStep * cornerIterations * 0.5f);
            Vector3 debugSpherePosition = cornerNormalized;
            debugSpherePosition.y += 0.25f + (lerpStep * cornerIterations * 0.25f);
            DebugDrawer.Sphere(cornerNormalized, debugSphereSize, Color.cyan, debugExpireTime);
        }

        /// <summary>
        /// Checks cover viability based on how many iterations the loop has gone through, the longer it goes, the more lax the restrictions.
        /// </summary>
        /// <param name="iterations">Number of iterations the loop has gone through</param>
        /// <returns>
        /// True if cover is viable, false otherwise.
        /// </returns>
        private bool CoverViability(float iterations)
        {
            if (Analyzer.FullCover)
            {
                return true;
            }
            else if (iterations > 70 && Analyzer.ChestCover && Analyzer.WaistCover && Analyzer.ProneCover)
            {
                return true;
            }
            else if (iterations > 85 && Analyzer.WaistCover && Analyzer.ProneCover)
            {
                return true;
            }
            else if (iterations > 95 && Analyzer.ProneCover)
            {
                return true;
            }

            return false;
        }
    }

    public class CoverAnalyzer
    {
        public CoverAnalyzer(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
        }

        private readonly BotOwner BotOwner;
        protected ManualLogSource Logger;

        public bool HeadCover { get; private set; }
        public bool ChestCover { get; private set; }
        public bool WaistCover { get; private set; }
        public bool ProneCover { get; private set; }
        public bool CanShoot { get; private set; }
        public Vector3? AcceptableCoverPosition { get; private set; }
        public bool FullCover { get; private set; }

        private Vector3 enemyPosition;

        /// <summary>
        /// Analyzes the cover position of the character and sets the appropriate cover values.
        /// </summary>
        /// <param name="coverPoint">The position of the cover.</param>
        /// <param name="head">Whether to analyze head cover.</param>
        /// <param name="chest">Whether to analyze chest cover.</param>
        /// <param name="waist">Whether to analyze waist cover.</param>
        /// <param name="prone">Whether to analyze prone cover.</param>
        /// <param name="reset">Whether to reset the previous saved cover values.</param>
        /// <returns>Whether the cover position is valid at all.</returns>
        public bool AnalyseCoverPosition(Vector3 coverPoint, bool debugMode = false)
        {
            ResetCoverValues();

            // CurrPosition is at ground level, so we need to raise it up
            enemyPosition = BotOwner.Memory.GoalEnemy.CurrPosition;
            enemyPosition.y += 1f;

            // Check each of the Bot's parts for visibility at the coverpoint
            CheckParts(coverPoint, debugMode);

            // Checks the results
            if (ChestCover && HeadCover & WaistCover && ProneCover)
            {
                FullCover = true;
            }

            // If we found cover, return true;
            if (HeadCover || ChestCover || WaistCover || ProneCover)
            {
                AcceptableCoverPosition = coverPoint;
                DebugCoverPosition(debugMode);
                return true;
            }
            else
            {
                Logger.LogWarning($"No Cover");
                return false;
            }
        }

        /// <summary>
        /// Checks the head, chest, waist, prone and shoot cover points for the given Vector3
        /// </summary>
        /// <param name="coverPoint">The Vector3 cover point to check.</param>
        private void CheckParts(Vector3 coverPoint, bool debugMode)
        {
            if (Head(coverPoint, debugMode))
            {
                HeadCover = true;
            }
            if (Chest(coverPoint, 1.15f, debugMode))
            {
                ChestCover = true;
            }
            if (Waist(coverPoint, 0.7f, debugMode))
            {
                WaistCover = true;
            }
            if (Prone(coverPoint, 0.3f, debugMode))
            {
                ProneCover = true;
            }
            if (Shoot(coverPoint, debugMode))
            {
                CanShoot = true;
            }
        }

        /// <summary>
        /// Resets all cover values to their default state.
        /// </summary>
        private void ResetCoverValues()
        {
            HeadCover = false;
            ChestCover = false;
            WaistCover = false;
            ProneCover = false;
            CanShoot = false;
            AcceptableCoverPosition = null;
            FullCover = false;
        }

        /// <summary>
        /// Draws debug lines to visualize the cover position for the bot owner.
        /// </summary>
        /// <param name="debugMode">Whether debug mode is enabled.</param>
        private void DebugCoverPosition(bool debugMode)
        {
            if (debugMode && DebugTimer < Time.time)
            {
                DebugTimer = Time.time + 1f;

                Logger.LogDebug($"Found GoodCover for {BotOwner.name}. Debug Lines are Blue");

                float botHeight = BotOwner.MyHead.position.y - BotOwner.Transform.position.y;

                DebugDrawer.Ray(AcceptableCoverPosition.Value, Vector3.up, botHeight, 0.25f, Color.blue, 2f);
                DebugDrawer.Line(AcceptableCoverPosition.Value, BotOwner.LookSensor._headPoint, 0.05f, Color.blue, 2f);
                DebugDrawer.Line(enemyPosition, BotOwner.LookSensor._headPoint, 0.05f, Color.blue, 2f);
                DebugDrawer.Line(enemyPosition, AcceptableCoverPosition.Value, 0.05f, Color.blue, 2f);
            }
        }

        /// <summary>
        /// Checks if a bot can shoot from a coverpoint
        /// </summary>
        /// <param name="coverPoint">The cover point to check from.</param>
        /// <returns>True if they can shoot.</returns>
        private bool Shoot(Vector3 coverPoint, bool debugMode = false)
        {
            coverPoint.y += BotOwner.WeaponRoot.position.y;
            if (!CheckVisiblity(coverPoint, enemyPosition, debugMode))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the head of the bot is covered by the given cover point.
        /// </summary>
        /// <param name="coverPoint">The cover point to check.</param>
        /// <param name="sphereSize">The size of the sphere to check.</param>
        /// <returns>True if the head is covered, false otherwise.</returns>
        private bool Head(Vector3 coverPoint, bool debugMode = false)
        {
            Vector3 myHeadPos = coverPoint;
            myHeadPos.y += BotOwner.LookSensor._headPoint.y;

            if (!CheckVisiblity(myHeadPos, enemyPosition, debugMode))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the chest is visible to the enemy gun position.
        /// </summary>
        /// <param name="coverPoint">The cover point.</param>
        /// <param name="chestY">The chest Y.</param>
        /// <param name="sphereSize">The size of the sphere.</param>
        /// <returns>
        /// Returns true if the chest is not visible to the enemy gun position, false otherwise.
        /// </returns>
        private bool Chest(Vector3 coverPoint, float chestY, bool debugMode = false)
        {
            Vector3 myChestPos = coverPoint;
            myChestPos.y += chestY;

            if (!CheckVisiblity(myChestPos, enemyPosition, debugMode))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the waist is covered by a sphere of given size.
        /// </summary>
        /// <param name="coverPoint">The point to check for waist cover.</param>
        /// <param name="waistY">The y-axis offset for the waist.</param>
        /// <param name="sphereSize">The size of the sphere to check for waist cover.</param>
        /// <returns>True if the waist is covered, false otherwise.</returns>
        private bool Waist(Vector3 coverPoint, float waistY, bool debugMode = false)
        {
            Vector3 myWaistPos = coverPoint;
            myWaistPos.y += waistY;

            if (!CheckVisiblity(myWaistPos, enemyPosition, debugMode))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the cover point is visible to the enemy gun position.
        /// </summary>
        /// <param name="coverPoint">The cover point to check.</param>
        /// <param name="enemyGunPos">The enemy gun position.</param>
        /// <param name="sphereSize">The size of the sphere to check.</param>
        /// <returns>True if the cover point is visible, false otherwise.</returns>
        private bool Prone(Vector3 coverPoint, float proneY, bool debugMode = false)
        {
            Vector3 myPronePos = coverPoint;
            myPronePos.y += proneY;

            if (!CheckVisiblity(myPronePos, enemyPosition, debugMode))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a line between two points is visible, using a spherecast.
        /// </summary>
        /// <param name="start">The start point of the line.</param>
        /// <param name="end">The end point of the line.</param>
        /// <param name="sphereSize">The size of the sphere used for the spherecast.</param>
        /// <returns>True if the line is visible, false otherwise.</returns>
        private bool CheckVisiblity(Vector3 start, Vector3 end, bool debugMode = false)
        {
            if (Physics.Linecast(start, end, out RaycastHit hit, LayerMaskClass.HighPolyWithTerrainMask) && hit.transform.name != BotOwner.Memory.GoalEnemy.Owner.gameObject.transform.name)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private float DebugTimer = 0f;
    }
}
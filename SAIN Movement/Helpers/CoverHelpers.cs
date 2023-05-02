using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;

namespace Movement.Helpers
{
    public class CoverFinder
    {
        public CoverFinder(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
            Analyzer = new CoverAnalyzer(bot);
        }

        protected ManualLogSource Logger;
        private readonly BotOwner BotOwner;

        public CoverAnalyzer Analyzer { get; private set; }
        public Vector3? CoverPosition { get; private set; }

        public bool FindFallbackPosition(out Vector3? coverPosition, bool debugMode = false, bool debugDrawAll = false)
        {
            if (BotOwner.Memory.GoalEnemy != null)
            {
                coverPosition = FindCover(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition, 3f, 3f, debugMode, debugDrawAll);

                if (CoverPosition != null)
                {
                    CoverPosition = coverPosition;
                    return true;
                }
            }

            coverPosition = null;
            return false;
        }

        private Vector3? FindCover(Vector3 bot, Vector3 target, float initialArcRadius, float radiusIncrementStep, bool debugMode = false, bool debugDrawAll = false)
        {
            const float incrementAngle = 10.0f;
            const float incrementRadiusBase = 5.0f;
            const int maxIterations = 10;

            float currentRadius = initialArcRadius;
            int iterations = 0;

            while (iterations < maxIterations)
            {
                for (float angle = -120.0f; angle <= 120.0f; angle += incrementAngle)
                {
                    Vector3 potentialCover = FindArcPoint(bot, target, currentRadius, angle, debugDrawAll);

                    if (CheckCover(potentialCover, debugMode))
                    {
                        return potentialCover;
                    }
                }

                float incrementRadius = incrementRadiusBase + (iterations * radiusIncrementStep);
                currentRadius += incrementRadius;
                iterations++;
            }

            // If no suitable point is found, return the bot's position as a fallback
            return null;
        }

        private bool CheckCover(Vector3 point, bool debugMode = false)
        {
            if (NavMesh.SamplePosition(point, out var navHit, 0.5f, NavMesh.AllAreas))
            {
                if (Analyzer.AnalyseCoverPosition(navHit.position, debugMode))
                {
                    if (debugMode)
                        DebugDrawer.Ray(navHit.position, Vector3.up, 3f, 0.1f, Color.white, 3f);

                    return true;
                }
            }

            return false;
        }


        public Vector3 FindArcPoint(Vector3 bot, Vector3 target, float arcRadius, float inputAngle, bool debugDrawAll = false)
        {
            Vector3 forward = bot - target;
            forward.y = 0.0f;
            forward.Normalize();

            Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);

            float angle = Mathf.Clamp(inputAngle, -120.0f, 120.0f);
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * right;
            Vector3 edgePosition = bot + direction * arcRadius;

            if (debugDrawAll)
                DebugDrawer.Sphere(edgePosition, 0.05f, Color.white, 1f);

            return edgePosition;
        }
    }

    public class CoverAnalyzer
    {
        /// <summary>
        /// Constructor for CoverAnalyzer class.
        /// </summary>
        /// <param name="bot">The BotOwner object.</param>
        /// <returns>A CoverAnalyzer object.</returns>
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
        public bool GoodCover { get; private set; }

        private Vector3 enemyGunPos;

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
        public bool AnalyseCoverPosition(Vector3 coverPoint, bool head = true, bool chest = true, bool waist = true, bool prone = true, bool reset = true, bool debugMode = false)
        {
            if (reset)
            {
                HeadCover = false;
                ChestCover = false;
                WaistCover = false;
                ProneCover = false;
                CanShoot = false;
                GoodCover = false;
            }

            enemyGunPos = BotOwner.Memory.GoalEnemy.Owner.GetPlayer.PlayerBones.WeaponRoot.position;

            if (head)
                Head(coverPoint, 0.01f, debugMode);

            if (chest)
                Chest(coverPoint, 1.1f, 0.01f, debugMode);

            if (waist)
                Waist(coverPoint, 0.6f, 0.01f, debugMode);

            if (prone)
                Prone(coverPoint, 0.2f, 0.01f, debugMode);

            // Check if there is any cover at all
            if (!HeadCover && !ChestCover && !WaistCover && !ProneCover)
            {
                AcceptableCoverPosition = null;
                return false;
            }

            AcceptableCoverPosition = coverPoint;

            // Can a bot shoot from this position?
            Shoot(coverPoint, debugMode);

            if (WaistCover && ProneCover)
            {
                GoodCover = true;

                if (debugMode)
                {
                    Logger.LogDebug($"Found GoodCover for {BotOwner.name}");

                    Vector3 enemyPosition = BotOwner.Memory.GoalEnemy.CurrPosition;
                    enemyPosition.y += 1.3f;
                    float botHeight = BotOwner.MyHead.position.y - BotOwner.Transform.position.y;

                    DebugDrawer.Ray(coverPoint, Vector3.up, botHeight, 0.1f, Color.white, 20f);
                    DebugDrawer.Line(coverPoint, BotOwner.MyHead.position, 0.05f, Color.blue, 20f);
                    DebugDrawer.Line(BotOwner.Memory.GoalEnemy.CurrPosition, BotOwner.MyHead.position, 0.05f, Color.blue, 20f);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a bot can shoot from a coverpoint
        /// </summary>
        /// <param name="coverPoint">The cover point to check from.</param>
        /// <returns>True if they can shoot.</returns>
        private bool Shoot(Vector3 coverPoint, bool debugMode = false)
        {
            Vector3 myHeadPos = coverPoint;
            myHeadPos.y += BotOwner.LookSensor._headPoint.y;

            if (!CheckVisiblity(BotOwner.WeaponRoot.position, enemyGunPos, 0.01f, debugMode))
            {
                CanShoot = true;
                return true;
            }
            else
            {
                CanShoot = false;
                return false;
            }
        }

        /// <summary>
        /// Checks if the head of the bot is covered by the given cover point.
        /// </summary>
        /// <param name="coverPoint">The cover point to check.</param>
        /// <param name="sphereSize">The size of the sphere to check.</param>
        /// <returns>True if the head is covered, false otherwise.</returns>
        private bool Head(Vector3 coverPoint, float sphereSize, bool debugMode = false)
        {
            Vector3 myHeadPos = coverPoint;
            myHeadPos.y += BotOwner.LookSensor._headPoint.y;

            if (!CheckVisiblity(myHeadPos, enemyGunPos, sphereSize, debugMode))
            {
                HeadCover = true;
                return true;
            }
            else
            {
                HeadCover = false;
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
        private bool Chest(Vector3 coverPoint, float chestY, float sphereSize, bool debugMode = false)
        {
            Vector3 myChestPos = coverPoint;
            myChestPos.y += chestY;

            if (!CheckVisiblity(myChestPos, enemyGunPos, sphereSize, debugMode))
            {
                ChestCover = true;
                return true;
            }
            else
            {
                ChestCover = false;
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
        private bool Waist(Vector3 coverPoint, float waistY, float sphereSize, bool debugMode = false)
        {
            Vector3 myWaistPos = coverPoint;
            myWaistPos.y += waistY;

            if (!CheckVisiblity(myWaistPos, enemyGunPos, sphereSize, debugMode))
            {
                WaistCover = true;
                return true;
            }
            else
            {
                WaistCover = false;
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
        private bool Prone(Vector3 coverPoint, float proneY, float sphereSize, bool debugMode = false)
        {
            Vector3 myPronePos = coverPoint;
            myPronePos.y += proneY;

            if (!CheckVisiblity(myPronePos, enemyGunPos, sphereSize, debugMode))
            {
                ProneCover = true;
                return true;
            }
            else
            {
                ProneCover = false;
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
        private bool CheckVisiblity(Vector3 start, Vector3 end, float sphereSize, bool debugMode = false)
        {
            Vector3 direction = end - start;
            Ray ray = new Ray(start, direction);
            float distance = Vector3.Distance(start, end);

            if (Physics.SphereCast(ray, sphereSize, out RaycastHit hit, distance))
            {
                if (debugMode)
                    DebugDrawer.Line(end, hit.point, 0.01f, Color.white, 20f);

                return false;
            }
            else
            {
                if (debugMode)
                    DebugDrawer.Line(end, hit.point, 0.01f, Color.red, 20f);

                return true;
            }
        }
    }
}
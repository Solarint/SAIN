using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using UnityEngine;
using static Movement.UserSettings.Debug;

namespace Movement.Helpers
{
    /// <summary>
    /// Analyzes Vector3 Positions using raycasts to see if they are useful cover points for bots
    /// </summary>
    public class CoverAnalyzer
    {
        public CoverAnalyzer(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
        }

        private bool DebugMode => DebugCoverSystem.Value;

        /// <summary>
        /// Analyzes a Vector3 and checks if the bot is visible from it, and if so, by how much.
        /// </summary>
        /// <param name="targetPos">The position of the enemy.</param>
        /// <param name="coverPos">The position of the cover.</param>
        /// <param name="coverPoint">The CustomCoverPoint object.</param>
        /// <param name="minCoverLevel">The minimum cover level.</param>
        /// <returns>True if the cover position is valid, false otherwise.</returns>
        public bool CheckPosition(Vector3 targetPos, Vector3 coverPos, out CustomCoverPoint coverPoint, float minCoverLevel = 0.5f)
        {
            // Assign values for use in other methods
            Target = targetPos;
            CoverPosition = coverPos;

            // Calculate Cover Viability
            float coverRatio = CalculateRatio();
            bool goodCover = coverRatio >= minCoverLevel;
            bool canShoot = !SightBlocked(BotOwner.WeaponRoot.position);

            // Check is cover is viable, if so create a new CustomCoverPoint to return
            coverPoint = goodCover ? new CustomCoverPoint(BotOwner.Transform.position, coverPos, coverRatio, canShoot) : null;

            // Return true if cover meets requirements
            return goodCover;
        }

        private float CalculateRatio()
        {
            int coverScoreCount = 0;
            int bodyPartCount = 0;

            // Check each body part on a bot to see if it will be visible at the cover position
            foreach (var part in BotOwner.MainParts.Values)
            {
                // Count the body part
                bodyPartCount++;

                // Check is that body part has line of sight
                if (SightBlocked(part.Position))
                {
                    // Count the blocked body part
                    coverScoreCount++;
                }
            }

            // Our cover amount is the number of blocked body parts divided by the total body parts
            return (float)coverScoreCount / bodyPartCount;
        }

        /// <summary>
        /// Checks if the sight from a given part to the target is blocked by an object.
        /// </summary>
        /// <param name="part">The part to check from.</param>
        /// <returns>True if the sight is blocked, false otherwise.</returns>
        private bool SightBlocked(Vector3 part)
        {
            // Assign the mask we are using for the raycast
            var mask = LayerMaskClass.HighPolyWithTerrainMaskAI;

            // Find the position our part will be at at the potential cover position
            var partPos = PartPosition(part);

            // Find direction from part to target
            var direction = Target - partPos;

            // Is line of sight blocked at this position for this part?
            bool sightBlocked = Physics.Raycast(partPos, direction, direction.magnitude, mask);

            // Debug
            if (sightBlocked && DebugMode)
            {
                DebugDrawer.Ray(partPos, direction, direction.magnitude, 0.01f, Color.magenta, 0.1f);
            }

            // Return true if sight is blocked
            return sightBlocked;
        }

        /// <summary>
        /// Checks the visibility a part will have at the potential cover position
        /// </summary>
        /// <param name="part">The part to calculate the position of.</param>
        private Vector3 PartPosition(Vector3 part)
        {
            return part - BotOwner.Transform.position + CoverPosition;
        }

        private readonly BotOwner BotOwner;
        protected ManualLogSource Logger;
        private Vector3 Target;
        private Vector3 CoverPosition;
    }
}
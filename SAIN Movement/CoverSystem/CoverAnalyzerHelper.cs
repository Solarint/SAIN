using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using System.Collections.Generic;
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
        /// Analyzes a Vector3 and checks if the Player is visible from it, and if so, by how much.
        /// </summary>
        /// <param name="targetPos">The position of the enemy.</param>
        /// <param name="coverPos">The position of the cover.</param>
        /// <param name="coverPoint">The CustomCoverPoint object.</param>
        /// <param name="minCoverLevel">The minimum cover level.</param>
        /// <returns>True if the cover position is valid, false otherwise.</returns>
        public bool CheckPosition(Vector3 targetPos, Vector3 coverPos, out CustomCoverPoint coverPoint, float minCoverLevel = 0.5f, int numberOfRaycasts = 6)
        {
            if (BotOwner.Memory.GoalEnemy == null)
            {
                coverPoint = null;
                return false;
            }

            BotOwner.Memory.GoalEnemy.Person.MainParts.TryGetValue(BodyPartType.head, out BodyPartClass EnemyHead);
            Target = EnemyHead.Position;
            CoverPosition = coverPos;

            // Calculate Cover Viability
            float coverRatio = CheckCoverLevel(numberOfRaycasts);
            bool goodCover = coverRatio >= minCoverLevel;

            // CheckForCalcPath is cover is viable, if so create a new CustomCoverPoint to return
            coverPoint = goodCover ? new CustomCoverPoint(BotOwner.Transform.position, coverPos, coverRatio, CanBotShoot()) : null;

            // Return true if cover meets requirements
            return goodCover;
        }

        /// <summary>
        /// Calculates the cover level of the player by performing raycasts from the enemy's position to each point within the player's bounding box.
        /// </summary>
        /// <param name="numberOfRaycasts">The number of raycasts to perform.</param>
        /// <returns>The cover level as a proportion of the total number of rays cast. With 1 being full cover, and 0 being no cover.</returns>
        private float CheckCoverLevel(int numberOfRaycasts)
        {
            Bounds playerBounds = BotOwner.GetPlayer.gameObject.GetComponent<Collider>().bounds;
            Vector3 size = playerBounds.size;
            Vector3 min = playerBounds.min;

            int coverScoreCount = 0;

            int subdivisions = Mathf.CeilToInt(Mathf.Pow(numberOfRaycasts, 1f / 3f));

            int rayCasts = 0;

            // Perform raycasts from the origin to each evenly spaced point within the player's bounding box
            for (int x = 0; x < subdivisions; x++)
            {
                for (int y = 0; y < subdivisions; y++)
                {
                    for (int z = 0; z < subdivisions; z++)
                    {
                        float lerpX = (float)x / (subdivisions - 1);
                        float lerpY = (float)y / (subdivisions - 1);
                        float lerpZ = (float)z / (subdivisions - 1);

                        Vector3 targetPoint = new Vector3(
                            min.x + size.x * lerpX,
                            min.y + size.y * lerpY,
                            min.z + size.z * lerpZ
                        );

                        rayCasts++;
                        targetPoint = targetPoint - BotOwner.Transform.position + CoverPosition;
                        Vector3 direction = targetPoint - Target;
                        if (Physics.Raycast(Target, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                        {
                            coverScoreCount++;
                        }
                    }
                }
            }
            //Logger.LogDebug($"CoverScore: [{coverScoreCount}]. RayCasts: [{rayCasts}]. Ratio: [{(float)coverScoreCount / rayCasts}].");
            // Return the cover score as a proportion of the total number of rays cast
            return (float)coverScoreCount / rayCasts;
        }

        /// <summary>
        /// Checks if the bot can shoot at the enemy by raycasting from the weapon to the enemy body parts.
        /// </summary>
        /// <returns>Returns true if the bot can shoot at the enemy, false otherwise. Returns false if the GoalEnemy is null</returns>
        public bool CanBotShoot()
        {
            if (BotOwner.Memory.GoalEnemy == null)
            {
                return false;
            }
            foreach (var part in BotOwner.Memory.GoalEnemy.Person.MainParts.Values)
            {
                // Assign the mask we are using for the raycast
                var mask = LayerMaskClass.HighPolyWithTerrainMaskAI;

                // Find the position our weapon will be at at the potential cover position
                var gunPosition = BotOwner.WeaponRoot.position - BotOwner.Transform.position + CoverPosition;

                // Find direction from weapon to enemy bodyparts
                var direction = part.Position - gunPosition;

                // Is there a clear line of sight for a bot to shoot this body part?
                if (!Physics.Raycast(gunPosition, direction, direction.magnitude, mask))
                {
                    if (DebugMode)
                    {
                        DebugDrawer.Ray(gunPosition, direction, direction.magnitude, 0.01f, Color.magenta, 0.1f);
                    }
                    return true;
                }
            }
            return false;
        }


        // old
        private float CalculateRatio()
        {
            int coverScoreCount = 0;
            int bodyPartCount = 0;

            // CheckForCalcPath each body part on a Player to see if it will be visible at the cover position
            foreach (var part in BotOwner.MainParts.Values)
            {
                // Count the body part
                bodyPartCount++;

                // CheckForCalcPath is that body part has line of sight
                if (SightBlocked(part.Position))
                {
                    // Count the blocked body part
                    coverScoreCount++;
                }
            }

            // Our cover amount is the number of blocked body parts divided by the total body parts
            return (float)coverScoreCount / bodyPartCount;
        }
        private bool SightBlocked(Vector3 part)
        {
            // Assign the mask we are using for the raycast
            var mask = LayerMaskClass.HighPolyWithTerrainMaskAI;

            // Find the position our part will be at at the potential cover position
            var partPos = part - BotOwner.Transform.position + CoverPosition;

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

        private readonly BotOwner BotOwner;
        protected ManualLogSource Logger;
        private Vector3 Target;
        private Vector3 CoverPosition;
    }
}
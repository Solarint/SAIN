using BepInEx.Logging;
using EFT;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;
using SAIN;
using SAIN_Helpers;

namespace SAIN.Helpers
{
    /// <summary>
    /// Analyzes Vector3 Positions using raycasts to see if they are useful cover points for bots
    /// </summary>
    public class CoverAnalyzer : SAINBotExt
    {
        public CoverAnalyzer(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        /// <summary>
        /// Analyzes a Vector3 and checks if the BotOwner is visible from it, and if so, by how much.
        /// </summary>
        /// <param name="targetPos">The position of the enemy.</param>
        /// <param name="coverPos">The position of the cover.</param>
        /// <param name="coverPoint">The CustomCoverPoint object.</param>
        /// <param name="minCoverLevel">The minimum cover level.</param>
        /// <returns>True if the cover position is valid, false otherwise.</returns>
        public bool CheckPosition(Vector3 coverPos, out CustomCoverPoint coverPoint, float minCoverLevel = 0.5f)
        {
            if (BotOwner.Memory.GoalEnemy == null)
            {
                coverPoint = null;
                return false;
            }

            BotOwner.Memory.GoalEnemy.Person.MainParts.TryGetValue(BodyPartType.head, out BodyPartClass EnemyHead);

            // Calculate Cover Viability
            float coverRatio = CheckCoverLevel(EnemyHead.Position, coverPos);
            bool goodCover = coverRatio >= minCoverLevel;

            // CheckForCalcPath is cover is viable, if so create a new CustomCoverPoint to return
            coverPoint = goodCover ? new CustomCoverPoint(BotOwner.Transform.position, coverPos, coverRatio, true) : null;

            // Return true if cover meets requirements
            if (goodCover)
            {
                //DebugDrawer.Sphere(coverPos, 0.25f, Color.blue, 15f);
                //DebugDrawer.Line(coverPos, BotOwner.MyHead.position, 0.1f, SAIN.Core.BotColor, 0.1f);
            }

            return goodCover;
        }

        /// <summary>
        /// Calculates the cover level of the player by performing raycasts from the enemy's position to each point within the player's bounding box.
        /// </summary>
        /// <param name="numberOfRaycasts">The number of raycasts to perform.</param>
        /// <returns>The cover level as a proportion of the total number of rays cast. With 1 being full cover, and 0 being no cover.</returns>
        private float CheckCoverLevel(Vector3 enemyPos, Vector3 coverPos)
        {
            int raycasts = 0;
            int cover = 0;

            foreach (var part in BotOwner.GetPlayer.MainParts.Values)
            {
                //DebugDrawer.Line(part.Position, BotOwner.Transform.position, 0.1f, Color.white, 0.25f);
                raycasts++;

                Vector3 coverPart = part.Position;
                coverPart -= BotOwner.Transform.position + coverPos;

                //DebugDrawer.Line(coverPart, coverPos, 0.1f, Color.white, 5f);

                Vector3 directions = coverPart - enemyPos;

                if (Physics.Raycast(enemyPos, directions, directions.magnitude, Components.SAINCoreComponent.SightMask))
                {
                    cover++;
                }
            }

            return (float)cover / raycasts;
        }

        protected ManualLogSource Logger;
    }
}
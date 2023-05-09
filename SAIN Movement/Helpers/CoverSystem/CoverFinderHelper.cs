using BepInEx.Logging;
using EFT;
using Movement.Components;
using UnityEngine;
using static Movement.UserSettings.Debug;

namespace Movement.Helpers
{
    /// <summary>
    /// Finds Points that can be checked to see if they provide cover for a Player
    /// </summary>
    public class CoverFinder
    {
        public CoverFinder(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
            Analyzer = new CoverAnalyzer(bot);
            CoverComponent = bot.gameObject.GetComponent<CoverFinderComponent>();
        }

        public CoverFinderComponent CoverComponent { get; private set; }

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
            coverPoint = null;
            var Points = CoverComponent.CloseCoverPoints;

            int i = 0;
            while (i < Points.Count)
            {
                if (Analyzer.CheckPosition(enemyPosition, Points[i].Position, out coverPoint, minCoverLevel))
                {
                    return true;
                }
                i++;
            }

            return false;
        }

        /// <summary>
        /// Calculates the position on an arc between two points with a given radius and angle.
        /// </summary>
        /// <param name="botPos">The position of the Player.</param>
        /// <param name="targetPos">The position of the target.</param>
        /// <param name="arcRadius">The radius of the arc.</param>
        /// <param name="angle">The angle of the arc.</param>
        /// <returns>The position on the arc.</returns>
        public static Vector3 FindArcPoint(Vector3 botPos, Vector3 targetPos, float arcRadius, float angle)
        {
            Vector3 direction = (botPos - targetPos).normalized;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 arcPoint = arcRadius * (rotation * direction);

            return botPos + arcPoint;
        }

        private bool DebugMode => DebugCoverSystem.Value;
        public CoverAnalyzer Analyzer { get; private set; }

        protected ManualLogSource Logger;
        private readonly BotOwner BotOwner;
    }
}
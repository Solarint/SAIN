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

        /// <summary>
        /// Analyzes a Vector3 and checks if the bot is visible from it, and if so, by how much.
        /// </summary>
        /// <param name="enemyPosition">The position of the enemy.</param>
        /// <param name="coverPosition">The position of the cover.</param>
        /// <param name="coverPoint">The CustomCoverPoint object.</param>
        /// <param name="minCoverLevel">The minimum cover level.</param>
        /// <returns>True if the cover position is valid, false otherwise.</returns>
        public bool CheckPosition(Vector3 enemyPosition, Vector3 coverPosition, out CustomCoverPoint coverPoint, float minCoverLevel = 0.5f)
        {
            coverPoint = null;

            TargetPosition = enemyPosition;
            CoverPosition = coverPosition;
            MinimumCover = minCoverLevel;

            if (CheckParts)
            {
                coverPoint = new CustomCoverPoint(BotPosition, coverPosition, CoverRatio, !IsVisible(WeaponPos));
                return true;
            }

            return false;
        }

        private bool CheckParts
        {
            get
            {
                var parts = BotOwner.MainParts;
                int coverScoreCount = 0;
                int bodyPartCount = 0;

                foreach (var part in parts.Values)
                {
                    bodyPartCount++;

                    if (IsVisible(part.Position))
                    {
                        coverScoreCount++;
                    }
                }

                CoverRatio = (float)coverScoreCount / bodyPartCount;
                return CoverRatio >= MinimumCover;
            }
        }

        private bool IsVisible(Vector3 point)
        {
            Vector3 partPosition = Position(point);
            Vector3 enemyDirection = Direction(point);
            float enemyDistance = Distance(enemyDirection);

            if (Physics.Raycast(partPosition, enemyDirection, enemyDistance, Mask))
            {
                if (DebugMode)
                {
                    DebugDrawer.Ray(partPosition, enemyDirection, enemyDistance, 0.025f, Color.green, 2f);
                }

                return true;
            }

            return false;
        }

        private Vector3 Direction(Vector3 point)
        {
            return Target() - point;
        }

        private Vector3 Target()
        {
            Vector3 target = TargetPosition;
            target.y += EnemyYOffset;
            return target;
        }

        private float Distance(Vector3 direction)
        {
            return direction.magnitude;
        }

        private Vector3 Position(Vector3 part)
        {
            return part - BotPosition + CoverPosition;
        }

        private bool DebugMode => DebugCoverSystem.Value;
        private Vector3 BotPosition => BotOwner.Transform.position;
        private LayerMask Mask => LayerMaskClass.HighPolyWithTerrainMaskAI;
        private Vector3 WeaponPos => BotOwner.WeaponRoot.position;

        private const float EnemyYOffset = 1.4f;

        private float CoverRatio = 1f;
        private readonly BotOwner BotOwner;
        protected ManualLogSource Logger;
        private Vector3 TargetPosition;
        private Vector3 CoverPosition;
        private float MinimumCover;
    }
}
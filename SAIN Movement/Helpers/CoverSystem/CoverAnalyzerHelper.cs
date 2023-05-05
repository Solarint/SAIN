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

        public bool AnalyseCoverPosition(Vector3 enemyPosition, Vector3 coverPosition, out CustomCoverPoint coverPoint, float minCoverLevel = 0.5f)
        {
            TargetPosition = enemyPosition;

            if (RayCastBodyParts(coverPosition, out float coverscore, out bool canShoot, minCoverLevel))
            {
                coverPoint = new CustomCoverPoint(BotOwner.Transform.position, coverPosition, coverscore, canShoot);
                return true;
            }
            else
            {
                coverPoint = null;
                return false;
            }
        }

        private bool RayCastBodyParts(Vector3 coverPoint, out float coverAmount, out bool canShoot, float minCoverLevel = 0.5f)
        {
            var parts = BotOwner.MainParts;
            int coverScoreCount = 0;
            int bodyPartCount = 0;
            foreach (var part in parts.Values)
            {
                bodyPartCount++;
                Vector3 partLocalPosition = part.Position - BotPosition;
                Vector3 partPositionFromCover = partLocalPosition + coverPoint;

                float distance = Vector3.Distance(partPositionFromCover, TargetPosition) / 2f;
                Ray ray = new Ray(partPositionFromCover, EnemyDirectionFromPoint(partPositionFromCover));
                if (Physics.Raycast(ray, out RaycastHit hit, distance, Mask))
                {
                    if (DebugMode)
                    {
                        Vector3 trgPos = TargetPosition;
                        trgPos.y += 1f;
                        DebugDrawer.Line(trgPos, hit.point, 0.1f, Color.red, 2f);
                    }

                    coverScoreCount++;
                }
            }

            if (Shoot(coverPoint))
            {
                canShoot = true;
            }
            else
            {
                canShoot = false;
            }

            float Ratio = (float)coverScoreCount / bodyPartCount;

            coverAmount = Ratio;

            if (coverAmount >= minCoverLevel)
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"True! Ratio: {Ratio} because min level {minCoverLevel}");
                }
                return true;
            }
            else
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"False! Ratio: {Ratio} because min level {minCoverLevel}");
                }
                return false;
            }
        }

        private bool Shoot(Vector3 coverPoint)
        {
            if (BotOwner.WeaponRoot != null)
            {
                if (!CheckVisiblity(coverPoint, BotOwner.WeaponRoot.position))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckVisiblity(Vector3 coverPoint, Vector3 partPos)
        {
            Vector3 partLocalPosition = partPos - BotPosition;

            Vector3 partPositionFromCover = partLocalPosition + coverPoint;

            float distance = Vector3.Distance(partPositionFromCover, TargetPosition) / 2f;
            Ray ray = new Ray(partPositionFromCover, EnemyDirectionFromPoint(partPositionFromCover));

            string botTransformName = BotOwner.gameObject.transform.name;
            if (Physics.Raycast(ray, out RaycastHit hit, distance, Mask) && hit.transform.name != botTransformName)
            {
                return false;
            }
            return true;
        }

        private Vector3 EnemyDirectionFromPoint(Vector3 point)
        {
            Vector3 target = TargetPosition;
            target.y += 1.3f;
            return TargetPosition - point;
        }

        private bool DebugMode => DebugCoverSystem.Value;
        private Vector3 BotPosition => BotOwner.Transform.position;
        private LayerMask Mask => LayerMaskClass.HighPolyWithTerrainMask;

        private readonly BotOwner BotOwner;
        protected ManualLogSource Logger;
        private Vector3 TargetPosition;
    }
}
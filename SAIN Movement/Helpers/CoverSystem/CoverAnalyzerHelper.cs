using BepInEx.Logging;
using EFT;
using HarmonyLib;
using SAIN_Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

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

        private readonly BotOwner BotOwner;
        protected ManualLogSource Logger;

        public bool CanShoot { get; private set; }
        public bool FullCover { get; private set; }

        private Vector3 enemyPosition;

        public CustomCoverPoint AnalyseCoverPosition(Vector3 coverPoint)
        {
            CanShoot = false;
            FullCover = false;

            // CurrPosition is at ground level, so we need to raise it up
            enemyPosition = BotOwner.Memory.GoalEnemy.CurrPosition;
            enemyPosition.y += 1f;

            if (RayCastBodyParts(coverPoint, out float coverscore))
            {
                return new CustomCoverPoint(coverPoint, coverscore, 0f, null);
            }
            else
            {
                return null;
            }
        }

        public bool RayCastBodyParts(Vector3 coverPoint, out float coverAmount)
        {
            var parts = BotOwner.MainParts;
            int coverScoreCount = 0;
            int bodyPartCount = 0;
            foreach (var part in parts.Values)
            {
                bodyPartCount++;
                Vector3 partLocalPosition = part.Position - BotOwner.Transform.position;
                Vector3 partPositionFromCover = partLocalPosition + coverPoint;

                Ray ray = new Ray(partPositionFromCover, EnemyDirectionFromPoint(partPositionFromCover));
                if (Physics.Raycast(ray, out RaycastHit hit, 3f, Mask))
                {
                    DebugDrawer.Line(EnemyPosition, hit.point, 0.1f, Color.red, 2f);

                    coverScoreCount++;
                }
            }

            if (Shoot(coverPoint))
            {
                CanShoot = true;
            }
            else
            {
                CanShoot = false;
            }

            float Ratio = (float)coverScoreCount / bodyPartCount;

            Logger.LogDebug($"NewCheckParts CoverScore = [{coverScoreCount}] number of bodyparts checked = [{bodyPartCount}] Ratio = [{Ratio}]");

            coverAmount = Ratio;

            if (coverAmount > 0.1f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool Shoot(Vector3 coverPoint)
        {
            if (!CheckVisiblity(coverPoint, BotOwner.WeaponRoot.position))
            {
                return true;
            }
            return false;
        }

        private bool CheckVisiblity(Vector3 coverPoint, Vector3 partPos)
        {
            Vector3 partLocalPosition = partPos - BotOwner.Transform.position;

            Vector3 partPositionFromCover = partLocalPosition + coverPoint;

            Ray ray = new Ray(partPositionFromCover, EnemyDirectionFromPoint(partPositionFromCover));

            if (Physics.Raycast(ray, out RaycastHit hit, 3f, Mask) && hit.transform.name != BotOwner.gameObject.transform.name)
            {
                DebugDrawer.Line(EnemyPosition, hit.point, 0.1f, Color.red, 2f);

                return false;
            }
            return true;
        }

        private Vector3 EnemyPosition => BotOwner.Memory.GoalEnemy.CurrPosition;
        private Vector3 BotPosition => BotOwner.Transform.position;
        private LayerMask Mask => LayerMaskClass.HighPolyWithTerrainMask;

        private Vector3 EnemyDirectionFromPoint(Vector3 point)
        {
            return EnemyPosition - point;
        }

        private float EnemyDistanceFromPoint(Vector3 point)
        {
            return Vector3.Distance(point, BotOwner.Memory.GoalEnemy.CurrPosition);
        }

        private float BotDistanceFromPoint(Vector3 point)
        {
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(BotPosition, point,-1, path);
            return path.CalculatePathLength();
        }

        private float DebugTimer = 0f;
    }
}
using BepInEx.Logging;
using EFT;
using HarmonyLib;
using SAIN_Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Movement.Helpers
{
    public class CoverPositions
    {
        protected ManualLogSource Logger;
        public CoverPositions(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            Finder = new CoverFinder(bot);
        }

        public Vector3[] SavedCoverPositions { get; private set; }
        private LayerMask Mask => LayerMaskClass.HighPolyWithTerrainMask;

        private readonly CoverFinder Finder;
        private Vector3 TargetPosition;
        private Vector3 BotPosition;

        public bool Start(BotOwner BotOwner, out Vector3 coverPosition)
        {
            try
            {
                ClearBadPositions();
            }
            catch
            {
                Logger.LogError($"ClearBadPositions Error");
            }
            try
            {
                if (CheckSavedPositions(out Vector3 oldCover, BotOwner))
                {
                    coverPosition = oldCover;
                    return true;
                }
            }
            catch
            {
                Logger.LogError($"StartSavedCheck Error");
            }

            coverPosition = Vector3.zero;
            return false;
        }

        private bool CheckSavedPositions(out Vector3 cover, BotOwner BotOwner)
        {
            TargetPosition = BotOwner.Memory.GoalEnemy.CurrPosition;
            BotPosition = BotOwner.Transform.position;

            if (SavedCoverPositions.Length > 0)
            {
                if (CheckOldPositions(out Vector3 oldPosition))
                {
                    cover = oldPosition;
                    return true;
                }
            }
            cover = Vector3.zero;
            return false;
        }

        private bool CheckOldPositions(out Vector3 goodPosition)
        {
            if (SavedCoverPositions.Length > 0)
            {
                foreach (var position in SavedCoverPositions)
                {
                    Vector3 coverCheckPoint = position;
                    coverCheckPoint.y += 1f;

                    /*
                    if (Finder.CheckPosition(coverCheckPoint))
                    {
                        Logger.LogDebug($"Found Good Old Position!");
                        goodPosition = coverCheckPoint;
                        return true;
                    }
                    */
                }
            }

            goodPosition = Vector3.zero;
            return false;
        }

        private void ClearBadPositions()
        {
            if (SavedCoverPositions.Length > 0)
            {
                List<Vector3> coverList = SavedCoverPositions.ToList();
                foreach (var position in coverList)
                {
                    Vector3 coverCheckPoint = position;
                    coverCheckPoint.y += 1f;
                    if (!Physics.Raycast(position, EnemyDirectionFromPoint(position), EnemyDistanceFromPoint(position), Mask))
                    {
                        coverList.Remove(position);
                    }
                }
                SavedCoverPositions = coverList.ToArray();
            }
        }

        private bool CheckClosestPosition(out Vector3 position)
        {
            Vector3 closestPosition = GetClosestPosition();
            /*
            if (Finder.CheckPosition(closestPosition))
            {
                Logger.LogDebug($"Found Good Closest Position!");
                position = closestPosition;
                return true;
            }
            */
            position = Vector3.zero;
            return false;
        }

        private Vector3 GetClosestPosition()
        {
            float minDistance = float.MaxValue;
            Vector3 closestCoverPosition = Vector3.zero;

            for (int i = 0; i < SavedCoverPositions.Length; i++)
            {
                float distance = Vector3.Distance(SavedCoverPositions[i], BotPosition);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCoverPosition = SavedCoverPositions[i];
                }
            }
            return closestCoverPosition;
        }

        private Vector3 EnemyDirectionFromPoint(Vector3 point)
        {
            return TargetPosition - point;
        }

        private float EnemyDistanceFromPoint(Vector3 point)
        {
            return Vector3.Distance(point, TargetPosition);
        }
    }
}
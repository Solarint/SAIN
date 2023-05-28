using EFT;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Helpers
{
    public class RaycastHelpers
    {
        public static bool CheckVisible(Vector3 startPosition, IAIDetails person, LayerMask mask)
        {
            foreach (var part in person.MainParts.Values)
            {
                if (IsVisible(startPosition, part.Position, mask))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckVisible(Vector3 startPosition, Vector3 position, LayerMask mask)
        {
            return IsVisible(startPosition, position, mask);
        }

        public static bool CheckVisible(Vector3 startPosition, Player player, LayerMask mask)
        {
            foreach (var part in player.MainParts.Values)
            {
                if (IsVisible(startPosition, part.Position, mask))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsVisible(Vector3 start, Vector3 end, LayerMask mask)
        {
            Vector3 direction = end - start;
            if (!Physics.Raycast(start, direction, direction.magnitude, mask))
            {
                return true;
            }
            return false;
        }

        public static List<Vector3> GetPartPositions(Dictionary<BodyPartType, BodyPartClass> parts)
        {
            List<Vector3> result = new List<Vector3>();
            foreach (var part in parts.Values)
            {
                result.Add(part.Position);
            }
            return result;
        }
    }
}
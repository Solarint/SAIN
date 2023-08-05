using EFT;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using UnityEngine;

namespace SAIN.Helpers
{
    public class CoverPointHelpers
    {
        public static Vector3 CoverDirection(CoverPoint point, Vector3 origin)
        {
            if (point == null)
            {
                return Vector3.zero;
            }
            return point.Position - origin;
        }

        public static float CoverDistanceSqr(CoverPoint point, Vector3 origin)
        {
            if (point == null)
            {
                return Mathf.Infinity;
            }
            return CoverDirection(point, origin).sqrMagnitude;
        }

        public static float CoverDistance(CoverPoint point, Vector3 origin)
        {
            if (point == null)
            {
                return Mathf.Infinity;
            }
            return CoverDirection(point, origin).magnitude;
        }
    }
}

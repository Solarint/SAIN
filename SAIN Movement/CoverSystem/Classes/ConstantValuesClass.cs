using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Movement.Classes
{
    public static class ConstantValues
    {
        /// <summary>
        /// Represents a constant float value for the NavMesh sample position range.
        /// </summary>
        public const float SamplePositionRange = 2.0f;

        /// <summary>
        /// How far a bot has to move in meters to start generating again.
        /// </summary>
        public const float MovementThreshold = 1.0f;
        /// <summary>
        /// How many paths can be generated while a bot is stationary.
        /// </summary>
        public const int MaxPathCount = 10;
        /// <summary>
        /// The current number of paths that have been calculated while the bot is stationary.
        /// </summary>

        /// <summary>
        /// Represents the maximum iterations for ranges to send to the path generator
        /// </summary>
        public const int MaxRangeIterations = 20;
        /// <summary>
        /// Represents the base range to send to the path generator.
        /// </summary>
        public const float RangeBase = 5f;
        /// <summary>
        /// Represents the range modifier to send to the path generator.
        /// </summary>
        public const float RangeModifier = 5f;
        /// <summary>
        /// How many iterations divided by max iterations before we start increasing the range more.
        /// </summary>
        public const float RangeThreshA = 0.5f;
        /// <summary>
        /// How many iterations divided by max iterations before we start increasing the range even more.
        /// </summary>
        public const float RangeThreshB = 0.75f;
        /// <summary>
        /// If over Threshold A or B, multiply the range by this number.
        /// </summary>
        public const float ExtraRangeIncrease = 3f;

        /// <summary>
        /// Represents the maximum number of NavMesh paths allowed per batch.
        /// </summary>
        public const float MaxPaths = 5;
        /// <summary>
        /// Represents the maximum iterations for the NavMesh Path Generator
        /// </summary>
        public const int MaxRandomPath = 5;
        /// <summary>
        /// Stores the max and min angle of the Y axis.
        /// </summary>
        public const float MaxYAngle = 5;
        /// <summary>
        /// Minimum distance before points are filtered out for Path Start Points
        /// </summary>
        public const float FilterDistancePath = 5f;

        /// <summary>
        /// Represents the maximum iterations for the Point Generator
        /// </summary>
        public const int MaxRandomGen = 5;
        /// <summary>
        /// Represents the minimum Range from start point for random generated points.
        /// </summary>
        public const float RandomGenMin = 5f;
        /// <summary>
        /// Represents the maximum Range from start point for random generated points.
        /// </summary>
        public const float RandomGenMax = 20f;
        /// <summary>
        /// Minimum distance before points are filtered out for random generated points
        /// </summary>
        public const float FilterDistancePoints = 1f;

        /// <summary>
        /// How far to raycast down, to make sure a point isn't floating.
        /// </summary>
        public const float FloatingCheck = 0.25f;
        /// <summary>
        /// How far to raycast up to make sure there is clearance for bots
        /// </summary>
        public const float HeightCheck = 1.5f;

        /// <summary>
        /// How high to raise up the coverpoint to check for cover around it.
        /// </summary>
        public const float RayYOffset = 0.35f;
        /// <summary>
        /// How far to raycast to check for cover.
        /// </summary>
        public const float RayCastDistance = 1f;
        /// <summary>
        /// If a cover point hits, but the magnitude is less than HitDistanceThreshold, divide the magnitude by PointDistanceDivideBy
        /// </summary>
        public const float HitDistanceThreshold = 0.5f;
        /// <summary>
        /// subtract the hit magnitude divided by This from the point LastPosition to Shift it away from the cover for more clearance
        /// </summary>
        public const float PointDistanceDivideBy = 2.0f;

        /// <summary>
        /// An Array of 8 even directions around a center point.
        /// </summary>
        public static readonly Vector3[] Directions = {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            (Vector3.forward + Vector3.left).normalized,
            (Vector3.forward + Vector3.right).normalized,
            (Vector3.back + Vector3.left).normalized,
            (Vector3.back + Vector3.right).normalized
        };
    }
}

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
        /// How many iterations divided by max iterations before we start increasing the range more.
        /// </summary>
        public const float RangeThreshA = 0.5f;

        /// <summary>
        /// How many iterations divided by max iterations before we start increasing the range even more.
        /// </summary>
        public const float RangeThreshB = 0.75f;

        /// <summary>
        /// Stores the max and min angle of the Y axis.
        /// </summary>
        public const float MaxYAngle = 5;

        /// <summary>
        /// Represents the minimum Range from start point for random generated points.
        /// </summary>
        public const float RandomGenMin = 5f;

        /// <summary>
        /// Represents the maximum Range from start point for random generated points.
        /// </summary>
        public const float RandomGenMax = 20f;

        /// <summary>
        /// How far to raycast down, to make sure a point isn't floating.
        /// </summary>
        public const float FloatingCheck = 0.25f;

        /// <summary>
        /// How far to raycast up to make sure there is clearance for bots
        /// </summary>
        public const float HeightCheck = 1.5f;
    }
}

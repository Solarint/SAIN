using UnityEngine;

namespace SAIN.SAINComponent.SubComponents.CoverFinder;

public struct ColliderFinderParams
    {
        public Vector3 OriginPoint;

        /// <summary>
        /// How many times to increase the size of the box before stopping the search
        /// </summary>
        public int MaxIterations;

        /// <summary>
        /// How many colliders to find before stopping the search
        /// </summary>
        public int HitThreshold;

        /// <summary>
        /// How wide the box is at the start of the search
        /// </summary>
        public float StartBoxWidth;

        /// <summary>
        /// How tall the box is at the start of the search
        /// </summary>
        public float StartBoxHeight;

        /// <summary>
        /// How much to increase the height of the box each iteration
        /// </summary>
        public float HeightIncreasePerIncrement;

        /// <summary>
        /// How much to decrease the height of the box each iteration
        /// </summary>
        public float HeightDecreasePerIncrement;

        /// <summary>
        /// How much to increase the width of the box each iteration
        /// </summary>
        public float LengthIncreasePerIncrement;
    }
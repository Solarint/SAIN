using BepInEx.Logging;
using Movement.Components;
using System.Collections.Generic;
using UnityEngine;
using static Movement.Classes.ConstantValues;
using static Movement.UserSettings.DebugConfig;

namespace Movement.Classes
{
    public class CoverChecker
    {
        public CoverChecker()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        private LayerMask Mask = LayerMaskClass.HighPolyWithTerrainNoGrassMask;

        private bool DebugMode => DebugCoverComponent.Value;

        private List<Vector3> PointsToCheck = new List<Vector3>();

        private const float minDistanceToCover = 0.25f;
        private const float rayYHeightOffset = 0.25f;
        private const float coverMaxDistance = 1.5f;
        private const float minCoverLevel0to1 = 0.33f;
        private const float height = 1.5f;

        public void ProcessPoints(out List<CoverPoint> coverPoints, List<Vector3> input, int batchSize = 10, int accuracy0to5 = 2, float filterDistance = 0.5f)
        {
            if (input.Count > 0)
            {
                var filtered = FilterDistance(input, filterDistance);

                Logger.LogDebug($"Checking [{filtered.Count}] points for cover");

                PointsToCheck.AddRange(filtered);
            }

            if (PointsToCheck.Count > batchSize)
            {
                List<Vector3> batch = PointsToCheck.GetRange(0, batchSize);

                PointsToCheck.RemoveRange(0, batchSize);

                coverPoints = CheckPointsForCover(batch, accuracy0to5);

                Logger.LogDebug($"Found [{coverPoints.Count}] CoverPoints from Batch Size of [{batchSize}]. Points that still need to be checked = [{PointsToCheck.Count}]");
            }
            else
            {
                coverPoints = null;
            }
        }

        /// <summary>
        /// Gets the settings based on the accuracy value.
        /// </summary>
        /// <param name="accuracy0to5">The accuracy value between 0 and 5.</param>
        /// <param name="maxRandomDirections">The maximum random directions.</param>
        /// <param name="maxHeightSteps">The maximum height steps.</param>
        /// <param name="maxAngleSteps">The maximum angle steps.</param>
        private void GetSettings(int accuracy0to5, out int maxRandomDirections, out int maxHeightSteps, out int maxAngleSteps)
        {
            accuracy0to5 = Mathf.Clamp(accuracy0to5, 0, 5);

            if (accuracy0to5 == 5)
            {
                maxRandomDirections = 48;
                maxHeightSteps = 48;
                maxAngleSteps = 90;
            }
            else if (accuracy0to5 == 4)
            {
                maxRandomDirections = 32;
                maxHeightSteps = 32;
                maxAngleSteps = 45;
            }
            else if (accuracy0to5 == 3)
            {
                maxRandomDirections = 24;
                maxHeightSteps = 24;
                maxAngleSteps = 30;
            }
            else if (accuracy0to5 == 2)
            {
                maxRandomDirections = 16;
                maxHeightSteps = 18;
                maxAngleSteps = 20;
            }
            else if (accuracy0to5 == 1)
            {
                maxRandomDirections = 8;
                maxHeightSteps = 12;
                maxAngleSteps = 15;
            }
            else
            {
                maxRandomDirections = 6;
                maxHeightSteps = 6;
                maxAngleSteps = 10;
            }
        }

        /// <summary>
        /// Checks a list of Vector3s for cover.
        /// </summary>
        /// <param name="batch">List of points to check for cover.</param>
        /// <param name="accuracy0to5">Accuracy of the cover check.</param>
        /// <returns>A list of cover points.</returns>
        private List<CoverPoint> CheckPointsForCover(List<Vector3> batch, int accuracy0to5)
        {
            GetSettings(accuracy0to5, out int maxRandomDirections, out int maxHeightSteps, out int maxAngleSteps);

            List<CoverPoint> coverPoints = new List<CoverPoint>();

            int i = 0;
            while (i < batch.Count)
            {
                if (CheckForCover(batch[i], out CoverPoint coverPoint, maxRandomDirections))
                {
                    if (CheckHeightAndPercent(coverPoint, maxHeightSteps))
                    {
                        FindCoverWidth(coverPoint, maxAngleSteps);

                        coverPoints.Add(coverPoint);
                    }
                }
                i++;
            }

            return coverPoints;
        }

        /// <summary>
        /// Checks for a cover point at the given point, and returns true if a valid cover point is found.
        /// </summary>
        /// <param name="point">The point to check for a cover point.</param>
        /// <param name="cover">The cover point found, if any.</param>
        /// <param name="minDistanceToCover">The minimum distance to the cover point.</param>
        /// <param name="rayYHeightOffset">The offset of the raycast in the Y axis.</param>
        /// <param name="coverMaxDistance">The maximum distance of the cover point.</param>
        /// <param name="minCoverLevel0to1">The minimum cover level, from 0 to 1.</param>
        /// <param name="maxIterations">The maximum number of random directions to check.</param>
        /// <param name="maxHeightSteps">The maximum number of height steps to check.</param>
        /// <param name="maxAngleSteps">The maximum number of angle steps to check.</param>
        /// <returns>True if a valid cover point is found, false otherwise.</returns>
        private bool CheckForCover(Vector3 point, out CoverPoint cover, int maxIterations = 15)
        {
            // initialize output point as null if the method returns false;
            cover = null;

            // Copies the points and then raises it so its not blocked by small ground objects
            Vector3 rayPoint = point;
            rayPoint.y += rayYHeightOffset;

            int i = 0;
            while (i < maxIterations)
            {
                // Find a random direction in the x/z axis, then normalize it so that its magnitude is equal to 1, then multiply that by the input maxdistance to set the raycast length
                Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                randomDirection = randomDirection.normalized * coverMaxDistance;
                // Does it intersect a collider in this direction?
                if (Physics.Raycast(rayPoint, randomDirection, out var hit, coverMaxDistance, Mask))
                {
                    Vector3 hitDirection = hit.point - rayPoint;

                    // If a cover point hits, but is too close to the objects that may provide cover, shift its position away from the object.
                    if (hitDirection.magnitude < minDistanceToCover)
                    {
                        Vector3 difference = hitDirection.normalized * (minDistanceToCover - hitDirection.magnitude);
                        point = hit.point + difference;
                    }

                    cover = new CoverPoint(point, randomDirection);

                    return true;
                }
                i++;
            }

            return false;
        }

        /// <summary>
        /// Checks the height and percent of a CoverPoint object.
        /// </summary>
        /// <param name="point">The CoverPoint object to check.</param>
        /// <param name="minCover0to1">The minimum cover amount from 0 to 1. Based on the number of raycast hits starting from the highest height down the lowest height.</param>
        /// <param name="maxIterations">The maximum height step iterations. Higher means more accuracy.</param>
        /// <returns>True if the cover amount is higher than the minimum input amount.</returns>
        private bool CheckHeightAndPercent(CoverPoint point, int maxIterations)
        {
            float heightStep = height / maxIterations;

            // Start the loop
            float heightHit = 0;
            int coverHit = 0;
            int i = 0;
            while (i < maxIterations)
            {
                // Take the position of the cover point, then raise it up to our character height. Then lower that in even steps to find the height of the cover.
                Vector3 rayPoint = point.position;
                rayPoint.y += height - (i * heightStep);

                if (Physics.Raycast(rayPoint, point.centerDirection, coverMaxDistance, Mask))
                {
                    // Store the first hit height as our cover height
                    if (coverHit == 0)
                    {
                        heightHit = rayPoint.y - point.position.y;
                    }
                    // Count up the amount of hits
                    coverHit++;
                }
                i++;
            }

            // How many cover hits were there compared to the number of iterations in the loop?
            float coverAmount = (float)coverHit / i;

            // Assign the cover height and amount we found to the CoverPoint object
            point.percent = coverAmount;
            point.height = heightHit;

            //Logger.LogDebug($"Final CheckForCalcPath for this point [{coverAmount > minCoverLevel0to1}] because [{coverHit}] and [{i}]. percent [{coverAmount}] Height [{heightHit}]");

            // Return true if the cover amount is higher than the minimum input amount
            return coverAmount > minCoverLevel0to1;
        }

        /// <summary>
        /// Finds the width of a cover point by finding the left and right edges and calculating the distance between them.
        /// </summary>
        /// <param name="point">The cover point to find the width of.</param>
        /// <param name="maxIterations">The maximum number of iterations to use when finding the edges. Higher means more accuracy.</param>
        private void FindCoverWidth(CoverPoint point, int maxIterations)
        {
            point.leftEdgeDirection = FindEdge(point, true, maxIterations);

            point.rightEdgeDirection = FindEdge(point, false, maxIterations);

            point.width = Vector3.Distance(point.leftEdgeDirection, point.rightEdgeDirection);

            Logger.LogDebug($"Cover Width = [{point.width}]");
        }

        /// <summary>
        /// Finds the edge of a cover point by casting a ray in even steps to the left or right of the direction of the original cover direction found.
        /// </summary>
        /// <param name="point">The cover point to find the edge of.</param>
        /// <param name="leftEdge">Whether to find the left edge or the right edge.</param>
        /// <param name="maxIterations">The maximum number of iterations to use when searching for the edge. Higher means more accuracy.</param>
        /// <returns>The edge of the cover point.</returns>
        private Vector3 FindEdge(CoverPoint point, bool leftEdge, int maxIterations)
        {
            float angleStep = maxIterations / 90f;

            Vector3 edge = point.centerDirection;

            Vector3 lastHitPoint = point.position;

            int i = 0;
            while (i < maxIterations)
            {
                float angle = leftEdge ? -i * angleStep : i * angleStep;

                Quaternion rotation = Quaternion.Euler(0, angle, 0);

                Vector3 direction = rotation * point.centerDirection;

                Vector3 rayPoint = point.position;
                rayPoint.y += point.height;

                if (Physics.Raycast(rayPoint, direction, out RaycastHit edgeHit, direction.magnitude, Mask))
                {
                    lastHitPoint = edgeHit.point;
                }
                else
                {
                    edge = lastHitPoint - point.position;
                    break;
                }
                i++;
            }

            return edge;
        }

        /// <summary>
        /// Filters a list of Vector3 points by removing any points that are within a certain distance of each other.
        /// </summary>
        /// <param name="points">The list of Vector3 points to filter.</param>
        /// <param name="min">The minimum distance between points.</param>
        /// <returns>The filtered list of Vector3 points.</returns>
        private List<Vector3> FilterDistance(List<Vector3> points, float min = 0.5f)
        {
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    if (Vector3.Distance(points[i], points[j]) < min)
                    {
                        points.RemoveAt(j);
                        j--;
                    }
                }
            }

            return points;
        }

        protected ManualLogSource Logger;
    }
}
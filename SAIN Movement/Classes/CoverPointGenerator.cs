using BepInEx.Logging;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Classes
{
    public class CoverPointGenerator
    {
        public CoverPointGenerator()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        private LayerMask Mask = LayerMaskClass.HighPolyWithTerrainNoGrassMask;

        private bool DebugMode => DebugCoverComponent.Value;

        private List<Vector3> PointsToCheck = new List<Vector3>();

        private const float RayCastDistance = 2.5f;
        private const float minDistanceToCover = 0.5f;
        private const float coverMaxDistance = 1.5f;

        private const float HeightRandomRotation = 5f;
        private const float rayYHeightOffset = 0.4f;
        private const float minCoverLevel0to1 = 0.45f;
        private const float height = 1.6f;

        private const float MinCoverWidth = 0.2f;

        public void ProcessPoints(out List<CoverPoint> coverPoints, List<Vector3> input, int batchSize = 10, int accuracy0to5 = 2, float filterDistance = 0.5f)
        {
            if (input.Count > 0)
            {
                totalPoints += input.Count;

                PointsToCheck.AddRange(input);
            }

            if (PointsToCheck.Count > 0)
            {
                int max = Mathf.Clamp(PointsToCheck.Count, 0, batchSize);
                List<Vector3> batch = PointsToCheck.GetRange(0, max);
                PointsToCheck.RemoveRange(0, max);

                coverPoints = CheckPointsForCover(batch, accuracy0to5);

                totalCoverPoints += coverPoints.Count;

                if (debugTime < Time.time)
                {
                    debugTime = Time.time + 5f;

                    float coverPointRatio = (float)totalCoverPoints / totalPoints;
                    coverPointRatio *= 100f;

                    float rayCastsToPointsRatio = (float)totalRayCasts / totalCoverPoints;

                    Logger.LogDebug($"Percentage of Points turned into cover: [{Mathf.Round(coverPointRatio)}%]. Num of Raycasts per CoverPoint: [{Mathf.Round(rayCastsToPointsRatio)}]. Total RayCasts: [{totalRayCasts}]. BackLog: [{PointsToCheck.Count}] FinalPoints: [{totalCoverPoints}] and Total Points Generated: [{totalPoints}]");
                }
            }
            else
            {
                coverPoints = null;
            }
        }

        int totalCoverPoints = 0;
        int totalPoints = 0;
        private float debugTime = 0;

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
                maxRandomDirections = 100;
                maxHeightSteps = 300;
                maxAngleSteps = 300;
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
                maxRandomDirections = 12;
                maxHeightSteps = 12;
                maxAngleSteps = 15;
            }
            else
            {
                maxRandomDirections = 12;
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
                        if (CheckWidth(coverPoint, maxAngleSteps))
                        {
                            coverPoints.Add(coverPoint);
                        }
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

            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            randomDirection = randomDirection.normalized;
            // Copies the points and then raises it so its not blocked by small ground objects
            Vector3 rayPoint = point;
            rayPoint.y += rayYHeightOffset;
            float anglestep = maxIterations / 360f;
            int i = 0;
            while (i < maxIterations)
            {
                Quaternion rotation = Quaternion.Euler(0, anglestep * i, 0);
                randomDirection = rotation * randomDirection;
                // Does it intersect a collider in this direction?
                totalRayCasts++;
                if (Physics.Raycast(rayPoint, randomDirection, out var hit, coverMaxDistance, Mask))
                {
                    Vector3 hitDirection = hit.point - rayPoint;

                    // If a cover point hits, but is too close to the objects that may provide cover, shift its position away from the object.
                    if (hitDirection.magnitude < minDistanceToCover)
                    {
                        i++;
                        continue;
                    }

                    cover = new CoverPoint(point, randomDirection);

                    return true;
                }
                i++;
            }

            return false;
        }

        /// <summary>
        /// Checks the height and coverPointRatio of a CoverPoint object.
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
                Vector3 rayPoint = point.Position;
                rayPoint.y += height - (i * heightStep);

                Quaternion rotation = Quaternion.Euler(0, Random.Range(-HeightRandomRotation, HeightRandomRotation), 0);
                Vector3 direction = rotation * point.CenterDirection;

                totalRayCasts++;
                if (Physics.Raycast(rayPoint, direction, RayCastDistance, Mask))
                {
                    // Store the first hit height as our cover height
                    if (coverHit == 0)
                    {
                        heightHit = rayPoint.y - point.Position.y;
                    }
                    // Count up the amount of hits
                    coverHit++;
                }
                i++;
            }
            // How many cover hits were there compared to the number of iterations in the loop?
            float coverAmount = (float)coverHit / i;

            // Assign the cover height and amount we found to the CoverPoint object
            point.CoverLevel = coverAmount;
            point.Height = heightHit;

            //Logger.LogDebug($"Final CheckForCalcPath for this point [{coverAmount > minCoverLevel0to1}] because [{coverHit}] and [{i}]. coverPointRatio [{coverAmount}] Height [{heightHit}]");

            // Return true if the cover amount is higher than the minimum input amount
            return coverAmount > minCoverLevel0to1;
        }

        int totalRayCasts = 0;

        /// <summary>
        /// Finds the width of a cover point by finding the left and right edges and calculating the distance between them.
        /// </summary>
        /// <param name="point">The cover point to find the width of.</param>
        /// <param name="maxIterations">The maximum number of iterations to use when finding the edges. Higher means more accuracy.</param>
        private bool CheckWidth(CoverPoint point, int maxIterations)
        {
            Vector3 leftEdge = FindEdge(point, true, maxIterations);
            leftEdge.y = 0f;
            point.LeftEdgeDirection = leftEdge;

            Vector3 rightEdge = FindEdge(point, false, maxIterations);
            rightEdge.y = 0f;
            point.RightEdgeDirection = rightEdge;

            point.CenterDirection = ((point.LeftEdgeDirection + point.RightEdgeDirection) / 2).normalized;

            point.Width = Vector3.Distance(point.LeftEdgeDirection, point.RightEdgeDirection);

            return point.Width > MinCoverWidth;
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

            Vector3 edge = point.CenterDirection;

            Vector3 lastHitPoint = point.Position;

            int i = 0;
            while (i < maxIterations)
            {
                float angle = leftEdge ? -i * angleStep : i * angleStep;

                Quaternion rotation = Quaternion.Euler(0, angle, 0);

                Vector3 direction = rotation * point.CenterDirection;

                direction = direction.normalized * RayCastDistance;

                Vector3 rayPoint = point.Position;
                rayPoint.y += point.Height;

                totalRayCasts++;
                if (Physics.Raycast(rayPoint, direction, out RaycastHit edgeHit, direction.magnitude, Mask))
                {
                    lastHitPoint = edgeHit.point;
                }
                else
                {
                    edge = lastHitPoint - rayPoint;
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
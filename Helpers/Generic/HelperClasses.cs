using EFT;
using SAIN.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAIN.Classes
{
    public class HelperClasses
    {
        public class CheckIfPlayerStationary
        {
            public CheckIfPlayerStationary(Player player, int maxStationaryCalc = 10, float tolerance = 1f)
            {
                Player = player;
                Tolerance = tolerance;
                MaxCalcPath = maxStationaryCalc;
                LastPosition = Player.Transform.position;
            }

            public bool CheckForCalcPath()
            {
                if (!Movement)
                {
                    // If we have calculated X new newPoints at the same position, stop running until that changes.
                    if (PathCount > MaxCalcPath)
                    {
                        return false;
                    }
                    // Count up the number of newPoints calulated at a stationary position.
                    PathCount++;
                }
                // If we aren't within the same distance, count down the cover check limit down to 0
                else if (PathCount > 0)
                {
                    PathCount--;
                }

                // Save the BotOwner's current position for reference later
                LastPosition = Player.Transform.position;

                return true;
            }

            private bool Movement => Distance > Tolerance;
            private float Distance => Vector3.Distance(LastPosition, Player.Transform.position);

            private Vector3 LastPosition;
            private readonly Player Player;
            private readonly float Tolerance;
            private readonly int MaxCalcPath;
            private int PathCount = 0;
        }

        /// <summary>
        /// This static class provides methods for sorting Vector3 objects.
        /// </summary>
        public static class Vector3Sorter
        {
            /// <summary>
            /// Sorts a list of Vector3 positions by their distance from a target Vector3 position.
            /// </summary>
            /// <param name="Positions">The list of Vector3 positions to sort.</param>
            /// <param name="targetPosition">The target Vector3 position to sort by.</param>
            /// <returns>A sorted list of Vector3 positions.</returns>
            public static List<Vector3> SortByDistance(List<Vector3> Positions, Vector3 targetPosition)
            {
                return Positions.OrderBy(cp => Vector3.Distance(cp, targetPosition)).ToList();
            }
        }


        /// <summary>
        /// Compares Vector3 objects based on their newPosition.
        /// </summary>
        public class Vector3PositionComparer : IEqualityComparer<Vector3>
        {
            public bool Equals(Vector3 v1, Vector3 v2)
            {
                return v1.Equals(v2);
            }

            public int GetHashCode(Vector3 v)
            {
                return v.GetHashCode();
            }
        }


        public class Vector3SortByDistance : IComparer<Vector3>
        {
            private Vector3 target;

            public Vector3SortByDistance(Vector3 distanceToTarget)
            {
                target = distanceToTarget;
            }

            public int Compare(Vector3 a, Vector3 b)
            {
                var targetPosition = target;
                return Vector3.Distance(a, targetPosition).CompareTo(Vector3.Distance(b, targetPosition));
            }
        }


        /// <summary>
        /// Calculates the position on an arc between two points with a given radius and angle.
        /// </summary>
        /// <param name="botPos">The position of the BotOwner.</param>
        /// <param name="targetPos">The position of the target.</param>
        /// <param name="arcRadius">The radius of the arc.</param>
        /// <param name="angle">The angle of the arc.</param>
        /// <returns>The position on the arc.</returns>
        public static Vector3 FindArcPoint(Vector3 botPos, Vector3 targetPos, float arcRadius, float angle)
        {
            Vector3 direction = (botPos - targetPos).normalized;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 arcPoint = arcRadius * (rotation * direction);

            return botPos + arcPoint;
        }

        public class Octree<T>
        {
            private readonly float x, y, z, size;
            private readonly List<T> points;
            private Octree<T>[] octants;

            public Octree(float x, float y, float z, float size)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.size = size;
                this.points = new List<T>();
            }

            public bool Insert(T point, Func<T, Vector3> getPosition)
            {
                Vector3 position = getPosition(point);

                if (position.x < x || position.x > x + size || position.y < y || position.y > y + size || position.z < z || position.z > z + size)
                {
                    return false; // point is out of bounds
                }

                points.Add(point);

                if (points.Count > 1 && octants == null) // split if there's more than one point and not already split
                {
                    float halfSize = size / 2;

                    octants = new Octree<T>[]
                    {
                new Octree<T>(x, y, z, halfSize),
                new Octree<T>(x + halfSize, y, z, halfSize),
                new Octree<T>(x, y + halfSize, z, halfSize),
                new Octree<T>(x + halfSize, y + halfSize, z, halfSize),
                new Octree<T>(x, y, z + halfSize, halfSize),
                new Octree<T>(x + halfSize, y, z + halfSize, halfSize),
                new Octree<T>(x, y + halfSize, z + halfSize, halfSize),
                new Octree<T>(x + halfSize, y + halfSize, z + halfSize, halfSize)
                    };

                    foreach (T oldPoint in points)
                    {
                        foreach (Octree<T> octant in octants)
                        {
                            if (octant.Insert(oldPoint, getPosition))
                            {
                                break;
                            }
                        }
                    }

                    points.Clear();
                }

                return true;
            }

            public List<T> Query(float qx, float qy, float qz, float qsize)
            {
                if (qx > x + size || qx + qsize < x || qy > y + size || qy + qsize < y || qz > z + size || qz + qsize < z)
                {
                    return new List<T>(); // query area is out of bounds
                }

                if (octants != null)
                {
                    List<T> results = new List<T>();

                    foreach (Octree<T> octant in octants)
                    {
                        results.AddRange(octant.Query(qx, qy, qz, qsize));
                    }

                    return results;
                }
                else
                {
                    return new List<T>(points);
                }
            }
        }
    }
}
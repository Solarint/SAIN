using BepInEx.Logging;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Helpers
{
    public class Corners
    {
        public class CornerProcessing
        {
            public static Vector3[] GetCorners(Vector3 botPosition, Vector3 targetPos, Vector3 botHeadPosition, bool raycastTrim = true, bool lerpTrim = false, bool cornerToHeadHeight = false, bool returnFromHeadHeight = false, float minLengthLerp = 2f)
            {
                Vector3 headOffset = botHeadPosition - botPosition;

                NavMeshPath Path = new NavMeshPath();
                NavMesh.CalculatePath(botPosition, targetPos, NavMesh.AllAreas, Path);
                Vector3[] corners = Path.corners;

                if (cornerToHeadHeight)
                {
                    corners = RaiseOrLowerCorners(corners, headOffset.y, true);
                }

                if (lerpTrim && corners.Length > 3)
                {
                    corners = LerpTrim(corners, minLengthLerp);
                }

                if (raycastTrim && corners.Length > 3)
                {
                    corners = RaycastTrim(corners);
                }

                if (cornerToHeadHeight && returnFromHeadHeight && corners.Length > 0)
                {
                    corners = RaiseOrLowerCorners(corners, headOffset.y, false);
                }

                return corners;
            }

            private static Vector3[] RaiseOrLowerCorners(Vector3[] corners, float headYOffset, bool raiseCorners)
            {
                var cornersList = corners.ToList();
                for (int i = 0; i < cornersList.Count; i++)
                {
                    Vector3 corner2 = cornersList[i];

                    corner2.y += raiseCorners ? headYOffset : -headYOffset;

                    cornersList[i] = corner2;
                }
                return cornersList.ToArray();
            }

            /// <summary>
            /// Raycasts between RawCorners of a navmesh and trims out unnecessary RawCorners.
            /// </summary>
            /// <param name="navMeshCorners">The RawCorners of the navmesh.</param>
            /// <param name="raycastCorners">The new RawCorners of the navmesh after trimming.</param>
            /// <returns>The new RawCorners of the navmesh after trimming.</returns>
            private static Vector3[] RaycastTrim(Vector3[] navMeshCorners)
            {
                List<Vector3> cornersList = new List<Vector3>(navMeshCorners);

                for (int i = 0; i < cornersList.Count - 1; i++)
                {
                    if (cornersList.Count < 3)
                    {
                        break;
                    }
                    if (i + 2 < cornersList.Count)
                    {
                        // CalculateDifficulty the direction from corner 1 to corner 3
                        Vector3 direction = cornersList[i + 2] - cornersList[i];
                        float distance = Vector3.Distance(cornersList[i + 2], cornersList[i]);

                        if (!Physics.Raycast(cornersList[i], direction, distance, LayerMaskClass.HighPolyWithTerrainMaskAI))
                        {
                            // If corner 1 can see corner 3 directly, remove corner 2 as it's not important
                            cornersList.RemoveAt(i + 1);
                            i--;
                        }
                    }
                }

                return cornersList.ToArray();
            }

            /// <summary>
            /// Lerps the RawCorners of a Vector3 array, removing any RawCorners that are too close together.
            /// </summary>
            /// <param name="navMeshCorners">The original array of Vector3 RawCorners.</param>
            /// <param name="averagedcorners">The averaged array of Vector3 RawCorners.</param>
            /// <param name="minlength">The minimum Distance between RawCorners.</param>
            /// <returns>The averaged array of Vector3 RawCorners.</returns>
            private static Vector3[] LerpTrim(Vector3[] navMeshCorners, float minlength)
            {
                List<Vector3> cornersList = new List<Vector3>(navMeshCorners);

                for (int i = cornersList.Count - 2; i >= 0; i--)
                {
                    if (cornersList.Count < 3)
                    {
                        break;
                    }

                    if ((cornersList[i] - cornersList[i + 1]).magnitude < minlength)
                    {
                        cornersList[i] = Vector3.Lerp(cornersList[i], cornersList[i + 1], 0.5f);

                        cornersList.RemoveAt(i + 1);
                    }
                }

                Vector3[] lerpCorners = cornersList.ToArray();
                return lerpCorners;
            }
        }
    }
}
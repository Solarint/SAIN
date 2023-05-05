using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static HairRenderer;

namespace Movement.Helpers
{
    public class Corners
    {
        /// <summary>
        /// Finds corners and generates an angle between the first 2 to calculate the direction a bot should lean in based on enemy position
        /// </summary>
        public class FindDirectionToLean
        {
            public FindDirectionToLean()
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            }

            public bool Leaning { get; set; }
            public float LeanAngle { get; private set; }
            private bool DebugMode;

            /// <summary>
            /// Finds the lean angle for a Bot based on the corners in a navmeshpath.
            /// </summary>
            /// <param name="maxDistance">The maximum distance between the two corners.</param>
            /// <param name="debugMode">Whether or not to enable debug mode.</param>
            /// <returns>The lean angle for a bot, 0 if no lean.</returns>
            public float FindLeanAngle(Vector3 botPosition, Vector3 targetPosition, float maxDistance, bool debugMode = false)
            {
                BotPosition = botPosition;
                DebugMode = debugMode;

                Vector3[] corners = CornerProcessing.GetCorners(botPosition, targetPosition, true, false, true, true);

                float leanAngle = 0f;
                if (PickLeanCorners(corners, out Vector3 corner1, out Vector3 corner2, maxDistance))
                {
                    leanAngle = Vector3.SignedAngle(corner1, corner2, Vector3.up);
                    LeanAngle = leanAngle;
                    Leaning = true;

                    if (DebugMode)
                    {
                        Logger.LogDebug($"Success. Lean Angle = [{leanAngle}]");
                    }
                }

                return leanAngle;
            }

            /// <summary>
            /// Picks the two lean corners from the given array of corners.
            /// </summary>
            /// <param name="corners">Array of corners.</param>
            /// <param name="firstcorner">The first corner.</param>
            /// <param name="secondcorner">The second corner.</param>
            /// <param name="maxdistance">The maximum distance.</param>
            /// <param name="debugMode">if set to <c>true</c> debug mode is enabled.</param>
            /// <returns>
            ///   <c>true</c> if two lean corners are picked; otherwise, <c>false</c>.
            /// </returns>
            private bool PickLeanCorners(Vector3[] corners, out Vector3 firstcorner, out Vector3 secondcorner, float maxdistance = 10f)
            {
                // Corner 0 is at bot position. So we need 3 corners to check lean angle.
                if (corners.Length < 3)
                {
                    firstcorner = Vector3.zero;
                    secondcorner = Vector3.zero;

                    if (DebugMode)
                    {
                        Logger.LogWarning($"Not Enough Corners to calculate lean! Corner Length = [{corners.Length}]");
                    }

                    return false;
                }

                // Check that corner 1 isn't past our input Max Distance
                float distance = Vector3.Distance(BotPosition, corners[1]);
                if (distance > maxdistance)
                {
                    firstcorner = Vector3.zero;
                    secondcorner = Vector3.zero;

                    if (DebugMode)
                    {
                        Logger.LogDebug($"Distance between Bot and First Corner is too far. Distance = [{distance}] Max Distance = [{maxdistance}]");
                    }

                    return false;
                }

                // Get Directions to corners for bot
                Vector3 B = corners[1];
                Vector3 C = corners[2];

                firstcorner = B - BotPosition;
                secondcorner = C - BotPosition;

                if (DebugMode)
                {
                    DebugDrawMode(firstcorner, secondcorner);
                }

                return true;
            }

            /// <summary>
            /// Draws debug lines and rays to visualize the corner positions for a lean.
            /// </summary>
            /// <param name="firstcorner">The first corner position.</param>
            /// <param name="secondcorner">The second corner position.</param>
            /// <param name="botPosition">The bot's position.</param>
            private void DebugDrawMode(Vector3 firstcorner, Vector3 secondcorner)
            {
                if (DebugTimer < Time.time)
                {
                    DebugTimer = Time.time + 5f;

                    Logger.LogDebug($"Success. Drawing Visualization");

                    Vector3 botPosition = BotPosition;
                    botPosition.y += 1.3f;
                    DebugDrawer.Line(botPosition, firstcorner, 0.025f, Color.red, 5f);
                    DebugDrawer.Line(botPosition, secondcorner, 0.025f, Color.white, 5f);
                    DebugDrawer.Line(firstcorner, secondcorner, 0.025f, Color.yellow, 5f);

                    DebugDrawer.Ray(firstcorner, Vector3.down, 1.3f, 0.025f, Color.red, 5f);
                    DebugDrawer.Ray(secondcorner, Vector3.down, 1.3f, 0.025f, Color.red, 5f);
                }
            }

            private Vector3 BotPosition;
            protected ManualLogSource Logger;
            private float DebugTimer = 0f;
        }

        /// <summary>
        /// Generates NavMesh paths and finds corners, then processes them if specified in GetCorners method
        /// </summary>
        public static class CornerProcessing
        {
            /// <summary>
            /// Calculates the corners of the navMeshPath between the bot and the target position, and optionally trims them using raycasting, length trimming, and lerp trimming.
            /// </summary>
            /// <param name="raycastTrim">Whether to trim the corners using raycasting.</param>
            /// <param name="lengthTrim">Whether to trim the corners using length trimming.</param>
            /// <param name="lerpTrim">Whether to trim the corners using lerp trimming.</param>
            /// <param name="cornerToHeadHeight">Whether to raise the corners to the bot's head height.</param>
            /// <param name="minlengthfortrim">The minimum length for length trimming.</param>
            /// <returns>The trimmed corners of the navMeshPath.</returns>
            public static Vector3[] GetCorners(Vector3 botPosition, Vector3 targetPos, bool raycastTrim = true, bool lengthTrim = false, bool lerpTrim = false, bool cornerToHeadHeight = false, bool returnFromHeadHeight = false, float minlengthfortrim = 2f, bool debugMode = false)
            {
                DebugMode = debugMode;
                NavMeshPath navMeshPath = new NavMeshPath();
                NavMesh.CalculatePath(botPosition, targetPos, -1, navMeshPath);
                Vector3[] corners = navMeshPath.corners;

                if (cornerToHeadHeight)
                {
                    List<Vector3> NavmeshCornersList = new List<Vector3>(navMeshPath.corners);
                    for (int i = 0; i < NavmeshCornersList.Count; i++)
                    {
                        Vector3 corner = NavmeshCornersList[i];
                        corner.y += 1.3f;
                        NavmeshCornersList[i] = corner;
                    }
                    corners = NavmeshCornersList.ToArray();
                }

                if (corners.Length > 1)
                {
                    corners = ProcessCorners(corners, raycastTrim, lengthTrim, lerpTrim, minlengthfortrim);
                }

                if (cornerToHeadHeight && returnFromHeadHeight && corners.Length > 0)
                {
                    List<Vector3> NavmeshCornersList2 = new List<Vector3>(navMeshPath.corners);
                    for (int i = 0; i < NavmeshCornersList2.Count; i++)
                    {
                        Vector3 corner2 = NavmeshCornersList2[i];
                        Physics.Raycast(corner2, Vector3.down, out RaycastHit hit, 2f);
                        NavmeshCornersList2[i] = hit.point;
                    }
                    corners = NavmeshCornersList2.ToArray();
                }

                if (DebugMode)
                {
                    string debugResult = $"Final Result = {corners.Length} corners found. Input Values: [raycastTrim: [{raycastTrim}] lengthTrim: [{lengthTrim}] lerpTrim: [{lerpTrim}] cornerToHeadHeight: [{cornerToHeadHeight}] returnFromHeadHeight: [{returnFromHeadHeight}]]";

                    if (corners.Length == 0)
                    {
                        Logger.LogWarning(debugResult);
                    }
                    else
                    {
                        Logger.LogDebug(debugResult);
                    }
                }

                return corners;
            }

            /// <summary>
            /// Processes the corners of a mesh by raycasting, length trimming, and lerp trimming.
            /// </summary>
            /// <param name="corners">The corners of the mesh.</param>
            /// <param name="raycastTrim">Whether to raycast trim the corners.</param>
            /// <param name="lengthTrim">Whether to length trim the corners.</param>
            /// <param name="lerpTrim">Whether to lerp trim the corners.</param>
            /// <param name="minlengthfortrim">The minimum length for trimming.</param>
            /// <returns>The processed corners.</returns>
            private static Vector3[] ProcessCorners(Vector3[] corners, bool raycastTrim, bool lengthTrim, bool lerpTrim, float minlengthfortrim)
            {
                if (raycastTrim)
                {
                    RaycastTrim(corners, out Vector3[] RaycastCorners);

                    if (DebugMode)
                    {
                        Logger.LogDebug($"RayCastTrim result = [{RaycastCorners.Length}] from [{corners.Length}]");
                    }

                    corners = RaycastCorners;
                }

                if (lengthTrim)
                {
                    LengthTrim(corners, out Vector3[] trimmedCorners, minlengthfortrim);

                    if (DebugMode)
                    {
                        Logger.LogDebug($"LengthTrim result = [{trimmedCorners.Length}] from [{corners.Length}]");
                    }

                    corners = trimmedCorners;
                }

                if (lerpTrim)
                {
                    LerpTrim(corners, out Vector3[] averagedcorners, 1f);

                    if (DebugMode)
                    {
                        Logger.LogDebug($"LerpTrim result = [{averagedcorners.Length}] from [{corners.Length}]");
                    }

                    corners = averagedcorners;
                }
                return corners;
            }

            /// <summary>
            /// Raycasts between corners of a navmesh and trims out unnecessary corners.
            /// </summary>
            /// <param name="navmeshcorners">The corners of the navmesh.</param>
            /// <param name="newcorners">The new corners of the navmesh after trimming.</param>
            /// <returns>The new corners of the navmesh after trimming.</returns>
            private static Vector3[] RaycastTrim(Vector3[] navmeshcorners, out Vector3[] newcorners)
            {
                List<Vector3> cornersList = new List<Vector3>(navmeshcorners);

                for (int i = 0; i < cornersList.Count - 1; i++)
                {
                    if (i + 2 < cornersList.Count)
                    {
                        // Calculate the direction from corner 1 to corner 3
                        Vector3 direction = cornersList[i + 2] - cornersList[i];
                        float distance = direction.magnitude;

                        if (!Physics.Raycast(cornersList[i], direction, out RaycastHit hit, distance, LayerMaskClass.HighPolyWithTerrainMask))
                        {
                            // If corner 1 can see corner 3 directly, remove corner 2 as it's not important
                            cornersList.RemoveAt(i + 1);
                            i--;
                        }
                    }
                }

                newcorners = cornersList.ToArray();
                return newcorners;
            }

            /// <summary>
            /// Trims the corners of an array of Vector3s to a minimum length.
            /// </summary>
            /// <param name="allcorners">The array of Vector3s to trim.</param>
            /// <param name="longcorners">The array of Vector3s after trimming.</param>
            /// <param name="minlength">The minimum length of the Vector3s.</param>
            /// <returns>The array of Vector3s after trimming.</returns>
            private static Vector3[] LengthTrim(Vector3[] allcorners, out Vector3[] longcorners, float minlength)
            {
                List<Vector3> cornersList = new List<Vector3>(allcorners);

                for (int i = cornersList.Count - 2; i > 0; i--)
                {
                    if (cornersList.Count < 2)
                    {
                        break;
                    }

                    if ((cornersList[i] - cornersList[i + 1]).magnitude < minlength)
                    {
                        cornersList.RemoveAt(i);
                    }
                }

                longcorners = cornersList.ToArray();

                return longcorners;
            }

            /// <summary>
            /// Lerps the corners of a Vector3 array, removing any corners that are too close together.
            /// </summary>
            /// <param name="oldcorners">The original array of Vector3 corners.</param>
            /// <param name="averagedcorners">The averaged array of Vector3 corners.</param>
            /// <param name="minlength">The minimum distance between corners.</param>
            /// <returns>The averaged array of Vector3 corners.</returns>
            private static Vector3[] LerpTrim(Vector3[] oldcorners, out Vector3[] averagedcorners, float minlength)
            {
                List<Vector3> cornersList = new List<Vector3>(oldcorners);

                for (int i = cornersList.Count - 2; i >= 0; i--)
                {
                    if (cornersList.Count <= 2)
                    {
                        break;
                    }

                    if ((cornersList[i] - cornersList[i + 1]).magnitude < minlength)
                    {
                        cornersList[i] = Vector3.Lerp(cornersList[i], cornersList[i + 1], 0.5f);

                        cornersList.RemoveAt(i + 1);
                    }
                }

                averagedcorners = cornersList.ToArray();
                return averagedcorners;
            }

            static bool DebugMode;
            static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Corner Processing");
        }
    }
}
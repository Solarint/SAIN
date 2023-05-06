using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Movement.UserSettings.Debug;

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
                Processing = new CornerProcessing();
            }

            /// <summary>
            /// Finds the lean angle for a Bot based on the corners in a navmeshpath.
            /// </summary>
            /// <param name="maxDistance">The maximum distance between the two corners.</param>
            /// <param name="debugMode">Whether or not to enable debug mode.</param>
            /// <returns>The lean angle for a bot, 0 if no lean.</returns>
            public float FindLeanAngle(Vector3 botPosition, Vector3 targetPosition, Vector3 botHeadPosition, float maxDistance)
            {
                BotPosition = botPosition;
                MaxDistance = maxDistance;
                BotHeadPosition = botHeadPosition;

                Vector3[] corners = Processing.GetCorners(botPosition, targetPosition, botHeadPosition, true, false, true, true);

                float leanAngle = 0f;
                if (PickLeanCorners(corners, out Vector3 corner1, out Vector3 corner2))
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
            private bool PickLeanCorners(Vector3[] corners, out Vector3 firstcorner, out Vector3 secondcorner)
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
                if (distance > MaxDistance * MaxDistance)
                {
                    firstcorner = Vector3.zero;
                    secondcorner = Vector3.zero;

                    if (DebugMode)
                    {
                        Logger.LogDebug($"Distance between Bot and First Corner is too far. Distance = [{distance}] Max Distance = [{MaxDistance * MaxDistance}]");
                    }

                    return false;
                }

                CornerA = corners[1];
                CornerB = corners[2];

                Vector3 cornerAOffset = CornerA - BotPosition;
                firstcorner = CornerA;
                secondcorner = CornerB - CornerA;

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

                    DebugDrawer.Ray(BotHeadPosition, firstcorner - BotHeadPosition, Vector3.Distance(BotHeadPosition, firstcorner), 0.025f, Color.red, 5f);
                    DebugDrawer.Ray(BotHeadPosition, secondcorner - BotHeadPosition, Vector3.Distance(BotHeadPosition, secondcorner), 0.025f, Color.white, 5f);

                    DebugDrawer.Ray(firstcorner, Vector3.down, 1.3f, 0.025f, Color.red, 5f);
                    DebugDrawer.Ray(secondcorner, Vector3.down, 1.3f, 0.025f, Color.red, 5f);
                }
            }

            public bool Leaning { get; set; }
            public float LeanAngle { get; private set; }

            private float MaxDistance;
            private Vector3 BotHeadPosition;
            private Vector3 CornerA;
            private Vector3 CornerB;
            private Vector3 BotPosition;
            protected ManualLogSource Logger;
            private float DebugTimer = 0f;
            private readonly CornerProcessing Processing;
            private bool DebugMode => DebugCornerLeanAngle.Value;
        }

        /// <summary>
        /// Generates NavMesh paths and finds corners, then processes them if specified in GetCorners method
        /// </summary>
        public class CornerProcessing
        {
            public CornerProcessing()
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            }

            private bool DebugMode => DebugCornerProcessing.Value;

            /// <summary>
            /// Calculates the corners of the navMeshPath between the bot and the target position, and optionally trims them using raycasting, length trimming, and lerp trimming.
            /// </summary>
            /// <param name="raycastTrim">Whether to trim the corners using raycasting.</param>
            /// <param name="lengthTrim">Whether to trim the corners using length trimming.</param>
            /// <param name="lerpTrim">Whether to trim the corners using lerp trimming.</param>
            /// <param name="cornerToHeadHeight">Whether to raise the corners to the bot's head height.</param>
            /// <param name="minlengthfortrim">The minimum length for length trimming.</param>
            /// <returns>The trimmed corners of the navMeshPath.</returns>
            public Vector3[] GetCorners(Vector3 botPosition, Vector3 targetPos, Vector3 botHeadPosition, bool raycastTrim = true, bool lengthTrim = false, bool lerpTrim = false, bool cornerToHeadHeight = false, bool returnFromHeadHeight = false, float minlengthfortrim = 2f)
            {
                Vector3 headLocalPos = botHeadPosition - botPosition;
                Vector3 botHeadOffset = botHeadPosition;
                botHeadOffset.y = botPosition.y;

                NavMeshPath navMeshPath = new NavMeshPath();
                NavMesh.CalculatePath(botHeadOffset, targetPos, -1, navMeshPath);
                Vector3[] corners = navMeshPath.corners;

                if (cornerToHeadHeight)
                {
                    List<Vector3> NavmeshCornersList = new List<Vector3>(navMeshPath.corners);
                    for (int i = 0; i < NavmeshCornersList.Count; i++)
                    {
                        Vector3 corner = NavmeshCornersList[i];
                        corner.y = headLocalPos.y;
                        NavmeshCornersList[i] = corner;

                        if (DebugMode)
                        {
                            DebugDrawer.Line(corner, botHeadPosition, 0.025f, Color.magenta, 0.25f);
                        }
                    }
                    corners = NavmeshCornersList.ToArray();
                }

                corners = ProcessCorners(corners, raycastTrim, lengthTrim, lerpTrim, minlengthfortrim);

                if (cornerToHeadHeight && returnFromHeadHeight && corners.Length > 0)
                {
                    List<Vector3> NavmeshCornersList2 = new List<Vector3>(navMeshPath.corners);
                    for (int i = 0; i < NavmeshCornersList2.Count; i++)
                    {
                        Vector3 corner2 = NavmeshCornersList2[i];
                        corner2.y -= headLocalPos.y;
                        NavmeshCornersList2[i] = corner2;

                        if (DebugMode)
                        {
                            DebugDrawer.Line(corner2, botHeadPosition, 0.025f, Color.magenta, 0.25f);
                        }
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
            /// <param name="minLengthTrim">The minimum length for trimming.</param>
            /// <returns>The processed corners.</returns>
            private Vector3[] ProcessCorners(Vector3[] corners, bool raycastTrim, bool lengthTrim, bool lerpTrim, float minLengthTrim)
            {
                if (lerpTrim && corners.Length > 3)
                {
                    Vector3[] lerpCorners = LerpTrim(corners, minLengthTrim);

                    if (DebugMode)
                    {
                        Logger.LogDebug($"LerpTrim result = [{lerpCorners.Length}] from [{corners.Length}]");
                    }

                    corners = lerpCorners;
                }

                if (lengthTrim && corners.Length > 3)
                {
                    Vector3[] lengthCorners = LengthTrim(corners, minLengthTrim);

                    if (DebugMode)
                    {
                        Logger.LogDebug($"LengthTrim result = [{lengthCorners.Length}] from [{corners.Length}]");
                    }

                    corners = lengthCorners;
                }

                if (raycastTrim && corners.Length > 3)
                {
                    Vector3[] raycastCorners = RaycastTrim(corners);

                    if (DebugMode)
                    {
                        Logger.LogDebug($"RayCastTrim result = [{raycastCorners.Length}] from [{corners.Length}]");
                    }

                    corners = raycastCorners;
                }

                return corners;
            }

            /// <summary>
            /// Raycasts between corners of a navmesh and trims out unnecessary corners.
            /// </summary>
            /// <param name="navMeshCorners">The corners of the navmesh.</param>
            /// <param name="raycastCorners">The new corners of the navmesh after trimming.</param>
            /// <returns>The new corners of the navmesh after trimming.</returns>
            private static Vector3[] RaycastTrim(Vector3[] navMeshCorners)
            {
                List<Vector3> cornersList = new List<Vector3>(navMeshCorners);

                for (int i = 0; i < cornersList.Count - 1; i++)
                {
                    if (cornersList.Count < 2)
                    {
                        break;
                    }
                    if (i + 2 < cornersList.Count)
                    {
                        // Calculate the direction from corner 1 to corner 3
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

                Vector3[] raycastCorners = cornersList.ToArray();
                return raycastCorners;
            }

            /// <summary>
            /// Trims the corners of an array of Vector3s to a minimum length.
            /// </summary>
            /// <param name="navMeshCorners">The array of Vector3s to trim.</param>
            /// <param name="longcorners">The array of Vector3s after trimming.</param>
            /// <param name="minlength">The minimum length of the Vector3s.</param>
            /// <returns>The array of Vector3s after trimming.</returns>
            private static Vector3[] LengthTrim(Vector3[] navMeshCorners, float minlength)
            {
                List<Vector3> cornersList = new List<Vector3>(navMeshCorners);

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

                Vector3[] longCorners = cornersList.ToArray();
                return longCorners;
            }

            /// <summary>
            /// Lerps the corners of a Vector3 array, removing any corners that are too close together.
            /// </summary>
            /// <param name="navMeshCorners">The original array of Vector3 corners.</param>
            /// <param name="averagedcorners">The averaged array of Vector3 corners.</param>
            /// <param name="minlength">The minimum distance between corners.</param>
            /// <returns>The averaged array of Vector3 corners.</returns>
            private static Vector3[] LerpTrim(Vector3[] navMeshCorners, float minlength)
            {
                List<Vector3> cornersList = new List<Vector3>(navMeshCorners);

                for (int i = cornersList.Count - 2; i >= 0; i--)
                {
                    if (cornersList.Count < 2)
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

            private readonly ManualLogSource Logger;
        }
    }
}
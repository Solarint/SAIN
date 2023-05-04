using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Movement.Helpers
{
    public class Corners
    {
        public Corners(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(Corners));
            BotOwner = bot;
        }

        protected ManualLogSource Logger;

        private readonly BotOwner BotOwner;

        public class FindDirectionToLean : Corners
        {
            public FindDirectionToLean(BotOwner bot) : base(bot)
            {
                Corners = new CornerProcessing(BotOwner);
            }

            /// <summary>
            /// Finds the lean angle for a Bot based on the corners in a navmeshpath.
            /// </summary>
            /// <param name="maxDistance">The maximum distance between the two corners.</param>
            /// <param name="debugMode">Whether or not to enable debug mode.</param>
            /// <returns>The lean angle for a bot, 0 if no lean.</returns>
            public float FindLeanAngle(float maxDistance, bool debugMode = false)
            {
                float leanAngle = 0f;

                Vector3[] corners = Corners.GetCorners(BotOwner.Transform.position, true, false, true, true, 0f, debugMode);

                if (PickLeanCorners(corners, out Vector3 corner1, out Vector3 corner2, maxDistance, debugMode))
                {
                    leanAngle = Vector3.SignedAngle(corner1, corner2, Vector3.up);
                    Leaning = true;
                }

                return leanAngle;
            }

            /// <summary>
            /// Picks the two lean corners from the given array of corners.
            /// </summary>
            /// <param name="allcorners">Array of corners.</param>
            /// <param name="firstcorner">The first corner.</param>
            /// <param name="secondcorner">The second corner.</param>
            /// <param name="maxdistance">The maximum distance.</param>
            /// <param name="debugMode">if set to <c>true</c> debug mode is enabled.</param>
            /// <returns>
            ///   <c>true</c> if two lean corners are picked; otherwise, <c>false</c>.
            /// </returns>
            private bool PickLeanCorners(Vector3[] allcorners, out Vector3 firstcorner, out Vector3 secondcorner, float maxdistance = 10f, bool debugMode = false)
            {
                if (allcorners.Length < 3)
                {
                    firstcorner = Vector3.zero;
                    secondcorner = Vector3.zero;
                    return false;
                }

                if (Vector3.Distance(BotOwner.Transform.position, allcorners[1]) > maxdistance)
                {
                    firstcorner = Vector3.zero;
                    secondcorner = Vector3.zero;
                    return false;
                }

                //Vector3 A = allcorners[0];
                Vector3 B = allcorners[1];
                Vector3 C = allcorners[2];

                firstcorner = B;
                secondcorner = C - B;

                if (debugMode)
                    DebugDrawMode(firstcorner, secondcorner, BotOwner.MyHead.position);

                return true;
            }

            /// <summary>
            /// Draws debug lines and rays to visualize the corner positions for a lean.
            /// </summary>
            /// <param name="firstcorner">The first corner position.</param>
            /// <param name="secondcorner">The second corner position.</param>
            /// <param name="botPosition">The bot's position.</param>
            private void DebugDrawMode(Vector3 firstcorner, Vector3 secondcorner, Vector3 botPosition)
            {
                if (DebugTimer < Time.time)
                {
                    DebugTimer = Time.time + 5f;

                    Logger.LogDebug($"Found Corner Positions for Lean for {BotOwner.name}");

                    DebugDrawer.Line(botPosition, firstcorner, 0.025f, Color.green, 5f);
                    DebugDrawer.Line(botPosition, secondcorner, 0.025f, Color.yellow, 5f);
                    DebugDrawer.Line(firstcorner, secondcorner, 0.025f, Color.red, 5f);

                    DebugDrawer.Ray(firstcorner, Vector3.down, 2f, 0.025f, Color.green, 5f);
                    DebugDrawer.Ray(secondcorner, Vector3.down, 2f, 0.025f, Color.yellow, 5f);
                }
            }

            private float DebugTimer = 0f;
            public bool Leaning { get; set; }

            private readonly CornerProcessing Corners;
        }

        public class CornerProcessing : Corners
        {
            public CornerProcessing(BotOwner bot) : base(bot)
            {
                NavMeshPath = new NavMeshPath();
            }

            public NavMeshPath NavMeshPath { get; private set; }

            /// <summary>
            /// Calculates the corners of the NavMeshPath between the bot and the target position, and optionally trims them using raycasting, length trimming, and lerp trimming.
            /// </summary>
            /// <param name="raycastTrim">Whether to trim the corners using raycasting.</param>
            /// <param name="lengthTrim">Whether to trim the corners using length trimming.</param>
            /// <param name="lerpTrim">Whether to trim the corners using lerp trimming.</param>
            /// <param name="cornerToHeadHeight">Whether to raise the corners to the bot's head height.</param>
            /// <param name="minlengthfortrim">The minimum length for length trimming.</param>
            /// <returns>The trimmed corners of the NavMeshPath.</returns>
            public Vector3[] GetCorners(Vector3 targetPos, bool raycastTrim = true, bool lengthTrim = false, bool lerpTrim = false, bool cornerToHeadHeight = false, float minlengthfortrim = 2f, bool debugMode = false)
            {
                Vector3 botPosition = BotOwner.Transform.position;

                if (NavMeshPath.corners.Length != 0 )
                {
                    NavMeshPath.ClearCorners();
                }

                NavMesh.CalculatePath(botPosition, targetPos, -1, NavMeshPath);

                List<Vector3> NavmeshCornersList = new List<Vector3>(NavMeshPath.corners);

                if (cornerToHeadHeight)
                {
                    for (int i = 0; i < NavmeshCornersList.Count; i++)
                    {
                        Vector3 corner = NavmeshCornersList[i];
                        corner.y = BotOwner.MyHead.position.y;
                        NavmeshCornersList[i] = corner;
                    }
                }

                Vector3[] corners = NavmeshCornersList.ToArray();

                if (corners.Length > 1)
                {
                    if (raycastTrim)
                    {
                        RaycastTrim(corners, out Vector3[] RaycastCorners);
                        corners = RaycastCorners;
                    }

                    if (lengthTrim)
                    {
                        TrimCorners(corners, out Vector3[] trimmedCorners, minlengthfortrim);
                        corners = trimmedCorners;
                    }

                    if (lerpTrim)
                    {
                        AverageCorners(corners, out Vector3[] averagedcorners, 1f);
                        corners = averagedcorners;
                    }
                }

                return corners;
            }

            /// <summary>
            /// Raycasts between corners of a navmesh and trims out unnecessary corners.
            /// </summary>
            /// <param name="navmeshcorners">The corners of the navmesh.</param>
            /// <param name="newcorners">The new corners of the navmesh after trimming.</param>
            /// <returns>The new corners of the navmesh after trimming.</returns>
            private Vector3[] RaycastTrim(Vector3[] navmeshcorners, out Vector3[] newcorners)
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
            private Vector3[] TrimCorners(Vector3[] allcorners, out Vector3[] longcorners, float minlength)
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
            private Vector3[] AverageCorners(Vector3[] oldcorners, out Vector3[] averagedcorners, float minlength)
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
        }
    }
}
using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Movement.UserSettings.Debug;

namespace Movement.Components
{
    public class FindCorners
    {
        public FindCorners(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(FindCorners));
            BotOwner = bot;
        }

        protected ManualLogSource Logger;
        private readonly BotOwner BotOwner;

        public class FindDirectionToLean : FindCorners
        {
            public FindDirectionToLean(BotOwner bot) : base(bot)
            {
                Corners = new CornerProcessing(BotOwner);
            }

            public float FindLeanAngle(float maxDistance)
            {
                float leanAngle = 0f;

                Vector3[] corners = Corners.GetCorners(true, false, true, true);

                if (PickLeanCorners(corners, out Vector3 corner1, out Vector3 corner2, maxDistance))
                {
                    leanAngle = Vector3.SignedAngle(corner1, corner2, Vector3.up);
                    Leaning = true;
                }

                return leanAngle;
            }

            private bool PickLeanCorners(Vector3[] allcorners, out Vector3 firstcorner, out Vector3 secondcorner, float maxdistance = 10f)
            {
                if (allcorners.Length < 3)
                {
                    firstcorner = Vector3.zero;
                    secondcorner = Vector3.zero;
                    return false;
                }

                if (Vector3.Distance(BotOwner.Transform.position, allcorners[0]) > maxdistance)
                {
                    firstcorner = Vector3.zero;
                    secondcorner = Vector3.zero;
                    return false;
                }

                Vector3 A = allcorners[0];
                Vector3 B = allcorners[1];
                Vector3 C = allcorners[2];

                firstcorner = A - B;
                secondcorner = C - B;

                if (DebugNavMesh.Value && DebugTimer < Time.time)
                {
                    DebugTimer = Time.time + 5f;

                    DebugDrawer.Line(BotOwner.MyHead.position, A, 0.025f, Color.blue, 5f);
                    DebugDrawer.Line(BotOwner.MyHead.position, B, 0.025f, Color.green, 5f);
                    DebugDrawer.Line(BotOwner.MyHead.position, C, 0.025f, Color.yellow, 5f);

                    DebugDrawer.Ray(A, Vector3.down, 2f, 0.025f, Color.blue, 5f);
                    DebugDrawer.Ray(B, Vector3.down, 2f, 0.025f, Color.green, 5f);
                    DebugDrawer.Ray(C, Vector3.down, 2f, 0.025f, Color.yellow, 5f);
                }

                return true;
            }

            private float DebugTimer = 0f;
            public bool Leaning { get; set; }

            private readonly CornerProcessing Corners;
        }

        public class CornerProcessing : FindCorners
        {
            public CornerProcessing(BotOwner bot) : base(bot)
            {
            }

            /// <summary>
            /// Calculates the corners of the NavMeshPath between the bot and the target position, and optionally trims them using raycasting, length trimming, and lerp trimming.
            /// </summary>
            /// <param name="raycastTrim">Whether to trim the corners using raycasting.</param>
            /// <param name="lengthTrim">Whether to trim the corners using length trimming.</param>
            /// <param name="lerpTrim">Whether to trim the corners using lerp trimming.</param>
            /// <param name="raisetoHeadHeight">Whether to raise the corners to the bot's head height.</param>
            /// <param name="minlengthfortrim">The minimum length for length trimming.</param>
            /// <returns>The trimmed corners of the NavMeshPath.</returns>
            public Vector3[] GetCorners(bool raycastTrim = true, bool lengthTrim = false, bool lerpTrim = false, bool raisetoHeadHeight = false, float minlengthfortrim = 2f)
            {
                Vector3 targetPosition = BotOwner.Memory.GoalEnemy.CurrPosition;
                Vector3 botPosition = BotOwner.Transform.position;

                NavMeshPath navMeshPath = new NavMeshPath();
                NavMesh.CalculatePath(botPosition, targetPosition, -1, navMeshPath);

                List<Vector3> NavmeshCornersList = new List<Vector3>(navMeshPath.corners);

                if (raisetoHeadHeight)
                {
                    for (int i = 0; i < NavmeshCornersList.Count; i++)
                    {
                        Vector3 corner = NavmeshCornersList[i];
                        corner.y += BotOwner.MyHead.position.y;
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

            private Vector3[] RaycastTrim(Vector3[] navmeshcorners, out Vector3[] newcorners)
            {
                List<Vector3> cornersList = new List<Vector3>(navmeshcorners);

                for (int i = 0; i < cornersList.Count - 1; i++)
                {
                    // Check if the next index is within bounds before raycasting
                    if (i + 2 < cornersList.Count)
                    {
                        // Calculate the direction from corner 1 to corner 3
                        Vector3 direction = cornersList[i + 2] - cornersList[i];
                        float distance = direction.magnitude;

                        if (!Physics.Raycast(cornersList[i], direction, out RaycastHit hit, distance, LayerMaskClass.HighPolyWithTerrainMask))
                        {
                            // If corner 1 can see corner 3 directly, remove corner 2 as it's not important
                            cornersList.RemoveAt(i + 1);
                            // Move back in the list to reevaluate the new corner configuration
                            i--;
                        }
                    }
                }

                newcorners = cornersList.ToArray();
                return newcorners;
            }

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
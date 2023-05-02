using BepInEx.Logging;
using EFT;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using SAIN_Helpers;
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

                if (FindCorners(out Vector3 corner1, out Vector3 corner2, maxDistance))
                {
                    leanAngle = Vector3.SignedAngle(corner1, corner2, Vector3.up);
                    Leaning = true;
                }

                return leanAngle;
            }

            private bool FindCorners(out Vector3 corner1, out Vector3 corner2, float maxdistance)
            {
                Vector3 targetPosition = BotOwner.Memory.GoalEnemy.CurrPosition;
                Vector3 botPosition = BotOwner.Transform.position;

                NavMeshPath navMeshPath = new NavMeshPath();
                NavMesh.CalculatePath(botPosition, targetPosition, -1, navMeshPath);

                List<Vector3> NavmeshCornersList = new List<Vector3>(navMeshPath.corners);

                for (int i = 0; i < NavmeshCornersList.Count; i++)
                {
                    Vector3 corner = NavmeshCornersList[i];
                    corner.y += BotOwner.MyHead.position.y;
                    NavmeshCornersList[i] = corner;
                }

                Vector3[] raisedcorners = NavmeshCornersList.ToArray();

                if (raisedcorners.Length > 1)
                {
                    Corners.RaycastTrim(raisedcorners, out Vector3[] RaycastCorners);
                    Corners.AverageCorners(RaycastCorners, out Vector3[] averagedcorners, 1f);

                    if (PickLeanCorners(averagedcorners, out Vector3 LeanCorner1, out Vector3 LeanCorner2, maxdistance))
                    {
                        corner1 = LeanCorner1;
                        corner2 = LeanCorner2;
                        return true;
                    }
                }

                corner1 = Vector3.zero;
                corner2 = Vector3.zero;
                return false;
            }

            private bool PickLeanCorners(Vector3[] allcorners, out Vector3 firstcorner, out Vector3 secondcorner, float maxdistance)
            {
                Vector3 A = allcorners[0];

                if (Vector3.Distance(BotOwner.Transform.position, A) > maxdistance || allcorners.Length < 3)
                {
                    firstcorner = Vector3.zero;
                    secondcorner = Vector3.zero;
                    return false;
                }

                Vector3 B = allcorners[1];
                Vector3 C = allcorners[2];

                firstcorner = A - B;
                secondcorner = C - B;

                if (DebugNavMesh.Value)
                {
                    DebugDrawer.Line(BotOwner.MyHead.position, firstcorner, 0.1f, Color.red, 0.25f);
                    DebugDrawer.Line(BotOwner.MyHead.position, secondcorner, 0.1f, Color.red, 0.25f);

                    DebugDrawer.Ray(A, Vector3.down, 2f, 0.025f, Color.yellow, 1f);
                    DebugDrawer.Ray(B, Vector3.down, 2f, 0.025f, Color.yellow, 1f);
                    DebugDrawer.Ray(C, Vector3.down, 2f, 0.025f, Color.yellow, 1f);
                }

                return true;
            }

            public bool Leaning { get; set; }

            private readonly CornerProcessing Corners;
        }

        public class CornerProcessing : FindCorners
        {
            public CornerProcessing(BotOwner bot) : base(bot)
            {
            }

            public Vector3[] RaycastTrim(Vector3[] navmeshcorners, out Vector3[] newcorners)
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

            public Vector3[] TrimCorners(Vector3[] allcorners, out Vector3[] longcorners, float minlength)
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

            public Vector3[] AverageCorners(Vector3[] oldcorners, out Vector3[] averagedcorners, float minlength)
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
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using SAIN.Helpers;
using System.Collections.Generic;
using System.Net;

namespace SAIN.Components.BotController
{
    public class PathManager : SAINControl
    {
        public PathManager()
        {
            Get8Directions();
        }

        private readonly Vector3[] Directions = new Vector3[8];

        public void Update()
        {
            FindExitForNextBot();
        }

        private void Get8Directions()
        {
            Vector3 center = Vector3.zero; // Center point
            float radius = 30.0f; // Radius of the circle
            float angleIncrement = 360f / 8; // Angle between each direction

            for (int i = 0; i < 8; i++)
            {
                float angle = i * angleIncrement;
                float radianAngle = Mathf.Deg2Rad * angle;

                float x = center.x + radius * Mathf.Cos(radianAngle);
                float y = center.y + radius * Mathf.Sin(radianAngle);

                Directions[i] = new Vector3(x, y, center.z);
            }
        }

        private int SAINCompCount { get; set; } = 0;

        private void FindExitForNextBot()
        {
            var Bots = BotController.SAINBots;
            int count = Bots.Count;
            int max = count - 1;
            if (SAINCompCount > max)
            {
                SAINCompCount = 0;
            }
            if (count > 0)
            {
                int i = SAINCompCount;
                var component = BotController.SAINBots.ElementAt(i).Value;
                Vector3[] exits = FindExits(component.Position);
                component.UpdateExitsToLoc(exits);
                SAINCompCount++;
            }
        }

        private Vector3[] FindExits(Vector3 botPosition)
        {
            List<Vector3> exits = new List<Vector3>();
            for (int i = 0; i < 8; i++)
            {
                Vector3 testPoint = Directions[i] + botPosition;
                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(botPosition, testPoint, -1, path) && path.corners.Length > 1)
                {
                    Vector3 exitPoint = path.corners[1];
                    bool AddPoint = true;
                    if (exits.Count > 0)
                    {
                        foreach (Vector3 t in exits)
                        {
                            if ((t - exitPoint).sqrMagnitude < 4f)
                            {
                                AddPoint = false;
                                break;
                            }
                        }
                    }
                    if (AddPoint)
                    {
                        exits.Add(exitPoint);
                        DebugGizmos.SingleObjects.Line(botPosition, exitPoint, Color.white, 0.025f, true, 1f, true);
                    }
                }
            }
            return exits.ToArray();
        }
    }
}
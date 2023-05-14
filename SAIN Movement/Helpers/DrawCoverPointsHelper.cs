using BepInEx.Logging;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.Helpers.DebugGizmos.SingleObjects;

namespace SAIN.Helpers
{
    public class DrawCoverPoints
    {
        private static ManualLogSource Logger;
        private Color ColorA;
        private Color ColorB;

        public DrawCoverPoints(Color colorA, Color colorB, string LogName = "", bool randomColor = false)
        {
            LogName += "[Drawer]";

            if (randomColor)
            {
                ColorA = new Color(Random.value, Random.value, Random.value);
                ColorB = new Color(Random.value, Random.value, Random.value);
            }
            else
            {
                ColorA = colorA;
                ColorB = colorB;
            }

            Logger = BepInEx.Logging.Logger.CreateLogSource(LogName);
            Logger.LogDebug($"Created {LogName} that uses colors [{ColorA}] and [{ColorB}]");
        }

        public void DrawList(List<CoverPoint> list, bool destroy, float size = 0.1f, bool extended = false, bool spheres = false)
        {
            if (destroy && DebugCoverPointObjects.Count > 0)
            {
                Logger.LogWarning($"Destroying {DebugCoverPointObjects.Count} Gizmos");

                DestroyDebug(DebugCoverPointObjects);
            }
            else if (!destroy && list.Count > 0 && DebugCoverPointObjects.Count == 0)
            {
                Logger.LogWarning($"Drawing {list.Count} CoverPoint Gizmos");

                var draw = Draw(list, size, extended, spheres);

                DebugCoverPointObjects.AddRange(draw);
            }
        }

        private List<GameObject> Draw(List<CoverPoint> list, float size = 0.1f, bool extended = false, bool spheres = false)
        {
            List<GameObject> debugObjects = new List<GameObject>();
            foreach (var point in list)
            {
                if (spheres)
                {
                    debugObjects.Add(Sphere(point.Position, size, ColorA));

                    if (extended)
                    {
                        Vector3 heightSphere = point.Position;
                        heightSphere.y += point.Height;
                        debugObjects.Add(Sphere(heightSphere, size, ColorB));
                        debugObjects.Add(Sphere(point.Position + point.LeftEdgeDirection, size, ColorB));
                        debugObjects.Add(Sphere(point.Position + point.RightEdgeDirection, size, ColorB));
                    }
                }
                else
                {
                    debugObjects.Add(Ray(point.Position, Vector3.up, ColorA, point.Height, size));

                    if (extended)
                    {
                        Vector3 debugPos = point.Position;
                        debugPos.y += point.Height;
                        debugObjects.Add(Ray(debugPos, point.CenterDirection, Color.white, point.CenterDirection.magnitude, size, true));
                        debugObjects.Add(Ray(debugPos, point.LeftEdgeDirection, ColorB, point.LeftEdgeDirection.magnitude, size, true));
                        debugObjects.Add(Ray(debugPos, point.RightEdgeDirection, ColorB, point.RightEdgeDirection.magnitude, size, true));
                    }
                }
            }

            return debugObjects;
        }

        private static void DestroyDebug(List<GameObject> debugObjects)
        {
            foreach (var point in debugObjects)
            {
                Object.Destroy(point);
            }

            debugObjects.Clear();
        }

        private readonly List<GameObject> DebugCoverPointObjects = new List<GameObject>();
    }
}
using BepInEx.Logging;
using EFT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Helpers
{
    public class DebugGizmos
    {
        public class SingleObjects
        {
            public static GameObject Sphere(Vector3 position, float size, Color color, bool temporary = false, float expiretime = 1f)
            {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.GetComponent<Renderer>().material.color = color;
                sphere.GetComponent<Collider>().enabled = false;
                sphere.transform.position = new Vector3(position.x, position.y, position.z); ;
                sphere.transform.localScale = new Vector3(size, size, size);

                if (temporary)
                {
                    TempCoroutine.DestroyAfterDelay(sphere, expiretime);
                }

                return sphere;
            }

            public static GameObject Line(Vector3 startPoint, Vector3 endPoint, Color color, float lineWidth = 0.1f, bool temporary = false, float expiretime = 1f, bool taperLine = false)
            {
                var lineObject = new GameObject();
                var lineRenderer = lineObject.AddComponent<LineRenderer>();

                // Set the color and width of the line
                lineRenderer.material.color = color;
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = taperLine ? lineWidth / 4f : lineWidth;

                // Set the start and end points of the line
                lineRenderer.SetPosition(0, startPoint);
                lineRenderer.SetPosition(1, endPoint);

                if (temporary)
                {
                    TempCoroutine.DestroyAfterDelay(lineObject, expiretime);
                }

                return lineObject;
            }

            public static GameObject Ray(Vector3 startPoint, Vector3 direction, Color color, float length = 0.35f, float lineWidth = 0.1f, bool temporary = false, float expiretime = 1f, bool taperLine = false)
            {
                var rayObject = new GameObject();
                var lineRenderer = rayObject.AddComponent<LineRenderer>();

                // Set the color and width of the line
                lineRenderer.material.color = color;
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = taperLine ? lineWidth / 4f : lineWidth;

                // Set the start and end points of the line to draw a rays
                lineRenderer.SetPosition(0, startPoint);
                lineRenderer.SetPosition(1, startPoint + direction.normalized * length);

                if (temporary)
                {
                    TempCoroutine.DestroyAfterDelay(rayObject, expiretime);
                }

                return rayObject;
            }
        }

        public class DrawLists
        {
            private static ManualLogSource Logger;
            private Color ColorA;
            private Color ColorB;

            public DrawLists(Color colorA, Color colorB, string LogName = "", bool randomColor = false)
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
            }

            public void DrawTempPath(NavMeshPath Path, bool active, Color colorActive, Color colorInActive, float lineSize = 0.05f, float expireTime = 0.5f, bool useDrawerSetColors = false)
            {
                for (int i = 0; i < Path.corners.Length - 1; i++)
                {
                    Vector3 corner1 = Path.corners[i];
                    Vector3 corner2 = Path.corners[i + 1];

                    Color color;
                    if (useDrawerSetColors)
                    {
                        color = active ? ColorA : ColorB;
                    }
                    else
                    {
                        color = active ? colorActive : colorInActive;
                    }

                    SingleObjects.Line(corner1, corner2, color, lineSize, true, expireTime);
                }
            }

            public void Draw(List<Vector3> list, bool destroy, float size = 0.1f, bool rays = false, float rayLength = 0.35f)
            {
                if (destroy)
                {
                    DestroyDebug();
                }
                else if (list.Count > 0 && DebugObjects == null)
                {
                    Logger.LogWarning($"Drawing {list.Count} Vector3s");

                    DebugObjects = Create(list, size, rays, rayLength);
                }
            }

            public void Draw(Vector3[] array, bool destroy, float size = 0.1f, bool rays = false, float rayLength = 0.35f)
            {
                if (destroy)
                {
                    DestroyDebug();
                }
                else if (array.Length > 0 && DebugObjects == null)
                {
                    Logger.LogWarning($"Drawing {array.Length} Vector3s");

                    DebugObjects = Create(array, size, rays, rayLength);
                }
            }

            private GameObject[] Create(List<Vector3> list, float size = 0.1f, bool rays = false, float rayLength = 0.35f)
            {
                List<GameObject> debugObjects = new List<GameObject>();
                foreach (var point in list)
                {
                    if (rays)
                    {
                        size *= Random.Range(0.5f, 1.5f);
                        rayLength *= Random.Range(0.5f, 1.5f);
                        var ray = SingleObjects.Ray(point, Vector3.up, ColorA, rayLength, size);
                        debugObjects.Add(ray);
                    }
                    else
                    {
                        var sphere = SingleObjects.Sphere(point, size, ColorA);
                        debugObjects.Add(sphere);
                    }
                }

                return debugObjects.ToArray();
            }

            private GameObject[] Create(Vector3[] array, float size = 0.1f, bool rays = false, float rayLength = 0.35f)
            {
                List<GameObject> debugObjects = new List<GameObject>();
                foreach (var point in array)
                {
                    if (rays)
                    {
                        size *= Random.Range(0.5f, 1.5f);
                        rayLength *= Random.Range(0.5f, 1.5f);
                        var ray = SingleObjects.Ray(point, Vector3.up, ColorA, rayLength, size);
                        debugObjects.Add(ray);
                    }
                    else
                    {
                        var sphere = SingleObjects.Sphere(point, size, ColorA);
                        debugObjects.Add(sphere);
                    }
                }

                return debugObjects.ToArray();
            }

            private void DestroyDebug()
            {
                if (DebugObjects != null)
                {
                    foreach (var point in DebugObjects)
                    {
                        Object.Destroy(point);
                    }

                    DebugObjects = null;
                }
            }

            private GameObject[] DebugObjects;
        }

        public class Components
        {
            /// <summary>
            /// Creates a line between two game objects and adds a script to update the line's position and color every frame.
            /// </summary>
            /// <param name="startObject">The starting game object.</param>
            /// <param name="endObject">The ending game object.</param>
            /// <param name="lineWidth">The width of the line.</param>
            /// <param name="color">The color of the line.</param>
            /// <returns>The game object containing the line renderer.</returns>
            public static GameObject FollowLine(GameObject startObject, GameObject endObject, float lineWidth, Color color)
            {
                var lineObject = new GameObject();
                var lineRenderer = lineObject.AddComponent<LineRenderer>();

                // Set the color and width of the line
                lineRenderer.material.color = color;
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;

                // Set the initial start and end points of the line
                lineRenderer.SetPosition(0, startObject.transform.position);
                lineRenderer.SetPosition(1, endObject.transform.position);

                // Add a script to update the line's position and color every frame
                var followLineScript = lineObject.AddComponent<FollowLineScript>();
                followLineScript.startObject = startObject;
                followLineScript.endObject = endObject;
                followLineScript.lineRenderer = lineRenderer;

                return lineObject;
            }

            public class FollowLineScript : MonoBehaviour
            {
                public GameObject startObject;
                public GameObject endObject;
                public LineRenderer lineRenderer;
                public float yOffset = 1f;

                private void Update()
                {
                    lineRenderer.SetPosition(0, startObject.transform.position + new Vector3(0, yOffset, 0));
                    lineRenderer.SetPosition(1, endObject.transform.position + new Vector3(0, yOffset, 0));
                }

                /// <summary>
                /// Sets the color of the line renderer material.
                /// </summary>
                /// <param name="color">The color to set.</param>
                public void SetColor(Color color)
                {
                    lineRenderer.material.color = color;
                }
            }
        }

        internal class TempCoroutine : MonoBehaviour
        {
            /// <summary>
            /// Class to run coroutines on a MonoBehaviour.
            /// </summary>
            internal class TempCoroutineRunner : MonoBehaviour { }

            /// <summary>
            /// Destroys the specified GameObject after a given delay.
            /// </summary>
            /// <param name="obj">The GameObject to be destroyed.</param>
            /// <param name="delay">The delay before the GameObject is destroyed.</param>
            public static void DestroyAfterDelay(GameObject obj, float delay)
            {
                if (obj != null)
                {
                    var runner = new GameObject("TempCoroutineRunner").AddComponent<TempCoroutineRunner>();
                    runner.StartCoroutine(RunDestroyAfterDelay(obj, delay));
                }
            }

            /// <summary>
            /// Runs a coroutine to destroy a GameObject after a delay.
            /// </summary>
            /// <param name="obj">The GameObject to destroy.</param>
            /// <param name="delay">The delay before destroying the GameObject.</param>
            /// <returns>The coroutine.</returns>
            private static IEnumerator RunDestroyAfterDelay(GameObject obj, float delay)
            {
                yield return new WaitForSeconds(delay);
                if (obj != null)
                {
                    Destroy(obj);
                }
                TempCoroutineRunner runner = obj?.GetComponentInParent<TempCoroutineRunner>();
                if (runner != null)
                {
                    Destroy(runner.gameObject);
                }
            }
        }
    }
}
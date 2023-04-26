using EFT;
using System.Collections;
using UnityEngine;

namespace SAIN_Helpers
{
    public class DebugDrawer : MonoBehaviour
    {

        /// <summary>
        /// Creates a sphere game object with the given parameters.
        /// </summary>
        /// <param name="position">The position of the sphere.</param>
        /// <param name="size">The size of the sphere.</param>
        /// <param name="color">The color of the sphere.</param>
        /// <param name="expiretime">The time after which the sphere will be destroyed.</param>
        /// <returns>The created sphere game object.</returns>
        public static GameObject Sphere(Vector3 position, float size, Color color, float expiretime)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.GetComponent<Renderer>().material.color = color;
            sphere.GetComponent<Collider>().enabled = false;
            sphere.transform.position = new Vector3(position.x, position.y, position.z); ;
            sphere.transform.localScale = new Vector3(size, size, size);

            DestroyAfterDelay(sphere, expiretime);

            return sphere;
        }

        /// <summary>
        /// Creates a line object with a given start and end point, width, color, and expiration time.
        /// </summary>
        /// <param name="startPoint">The start point of the line.</param>
        /// <param name="endPoint">The end point of the line.</param>
        /// <param name="lineWidth">The width of the line.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="expiretime">The time until the line is destroyed.</param>
        /// <returns>The created line object.</returns>
        public static GameObject Line(Vector3 startPoint, Vector3 endPoint, float lineWidth, Color color, float expiretime)
        {
            var lineObject = new GameObject();
            var lineRenderer = lineObject.AddComponent<LineRenderer>();

            // Set the color and width of the line
            lineRenderer.material.color = color;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;

            // Set the start and end points of the line
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);

            DestroyAfterDelay(lineObject, expiretime);

            return lineObject;
        }

        /// <summary>
        /// Class to run coroutines on a MonoBehaviour.
        /// </summary>
        private class TempCoroutineRunner : MonoBehaviour { }

        /// <summary>
        /// Destroys the specified GameObject after a given delay.
        /// </summary>
        /// <param name="obj">The GameObject to be destroyed.</param>
        /// <param name="delay">The delay before the GameObject is destroyed.</param>
        private static void DestroyAfterDelay(GameObject obj, float delay)
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

        /// <summary>
        /// Draws an aim line from the bot's weapon root to the target point, with a sphere at the end of the line. The line and sphere will expire after a certain amount of time.
        /// </summary>
        /// <param name="bot">The bot owner.</param>
        /// <param name="recoilvector">The recoil vector.</param>
        /// <param name="linecolor">The color of the line.</param>
        /// <param name="linewidth">The width of the line.</param>
        /// <param name="spherecolor">The color of the sphere.</param>
        /// <param name="sphereradius">The radius of the sphere.</param>
        /// <param name="expiretime">The time after which the line and sphere will expire.</param>
        public static void DrawAimLine(BotOwner bot, Vector3 recoilvector, Color linecolor, float linewidth, Color spherecolor, float sphereradius, float expiretime)
        {
            GameObject lineObject = new GameObject();

            GameObject sphereObject = Sphere(bot.AimingData.RealTargetPoint + recoilvector, sphereradius, spherecolor, expiretime);

            GameObject lineObjectSegment = Line(bot.WeaponRoot.position, bot.AimingData.RealTargetPoint + recoilvector, linewidth, linecolor, expiretime);

            lineObjectSegment.transform.parent = lineObject.transform;

            sphereObject.transform.parent = lineObject.transform;

            DestroyAfterDelay(lineObject, expiretime);
        }
    }
}
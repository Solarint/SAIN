using EFT;
using System.Collections;
using UnityEngine;

namespace SAIN.Combat.Helpers
{
    public class DebugDrawer : MonoBehaviour
    {
        private class TempCoroutineRunner : MonoBehaviour { }
        private static void DestroyAfterDelay(GameObject obj, float delay)
        {
            if (obj != null)
            {
                var runner = new GameObject("TempCoroutineRunner").AddComponent<TempCoroutineRunner>();
                runner.StartCoroutine(RunDestroyAfterDelay(obj, delay));
            }
        }
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
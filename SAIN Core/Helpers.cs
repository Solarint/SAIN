using Aki.Reflection.Patching;
using ChatShared;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.HandBook;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Weather;
using HarmonyLib;
using JetBrains.Annotations;
using SAIN.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
using System.Collections;

namespace SAIN.Helpers
{
    public class DebugDrawer : MonoBehaviour
    {
        private class TempCoroutineRunner : MonoBehaviour { }
        public static void DestroyAfterDelay(GameObject obj, float delay)
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

        public static GameObject Sphere(Vector3 position, float size, Color color)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.GetComponent<Renderer>().material.color = color;
            sphere.GetComponent<Collider>().enabled = false;
            sphere.transform.position = new Vector3(position.x, position.y, position.z); ;
            sphere.transform.localScale = new Vector3(size, size, size);

            return sphere;
        }

        public static GameObject Line(Vector3 startPoint, Vector3 endPoint, float lineWidth, Color color)
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

            return lineObject;
        }
    }
}

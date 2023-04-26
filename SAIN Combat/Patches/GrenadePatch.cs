using Aki.Reflection.Patching;
using HarmonyLib;
using SAIN_Audio.Combat.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static SAIN_Audio.Combat.Patches.Helpers;

namespace SAIN_Audio.Combat.Patches
{
    public class GrenadePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass513), "CanThrowGrenade2");
        }
        [PatchPrefix]
        public static bool PatchPrefix(ref GClass513 __result, Vector3 from, Vector3 trg, float maxPower, AIGreandeAng greandeAng, float minThrowDistSqrt = -1f, float maxPercent = 0.5f)
        {
            // Check if target is too close
            if (minThrowDistSqrt > 0f && (from - trg).sqrMagnitude < 5f)
            {
                __result = new GClass513();
                return false;
            }

            // Get angle properties
            GrenadeAngleProperties angleProperties = dictionary_0[greandeAng];
            float cos = angleProperties.Cos;
            float sin = angleProperties.Sin;
            float sin2 = angleProperties.Sin2;
            float tan = angleProperties.Tan;
            float ang = angleProperties.Ang;

            // Calculate distance and height difference
            Vector3 distanceVector = trg - from;
            float startY = from.y;
            float gravity = 9.8f;
            float deltaX = distanceVector.x;
            float deltaZ = distanceVector.z;
            float deltaY = distanceVector.y;
            float horizontalDist = Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

            // Calculate launch speed
            float launchSpeedNumerator = gravity * horizontalDist * horizontalDist;
            float launchSpeedDenominator = (horizontalDist * tan - deltaY) * 2f * cos * cos;
            float launchSpeedSquared = launchSpeedNumerator / launchSpeedDenominator;
            if (launchSpeedSquared < 0f)
            {
                __result = new GClass513();
                return false;
            }
            float launchSpeed = Mathf.Sqrt(launchSpeedSquared);
            if (launchSpeed > maxPower)
            {
                __result = new GClass513();
                return false;
            }

            // Calculate horizontal and vertical trajectory
            float distanceFactor = launchSpeed * launchSpeed * sin2 / gravity;
            Vector3 horizontalTrajectory = NormalizeFastSelf(distanceVector);
            horizontalTrajectory.y = 0f;
            Vector3 horizontalVelocity = horizontalTrajectory * distanceFactor;
            if ((from - trg).magnitude / horizontalVelocity.magnitude < 10f)
            {
                __result = new GClass513();
                return false;
            }

            float verticalTrajectory = launchSpeed * launchSpeed * sin * sin / (2f * gravity);

            Vector3 midPoint = from + horizontalVelocity / 2f;
            midPoint.y = startY + verticalTrajectory;

            //(from + horizontalVelocity / 2f).y = startY + verticalTrajectory;

            // Calculate trajectory points
            int numPoints = 6;
            Vector3[] points = new Vector3[numPoints + 2];

            for (int i = 0; i < numPoints; i++)
            {
                float currentDist = (float)(i + 1) * horizontalDist / (float)numPoints;
                float currentHeight = CalculateForceInMotion(currentDist, launchSpeed, tan, cos);
                Vector3 currentPosition = from + currentDist * horizontalTrajectory;
                currentPosition.y += currentHeight;
                points[i + 1] = currentPosition;
            }

            points[0] = from;
            points[points.Length - 1] = trg;

            // Check for obstacles
            for (int j = 0; j < points.Length - 1; j++)
            {
                Vector3 startPoint = points[j];
                Vector3 endPoint = points[j + 1];
                Vector3 direction = endPoint - startPoint;
                float segmentLength = direction.magnitude;

                bool hasObstacle = Physics.Raycast(new Ray(startPoint, direction), out RaycastHit hit, segmentLength, LayerMaskClass.HighPolyWithTerrainMask);

                DebugDrawer.Sphere(hit.point, 0.1f, Color.blue, 10f);
                DebugDrawer.Line(startPoint, endPoint, 0.05f, Color.blue, 10f);
                DebugDrawer.Line(startPoint, hit.point, 0.05f, Color.blue, 10f);

                // Return obstacle hit
                if (hasObstacle)
                {
                    __result = new GClass513(hit.collider.gameObject);
                }
            }

            // Return successful throw data
            __result = new GClass513(ang, launchSpeed, distanceVector, from, trg, false, null);
            return false;
        }
    }

    public class Helpers
    {
        public class GrenadeAngleProperties
        {
            /// <summary>
            /// Constructor for GrenadeAngleProperties class. 
            /// </summary>
            /// <param name="ang">Angle in degrees.</param>
            /// <returns>
            /// An instance of GrenadeAngleProperties with properties set according to the angle.
            /// </returns>
            public GrenadeAngleProperties(float ang)
            {
                float num = ang * 0.017453292f;
                this.Cos = Mathf.Cos(num);
                this.Sin = Mathf.Sin(num);
                this.Sin2 = Mathf.Sin(2f * num);
                this.Tan = Mathf.Tan(num);
                this.Ang = ang;
            }

            public float Cos;
            public float Sin;
            public float Sin2;
            public float Tan;
            public float Ang;
        }

        /// <summary>
        /// Calculates the force in motion given the parameters.
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <param name="power">The power value.</param>
        /// <param name="tan">The tan value.</param>
        /// <param name="cos">The cos value.</param>
        /// <returns>
        /// The calculated force in motion.
        /// </returns>
        public static float CalculateForceInMotion(float x, float power, float tan, float cos)
        {
            return tan * x - 9.8f * x * x / (2f * power * power * cos * cos);
        }

        /// <summary>
        /// Calculates a new Vector3 based on the given parameters.
        /// </summary>
        /// <param name="from">The starting Vector3.</param>
        /// <param name="force">The force Vector3.</param>
        /// <returns>
        /// A new Vector3 based on the given parameters.
        /// </returns>
        public static Vector3 smethod_3(Vector3 from, Vector3 force)
        {
            Vector3 v = new Vector3(force.x, 0f, force.z);
            Vector2 vector = new Vector2(v.magnitude, force.y);
            float num = 2f * vector.x * vector.y / 9.8f;
            if (vector.y < 0f)
            {
                num = -num;
            }
            return NormalizeFastSelf(v) * num + from;
        }

        /// <summary>
        /// Calculates the tangent of 45 degrees in radians. 
        /// </summary>
        public static float tan45 = Mathf.Tan(0.7853982f);

        /// <summary>
        /// Creates a dictionary of AIGreandeAng and GrenadeAngleProperties objects with angles of 5, 15, 25, 35, 45, 55, and 65.
        /// </summary>
        /// <param name=""></param>
        /// <returns>
        /// Dictionary of AIGreandeAng and GrenadeAngleProperties objects.
        /// </returns>
        public static readonly Dictionary<AIGreandeAng, GrenadeAngleProperties> dictionary_0 = new Dictionary<AIGreandeAng, GrenadeAngleProperties>
        {
            {
                AIGreandeAng.ang25,
                new GrenadeAngleProperties(25f)
            },
            {
                AIGreandeAng.ang45,
                new GrenadeAngleProperties(45f)
            },
            {
                AIGreandeAng.ang65,
                new GrenadeAngleProperties(65f)
            },
            {
                AIGreandeAng.ang5,
                new GrenadeAngleProperties(5f)
            },
            {
                AIGreandeAng.ang15,
                new GrenadeAngleProperties(15f)
            },
            {
                AIGreandeAng.ang35,
                new GrenadeAngleProperties(35f)
            },
            {
                AIGreandeAng.ang55,
                new GrenadeAngleProperties(55f)
            }
        };

        /// <summary>
        /// Normalizes the vector in a less performance heavy way than normal
        /// </summary>
        /// <param name="v">The vector to normalize.</param>
        /// <returns>The normalized vector.</returns>
        public static Vector3 NormalizeFastSelf(Vector3 v)
        {
            float num = (float)Math.Sqrt((double)(v.x * v.x + v.y * v.y + v.z * v.z));
            v.x /= num;
            v.y /= num;
            v.z /= num;
            return v;
        }
    }
}

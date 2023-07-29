using EFT.UI.Ragfair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Helpers
{
    public static class VectorHelpers
    {
        /// <summary>
        /// Calculates the point between two vectors using a given force and mass.
        /// </summary>
        /// <param name="position">The starting position.</param>
        /// <param name="force">The force to be applied.</param>
        /// <param name="mass">The mass of the object.</param>
        /// <returns>The point between the two vectors.</returns>
        public static Vector3 DangerPoint(Vector3 position, Vector3 force, float mass)
        {
            force /= mass;

            Vector3 vector = CalculateForce(position, force);

            Vector3 midPoint = (position + vector) / 2f;

            CheckThreePoints(position, midPoint, vector, out Vector3 result);

            return result;
        }

        /// <summary>
        /// Checks if three points are connected without any obstacles in between.
        /// </summary>
        /// <param name="from">The starting point.</param>
        /// <param name="midPoint">The middle point.</param>
        /// <param name="target">The target point.</param>
        /// <param name="hitPos">The hit position.</param>
        /// <returns>True if the three points are connected, false otherwise.</returns>
        private static bool CheckThreePoints(Vector3 from, Vector3 midPoint, Vector3 target, out Vector3 hitPos)
        {
            Vector3 direction = midPoint - from;
            if (Physics.Raycast(new Ray(from, direction), out RaycastHit raycastHit, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
            {
                hitPos = raycastHit.point;
                return false;
            }

            Vector3 direction2 = midPoint - target;
            if (Physics.Raycast(new Ray(midPoint, direction2), out raycastHit, direction2.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
            {
                hitPos = raycastHit.point;
                return false;
            }

            hitPos = target;
            return true;
        }

        /// <summary>
        /// Calculates the force vector from the given parameters.
        /// </summary>
        /// <param name="from">The starting point of the force vector.</param>
        /// <param name="force">The force vector.</param>
        private static Vector3 CalculateForce(Vector3 from, Vector3 force)
        {
            Vector3 v = new Vector3(force.x, 0f, force.z);

            Vector2 vector = new Vector2(v.magnitude, force.y);

            float num = 2f * vector.x * vector.y / GClass564.Core.G;

            if (vector.y < 0f)
            {
                num = -num;
            }

            return NormalizeFastSelf(v) * num + from;
        }

        /// <summary>
        /// Rotates a point around a pivot by a given angle.
        /// </summary>
        /// <param name="Point">The point to rotate.</param>
        /// <param name="Pivot">The pivot point.</param>
        /// <param name="Angle">The angle to rotate by.</param>
        /// <returns>The rotated point.</returns>
        public static Vector3 RotateAroundPivot(this Vector3 Point, Vector3 Pivot, Quaternion Angle)
        {
            return Angle * (Point - Pivot) + Pivot;
        }

        /// <summary>
        /// Rotates a point around a pivot point by a given Euler angles.
        /// </summary>
        /// <param name="Point">The point to rotate.</param>
        /// <param name="Pivot">The pivot point.</param>
        /// <param name="Euler">The Euler angles.</param>
        /// <returns>The rotated point.</returns>
        public static Vector3 RotateAroundPivot(this Vector3 Point, Vector3 Pivot, Vector3 Euler)
        {
            return Point.RotateAroundPivot(Pivot, Quaternion.Euler(Euler));
        }

        /// <summary>
        /// Rotates the transform around a pivot point by the given angle.
        /// </summary>
        /// <param name="Me">The transform to rotate.</param>
        /// <param name="Pivot">The pivot point to rotate around.</param>
        /// <param name="Angle">The angle to rotate by.</param>
        public static void RotateAroundPivot(this Transform Me, Vector3 Pivot, Quaternion Angle)
        {
            Me.position = Me.position.RotateAroundPivot(Pivot, Angle);
        }

        /// <summary>
        /// Rotates the transform around a pivot point by the given Euler angles.
        /// </summary>
        /// <param name="Me">The transform to rotate.</param>
        /// <param name="Pivot">The pivot point to rotate around.</param>
        /// <param name="Euler">The Euler angles to rotate by.</param>
        public static void RotateAroundPivot(this Transform Me, Vector3 Pivot, Vector3 Euler)
        {
            Me.position = Me.position.RotateAroundPivot(Pivot, Quaternion.Euler(Euler));
        }

        /// <summary>
        /// Multiplies two Vector3 objects together.
        /// </summary>
        /// <param name="multiplier1">The first Vector3 to be multiplied.</param>
        /// <param name="multiplier2">The second Vector3 to be multiplied.</param>
        /// <returns>
        /// A new Vector3 object containing the result of the multiplication.
        /// </returns>
        public static Vector3 Multiply(this Vector3 multiplier1, Vector3 multiplier2)
        {
            return new Vector3(multiplier1.x * multiplier2.x, multiplier1.y * multiplier2.y, multiplier1.z * multiplier2.z);
        }

        /// <summary>
        /// Multiplies two Vector2 objects together.
        /// </summary>
        /// <param name="multiplier1">The first Vector2 to be multiplied.</param>
        /// <param name="multiplier2">The second Vector2 to be multiplied.</param>
        /// <returns>A new Vector2 object containing the result of the multiplication.</returns>
        public static Vector2 Multiply(this Vector2 multiplier1, Vector2 multiplier2)
        {
            return new Vector2(multiplier1.x * multiplier2.x, multiplier1.y * multiplier2.y);
        }

        /// <summary>
        /// Divides a Vector3 by another Vector3.
        /// </summary>
        /// <param name="divisible">The Vector3 to be divided.</param>
        /// <param name="divisor">The Vector3 to divide by.</param>
        /// <returns>A new Vector3 with the result of the division.</returns>
        public static Vector3 Divide(this Vector3 divisible, Vector3 divisor)
        {
            return new Vector3(divisible.x / divisor.x, divisible.y / divisor.y, divisible.z / divisor.z);
        }

        /// <summary>
        /// Divides a Vector2 by another Vector2.
        /// </summary>
        /// <param name="divisible">The Vector2 to be divided.</param>
        /// <param name="divisor">The Vector2 to divide by.</param>
        /// <returns>A new Vector2 with the result of the division.</returns>
        public static Vector2 Divide(this Vector2 divisible, Vector2 divisor)
        {
            return new Vector2(divisible.x / divisor.x, divisible.y / divisor.y);
        }

        /// <summary>
        /// Clamps the given Vector3 between the given min and max Vector3s.
        /// </summary>
        /// <param name="vector">The Vector3 to clamp.</param>
        /// <param name="min">The minimum Vector3.</param>
        /// <param name="max">The maximum Vector3.</param>
        /// <returns>The clamped Vector3.</returns>
        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
        {
            return new Vector3(Mathf.Clamp(vector.x, Mathf.Min(min.x, max.x), Mathf.Max(min.x, max.x)), Mathf.Clamp(vector.y, Mathf.Min(min.y, max.y), Mathf.Max(min.y, max.y)), Mathf.Clamp(vector.z, Mathf.Min(min.z, max.z), Mathf.Max(min.z, max.z)));
        }

        /// <summary>
        /// Calculates the difference between two angles in degrees.
        /// </summary>
        /// <param name="from">The angle to calculate the difference from.</param>
        /// <param name="to">The angle to calculate the difference to.</param>
        /// <returns>The difference between the two angles in degrees.</returns>
        public static Vector3 DeltaAngle(this Vector3 from, Vector3 to)
        {
            return new Vector3(Mathf.DeltaAngle(from.x, to.x), Mathf.DeltaAngle(from.y, to.y), Mathf.DeltaAngle(from.z, to.z));
        }

        /// <summary>
        /// Calculates the angle between two normalized vectors.
        /// </summary>
        /// <param name="a">The first normalized vector.</param>
        /// <param name="b">The second normalized vector.</param>
        /// <returns>The angle between the two vectors in degrees.</returns>
        public static float AngOfNormazedVectors(Vector3 a, Vector3 b)
        {
            return Mathf.Acos(a.x * b.x + a.y * b.y + a.z * b.z) * 57.29578f;
        }

        /// <summary>
        /// Calculates the angle of two normalized vectors.
        /// </summary>
        /// <param name="a">The first normalized vector.</param>
        /// <param name="b">The second normalized vector.</param>
        /// <returns>The angle of the two normalized vectors.</returns>
        public static float AngOfNormazedVectorsCoef(Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        /// <summary>
        /// Checks if the angle between two vectors is less than the given cosine value.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <param name="cos">The cosine value.</param>
        /// <returns>True if the angle between the two vectors is less than the given cosine value, false otherwise.</returns>
        public static bool IsAngLessNormalized(Vector3 a, Vector3 b, float cos)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z > cos;
        }

        /// <summary>
        /// Calculates the normalized vector of the given vector.
        /// </summary>
        /// <param name="v">The vector to normalize.</param>
        /// <returns>The normalized vector.</returns>
        public static Vector3 NormalizeFast(Vector3 v)
        {
            float num = (float)System.Math.Sqrt((double)(v.x * v.x + v.y * v.y + v.z * v.z));
            return new Vector3(v.x / num, v.y / num, v.z / num);
        }

        /// <summary>
        /// Normalizes the vector and returns the normalized vector.
        /// </summary>
        /// <param name="v">The vector to normalize.</param>
        /// <returns>The normalized vector.</returns>
        public static Vector3 NormalizeFastSelf(Vector3 v)
        {
            float num = (float)System.Math.Sqrt((double)(v.x * v.x + v.y * v.y + v.z * v.z));
            v.x /= num;
            v.y /= num;
            v.z /= num;
            return v;
        }

        /// <summary>
        /// Rotates a Vector3 by 90 degrees in the specified direction.
        /// </summary>
        /// <param name="n">The Vector3 to rotate.</param>
        /// <param name="side">The direction to rotate.</param>
        /// <returns>The rotated Vector3.</returns>
        public static Vector3 Rotate90(Vector3 n, SideTurn side)
        {
            if (side == SideTurn.left)
            {
                return new Vector3(-n.z, n.y, n.x);
            }
            return new Vector3(n.z, n.y, -n.x);
        }

        /// <summary>
        /// Rotates a vector on a given angle to the Z axis.
        /// </summary>
        /// <param name="d">The vector to rotate.</param>
        /// <param name="angDegree">The angle to rotate in degrees.</param>
        /// <returns>The rotated vector.</returns>
        public static Vector3 RotateVectorOnAngToZ(Vector3 d, float angDegree)
        {
            Vector3 vector = NormalizeFastSelf(d);
            float f = 0.017453292f * angDegree;
            float num = Mathf.Cos(f);
            float y = Mathf.Sin(f);
            return new Vector3(vector.x * num, y, vector.z * num);
        }

        /// <summary>
        /// Rotates a Vector3 on the Up axis by a given angle in degrees.
        /// </summary>
        /// <param name="b">The Vector3 to rotate.</param>
        /// <param name="angDegree">The angle in degrees to rotate by.</param>
        /// <returns>The rotated Vector3.</returns>
        public static Vector3 RotateOnAngUp(Vector3 b, float angDegree)
        {
            float f = angDegree * 0.017453292f;
            float num = Mathf.Sin(f);
            float num2 = Mathf.Cos(f);
            float x = b.x * num2 - b.z * num;
            float z = b.z * num2 + b.x * num;
            return new Vector3(x, 0f, z);
        }

        /// <summary>
        /// Rotates a Vector2 on a given angle.
        /// </summary>
        /// <param name="b">The Vector2 to rotate.</param>
        /// <param name="a">The angle to rotate the Vector2.</param>
        /// <returns>
        /// The rotated Vector2.
        /// </returns>
        public static Vector2 RotateOnAng(Vector2 b, float a)
        {
            float f = a * 0.017453292f;
            float num = Mathf.Sin(f);
            float num2 = Mathf.Cos(f);
            float x = b.x * num2 - b.y * num;
            float y = b.y * num2 + b.x * num;
            return new Vector2(x, y);
        }

        /// <summary>
        /// Calculates the length of a quaternion.
        /// </summary>
        /// <param name="quaternion">The quaternion to calculate the length of.</param>
        /// <returns>The length of the quaternion.</returns>
        public static float Length(this Quaternion quaternion)
        {
            return Mathf.Sqrt(quaternion.x * quaternion.x + quaternion.y * quaternion.y + quaternion.z * quaternion.z + quaternion.w * quaternion.w);
        }

        /// <summary>
        /// Normalizes the quaternion.
        /// </summary>
        public static void Normalize(this Quaternion quaternion)
        {
            float num = quaternion.Length();
            if (Mathf.Approximately(num, 1f))
            {
                return;
            }
            if (Mathf.Approximately(num, 0f))
            {
                quaternion.Set(0f, 0f, 0f, 1f);
                return;
            }
            quaternion.Set(quaternion.x / num, quaternion.y / num, quaternion.z / num, quaternion.w / num);
        }

        /// <summary>
        /// Checks if a given Vector3 is on the NavMesh.
        /// </summary>
        /// <param name="v">The Vector3 to check.</param>
        /// <param name="dist">The Distance to check.</param>
        /// <returns>True if the Vector3 is on the NavMesh, false otherwise.</returns>
        public static bool IsOnNavMesh(Vector3 v, float dist = 0.04f)
        {
            NavMesh.SamplePosition(v, out NavMeshHit navMeshHit, dist, -1);
            return navMeshHit.hit;
        }

        /// <summary>
        /// Calculates the projection point of a given point onto a line defined by two points.
        /// </summary>
        /// <param name="p">The point to project.</param>
        /// <param name="p1">The first point defining the line.</param>
        /// <param name="p2">The second point defining the line.</param>
        /// <returns>The projection point of the given point onto the line.</returns>
        public static Vector3 GetProjectionPoint(Vector3 p, Vector3 p1, Vector3 p2)
        {
            float num = p1.z - p2.z;
            if (num == 0f)
            {
                return new Vector3(p.x, p1.y, p1.z);
            }
            float num2 = p2.x - p1.x;
            if (num2 == 0f)
            {
                return new Vector3(p1.x, p1.y, p.z);
            }
            float num3 = p1.x * p2.z - p2.x * p1.z;
            float num4 = num2 * p.x - num * p.z;
            float num5 = -(num2 * num3 + num * num4) / (num2 * num2 + num * num);
            return new Vector3(-(num3 + num2 * num5) / num, p1.y, num5);
        }

        /// <summary>
        /// Tests four sides of a given direction and returns the best direction.
        /// </summary>
        /// <param name="dir">The direction to test.</param>
        /// <param name="headPos">The position of the head.</param>
        /// <returns>The best direction.</returns>
        public static Vector3 Test4Sides(Vector3 dir, Vector3 headPos)
        {
            dir.y = 0f;
            Vector3[] array = FindAngleFromDir(dir);
            if (array == null)
            {
                Console.WriteLine("can' find posible dirs");
                return dir;
            }
            foreach (Vector3 vector in array)
            {
                if (TestDir(headPos, vector, 8f))
                {
                    return vector;
                }
            }
            return Vector3.one;
        }

        /// <summary>
        /// Tests a given direction from a given position for a given Distance.
        /// </summary>
        /// <param name="headPos">The position to start the test from.</param>
        /// <param name="dir">The direction to test.</param>
        /// <param name="dist">The Distance to test.</param>
        /// <returns>True if the test was successful, false otherwise.</returns>
        public static bool TestDir(Vector3 headPos, Vector3 dir, float dist)
        {
            return TestDir(headPos, dir, dist, out Vector3? vector);
        }

        /// <summary>
        /// Tests a given direction from a given position for a given Distance and returns a boolean indicating if the direction is clear and a Vector3 of the point of impact if it is not.
        /// </summary>
        /// <param name="headPos">The starting position of the test.</param>
        /// <param name="dir">The direction of the test.</param>
        /// <param name="dist">The Distance of the test.</param>
        /// <param name="outPos">The point of impact if the direction is not clear.</param>
        /// <returns>A boolean indicating if the direction is clear.</returns>
        public static bool TestDir(Vector3 headPos, Vector3 dir, float dist, out Vector3? outPos)
        {
            outPos = null;
            bool flag = Physics.Raycast(new Ray(headPos, dir), out RaycastHit raycastHit, dist, LayerMaskClass.HighPolyWithTerrainMask);
            bool result = !flag;
            if (flag)
            {
                outPos = new Vector3?(raycastHit.point);
            }
            return result;
        }

        /// <summary>
        /// Checks if a given point is inside any of the given BoxColliders.
        /// </summary>
        /// <param name="pos">The point to check.</param>
        /// <param name="colliders">The BoxColliders to check against.</param>
        /// <returns>True if the point is inside any of the BoxColliders, false otherwise.</returns>
        public static bool InBounds(Vector3 pos, BoxCollider[] colliders)
        {
            foreach (BoxCollider box in colliders)
            {
                if (PointInOABB(pos, box))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a point is inside an oriented bounding box (OOBB).
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="box">The OOBB to check against.</param>
        /// <returns>True if the point is inside the OOBB, false otherwise.</returns>
        public static bool PointInOABB(Vector3 point, BoxCollider box)
        {
            point = box.transform.InverseTransformPoint(point) - box.center;
            float num = box.size.x * 0.5f;
            float num2 = box.size.y * 0.5f;
            float num3 = box.size.z * 0.5f;
            return point.x < num && point.x > -num && point.y < num2 && point.y > -num2 && point.z < num3 && point.z > -num3;
        }

        /// <summary>
        /// Calculates the squared Distance between two Vector3 objects.
        /// </summary>
        /// <param name="a">The first Vector3 object.</param>
        /// <param name="b">The second Vector3 object.</param>
        /// <returns>The squared Distance between two Vector3 objects.</returns>
        public static float SqrDistance(this Vector3 a, Vector3 b)
        {
            Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
            return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
        }

        /// <summary>
        /// Checks if the given Vector2 is equal to zero.
        /// </summary>
        /// <param name="vector">The Vector2 to check.</param>
        /// <returns>True if the Vector2 is equal to zero, false otherwise.</returns>
        public static bool IsZero(this Vector2 vector)
        {
            return vector.x.IsZero() && vector.y.IsZero();
        }

        /// <summary>
        /// Generates an array of 8 Vector3s based on a start direction and an array of indices.
        /// </summary>
        /// <param name="startDir">The starting direction.</param>
        /// <param name="indexOfDirs">The array of indices.</param>
        private static void CreateVectorArray8Dir(Vector3 startDir, int[] indexOfDirs)
        {
            Vector3[] array = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                int num = indexOfDirs[i];
                Vector3 vector = RotateOnAngUp(startDir, EFTMath.GreateRandom((float)num, 0.1f));
                array[i] = vector;
            }
            dictionary_0.Add(startDir, array);
        }

        /// <summary>
        /// Finds the closest Vector3[] in a dictionary of Vector3 and Vector3[] based on the angle between the given Vector3 and the Vector3 in the dictionary.
        /// </summary>
        /// <param name="dir">The Vector3 to compare against the Vector3 in the dictionary.</param>
        /// <returns>The closest Vector3[] in the dictionary.</returns>
        private static Vector3[] FindAngleFromDir(Vector3 dir)
        {
            float num = float.MaxValue;
            Vector3[] result = null;
            foreach (KeyValuePair<Vector3, Vector3[]> keyValuePair in dictionary_0)
            {
                float num2 = Vector3.Angle(dir, keyValuePair.Key);
                if (num2 < num)
                {
                    num = num2;
                    result = keyValuePair.Value;
                }
            }
            return result;
        }

        public static void Init()
        {
            if (Started)
            {
                return;
            }

            int[] array = new int[8];
            int num = 45;
            int num2 = 4;
            int num3 = 1;
            for (int i = 0; i < num2; i++)
            {
                if (i == 0)
                {
                    array[0] = 0;
                    array[7] = 180;
                }
                else
                {
                    int num4 = i * num;
                    int num5 = 360 - num4;
                    array[num3] = num4;
                    array[num3 + 1] = num5;
                    num3 += 2;
                }
            }
            CreateVectorArray8Dir(Vector3.forward, array);
            CreateVectorArray8Dir(Vector3.left, array);
            CreateVectorArray8Dir(Vector3.right, array);
            CreateVectorArray8Dir(Vector3.back, array);
            Started = true;
        }

        private static bool Started = false;
        private static readonly Dictionary<Vector3, Vector3[]> dictionary_0 = new Dictionary<Vector3, Vector3[]>();
    }

    public enum SideTurn
    {
        left,
        right
    }
}

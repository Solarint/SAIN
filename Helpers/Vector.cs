using Aki.Reflection.Patching;
using EFT;
using EFT.UI.Ragfair;
using HarmonyLib;
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
    public static class Vector
    {
        public static bool Raycast(Vector3 start, Vector3 end, LayerMask mask)
        {
            Vector3 direction = end - start;
            return Physics.Raycast(start, direction.normalized, direction.magnitude, mask);
        }
        public static bool Raycast(Vector3 start, Vector3 end, out RaycastHit hitInfo, LayerMask mask)
        {
            Vector3 direction = end - start;
            return Physics.Raycast(start, direction.normalized, out hitInfo, direction.magnitude, mask);
        }

        public static float DistanceBetween(Vector3 A, Vector3 B) => (A - B).magnitude;
        public static float DistanceBetweenSqr(Vector3 A, Vector3 B) => (A - B).sqrMagnitude;

        public static Vector3 DangerPoint(Vector3 position, Vector3 force, float mass)
        {
            force /= mass;

            Vector3 vector = CalculateForce(position, force);

            Vector3 midPoint = (position + vector) / 2f;

            CheckThreePoints(position, midPoint, vector, out Vector3 result);

            return result;
        }

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

        private static Vector3 CalculateForce(Vector3 from, Vector3 force)
        {
            Vector3 v = new Vector3(force.x, 0f, force.z);

            Vector2 vector = new Vector2(v.magnitude, force.y);

            float num = 2f * vector.x * vector.y / HelpersGClass.Gravity;

            if (vector.y < 0f)
            {
                num = -num;
            }

            return NormalizeFastSelf(v) * num + from;
        }

        public static bool CanShootToTarget(ShootPointClass shootToPoint, Vector3 firePos, LayerMask mask, bool doubleSide = false)
        {
            if (shootToPoint == null)
            {
                return false;
            }
            bool flag = false;
            Vector3 vector = shootToPoint.Point - firePos;
            Ray ray = new Ray(firePos, vector);
            float magnitude = vector.magnitude;
            if (!Physics.Raycast(ray, out RaycastHit raycastHit, magnitude * shootToPoint.DistCoef, mask))
            {
                if (doubleSide)
                {
                    if (!Physics.Raycast(new Ray(shootToPoint.Point, -vector), out raycastHit, magnitude, mask))
                    {
                        flag = true;
                    }
                }
                else
                {
                    flag = true;
                }
            }
            return flag;
        }

        public static Vector3 RotateAroundPivot(this Vector3 Point, Vector3 Pivot, Quaternion Angle)
        {
            return Angle * (Point - Pivot) + Pivot;
        }

        public static Vector3 RotateAroundPivot(this Vector3 Point, Vector3 Pivot, Vector3 Euler)
        {
            return Point.RotateAroundPivot(Pivot, Quaternion.Euler(Euler));
        }

        public static void RotateAroundPivot(this Transform Me, Vector3 Pivot, Quaternion Angle)
        {
            Me.position = Me.position.RotateAroundPivot(Pivot, Angle);
        }

        public static void RotateAroundPivot(this Transform Me, Vector3 Pivot, Vector3 Euler)
        {
            Me.position = Me.position.RotateAroundPivot(Pivot, Quaternion.Euler(Euler));
        }

        public static Vector3 Multiply(this Vector3 multiplier1, Vector3 multiplier2)
        {
            return new Vector3(multiplier1.x * multiplier2.x, multiplier1.y * multiplier2.y, multiplier1.z * multiplier2.z);
        }

        public static Vector2 Multiply(this Vector2 multiplier1, Vector2 multiplier2)
        {
            return new Vector2(multiplier1.x * multiplier2.x, multiplier1.y * multiplier2.y);
        }

        public static Vector3 Divide(this Vector3 divisible, Vector3 divisor)
        {
            return new Vector3(divisible.x / divisor.x, divisible.y / divisor.y, divisible.z / divisor.z);
        }

        public static Vector2 Divide(this Vector2 divisible, Vector2 divisor)
        {
            return new Vector2(divisible.x / divisor.x, divisible.y / divisor.y);
        }

        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
        {
            return new Vector3(Mathf.Clamp(vector.x, Mathf.Min(min.x, max.x), Mathf.Max(min.x, max.x)), Mathf.Clamp(vector.y, Mathf.Min(min.y, max.y), Mathf.Max(min.y, max.y)), Mathf.Clamp(vector.z, Mathf.Min(min.z, max.z), Mathf.Max(min.z, max.z)));
        }

        public static Vector3 DeltaAngle(this Vector3 from, Vector3 to)
        {
            return new Vector3(Mathf.DeltaAngle(from.x, to.x), Mathf.DeltaAngle(from.y, to.y), Mathf.DeltaAngle(from.z, to.z));
        }

        public static float AngOfNormazedVectors(Vector3 a, Vector3 b)
        {
            return Mathf.Acos(a.x * b.x + a.y * b.y + a.z * b.z) * 57.29578f;
        }

        public static float AngOfNormazedVectorsCoef(Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static bool IsAngLessNormalized(Vector3 a, Vector3 b, float cos)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z > cos;
        }

        public static Vector3 NormalizeFast(Vector3 v)
        {
            float num = (float)System.Math.Sqrt((double)(v.x * v.x + v.y * v.y + v.z * v.z));
            return new Vector3(v.x / num, v.y / num, v.z / num);
        }

        public static Vector3 NormalizeFastSelf(Vector3 v)
        {
            float num = (float)System.Math.Sqrt((double)(v.x * v.x + v.y * v.y + v.z * v.z));
            v.x /= num;
            v.y /= num;
            v.z /= num;
            return v;
        }

        public static Vector3 Rotate90(Vector3 n, SideTurn side)
        {
            if (side == SideTurn.left)
            {
                return new Vector3(-n.z, n.y, n.x);
            }
            return new Vector3(n.z, n.y, -n.x);
        }

        public static Vector3 RotateVectorOnAngToZ(Vector3 d, float angDegree)
        {
            Vector3 vector = NormalizeFastSelf(d);
            float f = 0.017453292f * angDegree;
            float num = Mathf.Cos(f);
            float y = Mathf.Sin(f);
            return new Vector3(vector.x * num, y, vector.z * num);
        }

        public static Vector3 RotateOnAngUp(Vector3 b, float angDegree)
        {
            float f = angDegree * 0.017453292f;
            float num = Mathf.Sin(f);
            float num2 = Mathf.Cos(f);
            float x = b.x * num2 - b.z * num;
            float z = b.z * num2 + b.x * num;
            return new Vector3(x, 0f, z);
        }

        public static Vector2 RotateOnAng(Vector2 b, float a)
        {
            float f = a * 0.017453292f;
            float num = Mathf.Sin(f);
            float num2 = Mathf.Cos(f);
            float x = b.x * num2 - b.y * num;
            float y = b.y * num2 + b.x * num;
            return new Vector2(x, y);
        }

        public static float Length(this Quaternion quaternion)
        {
            return Mathf.Sqrt(quaternion.x * quaternion.x + quaternion.y * quaternion.y + quaternion.z * quaternion.z + quaternion.w * quaternion.w);
        }

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

        public static bool IsOnNavMesh(Vector3 v, float dist = 0.04f)
        {
            NavMesh.SamplePosition(v, out NavMeshHit navMeshHit, dist, -1);
            return navMeshHit.hit;
        }

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

        public static bool TestDir(Vector3 headPos, Vector3 dir, float dist)
        {
            return TestDir(headPos, dir, dist, out Vector3? vector);
        }

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

        public static bool PointInOABB(Vector3 point, BoxCollider box)
        {
            point = box.transform.InverseTransformPoint(point) - box.center;
            float num = box.size.x * 0.5f;
            float num2 = box.size.y * 0.5f;
            float num3 = box.size.z * 0.5f;
            return point.x < num && point.x > -num && point.y < num2 && point.y > -num2 && point.z < num3 && point.z > -num3;
        }

        public static float SqrDistance(this Vector3 a, Vector3 b)
        {
            Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
            return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
        }

        public static bool IsZero(this Vector2 vector)
        {
            return vector.x.IsZero() && vector.y.IsZero();
        }

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
        }

        private static readonly Dictionary<Vector3, Vector3[]> dictionary_0 = new Dictionary<Vector3, Vector3[]>();
    }

    public enum SideTurn
    {
        left,
        right
    }
}

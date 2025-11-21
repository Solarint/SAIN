using EFT;
using SAIN.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace SAIN.Helpers;

public static class Vector
{
    public static void GeneratePointsAlongDirection(List<Vector3> points, Vector3 start, Vector3 direction, float distance, float spacing)
    {
        Vector3 step = direction.normalized * spacing;
        int pointCount = Mathf.FloorToInt(distance / spacing);
        for (int i = 1; i <= pointCount; i++)
        {
            Vector3 point = start + step * i;
            points.Add(point);
        }
    }

    public static float FindFlatSignedAngle(Vector3 a, Vector3 b, Vector3 origin)
    {
        a.y = 0;
        b.y = 0;
        origin.y = 0;
        return Vector3.SignedAngle(a - origin, b - origin, Vector3.up);
    }

    public static bool Raycast(Vector3 start, Vector3 end, LayerMask mask)
    {
        Vector3 direction = end - start;
        return Physics.Raycast(start, direction.normalized, direction.magnitude, mask);
    }

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
        Vector3 v = new(force.x, 0f, force.z);

        Vector2 vector = new(v.magnitude, force.y);

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
        Ray ray = new(firePos, vector);
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

    public static Vector3 Rotate(Vector3 direction, float degX, float degY, float degZ)
    {
        return Quaternion.Euler(degX, degY, degZ) * direction;
    }

    public static Vector3 Offset(Vector3 targetDirection, Vector3 offsetDirection, float magnitude)
    {
        return targetDirection + offsetDirection.normalized * magnitude;
    }

    public static float SignedAngle(Vector3 from, Vector3 to, bool normalize = false)
    {
        if (normalize)
        {
            to.Normalize();
            from.Normalize();
        }
        float result = Vector3.SignedAngle(from, to, Vector3.up);
        return result.Round10();
    }

    public static List<Vector3> NavMeshPointsFromSampledPoint(Vector3 point, Vector3 start, List<Vector3> list, int count = 5, float magnitude = 4f, float sampleDistance = 0.25f, int maxIterations = 15)
    {
        if (list == null)
        {
            return null;
        }
        list.Clear();
        for (int i = 0; i < maxIterations; i++)
        {
            Vector3 randomDirection = RandomVector3(1, 0, 1).normalized * magnitude;
            if (NavMesh.SamplePosition(randomDirection + point, out var hit, sampleDistance, -1))
            {
                NavMeshPath Path = new();
                if (NavMesh.CalculatePath(point, hit.position, -1, Path))
                {
                    if (Path.status == NavMeshPathStatus.PathPartial)
                    {
                        list.Add(Path.corners[Path.corners.Length - 1]);
                    }
                    else
                    {
                        list.Add(hit.position);
                    }
                }
            }
            if (list.Count >= count)
            {
                break;
            }
        }
        return list;
    }

    public static Vector3 RandomVector3(float x, float y, float z)
    {
        return new Vector3(RandomRange(x), RandomRange(y), RandomRange(z));
    }

    public static float RandomRange(float magnitude)
    {
        return Random.Range(-magnitude, magnitude);
    }

    public static Vector3 RotateAroundPivot(this Vector3 Point, Vector3 Pivot, Quaternion Angle)
    {
        return Angle * (Point - Pivot) + Pivot;
    }

    public static bool IsAngLessNormalized(Vector3 a, Vector3 b, float cos)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z > cos;
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

    public static Vector3 RotateOnAngUp(Vector3 b, float angDegree)
    {
        float f = angDegree * 0.017453292f;
        float num = Mathf.Sin(f);
        float num2 = Mathf.Cos(f);
        float x = b.x * num2 - b.z * num;
        float z = b.z * num2 + b.x * num;
        return new Vector3(x, 0f, z);
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

    // I have no idea what the fuck this does, but im copying it here to avoid using gclass references. names are guesses based on the functions that call it, also WHAT THE FUCK

    public struct CrossPoint
    {
        public CrossPoint(float dx, float dy)
        {
            this.x = dx;
            this.y = dy;
        }

        public CrossPoint(Vector3 v)
        {
            this.x = v.x;
            this.y = v.z;
        }

        public float x;
        public float y;
    }

    public struct VectorPair
    {
        public VectorPair(Vector3 a, Vector3 b)
        {
            this.a = a;
            this.b = b;
        }

        public Vector3 a;
        public Vector3 b;
    }

    private static readonly Dictionary<Vector3, Vector3[]> dictionary_0 = new();
}

public enum SideTurn
{
    left,
    right
}
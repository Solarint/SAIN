using EFT.UI.Ragfair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Movement.Helpers
{
    public static class MathHelpers
    {
        private static string smethod_1(float seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds((double)seconds);
            return string.Format("{0:D2}:{1:D2}:{2:D2}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
        }
        public static int RandomInclude(int a, int b)
        {
            b++;
            if (a > b)
            {
                return random_0.Next(b, a);
            }
            return random_0.Next(a, b);
        }
        public static int RandomSing()
        {
            if (Random(0f, 100f) < 50f)
            {
                return 1;
            }
            return -1;
        }
        public static float Random(float a, float b)
        {
            float num = (float)random_0.NextDouble();
            return a + (b - a) * num;
        }
        public static bool IsTrue100(float v)
        {
            return Random(0f, 100f) < v;
        }
        public static bool RandomBool(float chanceInPercent = 50f)
        {
            return IsTrue100(chanceInPercent);
        }
        public static T ParseEnum<T>(this string value)
        {
            return (T)((object)Enum.Parse(typeof(T), value, true));
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
        public static float NextFloat(this System.Random random, int min, int max)
        {
            float num = (float)(random.NextDouble() * 2.0 - 1.0);
            double num2 = Math.Pow(2.0, (double)random.Next(min, max));
            return (float)((double)num * num2);
        }
        public static bool ApproxEquals(this float value, float value2)
        {
            return Math.Abs(value - value2) < float.Epsilon;
        }

        public static bool ApproxEquals(this double value, double value2)
        {
            return Math.Abs(value - value2) < 1.401298464324817E-45;
        }

        public static bool LowAccuracyApprox(this float value, float value2)
        {
            return Math.Abs(value - value2) < 0.001f;
        }

        public static bool IsZero(this float value)
        {
            return Math.Abs(value) < float.Epsilon;
        }

        public static bool IsZero(this Vector2 vector)
        {
            return vector.x.IsZero() && vector.y.IsZero();
        }

        public static bool IsZero(this double value)
        {
            return Math.Abs(value) < 1.401298464324817E-45;
        }

        public static bool Positive(this double value)
        {
            return value >= 1.401298464324817E-45;
        }

        public static bool Positive(this float value)
        {
            return value >= float.Epsilon;
        }

        public static bool Negative(this double value)
        {
            return value <= -1.401298464324817E-45;
        }

        public static bool Negative(this float value)
        {
            return value <= -1E-45f;
        }

        public static bool ZeroOrNegative(this float value)
        {
            return value < float.Epsilon;
        }

        public static bool ZeroOrPositive(this float value)
        {
            return value > -1E-45f;
        }

        public static double Clamp01(this double value)
        {
            if (value < 0.0)
            {
                return 0.0;
            }
            if (value <= 1.0)
            {
                return value;
            }
            return 1.0;
        }

        public static double Clamp(this double value, double limit1, double limit2)
        {
            if (limit1 < limit2)
            {
                value = Math.Max(value, limit1);
                value = Math.Min(value, limit2);
            }
            else
            {
                value = Math.Max(value, limit2);
                value = Math.Min(value, limit1);
            }
            return value;
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

        public static Rect Scale(this Rect rect, Vector2 scale)
        {
            return new Rect(rect.x * scale.x, rect.y * scale.y, rect.width * scale.x, rect.height * scale.y);
        }

        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
        {
            return new Vector3(Mathf.Clamp(vector.x, Mathf.Min(min.x, max.x), Mathf.Max(min.x, max.x)), Mathf.Clamp(vector.y, Mathf.Min(min.y, max.y), Mathf.Max(min.y, max.y)), Mathf.Clamp(vector.z, Mathf.Min(min.z, max.z), Mathf.Max(min.z, max.z)));
        }

        public static Vector3 DeltaAngle(this Vector3 from, Vector3 to)
        {
            return new Vector3(Mathf.DeltaAngle(from.x, to.x), Mathf.DeltaAngle(from.y, to.y), Mathf.DeltaAngle(from.z, to.z));
        }

        public static T GetRandomItem<T>(this List<T> list, T excludedItem)
        {
            if (list == null)
            {
                return default;
            }
            int count = list.Count;
            if (count == 0)
            {
                return default;
            }
            if (count == 1)
            {
                return list[0];
            }
            int num = 0;
            T result;
            for (; ; )
            {
                int index = UnityEngine.Random.Range(0, count);
                result = list[index];
                num++;
                if (result.Equals(excludedItem))
                {
                    if (num != 100)
                    {
                        break;
                    }
                }
            }
            return result;
        }
        public static T GetRandomItem<T>(this List<T> list)
        {
            if (list != null && list.Count != 0)
            {
                int index = UnityEngine.Random.Range(0, list.Count);
                return list[index];
            }
            return default;
        }
        public static double ExactLength(this AudioClip clip)
        {
            return (double)clip.samples / (double)clip.frequency;
        }
        private static Func<object, T> smethod_0<T>(MethodInfo methodInfo)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "obj");
            UnaryExpression arg = Expression.Convert(parameterExpression, methodInfo.GetParameters().First<ParameterInfo>().ParameterType);
            return Expression.Lambda<Func<object, T>>(Expression.Call(methodInfo, arg), new ParameterExpression[]
            {
            parameterExpression
            }).Compile();
        }
        private static Action<object, T> smethod_1<T>(MethodInfo methodInfo)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "obj");
            ParameterExpression parameterExpression2 = Expression.Parameter(typeof(T), "value");
            UnaryExpression arg = Expression.Convert(parameterExpression, methodInfo.GetParameters().First<ParameterInfo>().ParameterType);
            UnaryExpression arg2 = Expression.Convert(parameterExpression2, methodInfo.GetParameters().Last<ParameterInfo>().ParameterType);
            return Expression.Lambda<Action<object, T>>(Expression.Call(methodInfo, arg, arg2), new ParameterExpression[]
            {
            parameterExpression,
            parameterExpression2
            }).Compile();
        }
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> collection, int n)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException("n", "n must be 0 or greater");
            }
            LinkedList<T> linkedList = new LinkedList<T>();
            foreach (T value in collection)
            {
                linkedList.AddLast(value);
                if (linkedList.Count > n)
                {
                    linkedList.RemoveFirst();
                }
            }
            return linkedList;
        }
        public static Func<TOBjectType, TValueType> CreateGetter<TOBjectType, TValueType>(FieldInfo fieldInfo)
        {
            Type typeFromHandle = typeof(TValueType);
            Type typeFromHandle2 = typeof(TOBjectType);
            return (Func<TOBjectType, TValueType>)CreateGetterFieldDynamicMethod(fieldInfo, typeFromHandle2, typeFromHandle).CreateDelegate(typeof(Func<TOBjectType, TValueType>));
        }
        public static Func<object, TValueType> CreateGetter<TValueType>(FieldInfo fieldInfo, Type objectType)
        {
            Type typeFromHandle = typeof(TValueType);
            return smethod_0<TValueType>(CreateGetterFieldDynamicMethod(fieldInfo, objectType, typeFromHandle).GetBaseDefinition());
        }
        public static DynamicMethod CreateGetterFieldDynamicMethod(FieldInfo fieldInfo, Type objectType, Type valueType)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(fieldInfo.ReflectedType.FullName + ".get_" + fieldInfo.Name, valueType, new Type[]
            {
            objectType
            }, true);
            ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
            if (fieldInfo.IsStatic)
            {
                ilgenerator.Emit(OpCodes.Ldsfld, fieldInfo);
            }
            else
            {
                ilgenerator.Emit(OpCodes.Ldarg_0);
                ilgenerator.Emit(OpCodes.Ldfld, fieldInfo);
            }
            ilgenerator.Emit(OpCodes.Ret);
            return dynamicMethod;
        }
        public static List<Transform> GetChildsName(Transform transform, string name, bool onlyActive = true)
        {
            List<Transform> list = new List<Transform>();
            foreach (object obj in transform)
            {
                Transform transform2 = (Transform)obj;
                if (transform2.name.Contains(name))
                {
                    if (onlyActive)
                    {
                        if (transform2.gameObject.activeSelf)
                        {
                            list.Add(transform2);
                        }
                    }
                    else
                    {
                        list.Add(transform2);
                    }
                }
            }
            return list;
        }
        public static Transform GetChildName(Transform transform, string name, string nocontains = "")
        {
            foreach (object obj in transform)
            {
                Transform transform2 = (Transform)obj;
                if (transform2.name.Contains(name) && transform2.gameObject.activeSelf)
                {
                    if (nocontains.Length <= 0)
                    {
                        return transform2;
                    }
                    if (!transform2.name.Contains(nocontains))
                    {
                        return transform2;
                    }
                }
            }
            return null;
        }
        public static bool IsCloseDebug(Vector3 v, float x, float z)
        {
            float num = 0.1f;
            return v.x > x - num && v.x < x + num && v.z > z - num && v.z < z + num;
        }
        public static bool IsCloseDebug(Vector3 v, float x, float y, float z)
        {
            float num = 0.1f;
            return v.x > x - num && v.x < x + num && v.z > z - num && v.z < z + num && v.y > y - num && v.y < y + num;
        }
        public static Action<TOBjectType, TValueType> CreateSetter<TOBjectType, TValueType>(FieldInfo field)
        {
            Type typeFromHandle = typeof(TOBjectType);
            Type typeFromHandle2 = typeof(TValueType);
            return (Action<TOBjectType, TValueType>)smethod_2(field, typeFromHandle, typeFromHandle2).CreateDelegate(typeof(Action<TOBjectType, TValueType>));
        }
        public static Action<object, TValueType> CreateSetter<TValueType>(FieldInfo field, Type objectType)
        {
            Type typeFromHandle = typeof(TValueType);
            return smethod_1<TValueType>(smethod_2(field, objectType, typeFromHandle).GetBaseDefinition());
        }
        private static DynamicMethod smethod_2(FieldInfo field, Type objType, Type valueType)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(field.ReflectedType.FullName + ".set_" + field.Name, null, new Type[]
            {
            objType,
            valueType
            }, true);
            ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
            if (field.IsStatic)
            {
                ilgenerator.Emit(OpCodes.Ldarg_1);
                ilgenerator.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                ilgenerator.Emit(OpCodes.Ldarg_0);
                ilgenerator.Emit(OpCodes.Ldarg_1);
                ilgenerator.Emit(OpCodes.Stfld, field);
            }
            ilgenerator.Emit(OpCodes.Ret);
            return dynamicMethod;
        }
        public static float RandomNormal(float min, float max)
        {
            double num = 3.5;
            double num2;
            while ((num2 = BoxMuller((double)min + (double)(max - min) / 2.0, (double)(max - min) / 2.0 / num)) > (double)max || num2 < (double)min)
            {
            }
            return (float)num2;
        }
        public static double BoxMuller(double mean, double standard_deviation)
        {
            return mean + BoxMuller() * standard_deviation;
        }
        public static double BoxMuller()
        {
            if (bool_1)
            {
                bool_1 = false;
                return double_0;
            }
            double num;
            double num2;
            double num3;
            do
            {
                num = 2.0 * random_0.NextDouble() - 1.0;
                num2 = 2.0 * random_0.NextDouble() - 1.0;
                num3 = num * num + num2 * num2;
            }
            while (num3 >= 1.0 || num3 == 0.0);
            num3 = Math.Sqrt(-2.0 * Math.Log(num3) / num3);
            double_0 = num2 * num3;
            bool_1 = true;
            return num * num3;
        }
        public static bool RemoveFromQueue<T>(T item, Queue<T> q)
        {
            bool result = false;
            Queue<T> queue = new Queue<T>();
            while (q.Count > 0)
            {
                T item2 = q.Dequeue();
                if (item2.Equals(item))
                {
                    result = true;
                }
                else
                {
                    queue.Enqueue(item2);
                }
            }
            while (queue.Count > 0)
            {
                q.Enqueue(queue.Dequeue());
            }
            return result;
        }
        public static Mesh MakeFullScreenMesh(Camera cam)
        {
            Mesh mesh = new Mesh
            {
                name = "Utils MakeFullScreenMesh"
            };
            cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
            Vector3[] vertices = new Vector3[]
            {
            cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)),
            cam.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)),
            cam.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)),
            cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f))
            };
            mesh.vertices = vertices;
            Vector2[] uv = new Vector2[]
            {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f)
            };
            mesh.uv = uv;
            int[] triangles = new int[]
            {
            2,
            1,
            0,
            2,
            3,
            1
            };
            mesh.triangles = triangles;
            return mesh;
        }
        public static void ProcessException(Exception exception)
        {
            Debug.LogException(exception);
        }
        public static bool IsOnNavMesh(Vector3 v, float dist = 0.04f)
        {
            NavMeshHit navMeshHit;
            NavMesh.SamplePosition(v, out navMeshHit, dist, -1);
            return navMeshHit.hit;
        }
        public static bool IsDisplayChildCount(this EViewListType type)
        {
            return type == EViewListType.RequirementsWindow || type == EViewListType.Handbook || type == EViewListType.WishList || type == EViewListType.WeaponBuild;
        }
        public static bool IsUpdateChildStatus(this EViewListType type)
        {
            return type == EViewListType.AllOffers || type == EViewListType.MyOffers || type == EViewListType.WishList || type == EViewListType.WeaponBuild;
        }
        public static void ClearTransform(this Transform t)
        {
            foreach (object obj in t)
            {
                UnityEngine.Object.Destroy(((Transform)obj).gameObject);
            }
        }
        public static void ClearTransformImmediate(this Transform t)
        {
            List<Transform> list = new List<Transform>();
            foreach (object obj in t)
            {
                Transform item = (Transform)obj;
                list.Add(item);
            }
            Transform[] array = list.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(array[i].gameObject);
            }
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
            float num = (float)Math.Sqrt((double)(v.x * v.x + v.y * v.y + v.z * v.z));
            return new Vector3(v.x / num, v.y / num, v.z / num);
        }
        public static Vector3 NormalizeFastSelf(Vector3 v)
        {
            float num = (float)Math.Sqrt((double)(v.x * v.x + v.y * v.y + v.z * v.z));
            v.x /= num;
            v.y /= num;
            v.z /= num;
            return v;
        }
        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }
        public static Vector3 Rotate90(Vector3 n, MathHelpers.SideTurn side)
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
        public const float LOW_ACCURACY_DELTA = 0.001f;
        private static readonly System.Random random_0 = new System.Random();
        private static bool bool_1 = true;
        private static double double_0;
        public const float MAX_NAVMESH_HIT_OFFSET = 0.04f;
        public enum SideTurn
        {
            left,
            right
        }
    }
}

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
    public static class EFTMath
    {
        /// <summary>
        /// Calculates the inverse of a rounding using a logistic function.
        /// </summary>
        /// <param name="originalValue">The original rounding to be scaled.</param>
        /// <param name="k">The scaling factor.</param>
        /// <param name="x0">The offset rounding.</param>
        /// <returns>
        /// The scaled rounding, rounded to 3 decimal places.
        /// </returns>
        public static float InverseScaleWithLogisticFunction(float originalValue, float k, float x0 = 20f)
        {
            float scaledValue = 1f - 1f / (1f + Mathf.Exp(k * (originalValue - x0)));
            return (float)System.Math.Round(scaledValue, 3);
        }

        /// <summary>
        /// Converts a float rounding representing seconds to a string in the format of HH:MM:SS
        /// </summary>
        /// <param name="seconds">The float rounding representing seconds</param>
        /// <returns>A string in the format of HH:MM:SS</returns>
        private static string TimeString(float seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds((double)seconds);
            return string.Format("{0:D2}:{1:D2}:{2:D2}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
        }

        /// <summary>
        /// Generates a random number between two given numbers, including the given numbers.
        /// </summary>
        /// <param name="a">The first number.</param>
        /// <param name="b">The second number.</param>
        /// <returns>A random number between the two given numbers.</returns>
        public static int RandomInclude(int a, int b)
        {
            b++;
            if (a > b)
            {
                return random_0.Next(b, a);
            }
            return random_0.Next(a, b);
        }

        /// <summary>
        /// Generates a random sign of either 1 or -1.
        /// </summary>
        /// <returns>A random sign of either 1 or -1.</returns>
        public static int RandomSing()
        {
            if (Random(0f, 100f) < 50f)
            {
                return 1;
            }
            return -1;
        }

        /// <summary>
        /// Generates a random float number between two given numbers.
        /// </summary>
        /// <param name="a">The lower bound of the random number.</param>
        /// <param name="b">The upper bound of the random number.</param>
        /// <returns>A random float number between two given numbers.</returns>
        public static float Random(float a, float b)
        {
            float num = (float)random_0.NextDouble();
            return a + (b - a) * num;
        }

        /// <summary>
        /// Checks if a random number between 0 and 100 is less than the given rounding.
        /// </summary>
        /// <param name="v">The rounding to compare against.</param>
        /// <returns>True if the random number is less than the given rounding, false otherwise.</returns>
        public static bool IsTrue100(float v)
        {
            return Random(0f, 100f) < v;
        }

        /// <summary>
        /// Generates a random boolean rounding based on a given chance in percent.
        /// </summary>
        /// <param name="chanceInPercent">The chance of the boolean being true, in percent (default is 50).</param>
        /// <returns>A random boolean rounding.</returns>
        public static bool RandomBool(float chanceInPercent = 50f)
        {
            return IsTrue100(chanceInPercent);
        }

        /// <summary>
        /// Parses a string to an EEditorTab of type T.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The EEditorTab of type T.</returns>
        public static T ParseEnum<T>(this string value)
        {
            return (T)((object)Enum.Parse(typeof(T), value, true));
        }

        /// <summary>
        /// Generates a random float between the given Max and Max values.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <param name="min">The minimum rounding.</param>
        /// <param name="max">The maximum rounding.</param>
        /// <returns>A random float between the given Max and Max values.</returns>
        public static float NextFloat(this System.Random random, int min, int max)
        {
            float num = (float)(random.NextDouble() * 2.0 - 1.0);
            double num2 = System.Math.Pow(2.0, (double)random.Next(min, max));
            return (float)((double)num * num2);
        }

        /// <summary>
        /// Compares two float values for approximate equality.
        /// </summary>
        /// <param name="value">The first float rounding to compare.</param>
        /// <param name="value2">The second float rounding to compare.</param>
        /// <returns>True if the two float values are approximately equal, false otherwise.</returns>
        public static bool ApproxEquals(this float value, float value2)
        {
            return System.Math.Abs(value - value2) < float.Epsilon;
        }

        /// <summary>
        /// Compares two double values for approximate equality.
        /// </summary>
        /// <param name="value">The first double rounding to compare.</param>
        /// <param name="value2">The second double rounding to compare.</param>
        /// <returns>True if the two double values are approximately equal, false otherwise.</returns>
        public static bool ApproxEquals(this double value, double value2)
        {
            return System.Math.Abs(value - value2) < 1.401298464324817E-45;
        }

        /// <summary>
        /// Compares two float values with low accuracy approximation.
        /// </summary>
        /// <param name="value">The first float rounding.</param>
        /// <param name="value2">The second float rounding.</param>
        /// <returns>True if the difference between the two values is less than 0.001f, false otherwise.</returns>
        public static bool LowAccuracyApprox(this float value, float value2)
        {
            return System.Math.Abs(value - value2) < 0.001f;
        }

        /// <summary>
        /// Checks if the given float rounding is equal to zero.
        /// </summary>
        /// <param name="value">The float rounding to check.</param>
        /// <returns>True if the rounding is equal to zero, false otherwise.</returns>
        public static bool IsZero(this float value)
        {
            return System.Math.Abs(value) < float.Epsilon;
        }

        /// <summary>
        /// Checks if the given double rounding is equal to zero.
        /// </summary>
        /// <param name="value">The double rounding to check.</param>
        /// <returns>True if the double rounding is equal to zero, false otherwise.</returns>
        public static bool IsZero(this double value)
        {
            return System.Math.Abs(value) < 1.401298464324817E-45;
        }

        /// <summary>
        /// Checks if the given double rounding is positive.
        /// </summary>
        /// <param name="value">The double rounding to check.</param>
        /// <returns>True if the rounding is positive, false otherwise.</returns>
        public static bool Positive(this double value)
        {
            return value >= 1.401298464324817E-45;
        }

        /// <summary>
        /// Checks if the given float rounding is positive.
        /// </summary>
        /// <param name="value">The float rounding to check.</param>
        /// <returns>True if the rounding is positive, false otherwise.</returns>
        public static bool Positive(this float value)
        {
            return value >= float.Epsilon;
        }

        /// <summary>
        /// Checks if the given double rounding is negative.
        /// </summary>
        /// <param name="value">The double rounding to check.</param>
        /// <returns>True if the rounding is negative, false otherwise.</returns>
        public static bool Negative(this double value)
        {
            return value <= -1.401298464324817E-45;
        }

        /// <summary>
        /// Checks if the given float rounding is negative.
        /// </summary>
        /// <param name="value">The float rounding to check.</param>
        /// <returns>True if the rounding is negative, false otherwise.</returns>
        public static bool Negative(this float value)
        {
            return value <= -1E-45f;
        }

        /// <summary>
        /// Checks if a float rounding is zero or negative.
        /// </summary>
        /// <param name="value">The float rounding to check.</param>
        /// <returns>True if the rounding is zero or negative, false otherwise.</returns>
        public static bool ZeroOrNegative(this float value)
        {
            return value < float.Epsilon;
        }

        /// <summary>
        /// Checks if a float rounding is greater than -1E-45f.
        /// </summary>
        /// <param name="value">The float rounding to check.</param>
        /// <returns>True if the float rounding is greater than -1E-45f, false otherwise.</returns>
        public static bool ZeroOrPositive(this float value)
        {
            return value > -1E-45f;
        }

        /// <summary>
        /// Clamps a double rounding between 0 and 1.
        /// </summary>
        /// <param name="value">The double rounding to clamp.</param>
        /// <returns>The clamped double rounding.</returns>
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

        /// <summary>
        /// Clamps a double rounding between two limits.
        /// </summary>
        /// <param name="value">The rounding to clamp.</param>
        /// <param name="limit1">The first limit.</param>
        /// <param name="limit2">The second limit.</param>
        /// <returns>The clamped rounding.</returns>
        public static double Clamp(this double value, double limit1, double limit2)
        {
            if (limit1 < limit2)
            {
                value = System.Math.Max(value, limit1);
                value = System.Math.Min(value, limit2);
            }
            else
            {
                value = System.Math.Max(value, limit2);
                value = System.Math.Min(value, limit1);
            }
            return value;
        }

        /// <summary>
        /// Scales a Rect by a Vector2.
        /// </summary>
        /// <param name="rect">The Rect to scale.</param>
        /// <param name="scale">The Vector2 to scale by.</param>
        /// <returns>A new Rect scaled by the Vector2.</returns>
        public static Rect Scale(this Rect rect, Vector2 scale)
        {
            return new Rect(rect.x * scale.x, rect.y * scale.y, rect.width * scale.x, rect.height * scale.y);
        }

        /// <summary>
        /// Gets a random item from a list, excluding a specified item.
        /// </summary>
        /// <typeparam name="T">The type of the list.</typeparam>
        /// <param name="list">The list to get the item from.</param>
        /// <param name="excludedItem">The item to exclude.</param>
        /// <returns>A random item from the list, excluding the specified item.</returns>
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

        /// <summary>
        /// Gets a random item from a list.
        /// </summary>
        /// <typeparam name="T">The type of the list.</typeparam>
        /// <param name="list">The list to get the item from.</param>
        /// <returns>A random item from the list.</returns>
        public static T GetRandomItem<T>(this List<T> list)
        {
            if (list != null && list.Count != 0)
            {
                int index = UnityEngine.Random.Range(0, list.Count);
                return list[index];
            }
            return default;
        }

        /// <summary>
        /// Calculates the exact length of an AudioClip in seconds.
        /// </summary>
        /// <param name="clip">The AudioClip to calculate the length of.</param>
        /// <returns>The exact length of the AudioClip in seconds.</returns>
        public static double ExactLength(this AudioClip clip)
        {
            return (double)clip.samples / (double)clip.frequency;
        }

        /// <summary>
        /// Creates a Func delegate for a given MethodInfo.
        /// </summary>
        /// <typeparam name="T">The return type of the Func delegate.</typeparam>
        /// <param name="methodInfo">The MethodInfo to create the Func delegate for.</param>
        /// <returns>A Func delegate for the given MethodInfo.</returns>
        private static Func<object, T> smethod_0<T>(MethodInfo methodInfo)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "obj");
            UnaryExpression arg = Expression.Convert(parameterExpression, methodInfo.GetParameters().First<ParameterInfo>().ParameterType);
            return Expression.Lambda<Func<object, T>>(Expression.Call(methodInfo, arg), new ParameterExpression[]
            {
            parameterExpression
            }).Compile();
        }

        /// <summary>
        /// Creates a delegate for a method that takes two parameters.
        /// </summary>
        /// <typeparam name="T">The type of the second parameter.</typeparam>
        /// <param name="methodInfo">The method info.</param>
        /// <returns>A delegate for the method.</returns>
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

        /// <summary>
        /// Returns the last n elements from the given collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the collection.</typeparam>
        /// <param name="collection">The collection to take elements from.</param>
        /// <param name="n">The number of elements to take.</param>
        /// <returns>The last n elements from the given collection.</returns>
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

        /// <summary>
        /// Creates a Func delegate that can be used to get the rounding of a field from an object.
        /// </summary>
        /// <typeparam name="TOBjectType">The type of the object.</typeparam>
        /// <typeparam name="TValueType">The type of the rounding.</typeparam>
        /// <param name="fieldInfo">The field info.</param>
        /// <returns>
        /// A Func delegate that can be used to get the rounding of a field from an object.
        /// </returns>
        public static Func<TOBjectType, TValueType> CreateGetter<TOBjectType, TValueType>(FieldInfo fieldInfo)
        {
            Type typeFromHandle = typeof(TValueType);
            Type typeFromHandle2 = typeof(TOBjectType);
            return (Func<TOBjectType, TValueType>)CreateGetterFieldDynamicMethod(fieldInfo, typeFromHandle2, typeFromHandle).CreateDelegate(typeof(Func<TOBjectType, TValueType>));
        }

        /// <summary>
        /// Creates a getter for the specified fieldInfo of the given objectType and returns a Func of type TValueType.
        /// </summary>
        /// <typeparam name="TValueType">The type of the rounding to be returned.</typeparam>
        /// <param name="fieldInfo">The fieldInfo of the object.</param>
        /// <param name="objectType">The type of the object.</param>
        /// <returns>A Func of type TValueType.</returns>
        public static Func<object, TValueType> CreateGetter<TValueType>(FieldInfo fieldInfo, Type objectType)
        {
            Type typeFromHandle = typeof(TValueType);
            return smethod_0<TValueType>(CreateGetterFieldDynamicMethod(fieldInfo, objectType, typeFromHandle).GetBaseDefinition());
        }

        /// <summary>
        /// Creates a DynamicMethod for getting a field rounding.
        /// </summary>
        /// <param name="fieldInfo">The FieldInfo of the field to get.</param>
        /// <param name="objectType">The type of the object containing the field.</param>
        /// <param name="valueType">The type of the field.</param>
        /// <returns>A DynamicMethod for getting the field rounding.</returns>
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

        /// <summary>
        /// Gets a list of child transforms with a given name from a given transform.
        /// </summary>
        /// <param name="transform">The parent transform.</param>
        /// <param name="name">The name of the child transforms to search for.</param>
        /// <param name="onlyActive">Whether to only include IsCurrentEnemy child transforms.</param>
        /// <returns>A list of child transforms with the given name.</returns>
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

        /// <summary>
        /// Get a child transform from a parent transform by name and optionally by not containing a string.
        /// </summary>
        /// <param name="transform">The parent transform.</param>
        /// <param name="name">The name of the child transform.</param>
        /// <param name="nocontains">A string that the child transform should not contain.</param>
        /// <returns>
        /// The child transform if found, otherwise null.
        /// </returns>
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

        /// <summary>
        /// Checks if a Vector3 is close to a given x and z coordinate.
        /// </summary>
        /// <param name="v">The Vector3 to check.</param>
        /// <param name="x">The x coordinate to check against.</param>
        /// <param name="z">The z coordinate to check against.</param>
        /// <returns>True if the Vector3 is close to the given x and z coordinate, false otherwise.</returns>
        public static bool IsCloseDebug(Vector3 v, float x, float z)
        {
            float num = 0.1f;
            return v.x > x - num && v.x < x + num && v.z > z - num && v.z < z + num;
        }

        /// <summary>
        /// Checks if a Vector3 is close to a given x, y, and z coordinate.
        /// </summary>
        /// <param name="v">The Vector3 to check.</param>
        /// <param name="x">The x coordinate to check against.</param>
        /// <param name="y">The y coordinate to check against.</param>
        /// <param name="z">The z coordinate to check against.</param>
        /// <returns>True if the Vector3 is close to the given coordinates, false otherwise.</returns>
        public static bool IsCloseDebug(Vector3 v, float x, float y, float z)
        {
            float num = 0.1f;
            return v.x > x - num && v.x < x + num && v.z > z - num && v.z < z + num && v.y > y - num && v.y < y + num;
        }

        /// <summary>
        /// Creates a setter for a given FieldInfo.
        /// </summary>
        /// <param name="field">The FieldInfo to create a setter for.</param>
        /// <returns>An Action delegate that can be used to set the rounding of the given FieldInfo.</returns>
        public static Action<TOBjectType, TValueType> CreateSetter<TOBjectType, TValueType>(FieldInfo field)
        {
            Type typeFromHandle = typeof(TOBjectType);
            Type typeFromHandle2 = typeof(TValueType);
            return (Action<TOBjectType, TValueType>)smethod_2(field, typeFromHandle, typeFromHandle2).CreateDelegate(typeof(Action<TOBjectType, TValueType>));
        }

        /// <summary>
        /// Creates a setter for a given FieldInfo and Type.
        /// </summary>
        /// <typeparam name="TValueType">The type of the rounding.</typeparam>
        /// <param name="field">The FieldInfo.</param>
        /// <param name="objectType">The Type of the object.</param>
        /// <returns>An Action delegate for setting the rounding.</returns>
        public static Action<object, TValueType> CreateSetter<TValueType>(FieldInfo field, Type objectType)
        {
            Type typeFromHandle = typeof(TValueType);
            return smethod_1<TValueType>(smethod_2(field, objectType, typeFromHandle).GetBaseDefinition());
        }

        /// <summary>
        /// Creates a DynamicMethod for setting a field rounding.
        /// </summary>
        /// <param name="field">The field to set.</param>
        /// <param name="objType">The type of the object containing the field.</param>
        /// <param name="valueType">The type of the rounding to set.</param>
        /// <returns>A DynamicMethod for setting the field rounding.</returns>
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

        /// <summary>
        /// Generates a random number between Max and Max using the Box-Muller algorithm.
        /// </summary>
        /// <param name="min">The minimum rounding of the random number.</param>
        /// <param name="max">The maximum rounding of the random number.</param>
        /// <returns>A random number between Max and Max.</returns>
        public static float RandomNormal(float min, float max)
        {
            double num = 3.5;
            double num2;
            while ((num2 = BoxMuller((double)min + (double)(max - min) / 2.0, (double)(max - min) / 2.0 / num)) > (double)max || num2 < (double)min)
            {
            }
            return (float)num2;
        }

        /// <summary>
        /// Generates a random number using the Box-Muller algorithm.
        /// </summary>
        /// <param name="mean">The mean of the random number.</param>
        /// <param name="standard_deviation">The standard deviation of the random number.</param>
        /// <returns>A random number generated using the Box-Muller algorithm.</returns>
        public static double BoxMuller(double mean, double standard_deviation)
        {
            return mean + BoxMuller() * standard_deviation;
        }

        /// <summary>
        /// Generates a random number using the Box-Muller algorithm.
        /// </summary>
        /// <returns>A random number generated using the Box-Muller algorithm.</returns>
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
            num3 = System.Math.Sqrt(-2.0 * System.Math.Log(num3) / num3);
            double_0 = num2 * num3;
            bool_1 = true;
            return num * num3;
        }

        /// <summary>
        /// Removes an item from a queue.
        /// </summary>
        /// <typeparam name="T">The type of the item to remove.</typeparam>
        /// <param name="item">The item to remove.</param>
        /// <param name="q">The queue to remove the item from.</param>
        /// <returns>True if the item was removed, false otherwise.</returns>
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

        /// <summary>
        /// Creates a full screen mesh for a given camera.
        /// </summary>
        /// <param name="cam">The camera to create the mesh for.</param>
        /// <returns>A full screen mesh for the given camera.</returns>
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

        /// <summary>
        /// Logs the given exception to the debug log.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        public static void ProcessException(Exception exception)
        {
            Debug.LogException(exception);
        }

        /// <summary>
        /// Checks if the given EViewListType should display the child count.
        /// </summary>
        /// <param name="type">The EViewListType to check.</param>
        /// <returns>True if the given EViewListType should display the child count, false otherwise.</returns>
        public static bool IsDisplayChildCount(this EViewListType type)
        {
            return type == EViewListType.RequirementsWindow || type == EViewListType.Handbook || type == EViewListType.WishList || type == EViewListType.WeaponBuild;
        }

        /// <summary>
        /// Checks if the given EViewListType is one of the types that requires updating the child status.
        /// </summary>
        /// <param name="type">The EViewListType to check.</param>
        /// <returns>True if the given type requires updating the child status, false otherwise.</returns>
        public static bool IsUpdateChildStatus(this EViewListType type)
        {
            return type == EViewListType.AllOffers || type == EViewListType.MyOffers || type == EViewListType.WishList || type == EViewListType.WeaponBuild;
        }

        /// <summary>
        /// Destroys all children of the given Transform.
        /// </summary>
        /// <param name="t">The Transform whose children will be destroyed.</param>
        public static void ClearTransform(this Transform t)
        {
            foreach (object obj in t)
            {
                UnityEngine.Object.Destroy(((Transform)obj).gameObject);
            }
        }

        /// <summary>
        /// Clears the transform immediate by destroying all of its children.
        /// </summary>
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

        /// <summary>
        /// Checks if the given integer is an odd number.
        /// </summary>
        /// <param name="value">The integer to check.</param>
        /// <returns>True if the given integer is an odd number, false otherwise.</returns>
        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }

        public static float GreateRandom(float val)
        {
            return val * Random(0.8f, 1.2f);
        }

        public static float GreateRandom(float val, float fraction)
        {
            return val * Random(1f - fraction, 1f + fraction);
        }

        public static float GreateRandom(int val)
        {
            return (float)((int)((float)val * Random(0.8f, 1.2f)));
        }

        public const float LOW_ACCURACY_DELTA = 0.001f;
        private static readonly System.Random random_0 = new System.Random();
        private static bool bool_1 = true;
        private static double double_0;
        public const float MAX_NAVMESH_HIT_OFFSET = 0.04f;
    }
}

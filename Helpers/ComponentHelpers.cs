
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;

namespace SAIN.Helpers
{
    internal class ComponentHelpers
    {
        public static T AddOrDestroyComponent<T, K>(T component, K condition) where T : Component where K : Component
        {
            if (component == null && condition != null)
            {
                component = GetOrAddComponent<T, K>(condition);
            }
            else if (component != null && condition == null)
            {
                DestroyComponent(component);
            }
            return component;
        }

        public static void DestroyComponent<T>(T component) where T : Component
        {
            if (component == null)
            {
                LogDebug("Component is already null, no need to dispose");
                return;
            }

            try
            {
                if (TryGetDisposeMethod<T>(out var disposeMethod))
                {
                    disposeMethod.Invoke(component, null);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Dispose");
            }

            if (component != null)
            {
                try
                {
                    UnityEngine.Object.Destroy(component);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Destroy");
                }
            }
        }

        private static bool TryGetDisposeMethod<T>(out MethodInfo methodInfo)
        {
            methodInfo = null;
            Type type = typeof(T);
            if (!NoDisposeMethods.Contains(type.Name) && !DisposeMethods.ContainsKey(type))
            {
                methodInfo = type.GetMethod("Dispose");
                if (methodInfo != null)
                {
                    LogDebug($"Caching Dispose Method {type.Name}");
                    DisposeMethods.Add(type, methodInfo);
                }
                else
                {
                    NoDisposeMethods.Add(type.Name);
                }
            }
            else if (DisposeMethods.ContainsKey(type))
            {
                methodInfo = DisposeMethods[type];
            }
            return methodInfo != null;
        }

        private static readonly List<string> NoDisposeMethods = new List<string>();

        private static void LogError(Exception ex, string message)
        {
            Logger.LogError($"{message} Error");
            Logger.LogError(ex);
        }
        private static void LogDebug(string message)
        {
            Logger.LogDebug(message);
        }

        public static void ClearCache()
        {
            ListHelpers.ClearCache(DisposeMethods);
        }

        private static readonly Dictionary<Type, MethodInfo> DisposeMethods = new Dictionary<Type, MethodInfo>();

        public static T GetOrAddComponent<T, K>(K original) where T : Component where K : Component
        {
            return original.GetComponent<T>() ?? original.gameObject.AddComponent<T>();
        }
    }
}

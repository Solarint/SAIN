using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace SAIN
{
    internal static class Logger
    {
        public static void LogInfo(object data, Type sourceClass = null, bool getMethodInfo = false)
        {
            data = BuildMessage(data, getMethodInfo);
            SelectLogSource(sourceClass).LogInfo(data);
        }

        public static void LogDebug(object data, Type sourceClass = null, bool getMethodInfo = false)
        {
            data = BuildMessage(data, getMethodInfo);
            SelectLogSource(sourceClass).LogDebug(data);
        }

        public static void LogWarning(object data, Type sourceClass = null, bool getMethodInfo = false)
        {
            data = BuildMessage(data, getMethodInfo);
            SelectLogSource(sourceClass).LogWarning(data);
        }

        public static void LogError(object data, Type sourceClass = null, bool getMethodInfo = false)
        {
            data = BuildMessage(data, getMethodInfo);
            SelectLogSource(sourceClass).LogError(data);
        }

        private static ManualLogSource SelectLogSource(Type type = null)
        {
            string name = type?.Name ?? "SAIN";
            if (!LogSourcesDictionary.ContainsKey(name))
            {
                LogSourcesDictionary.Add(name, BepInEx.Logging.Logger.CreateLogSource(name));
            }
            return LogSourcesDictionary[name];
        }

        public static Dictionary<string, ManualLogSource> LogSourcesDictionary = new Dictionary<string, ManualLogSource>();

        public static object BuildMessage(object data, bool methodInfo = false)
        {
            if (methodInfo)
            {
                StackTrace stackTrace = new StackTrace();
                data = CreateMessageString
                (
                    data,
                    stackTrace.GetFrame(2).GetMethod(),
                    stackTrace.GetFrame(3).GetMethod(),
                    stackTrace.GetFrame(4).GetMethod()
                );
            }
            return data;
        }

        private static object CreateMessageString(object message, params MethodBase[] methods)
        {
            string methodName = null;
            string className = null;
            foreach (MethodBase method in methods)
            {
                methodName = methodName == null ? method.Name : $"{methodName} {method.Name}";
                className = className ?? method.DeclaringType.Name;
            }

            string output = $"[{className}] [{methodName}] [{message}]";
            return output;
        }
    }
}

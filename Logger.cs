using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace SAIN
{
    internal static class Logger
    {
        static Logger()
        {   
            DefaultLogger = BepInEx.Logging.Logger.CreateLogSource("SAIN");
            LogSources.Add(nameof(DefaultLogger), DefaultLogger);
        }

        private static Action<string> LogInfoAction => LogSources[SelectedLogger].LogInfo;
        private static Action<string> LogDebugAction => LogSources[SelectedLogger].LogDebug;
        private static Action<string> LogWarningAction => LogSources[SelectedLogger].LogWarning;
        private static Action<string> LogErrorAction => LogSources[SelectedLogger].LogError;

        public static void LogInfo(string message, Type source = null, bool methodInfo = false)
        {
            SelectLogSource(source);
            Log(LogInfoAction, message, methodInfo);
        }

        public static void LogDebug(string message, Type source = null, bool methodInfo = false)
        {
            SelectLogSource(source);
            Log(LogDebugAction, message, methodInfo);
        }

        public static void LogWarning(string message, Type source = null, bool methodInfo = false)
        {
            SelectLogSource(source);
            Log(LogWarningAction, message, methodInfo);
        }

        public static void LogError(string message, Type source = null, bool methodInfo = false)
        {
            SelectLogSource(source);
            Log(LogErrorAction, message, methodInfo);
        }

        private static void SelectLogSource(Type type)
        {
            if (type == null)
            {
                SelectedLogger = nameof(DefaultLogger);
            }
            string name = type.Name;
            if (!LogSources.ContainsKey(name))
            {
                LogSources.Add(name, BepInEx.Logging.Logger.CreateLogSource(name));
            }
            SelectedLogger = name;
        }


        private static string SelectedLogger = nameof(DefaultLogger);
        public static Dictionary<string, ManualLogSource> LogSources = new Dictionary<string, ManualLogSource>();
        private static readonly ManualLogSource DefaultLogger;

        public static void Log(Action<string> logAction, string message, bool methodInfo = false)
        {
            if (methodInfo)
            {
                StackTrace stackTrace = new StackTrace();
                StackFrame stackFrame = stackTrace.GetFrame(2);
                MethodBase method = stackFrame.GetMethod();
                message = CreateMessage(method, message);
            }
            logAction(message);
        }

        private static string CreateMessage(MethodBase method, string message)
        {
            string methodName = method.Name;
            string className = method.DeclaringType.Name;

            string output = "[" + className + "." + methodName + "] " + message;
            return output;
        }
    }
}

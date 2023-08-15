using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace SAIN
{
    internal static class Logger
    {
        public static void LogInfo(object data) => Log(LogLevel.Info, data);
        public static void LogDebug(object data) => Log(LogLevel.Debug, data);
        public static void LogWarning(object data) => Log(LogLevel.Warning, data);
        public static void LogError(object data) => Log(LogLevel.Error, data);

        private static void Log(LogLevel level, object data)
        {
            string methods = string.Empty;
            Type declaringType = null;

            if (level != LogLevel.Debug)
            {
                int max = GetMaxFrames(level);
                int count = 0;
                StackTrace stackTrace = new StackTrace();
                for (int i = 0; i < stackTrace.FrameCount; i++)
                {
                    var method = stackTrace.GetFrame(i).GetMethod();
                    Type methodType = method.DeclaringType;

                    if (methodType == typeof(Logger)) continue;

                    declaringType = declaringType ?? methodType;
                    methods += "." + method.Name;

                    if (count >= max) break;
                    count++;
                }
                methods = $"[{methods}]:";
            }

            LastLogData = data;
            string result = $"{methods} [{data}]";
            SelectLogSource(declaringType).Log(level, result);
        }

        private static int GetMaxFrames(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug: return 0;
                case LogLevel.Info: return 1;
                case LogLevel.Warning: return 2;
                case LogLevel.Error: return 3;
                case LogLevel.Fatal: return 4;
                default: return 0;
            }
        }

        public static object LastLogData { get; private set; }

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

    }
}
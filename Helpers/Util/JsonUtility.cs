using Aki.Common.Utils;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SAIN.BotPresets;
using SAIN.Classes;
using SAIN.Editor;
using SAIN.Editor.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SAIN.BotSettings;
using SAIN.SAINPreset;
using SAIN.SAINPreset.Personalities;

namespace SAIN.Helpers
{
    public static class JsonUtility
    {
        private static ManualLogSource Logger => Utility.Logger;

        public const string EditorName = "Editor";
        public const string SAINName = "SAIN";
        public const string PresetsFolder = "Presets";
        public const string SAINConfigPresetFolder = "SAINConfig";
        public const string BotConfigFolder = "EFTBotConfig";
        public const string UIFolder = "UI";
        public const string TextureFolder = "Textures";
        public const string ColorsFolder = "Colors";
        public const string ColorScheme = nameof(ColorScheme);
        public const string ColorNames = nameof(ColorNames);
        public const string SettingsFolder = "BotSettings";
        public const string JSON = ".json";
        public const string JSONSearch = "*" + JSON;
        public const string Settings = "Settings";
        public const string Info = "Info";

        public static class Save
        {
            public static void SaveObject(object objectToSave, string fileExtension, string fileName, params string[] folders)
            {
                if (CheckNull(objectToSave)) return;

                string foldersPath = GetFoldersPath(folders);
                string filePath = Path.Combine(foldersPath, fileName);
                filePath += fileExtension;

                string jsonString = JsonConvert.SerializeObject(objectToSave, Formatting.Indented);
                File.Create(filePath).Dispose();
                StreamWriter streamWriter = new StreamWriter(filePath);
                streamWriter.Write(jsonString);
                streamWriter.Flush();
                streamWriter.Close();
            }

            public static void SaveJson(object objectToSave, string fileName, params string[] folders)
            {
                if (CheckNull(objectToSave)) return;

                SaveObject(objectToSave, ".json", fileName, folders);
            }

            static bool CheckNull(object obj)
            {
                bool isNull = obj == null;
                if (isNull)
                {
                    LogError("Object is Null, cannot save.");
                }
                return isNull;
            }
        }

        public static class Load
        {
            public static List<T> LoadAllJsonFiles<T>(params string[] folders)
            {
                return LoadAllFiles<T>(JSONSearch, folders);
            }

            public static List<SAINPresetDefinition> GetPresetOptions(List<SAINPresetDefinition> list)
            {
                list.Clear();
                string foldersPath = GetFoldersPath(PresetsFolder);
                var array = Directory.GetDirectories(foldersPath);
                foreach ( var item in array )
                {
                    string path = Path.Combine(item, Info) + JSON;
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        var obj = DeserializeObject<SAINPresetDefinition>(json);
                        list.Add(obj);
                    }
                    else
                    {
                        SAIN.Logger.LogError(path, typeof(JsonUtility), true);
                    }
                }
                return list;
            }

            public static List<T> LoadAllFiles<T>(string searchPattern , params string[] folders)
            {
                string foldersPath = GetFoldersPath(folders);
                var files = Directory.GetFiles(foldersPath, searchPattern);
                var result = new List<T>();
                foreach (var file in files)
                {
                    string jsonContent = File.ReadAllText(file);
                    var loaded = JsonConvert.DeserializeObject<T>(jsonContent);
                    result.Add(loaded);
                }
                return result;
            }

            public static T DeserializeObject<T>(string file)
            {
                return JsonConvert.DeserializeObject<T>(file);
            }

            public static string LoadTextFile(string fileExtension, string fileName, params string[] folders)
            {
                string foldersPath = GetFoldersPath(folders);
                string filePath = Path.Combine(foldersPath, fileName);

                filePath += fileExtension;

                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath);
                }
                return null;
            }

            public static bool LoadJsonFile(out string json, string fileName, params string[] folders)
            {
                json = LoadTextFile(JSON, fileName, folders);
                return json != null;
            }

            public static bool LoadObject<T>(out T obj, string fileName, params string[] folders)
            {
                string json = LoadTextFile(JSON, fileName, folders);
                if (json != null)
                {
                    obj = DeserializeObject<T>(json);
                    return true;
                }
                obj = default;
                return false;
            }
        }

        private static void CheckDirectionary(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static string GetFoldersPath(params string[] folders)
        {
            string path = GetSAINPluginPath();
            for (int i = 0; i < folders.Length; i++)
            {
                path = Path.Combine(path, folders[i]);
            }
            CheckDirectionary(path);
            return path;
        }

        private static string GetSAINPluginPath()
        {
            var path = Path.Combine(PluginFolder, SAINFolder);
            CheckDirectionary(path);
            return path;
        }

        private const string SAINFolder = nameof(SAIN);
        private static readonly string PluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private static void LogInfo(string message)
        {
            SAIN.Logger.LogInfo(message, typeof(JsonUtility), true);
        }
        private static void LogDebug(string message)
        {
            SAIN.Logger.LogDebug(message, typeof(JsonUtility), true);
        }
        private static void LogWarning(string message)
        {
            SAIN.Logger.LogWarning(message, typeof(JsonUtility), true);
        }
        private static void LogError(string message)
        {
            SAIN.Logger.LogError(message, typeof(JsonUtility), true);
        }
    }
}
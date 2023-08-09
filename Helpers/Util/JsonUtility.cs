using Aki.Common.Utils;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SAIN.Preset;
using SAIN.Editor;
using SAIN.Editor.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SAIN.Preset.Personalities;

namespace SAIN.Helpers
{
    public enum JsonUtilityEnum
    {
        Json,
        JsonSearch,
        Presets,
        BigBrain,
        BigBrainBrains,
        BigBrainLayers,
        BigBrainLayerNames,
        GlobalSettings,
        EFTBotSettings,
        SAINBotSettings,
        DefaultEditorSettings,
        PresetDefinition,
        Personalities,
    }

    public static class JsonUtility
    {
        public static readonly Dictionary<JsonUtilityEnum, string> FileAndFolderNames = new Dictionary<JsonUtilityEnum, string> 
        {
            { JsonUtilityEnum.Json, ".json" },
            { JsonUtilityEnum.JsonSearch, "*.json" },
            { JsonUtilityEnum.Presets, "Presets" },
            { JsonUtilityEnum.BigBrain, "BigBrain - DO NOT TOUCH" },
            { JsonUtilityEnum.EFTBotSettings, "EFT Bot Settings - DO NOT TOUCH" },
            { JsonUtilityEnum.BigBrainBrains, "BrainInfos" },
            { JsonUtilityEnum.BigBrainLayers, "LayerInfos" },
            { JsonUtilityEnum.BigBrainLayerNames, "LayerNames" },
            { JsonUtilityEnum.GlobalSettings, "GlobalSettings" },
            { JsonUtilityEnum.SAINBotSettings, "BotSettings" },
            { JsonUtilityEnum.DefaultEditorSettings, "*.json" },
            { JsonUtilityEnum.PresetDefinition, "*Info" },
            { JsonUtilityEnum.Personalities, "*.json" },
        };

        public const string PresetsFolder = "Presets";
        public const string JSON = ".json";
        public const string JSONSearch = "*" + JSON;
        public const string Info = "Info";

        public static void SaveObjectToJson(object objectToSave, string fileName, params string[] folders)
        {
            if (objectToSave == null)
            {
                return;
            }

            try
            {
                string foldersPath = GetFoldersPath(folders);
                string filePath = Path.Combine(foldersPath, fileName);
                filePath += ".json";

                string jsonString = JsonConvert.SerializeObject(objectToSave, Formatting.Indented);
                File.Create(filePath).Dispose();
                StreamWriter streamWriter = new StreamWriter(filePath);
                streamWriter.Write(jsonString);
                streamWriter.Flush();
                streamWriter.Close();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        public static class Load
        {
            public static void LoadAllJsonFiles<T>(List<T> list, params string[] folders)
            {
                LoadAllFiles(list, "*.json", folders);
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

            public static void LoadAllFiles<T>(List<T> list, string searchPattern = null , params string[] folders)
            {
                string foldersPath = GetFoldersPath(folders);

                string[] files;
                if (searchPattern != null)
                {
                    files = Directory.GetFiles(foldersPath, searchPattern);
                }
                else
                {
                    files = Directory.GetFiles(foldersPath);
                }

                if (list == null)
                {
                    list = new List<T>();
                }
                foreach (var file in files)
                {
                    string jsonContent = File.ReadAllText(file);
                    list.Add(JsonConvert.DeserializeObject<T>(jsonContent));
                }
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
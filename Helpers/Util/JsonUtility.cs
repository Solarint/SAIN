using BepInEx.Logging;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SAIN.BotPresets;
using SAIN.Editor;
using SAIN.Editor.Util;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using static HBAO_Core;

namespace SAIN.Helpers
{
    public static class JsonUtility
    {
        private static ManualLogSource Logger => Utility.Logger;

        public static class Save
        {
            private static int count = 1;

            public static void SaveObject(object objectToSave, string fileExtension, string fileName, params string[] folders)
            {
                if (CheckNull(objectToSave)) return;

                string foldersPath = GetFoldersPath(folders);
                string filePath = Path.Combine(foldersPath, fileName);
                filePath += fileExtension;

                string json = JsonConvert.SerializeObject(objectToSave);
                File.WriteAllText(filePath, json);
            }

            public static void SaveJson(object objectToSave, string fileName, params string[] folders)
            {
                if (CheckNull(objectToSave)) return;

                SaveObject(objectToSave, ".json", fileName, folders);
            }

            public static void SaveScheme(BaseColorSchemeClass scheme)
            {
                if (CheckNull(scheme)) return;

                SaveJson(scheme, ColorScheme, UIFolder, ColorsFolder);
            }

            public static void SavePreset(BotPreset preset)
            {
                if (CheckNull(preset)) return;

                SaveJson(preset, preset.WildSpawnType.ToString(), PresetsFolder);
            }

            public static void SavePresets(BotPreset[] presets)
            {
                for (int i = 0; i < presets.Length; i++)
                {
                    SavePreset(presets[i]);
                }
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

            public static void EditorSettings(EditorCustomization custom)
            {
                if (CheckNull(custom)) return;

                SaveJson(custom, "EditorSettings", UIFolder);
            }

            public static void List<T>(List<T> inputList, string inputFolder, string inputName, bool useMapName, bool overwrite = true)
            {
                if (useMapName && Singleton<GameWorld>.Instance?.MainPlayer?.Location == null)
                {
                    return;
                }

                var path = GetSendPath(inputFolder, inputName, useMapName, overwrite);

                var json = inputList.ToJson();
                File.WriteAllText(path, json);

                Logger.LogDebug($"Saved [{inputList.GetType()}] to [{path}]");
            }

            public static void Array<T>(T[] inputArray, string inputFolder, string inputName, bool useMapName = true, bool overwrite = true)
            {
                if (useMapName && Singleton<GameWorld>.Instance?.MainPlayer?.Location == null)
                {
                    return;
                }

                var path = GetSendPath(inputFolder, inputName, useMapName, overwrite);

                var json = JsonConvert.SerializeObject(inputArray, Formatting.Indented);
                File.WriteAllText(path, json);

                Logger.LogDebug($"Saved [{inputArray.GetType()}] to [{path}]");
            }

            private static string GetSendPath(string inputFolder, string inputName, bool useMapName, bool overwrite)
            {
                string name = GetNameAndSubNames(inputName, useMapName);

                if (!overwrite)
                {
                    string unique = Time.timeSinceLevelLoad + "_" + count.ToString();
                    count++;
                    name += unique;
                }

                name += ".json";

                return Path.Combine(GetSAINPluginPath(inputFolder), name);
            }
        }

        const string EditorName = "Editor";
        const string SAINName = "SAIN";
        const string PresetsFolder = "BotPresets";
        const string UIFolder = "UI";
        const string TextureFolder = "Textures";
        const string ColorsFolder = "Colors";
        const string ColorScheme = nameof(ColorScheme);
        const string ColorNames = nameof(ColorNames);

        public static class Load
        {
            public static BaseColorSchemeClass LoadColorScheme()
            {
                BaseColorSchemeClass result;
                if (LoadJsonFile( out string schemeJson , ColorScheme, UIFolder, ColorsFolder ))
                {
                    result = DeserializeObject<BaseColorSchemeClass>( schemeJson );
                }
                else
                {
                    result = new BaseColorSchemeClass(nameof(SAIN));
                }
                if (LoadJsonFile(out string namesJson, ColorNames, UIFolder, ColorsFolder))
                {
                    DeserializeObject<ColorNames>(namesJson);
                }
                else
                {
                    var names = new ColorNames(nameof(SAIN));
                    Save.SaveJson(names, ColorNames, UIFolder, ColorsFolder);
                }
                return result;
            }

            public static T DeserializeObject<T>(string file)
            {
                return JsonConvert.DeserializeObject<T>(file);
            }

            [CanBeNull]
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
                json = LoadTextFile(".json", fileName, folders);
                return json != null;
            }

            public static EditorCustomization EditorSettings()
            {
                if (LoadJsonFile(out string json, EditorName, UIFolder))
                {
                    return DeserializeObject<EditorCustomization>(json);
                }
                return new EditorCustomization();
            }

            public static BotPreset BotPreset(WildSpawnType type)
            {
                if (LoadJsonFile(out string json, type.ToString(), PresetsFolder))
                {
                    return DeserializeObject<BotPreset>(json); // Deserialize to BotPreset
                }
                else
                {
                    LogWarning($"BotPreset for {type} does not exist");
                    return null;
                }
            }

            public static bool GetSingle<T>(out List<T> outputList, string inputFolder, string inputName, bool forMap)
            {
                outputList = null;

                var name = GetNameAndSubNames(inputName, forMap);
                name += ".json";

                var filePath = Path.Combine(GetSAINPluginPath(inputFolder), name);

                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    outputList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(json);
                }
                else
                {
                    Logger.LogWarning($"File {filePath} does not exist");
                }

                return outputList != null;
            }

            public static bool GetAll<T>(out List<T> outputList, string inputFolder, string inputName, bool forMap)
            {
                outputList = null;

                string name = GetNameAndSubNames(inputName, forMap);

                string filePath = Path.Combine(GetSAINPluginPath(inputFolder), name);

                List<T> values = new List<T>();

                if (File.Exists(filePath))
                {
                    var list = Directory.EnumerateFiles(filePath);

                    foreach (var item in list)
                    {
                        if (item.StartsWith(name))
                        {
                            string json = File.ReadAllText(item);

                            var savedList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(json);

                            values.AddRange(savedList);
                        }
                    }
                    outputList = values;
                }
                else
                {
                    Logger.LogWarning($"File {filePath} does not exist");
                }

                return outputList != null;
            }
        }

        private static void CheckDirectionary(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static string GetNameAndSubNames(string filename, bool useMap)
        {
            return useMap ? CurrentMap + "_" + filename : filename;
        }

        private static string CurrentMap
        {
            get
            {
                if (Singleton<GameWorld>.Instance?.MainPlayer?.Location == null)
                {
                    return "null";
                }
                else
                {
                    return Singleton<GameWorld>.Instance.MainPlayer.Location.ToLower();
                }
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

        private static string GetSAINPluginPath(string folder)
        {
            string path = Path.Combine(GetSAINPluginPath(), folder);
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
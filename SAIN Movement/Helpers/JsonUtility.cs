using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SAIN.Classes
{
    public static class JsonUtility
    {
        public static class SaveToJson
        {
            private static int count = 1;
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

                return Path.Combine(GetPluginPath(inputFolder), name);
            }
        }

        public static class LoadFromJson
        {
            public static bool GetSingle<T>(out List<T> outputList, string inputFolder, string inputName, bool forMap)
            {
                outputList = null;

                var name = GetNameAndSubNames(inputName, forMap);
                name += ".json";

                var filePath = Path.Combine(GetPluginPath(inputFolder), name);

                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    outputList = JsonConvert.DeserializeObject<List<T>>(json);
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

                string filePath = Path.Combine(GetPluginPath(inputFolder), name);

                List<T> values = new List<T>();

                if (File.Exists(filePath))
                {
                    var list = Directory.EnumerateFiles(filePath);

                    foreach (var item in list)
                    {
                        if (item.StartsWith(name))
                        {
                            string json = File.ReadAllText(item);

                            var savedList = JsonConvert.DeserializeObject<List<T>>(json);

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

        private static string GetPluginPath(string folder)
        {
            var path = Path.Combine(PluginFolder, folder);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        private static readonly string PluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("JsonUtility");
    }
}
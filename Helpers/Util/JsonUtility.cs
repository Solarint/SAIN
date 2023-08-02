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

namespace SAIN.Helpers
{
    public static class JsonUtility
    {
        private static ManualLogSource Logger => Utility.Logger;

        const string EditorName = "Editor";
        const string SAINName = "SAIN";
        const string PresetsFolder = "BotPresets";
        const string SAINConfigPresetFolder = "SAINConfig";
        const string BotConfigFolder = "EFTBotConfig";
        const string UIFolder = "UI";
        const string TextureFolder = "Textures";
        const string ColorsFolder = "Colors";
        const string ColorScheme = nameof(ColorScheme);
        const string ColorNames = nameof(ColorNames);
        const string SettingsFolder = "BotSettings";
        const string JSON = ".json";
        const string JSONSearch = "*" + JSON;
        const string PresetEnd = "_preset";
        const string PresetSearch = "*" + PresetEnd;
        const string Settings = "settings";
        const string Info = "info";

        public static class Save
        {
            private static int count = 1;

            public static string SelectedPresetName { get; set; }

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

            public static void SavePreset(string name, string description, string version, List<BotPreset> presets)
            {
                var info = new SAINPresetInfo
                {
                    Name = name,
                    Description = description,
                    SAINVersion = version,
                    Presets = presets
                };
                SavePreset(info);
            }

            public static void SavePreset(SAINPresetInfo preset)
            {
                string folderName = preset.Name + PresetEnd;
                SaveJson(preset, Info, PresetFolders(folderName));
                SavePresets(preset.Presets, folderName);
            }

            public static void SavePersonalities(Dictionary<SAINPersonality, PersonalitySettingsClass> Personalities)
            {
                foreach (var pers in Personalities)
                {
                    SaveJson(pers.Value, pers.Key.ToString(), "Personalities");
                }
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

            public static void SavePreset(BotPreset preset, string namedPresetFolder)
            {
                if (CheckNull(preset)) return;

                SaveJson(preset, preset.DisplayName, SAINConfigFolders(namedPresetFolder));
            }

            public static void SavePreset(BotPreset preset)
            {
                SavePreset(preset, SelectedPresetName);
            }

            public static void SavePresets(BotPreset[] presets, string namedPresetFolder)
            {
                for (int i = 0; i < presets.Length; i++)
                {
                    SavePreset(presets[i], namedPresetFolder);
                }
            }

            public static void SavePresets(BotPreset[] presets)
            {
                SavePresets(presets, SelectedPresetName);
            }

            public static void SavePresets(List<BotPreset> presets, string namedPresetFolder)
            {
                for (int i = 0; i < presets.Count; i++)
                {
                    SavePreset(presets[i], namedPresetFolder);
                }
            }

            public static void SavePresets(List<BotPreset> presets)
            {
                SavePresets(presets, SelectedPresetName);
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

            public static void SaveBotSettings(SAINBotSettingsClass settings, BotOwner owner)
            {
                SaveBotSettings(settings, owner, SelectedPresetName);
            }

            public static void SaveBotSettings(SAINBotSettingsClass settings, BotOwner owner, string presetName)
            {
                var role = owner.Profile.Info.Settings.Role;
                var diff = owner.Profile.Info.Settings.BotDifficulty;
                SaveBotSettings(settings, diff, role, presetName);
            }

            public static void SaveBotSettings(SAINBotSettingsClass settings, BotDifficulty difficulty, WildSpawnType role)
            {
                SaveBotSettings(settings, difficulty, role, SelectedPresetName);
            }

            public static void SaveBotSettings(SAINBotSettingsClass settings, BotDifficulty difficulty, WildSpawnType role, string presetName)
            {
                string filename = role.ToString();
                SaveJson(settings, filename, EFTBotConfigFolders(presetName));
            }
        }

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
                    //Save.Export(result, ColorScheme, UIFolder, ColorsFolder);
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

            public static List<T> LoadAllJsonFiles<T>(params string[] folders)
            {
                return LoadAllFiles<T>(JSONSearch, folders);
            }

            public static List<BotPreset> LoadPresetsFromFolder(params string[] folders)
            {
                return LoadAllFiles<BotPreset>(JSONSearch, folders);
            }

            public static SAINPresetInfo LoadPreset(string name)
            {
                string json = LoadTextFile(JSON, Info, PresetFolders(name));
                if (json == null)
                {
                    return null;
                }

                var result = DeserializeObject<SAINPresetInfo>(json);
                return result;
            }

            public static SAINPresetInfo LoadPresetList(SAINPresetInfo preset)
            {
                if (preset != null)
                {
                    preset.Presets = LoadPresetsFromFolder(SAINConfigFolders(preset.Name));
                }
                return preset;
            }

            public static string[] GetPresetOptions()
            {
                string foldersPath = GetFoldersPath(PresetsFolder);
                return Directory.GetDirectories(foldersPath, PresetSearch);
            }

            public static List<string> GetPresetOptions(List<string> list)
            {
                list.Clear();
                string foldersPath = GetFoldersPath(PresetsFolder);
                var array = Directory.GetDirectories(foldersPath, PresetSearch);
                list.AddRange(array);
                return list;
            }

            public static List<SAINPresetInfo> GetPresetOptions(List<SAINPresetInfo> list)
            {
                list.Clear();
                string foldersPath = GetFoldersPath(PresetsFolder);
                var array = Directory.GetDirectories(foldersPath, PresetSearch);
                foreach ( var item in array )
                {
                    string path = Path.Combine(item, Info) + JSON;
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        var obj = DeserializeObject<SAINPresetInfo>(json);
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

            public static Dictionary<SAINPersonality, PersonalitySettingsClass> LoadPersonalityClasses()
            {
                var Personalities = new Dictionary<SAINPersonality, PersonalitySettingsClass>();

                var array = (SAINPersonality[])Enum.GetValues(typeof(SAINPersonality));
                foreach (var item in array)
                {
                    if (LoadJsonFile(out string json, item.ToString(), "Personalities"))
                    {
                        var persClass = DeserializeObject<PersonalitySettingsClass>(json);
                        Personalities.Add(item, persClass);
                    }
                }
                return Personalities;
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

            public static PresetEditorDefaults LoadPresetSettings(string DefaultPreset)
            {
                if (!LoadObject<PresetEditorDefaults>(out var result, Settings, PresetsFolder))
                {
                    result = new PresetEditorDefaults
                    {
                        SelectedPreset = DefaultPreset,
                        DefaultPreset = DefaultPreset
                    };
                    Save.SaveJson(result, Settings, PresetsFolder);
                }
                result.DefaultPreset = DefaultPreset;
                return result;
            }

            public static BotPreset BotPreset(string name)
            {
                if (LoadJsonFile(out string json, name, SAINConfigFolders(Save.SelectedPresetName)))
                {
                    return DeserializeObject<BotPreset>(json); // Deserialize to BotPreset
                }
                else
                {
                    LogWarning($"BotPreset for {name} does not exist");
                    return null;
                }
            }
        }

        public static string[] SAINConfigFolders(string presetName)
        {
            string[] result = new string[]
            {
                    PresetsFolder,
                    presetName + PresetEnd,
                    SAINConfigPresetFolder
            };
            return result;
        }
        public static string[] EFTBotConfigFolders(string presetName)
        {
            string[] result = new string[]
            {
                    PresetsFolder,
                    presetName + PresetEnd,
                    BotConfigFolder
            };
            return result;
        }

        public static string[] PresetFolders(string presetName)
        {
            string[] result = new string[]
            {
                    PresetsFolder,
                    presetName + PresetEnd
            };
            return result;
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
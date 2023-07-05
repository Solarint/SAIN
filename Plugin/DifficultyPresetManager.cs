using BepInEx.Configuration;
using EFT;
using Newtonsoft.Json;
using SAIN.Editor;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAIN
{
    public class SAINBotPresetManager
    {
        public static string[] WildSpawnTypes { get; private set; }
        public static string[] Difficulties { get; private set; }
        public static string[] PropertyNames { get; private set; }

        public static Action<WildSpawnType, BotDifficulty, SAINBotPreset> PresetUpdated { get; set; }

        public static void Init()
        {
            CreateJsons();
            AssignProperties();
        }

        public static void SavePreset(SAINBotPreset preset)
        {
            UpdatePreset(preset);
        }

        public static void ClonePresetList(List<SAINBotPreset> presets, SAINBotPreset clone)
        {
            for (int i = 0; i < presets.Count; i++)
            {
                presets[i] = CopyAllProperties(presets[i], clone);
                UpdatePreset(presets[i]);
            }
        }

        public static SAINBotPreset LoadPreset(KeyValuePair<WildSpawnType, BotDifficulty> keypair)
        {
            if (Presets.ContainsKey(keypair))
            {
                return Presets[keypair];
            }
            return null;
        }

        public static SAINBotPreset LoadPreset(WildSpawnType type, BotDifficulty difficulty)
        {
            foreach (var pair in TypeDiffs)
            {
                if (pair.Key == type && pair.Value == difficulty)
                {
                    return LoadPreset(pair);
                }
            }
            return null;
        }

        public static SAINBotPreset LoadPreset(string type, string difficulty)
        {
            return LoadPreset(GetType(type), GetDiff(difficulty));
        }

        public static SAINBotPreset LoadPreset(WildSpawnType type, string difficulty)
        {
            return LoadPreset(type, GetDiff(difficulty));
        }

        public static SAINBotPreset LoadPreset(string type, BotDifficulty difficulty)
        {
            return LoadPreset(GetType(type), difficulty);
        }

        public static WildSpawnType GetType(string type)
        {
            return (WildSpawnType)Enum.Parse(typeof(WildSpawnType), type);
        }

        public static BotDifficulty GetDiff(string diff)
        {
            return (BotDifficulty)Enum.Parse(typeof(BotDifficulty), diff);
        }

        public static void CreateJsons()
        {
            List<string> types = new List<string>();
            List<string> difficulties = new List<string>();
            foreach (WildSpawnType type in Enum.GetValues(typeof(WildSpawnType)))
            {
                if (type.ToString().ToLower().Contains("test"))
                {
                    continue;
                }
                types.Add(type.ToString());
                foreach (BotDifficulty diff in Enum.GetValues(typeof(BotDifficulty)))
                {
                    TypeDiffs.Add(new KeyValuePair<WildSpawnType, BotDifficulty>(type, diff));
                    string diffstring = diff.ToString();
                    if (!difficulties.Contains(diffstring))
                    {
                        difficulties.Add(diffstring);
                    }
                }
            }

            WildSpawnTypes = types.ToArray();
            Difficulties = difficulties.ToArray();
            types.Clear();
            difficulties.Clear();

            foreach (var pair in TypeDiffs)
            {
                SAINBotPreset preset = JsonUtility.LoadFromJson.DifficultyPreset(pair) ?? new SAINBotPreset(pair);
                Presets.Add(pair, preset);
            }
            foreach (var preset in Presets)
            {
                JsonUtility.SaveToJson.DifficultyPreset(preset.Value);
            }
        }

        public static void UpdatePreset(SAINBotPreset preset)
        {
            var Pair = preset.KeyPair;
            if (Presets.ContainsKey(Pair))
            {
                Presets[Pair] = preset;
                JsonUtility.SaveToJson.DifficultyPreset(preset);
                if (SAINPlugin.BotController != null && SAINPlugin.BotController.Bots.Count > 0)
                {
                    PresetUpdated(Pair.Key, Pair.Value, preset);
                }
            }
        }

        private static void AssignProperties()
        {
            if (Properties == null)
            {
                var list = new List<PropertyInfo>();
                var names = new List<string>();
                PropertyInfo[] properties = typeof(SAINBotPreset).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo property in properties)
                {
                    Type propertyType = property.PropertyType;
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(SAINProperty<>))
                    {
                        names.Add(property.Name);
                        list.Add(property);
                    }
                }
                PropertyNames = names.ToArray();
                Properties = list.ToArray();
            }
        }

        public static SAINBotPreset CopyAllProperties(SAINBotPreset target, SAINBotPreset source)
        {
            foreach (PropertyInfo property in Properties)
            {
                Copy(target, source, property);
            }
            return target;
        }

        public static SAINBotPreset Copy(SAINBotPreset target, SAINBotPreset source, PropertyInfo property)
        {
            var targetProp = property.GetValue(target);
            var sourceProp = property.GetValue(source);
            Copy(targetProp, sourceProp);
            return target;
        }

        public static void Copy(object targetProp, object sourceProp)
        {
            if (targetProp is SAINProperty<float> targetFloat && sourceProp is SAINProperty<float> sourceFloat)
            {
                targetFloat.Value = sourceFloat.Value;
            }
            if (targetProp is SAINProperty<int> targetInt && sourceProp is SAINProperty<int> sourceInt)
            {
                targetInt.Value = sourceInt.Value;
            }
            if (targetProp is SAINProperty<bool> targetBool && sourceProp is SAINProperty<bool> sourceBool)
            {
                targetBool.Value = sourceBool.Value;
            }
        }

        public static void UpdatePropertyValue<T>(List<SAINBotPreset> presets, PropertyInfo property, T value)
        {
            for (int i = 0; i < presets.Count; i++)
            {
                UpdatePropertyValue(presets[i], property, value);
            }
        }

        public static void UpdatePropertyValue<T>(SAINBotPreset preset, PropertyInfo property, T value)
        {
            var targetProp = property.GetValue(preset);
            if (targetProp is SAINProperty<T> prop)
            {
                UpdatePropertyValue(prop, value);
            }
        }

        public static void UpdatePropertyValue<T>(SAINProperty<T> property, T value)
        {
            property.Value = value;
        }

        public static PropertyInfo[] Properties { get; private set; }

        private static readonly List<KeyValuePair<WildSpawnType, BotDifficulty>> TypeDiffs = new List<KeyValuePair<WildSpawnType, BotDifficulty>>();
        public static readonly Dictionary<KeyValuePair<WildSpawnType, BotDifficulty>, SAINBotPreset> Presets = new Dictionary<KeyValuePair<WildSpawnType, BotDifficulty>, SAINBotPreset>();
    }
}

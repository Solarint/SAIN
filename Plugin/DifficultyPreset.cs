using EFT;
using Newtonsoft.Json;
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

        public static void SavePreset(List<SAINBotPreset> presets)
        {
            SAINBotPreset clone = presets[0];
            for (int i = 0; i < presets.Count; i++)
            {
                if (presets[i] == null)
                {
                    Console.WriteLine($"Preset {i} == null!");
                    continue;
                }
                if (i > 0)
                {
                    presets[i].Copy(clone);
                }
                UpdatePreset(presets[i]);
            }
        }

        public static SAINBotPreset LoadPreset(WildSpawnType spawnType, BotDifficulty botDifficulty)
        {
            var Pair = GetKeyPair(spawnType, botDifficulty);
            if (Presets.ContainsKey(Pair))
            {
                return Presets[Pair];
            }
            return null;
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
                SAINBotPreset preset = JsonUtility.LoadFromJson.DifficultyPreset(pair.Key, pair.Value);
                if (preset == null)
                {
                    preset = new SAINBotPreset(pair.Key, pair.Value);
                }
                Presets.Add(pair, preset);
            }
            foreach (var preset in Presets)
            {
                JsonUtility.SaveToJson.DifficultyPreset(preset.Value);
            }
        }

        public static void UpdatePreset(SAINBotPreset preset)
        {
            if (preset == null)
            {
                return;
            }
            var Pair = GetKeyPair(preset.BotType, preset.Difficulty);
            if (Presets.ContainsKey(Pair))
            {
                if (Presets[Pair] == null)
                {
                    Presets[Pair] = preset;
                }
                else
                {
                    Presets[Pair].Copy(preset);
                }
                JsonUtility.SaveToJson.DifficultyPreset(preset);
                if (SAINPlugin.BotController != null && SAINPlugin.BotController.Bots.Count > 0)
                {
                    PresetUpdated(Pair.Key, Pair.Value, preset);
                }
            }
        }

        public static KeyValuePair<WildSpawnType, BotDifficulty> GetKeyPair(WildSpawnType spawnType, BotDifficulty botDifficulty)
        {
            return GetNullableKeyPair(spawnType, botDifficulty).Value;
        }

        private static KeyValuePair<WildSpawnType, BotDifficulty>? GetNullableKeyPair(WildSpawnType spawnType, BotDifficulty botDifficulty)
        {
            foreach (var pair in TypeDiffs)
            {
                if (pair.Key == spawnType && pair.Value == botDifficulty)
                {
                    return pair;
                }
            }
            return null;
        }

        private static void AssignProperties()
        {
            if (Properties == null)
            {
                var list = new List<PropertyInfo>();
                PropertyInfo[] properties = typeof(SAINBotPreset).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo property in properties)
                {
                    Type propertyType = property.PropertyType;
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(SAINProperty<>))
                    {
                        list.Add(property);
                    }
                }
                Properties = list.ToArray();
            }
        }

        public static PropertyInfo[] Properties { get; private set; }

        private static readonly List<KeyValuePair<WildSpawnType, BotDifficulty>> TypeDiffs = new List<KeyValuePair<WildSpawnType, BotDifficulty>>();
        public static readonly Dictionary<KeyValuePair<WildSpawnType, BotDifficulty>, SAINBotPreset> Presets = new Dictionary<KeyValuePair<WildSpawnType, BotDifficulty>, SAINBotPreset>();
    }
    public class SAINBotPreset
    {
        public SAINBotPreset(WildSpawnType spawnType, BotDifficulty botDifficulty) 
        {
            BotType = spawnType;
            Difficulty = botDifficulty;
            Init();
        }

        private void Init()
        {
            VisibleDistance = new SAINProperty<float>(nameof(VisibleDistance), 150f, 45f, 500f, "The Maximum Vision Distance for this bot", 1f);
            VisibleAngle = new SAINProperty<float>(nameof(VisibleAngle), 160f, 45f, 180f, "The Maximum Vision Cone for a bot", 1f);
            VisionSpeedModifier = new SAINProperty<float>(nameof(VisionSpeedModifier), 1f, 0.25f, 5f, "Modifies the vision speed for this bot", 100f);
            TalkFrequency = new SAINProperty<float>(nameof(TalkFrequency), 1f, 0.5f, 5f, "Multiplies the time between phrases said for this bot", 100f);

            CanTalk = new SAINProperty<bool>(nameof(CanTalk), "Sets whether this bot can talk or not", true);
            FasterCQBReactions = new SAINProperty<bool>(nameof(FasterCQBReactions), "Sets whether this bot reacts faster at close ranges", true);
            CanUseGrenades = new SAINProperty<bool>(nameof(CanUseGrenades), "Can This Bot Use Grenades at all?", true);
        }

        public void Copy(SAINBotPreset preset)
        {
            VisibleAngle.Value = preset.VisibleAngle.Value;
            VisibleDistance.Value = preset.VisibleDistance.Value;
            CanTalk.Value = preset.CanTalk.Value;
            TalkFrequency.Value = preset.TalkFrequency.Value;
            VisionSpeedModifier.Value = preset.VisionSpeedModifier.Value;
            FasterCQBReactions.Value = preset.FasterCQBReactions.Value;
        }

        public PropertyInfo[] GetProperties()
        {
            return SAINBotPresetManager.Properties;
        }

        [JsonProperty]
        public WildSpawnType BotType { get; private set; }
        [JsonProperty]
        public BotDifficulty Difficulty { get; private set; }

        [JsonProperty]
        public SAINProperty<float> VisibleDistance { get; set; }
        [JsonProperty]
        public SAINProperty<float> VisibleAngle { get; set; }
        [JsonProperty]
        public SAINProperty<float> VisionSpeedModifier { get; set; }
        [JsonProperty]
        public SAINProperty<float> TalkFrequency { get; set; }
        [JsonProperty]
        public SAINProperty<bool> CanTalk { get; set; }
        [JsonProperty]
        public SAINProperty<bool> FasterCQBReactions { get; set; }
        [JsonProperty]
        public SAINProperty<bool> CanUseGrenades { get; set; }
    }

    public class SAINProperty<T>
    {
        [JsonConstructor]
        public SAINProperty(string name, T defaultValue, T minValue, T maxValue, string description, float rounding = 1f, bool unused = true)
        {
            Name = name;
            DefaultVal = defaultValue;
            Min = minValue;
            Max = maxValue;
            Description = description;
            Rounding = rounding;
        }

        public SAINProperty(string name, T defaultVal, T minVal, T maxVal, string description = null, float rounding = 1f)
        {
            Name = name;
            Description = description;
            DefaultVal = defaultVal;
            Min = minVal;
            Max = maxVal;
            Rounding = rounding;
            Value = defaultVal;
        }

        public SAINProperty(string name, string description, bool defaultVal)
        {
            Name = name;
            Description = description;
            DefaultVal = (T)(object)defaultVal;
            Min = (T)(object)false;
            Max = (T)(object)true;
            Value = (T)(object)defaultVal;
        }

        [JsonProperty]
        public readonly float Rounding;
        [JsonProperty]
        public readonly string Name;
        [JsonProperty]
        public readonly string Description;
        [JsonProperty]
        public readonly T DefaultVal;
        [JsonProperty]
        public readonly T Min;
        [JsonProperty]
        public readonly T Max;

        [JsonProperty]
        public T Value { get; set; }
    }
}

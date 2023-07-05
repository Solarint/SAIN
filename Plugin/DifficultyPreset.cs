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
                presets[i].Copy(clone);
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
            var Pair = GetKeyPair(preset.BotType, preset.Difficulty);
            if (Presets.ContainsKey(Pair))
            {
                Presets[Pair].Copy(preset);
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
            string name;
            string desc;
            float def;
            float min;
            float max;
            float round;

            name = "Visible Distance";
            desc = "The Maximum Vision Distance for this bot";
            def = 150f;
            min = 50f;
            max = 500f;
            round = 1f;
            VisibleDistance = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Visible Angle";
            desc = "The Maximum Vision Cone for a bot";
            def = 160f;
            min = 45;
            max = 180f;
            round = 1f;
            VisibleAngle = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Base Vision Speed Multiplier";
            desc = "The Base vision speed multiplier. Bots will see this much faster, or slower, at any range.";
            def = 1f;
            min = 0.25f;
            max = 3f;
            round = 100f;
            VisionSpeedModifier = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Talk Frequency";
            desc = "Multiplies how often this bot can say voicelines.";
            def = 1f;
            min = 0.25f;
            max = 3f;
            round = 100f;
            TalkFrequency = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Can Talk";
            desc = "Sets whether this bot can talk or not";
            CanTalk = new SAINProperty<bool>(name, desc, true);

            name = "Taunts";
            desc = "Enables bots yelling nasty words at you.";
            BotTaunts = new SAINProperty<bool>(name, desc, true);

            name = "Faster CQB Reactions";
            desc = "Sets whether this bot reacts faster at close ranges";
            FasterCQBReactions = new SAINProperty<bool>(name, desc, true);

            name = "Can Use Grenades";
            desc = "Can This Bot Use Grenades at all?";
            CanUseGrenades = new SAINProperty<bool>(name, desc, true);

            name = "Close Speed";
            desc = "Vision speed multiplier at close range. Bots will see this much faster, or slower, at close range.";
            def = 1f;
            min = 0.25f;
            max = 3f;
            round = 100f;
            CloseVisionSpeed = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Far Speed";
            desc = "Vision speed multiplier at Far range. Bots will see this much faster, or slower, at Far range.";
            def = 1f;
            min = 0.25f;
            max = 3f;
            round = 100f;
            FarVisionSpeed = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Close/Far Threshold";
            desc = "The Distance that defines what is Close Or Far for the above options.";
            def = 50f;
            min = 5f;
            max = 100f;
            round = 1f;
            CloseFarThresh = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Audible Range Multiplier";
            desc = "Modifies the distance that this bot can hear sounds.";
            def = 1f;
            min = 0.25f;
            max = 3f;
            round = 100f;
            AudibleRangeMultiplier = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Accuracy Multiplier";
            desc = "Modifies a bot's base accuracy. Higher = less accurate. 1.5 = 1.5x higher accuracy spread";
            def = 1f;
            min = 0.25f;
            max = 3f;
            round = 100f;
            AccuracyMulti = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Recoil Scatter Multiplier";
            desc = "Modifies a bot's recoil impulse from a shot. Higher = less accurate. 1.5 = 1.5x more recoil and scatter per shot";
            def = 1f;
            min = 0.25f;
            max = 3f;
            round = 100f;
            RecoilMultiplier = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Burst Length Multiplier";
            desc = "Modifies how long bots shoot a burst during full auto fire. Higher = longer full auto time. 1.5 = 1.5x longer bursts";
            def = 1.25f;
            min = 0.25f;
            max = 3f;
            round = 100f;
            BurstMulti = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Semiauto Firerate Multiplier";
            desc = "Modifies the time a bot waits between semiauto fire. Higher = faster firerate. 1.5 = 1.5x more shots per second";
            def = 1.35f;
            min = 0.25f;
            max = 3f;
            round = 100f;
            FireratMulti = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Squad Talk";
            desc = "Enables bots talking to each other in a squad";
            SquadTalk = new SAINProperty<bool>(name, desc, true);

            name = "Squad Talk Multiplier";
            desc = "Multiplies the time between squad voice communication";
            def = 1f;
            min = 0.1f;
            max = 5f;
            round = 100f;
            SquadMemberTalkFreq = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Squad Leader Talk Multiplier";
            desc = "Multiplies the time between squad Leader commands and callouts";
            def = 1f;
            min = 0.1f;
            max = 5f;
            round = 100f;
            SquadLeadTalkFreq = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Extracts";
            desc = "Can This Bot Use Extracts?";
            EnableExtracts = new SAINProperty<bool>(name, desc, true);

            name = "Extract Max Percentage";
            desc = "The shortest possible time before this bot can decide to move to extract. Based on total raid timer and time remaining. 60 min total raid time with 6 minutes remaining would be 10 percent";
            def = 35f;
            min = 1f;
            max = 99f;
            round = 1f;
            MaxPercentage = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Extract Min Percentage";
            desc = "The longest possible time before this bot can decide to move to extract. Based on total raid timer and time remaining. 60 min total raid time with 6 minutes remaining would be 10 percent";
            def = 5f;
            min = 1f;
            max = 99f;
            round = 1f;
            MinPercentage = new SAINProperty<float>(name, def, min, max, desc, round);
        }

        public void Copy(SAINBotPreset preset)
        {
            foreach (PropertyInfo property in GetProperties())
            {
                var newProp = property.GetValue(preset);
                if (newProp is SAINProperty<dynamic> dynProp)
                {
                    CopyValue(dynProp, property);
                }
            }
        }

        private void CopyValue<T>(SAINProperty<T> newProp, PropertyInfo property)
        {
            var oldProp = property.GetValue(this) as SAINProperty<T>;
            oldProp.Value = newProp.Value;
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
        public SAINProperty<float> AudibleRangeMultiplier { get; set; }
        [JsonProperty]
        public SAINProperty<float> AccuracyMulti { get; set; }

        [JsonProperty]
        public SAINProperty<float> CloseVisionSpeed { get; set; }
        [JsonProperty]
        public SAINProperty<float> FarVisionSpeed { get; set; }
        [JsonProperty]
        public SAINProperty<float> CloseFarThresh { get; set; }
        [JsonProperty]
        public SAINProperty<float> RecoilMultiplier { get; set; }

        [JsonProperty]
        public SAINProperty<float> BurstMulti { get; set; }
        [JsonProperty]
        public SAINProperty<float> FireratMulti { get; set; }

        [JsonProperty]
        public SAINProperty<float> TalkFrequency { get; set; }
        [JsonProperty]
        public SAINProperty<bool> CanTalk { get; set; }
        [JsonProperty]
        public SAINProperty<bool> BotTaunts { get; set; }
        [JsonProperty]
        public SAINProperty<bool> SquadTalk { get; set; }
        [JsonProperty]
        public SAINProperty<float> SquadMemberTalkFreq { get; set; }
        [JsonProperty]
        public SAINProperty<float> SquadLeadTalkFreq { get; set; }

        [JsonProperty]
        public SAINProperty<float> MaxPercentage { get; set; }
        [JsonProperty]
        public SAINProperty<float> MinPercentage { get; set; }
        [JsonProperty]
        public SAINProperty<bool> EnableExtracts { get; set; }

        [JsonProperty]
        public SAINProperty<float> VisibleDistance { get; set; }
        [JsonProperty]
        public SAINProperty<float> VisibleAngle { get; set; }
        [JsonProperty]
        public SAINProperty<float> VisionSpeedModifier { get; set; }
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

using EFT;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SAIN.BotPresets
{
    public class BotPreset
    {
        [JsonConstructor]
        public BotPreset()
        {
        }

        public BotPreset(WildSpawnType wildSpawnType)
        {
            WildSpawnType = wildSpawnType;
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

            name = "Faster CQB Reactions Max Distance";
            desc = "Sets whether this bot reacts faster at close ranges";
            def = 30f;
            min = 1f;
            max = 100f;
            round = 1f;
            FasterCQBReactionsDistance = new SAINProperty<float>(name, def, min, max, desc, round);

            name = "Faster CQB Reactions Minimum Speed";
            desc = "Sets whether this bot reacts faster at close ranges";
            def = 1f;
            min = 0.25f;
            max = 3f;
            round = 100f;
            FasterCQBReactionsMinimum = new SAINProperty<float>(name, def, min, max, desc, round);

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

        [JsonProperty]
        public WildSpawnType WildSpawnType { get; private set; }

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
        public SAINProperty<float> FasterCQBReactionsDistance { get; set; }
        [JsonProperty]
        public SAINProperty<float> FasterCQBReactionsMinimum { get; set; }
        [JsonProperty]
        public SAINProperty<bool> CanUseGrenades { get; set; }
    }

    public class SAINProperty<T>
    {
        [JsonConstructor]
        public SAINProperty() {}

        public SAINProperty(string name, T defaultVal, T minVal, T maxVal, string description = null, float rounding = 1f)
        {
            Name = name;
            Description = description;
            DefaultVal = defaultVal;
            Min = minVal;
            Max = maxVal;
            Rounding = rounding;

            T Value = defaultVal;
            DifficultyValue = new Dictionary<BotDifficulty, T>
            {
                { BotDifficulty.easy, Value },
                { BotDifficulty.normal, Value },
                { BotDifficulty.hard, Value },
                { BotDifficulty.impossible, Value }
            };
        }

        public SAINProperty(string name, string description, bool defaultVal)
        {
            Name = name;
            Description = description;
            DefaultVal = (T)(object)defaultVal;
            Min = (T)(object)false;
            Max = (T)(object)true;

            T Value = (T)(object) defaultVal;
            DifficultyValue = new Dictionary<BotDifficulty, T>
            {
                { BotDifficulty.easy, Value },
                { BotDifficulty.normal, Value },
                { BotDifficulty.hard, Value },
                { BotDifficulty.impossible, Value }
            };
        }

        [JsonProperty]
        public Dictionary<BotDifficulty, T> DifficultyValue { get; set; }

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

        public object GetValue(BotDifficulty difficulty)
        {
            return DifficultyValue[difficulty];
        }

        public void SetValue(BotDifficulty difficulty, T Value)
        {
            DifficultyValue[difficulty] = Value;
        }
    }
}

using EFT;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SAIN.BotPresets
{
    public class BotPreset
    {
        [JsonConstructor]
        public BotPreset() { }

        public BotPreset(WildSpawnType wildSpawnType)
        {
            WildSpawnType = wildSpawnType;
            PresetDefaults.Init(this);
        }

        public PresetValues GetValues(BotDifficulty difficulty)
        {
            PresetValues result;
            if (DifficultyValues.ContainsKey(difficulty))
            {
                result = DifficultyValues[difficulty] ?? new PresetValues(this, difficulty);
                result.SetValues(this, difficulty);

                DifficultyValues[difficulty] = result;
            }
            else
            {
                result = new PresetValues(this, difficulty);
                DifficultyValues.Add(difficulty, result);
            }
            return result;
        }

        [JsonIgnore]
        public Dictionary<BotDifficulty, PresetValues> DifficultyValues { get; set; } = new Dictionary<BotDifficulty, PresetValues>();

        [JsonProperty]
        public WildSpawnType WildSpawnType { get; private set; }

        [JsonProperty]
        public SAINProperty<float> AudibleRangeMultiplier { get; set; }
        [JsonProperty]
        public SAINProperty<float> MaxFootstepAudioDistance { get; set; }

        [JsonProperty]
        public SAINProperty<float> AccuracySpreadMulti { get; set; }
        [JsonProperty]
        public SAINProperty<float> AimUpgradeByTimeMin { get; set; }
        [JsonProperty]
        public SAINProperty<float> MaxAimTime { get; set; }

        [JsonProperty]
        public SAINProperty<float> VisionSpeedModifier { get; set; }
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
        public SAINProperty<bool> FasterCQBReactions { get; set; }
        [JsonProperty]
        public SAINProperty<float> FasterCQBReactionsDistance { get; set; }
        [JsonProperty]
        public SAINProperty<float> FasterCQBReactionsMinimum { get; set; }
        [JsonProperty]
        public SAINProperty<bool> CanUseGrenades { get; set; }
    }

    public class PresetValues
    {
        public readonly BotDifficulty BotDifficulty;

        public PresetValues(BotPreset preset, BotDifficulty difficulty)
        {
            BotDifficulty = difficulty;
            SetValues(preset, difficulty);
        }

        public void SetValues(BotPreset preset, BotDifficulty difficulty)
        {
            VisibleDistance = (float)preset.VisibleDistance.GetValue(difficulty);
            VisibleAngle = (float)preset.VisibleAngle.GetValue(difficulty);
            CanGrenade = (bool)preset.CanUseGrenades.GetValue(difficulty);

            MinPercentage = (float)preset.MinPercentage.GetValue(difficulty);
            MaxPercentage = (float)preset.MaxPercentage.GetValue(difficulty);
            EnableExtracts = (bool)preset.EnableExtracts.GetValue(difficulty);

            RecoilMultiplier = (float)preset.RecoilMultiplier.GetValue(difficulty);
            AccuracySpreadMulti = (float)preset.AccuracySpreadMulti.GetValue(difficulty);
            MAX_AIMING_UPGRADE_BY_TIME = (float)preset.AimUpgradeByTimeMin.GetValue(difficulty);
            MAX_AIM_TIME = (float)preset.MaxAimTime.GetValue(difficulty);

            FasterCQBReactions = (bool)preset.FasterCQBReactions.GetValue(difficulty);
            FasterCQBReactionsDistance = (float)preset.FasterCQBReactionsDistance.GetValue(difficulty);
            FasterCQBReactionsMinimum = (float)preset.FasterCQBReactionsMinimum.GetValue(difficulty);

            AudibleRangeMultiplier = (float)preset.AudibleRangeMultiplier.GetValue(difficulty);
            MaxFootstepAudioDistance = (float)preset.MaxFootstepAudioDistance.GetValue(difficulty);

            BurstMulti = (float)preset.BurstMulti.GetValue(difficulty);
            FireratMulti = (float)preset.FireratMulti.GetValue(difficulty);

            VisionSpeedModifier = (float)preset.VisionSpeedModifier.GetValue(difficulty);
            CloseVisionSpeed = (float)preset.CloseVisionSpeed.GetValue(difficulty);
            FarVisionSpeed = (float)preset.FarVisionSpeed.GetValue(difficulty);
            CloseFarThresh = (float)preset.CloseFarThresh.GetValue(difficulty);

            CanTalk = (bool)preset.CanTalk.GetValue(difficulty);
            BotTaunts = (bool)preset.BotTaunts.GetValue(difficulty);
            SquadTalk = (bool)preset.SquadTalk.GetValue(difficulty);
            SquadMemberTalkFreq = (float)preset.SquadMemberTalkFreq.GetValue(difficulty);
            SquadLeadTalkFreq = (float)preset.SquadLeadTalkFreq.GetValue(difficulty);
            TalkFrequency = (float)preset.TalkFrequency.GetValue(difficulty);
        }

        public float AudibleRangeMultiplier { get; set; }
        public float MaxFootstepAudioDistance { get; set; }
        public float AccuracySpreadMulti { get; set; }
        public float MAX_AIMING_UPGRADE_BY_TIME { get; set; }
        public float MAX_AIM_TIME { get; set; }
        public float VisionSpeedModifier { get; set; }
        public float CloseVisionSpeed { get; set; }
        public float FarVisionSpeed { get; set; }
        public float CloseFarThresh { get; set; }
        public float RecoilMultiplier { get; set; }
        public float BurstMulti { get; set; }
        public float FireratMulti { get; set; }
        public float TalkFrequency { get; set; }
        public bool CanTalk { get; set; }
        public bool BotTaunts { get; set; }
        public bool SquadTalk { get; set; }
        public float SquadMemberTalkFreq { get; set; }
        public float SquadLeadTalkFreq { get; set; }
        public float MaxPercentage { get; set; }
        public float MinPercentage { get; set; }
        public bool EnableExtracts { get; set; }
        public float VisibleDistance { get; set; }
        public float VisibleAngle { get; set; }
        public bool FasterCQBReactions { get; set; }
        public float FasterCQBReactionsDistance { get; set; }
        public float FasterCQBReactionsMinimum { get; set; }
        public bool CanGrenade { get; set; }
    }

    public class SAINProperty<T>
    {
        [JsonConstructor]
        public SAINProperty() {}

        public SAINProperty(string name, T defaultVal, T minVal, T maxVal, string description, string section, float rounding = 1f)
        {
            Name = name;
            Description = description;
            Section = section;
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

        public SAINProperty(string name, string description, string section, bool defaultVal)
        {
            Name = name;
            Description = description;
            Section = section;
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
        public readonly string Section;
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

        public void Reset(BotDifficulty difficulty)
        {
            DifficultyValue[difficulty] = DefaultVal;
        }

        public void ResetAll()
        {
            Reset(BotDifficulty.easy);
            Reset(BotDifficulty.normal);
            Reset(BotDifficulty.hard);
            Reset(BotDifficulty.impossible);
        }
    }
}

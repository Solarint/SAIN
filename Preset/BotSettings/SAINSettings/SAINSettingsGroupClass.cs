using EFT;
using Newtonsoft.Json;
using SAIN.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace SAIN.Preset.BotSettings.SAINSettings
{
    public class SAINSettingsGroupClass
    {
        [JsonConstructor]
        public SAINSettingsGroupClass()
        {
        }

        public SAINSettingsGroupClass(BotDifficulty[] difficulties)
        {
            foreach (var difficulty in difficulties)
            {
                Settings.Add(difficulty, new SAINSettingsClass());
            }
        }

        [JsonProperty]
        public string Name;

        [JsonProperty]
        public WildSpawnType WildSpawnType;

        [JsonProperty]
        [NameAndDescription("Difficulty Modifier", "How much to improve this bot type's recoil handling, fire-rate, and full auto burst length, reaction time, general stats that are used in SAIN.")]
        [DefaultValue(0.5f)]
        [MinMax(0.01f, 1f)]
        public float DifficultyModifier = 0.5f;

        [JsonProperty]
        public Dictionary<BotDifficulty, SAINSettingsClass> Settings = new Dictionary<BotDifficulty, SAINSettingsClass>();
    }
}
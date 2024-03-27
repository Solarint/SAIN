using EFT;
using Newtonsoft.Json;
using SAIN.Preset.GlobalSettings.Categories;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings
{
    public sealed class BotBrainDefinition
    {
        public BotBrainDefinition(Brain brain, string name = null, string description = null)
        {
            Brain = brain;
            Name = name ?? brain.ToString();
            Description = description ?? string.Empty;
        }

        public BotBrainDefinition(BotOwner bot, string name = null, string description = null)
        {
            Name = name ?? BaseBrain;
            Description = description ?? string.Empty;
            BaseBrain = bot.Brain.BaseBrain.ShortName();
            Brain = SAIN.Helpers.EnumValues.Parse<Brain>(BaseBrain);
            WildSpawnType = bot.Profile.Info.Settings.Role;

            if (BotTypeDefinitions.BotTypes.ContainsKey(WildSpawnType))
            {
                BotType = BotTypeDefinitions.BotTypes[WildSpawnType];
            }
            else
            {
                BotType = BotTypeDefinitions.BotTypes[WildSpawnType.assault];
                Logger.LogError($"WildSpawnType {WildSpawnType} does not exist in BotType Dictionary");
            }

            LayersToRemove = new List<string>
            {

            };
        }

        [JsonConstructor]
        public BotBrainDefinition() { }

        public string Name;
        public string Description;
        public Brain Brain;
        public string BaseBrain;
        public WildSpawnType WildSpawnType;
        public BotType BotType;
        public bool SAINEnabled = false;
        public bool IsPlayerScav = false;
        public List<string> LayersToRemove = new List<string>();
    }
}
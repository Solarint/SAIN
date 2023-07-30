using EFT;
using SAIN.Classes;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using SAIN.BotSettings.Categories;
using Newtonsoft.Json;
using System.Data;

namespace SAIN.BotSettings
{
    public class BotSettingsManager
    {
        static BotSettingsManager()
        {
            var botTypes = BotPresets.BotTypeDefinitions.BotTypes;
            var diffs = (BotDifficulty[])Enum.GetValues(typeof(BotDifficulty));
            foreach (var botType in botTypes)
            {
                var type = botType.WildSpawnType;
                foreach (var diff in diffs)
                {
                }
            }
        }
    }
}

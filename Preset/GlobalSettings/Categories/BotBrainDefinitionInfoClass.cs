using SAIN.Preset.GlobalSettings.Categories;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings
{
    public class BotBrainDefinitionInfoClass
    {
        static BotBrainDefinitionInfoClass()
        {
            BotBrainInfos = new Dictionary<Brain, BotBrainDefinition>();
            BotBrainInfosList = new List<BotBrainDefinition>();

            List<Brain> allBrains = BotBrains.AllBrainsList;
            for (int i = 0; i < allBrains.Count; i++)
            {
                Brain brain = allBrains[i];
                BotBrainDefinition brainDef = new BotBrainDefinition(brain);

                BotBrainInfos.Add(brain, brainDef);
                BotBrainInfosList.Add(brainDef);
            }
        }

        public static Dictionary<Brain, BotBrainDefinition> BotBrainInfos;
        public static List<BotBrainDefinition> BotBrainInfosList;

    }
}
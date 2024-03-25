using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Layers;
using SAIN.Layers.Combat.Solo;
using SAIN.Layers.Combat.Squad;
using SAIN.Preset.GlobalSettings.Categories;
using System.Collections.Generic;

namespace SAIN
{
    public class BigBrainHandler
    {
        public static bool IsBotUsingSAINLayer(BotOwner bot)
        {
            if (bot?.Brain?.Agent != null)
            {
                if (BrainManager.IsCustomLayerActive(bot))
                {
                    string layer = bot.Brain.ActiveLayerName();
                    if (SAINLayers.Contains(layer))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void Init()
        {
            List<Brain> enabledBrains = SAINPlugin.LoadedPreset.GlobalSettings.General.SAINEnabledTypes;

            List<string> stringList = new List<string>();
            List<string> LayersToRemove = new List<string>
            {
                "Help",
                "AdvAssaultTarget",
                "Hit",
                "Pmc",
                "AssaultHaveEnemy",
                "Request"
            };

            for (int i = 0; i < enabledBrains.Count; i++)
            {
                stringList.Add(enabledBrains[i].ToString());
            }

            foreach (var brain in BotBrains.AllBrains)
            {
                //stringList.Add(brain.ToString());
            }

            //Logger.LogInfo(stringList.Count);
            //Logger.LogInfo(LayersToRemove.Count);
            //Logger.LogInfo(SAINLayers.Count);

            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), stringList, 24);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), stringList, 22);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), stringList, 20);

            BrainManager.RemoveLayers(LayersToRemove, stringList);

            BigBrainInitialized = true;
        }

        public static readonly List<string> SAINLayers = new List<string>
        {
            CombatSquadLayer.Name,
            ExtractLayer.Name,
            CombatSoloLayer.Name,
        };

        public static bool BigBrainInitialized;
    }
}
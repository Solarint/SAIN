using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Layers;
using SAIN.Layers.Combat.Solo;
using SAIN.Layers.Combat.Squad;
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
            var bigBrainSetting = SAINPlugin.LoadedPreset.GlobalSettings.BigBrain.BrainSettings;

            List<string> stringList = new List<string>();
            foreach (var brain in bigBrainSetting)
            {
                stringList.Add(brain.BrainName);

                BrainManager.AddCustomLayer(typeof(CombatSquadLayer), stringList, brain.SquadLayerPriority);
                BrainManager.AddCustomLayer(typeof(ExtractLayer), stringList, brain.ExtractLayerPriority);
                BrainManager.AddCustomLayer(typeof(CombatSoloLayer), stringList, brain.CombatSoloLayerPriority);

                BrainManager.RemoveLayers(brain.LayersToRemove, stringList);

                stringList.Clear();
            }
        }

        public static readonly List<string> SAINLayers = new List<string>
        {
            CombatSquadLayer.Name,
            ExtractLayer.Name,
            CombatSoloLayer.Name,
        };
    }
}
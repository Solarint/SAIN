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
            var bigBrainSetting = SAINPlugin.LoadedPreset.GlobalSettings.BigBrain;

            var bots = bigBrainSetting.BotsWhoGetSAINLayers;
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), bots, bigBrainSetting.SquadLayerPriority);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), bots, bigBrainSetting.ExtractLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), bots, bigBrainSetting.CombatSoloLayerPriority);

            BrainManager.RemoveLayers(bigBrainSetting.LayersToRemove, bots);
        }

        public static readonly List<string> SAINLayers = new List<string>
        {
            CombatSquadLayer.Name,
            ExtractLayer.Name,
            CombatSoloLayer.Name,
        };
    }
}
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Helpers;
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

        public static List<string> BotBrainList = new List<string>();
        public static List<string> BotLayerList = new List<string>();
        public static readonly List<string> LayersToRemove = new List<string>
            {
                "Help",
                "AdvAssaultTarget",
                "Hit",
                "Pmc",
                "AssaultHaveEnemy",
                "Request"
            };

        static BigBrainHandler()
        {
            if (JsonUtility.Load.LoadObject(out List<string> layersList, "DefaultBotLayers"))
            {
                AllLayersList = layersList;
            }
            else
            {
                AllLayersList = new List<string>(LayersToRemove);
                JsonUtility.SaveObjectToJson(AllLayersList, "DefaultBotLayers");
            }
        }

        public static List<string> AllLayersList;

        public static void CheckLayers()
        {
            if (!SAINPlugin.EditorDefaults.CollectBotLayerBrainInfo)
            {
                return;
            }

            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld != null)
            {
                var players = gameWorld.AllAlivePlayersList;
                foreach (var player in players)
                {
                    if (player != null && player.AIData?.BotOwner != null)
                    {
                        UpdateLayersList(player.AIData.BotOwner);
                    }
                }
            }
        }

        public static void UpdateLayersList(BotOwner bot)
        {
            if (BrainManager.IsCustomLayerActive(bot))
            {
                return;
            }

            string layerName = BrainManager.GetActiveLayerName(bot);

            if (!AllLayersList.Contains(layerName))
            {
                AllLayersList.Add(layerName);
                JsonUtility.SaveObjectToJson(AllLayersList, "DefaultBotLayers");
            }
        }

        public static void Init()
        {
            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General;
            List<Brain> enabledBrains = settings.EnabledBrains;

            List<string> stringList = new List<string>();

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

            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), stringList, settings.SAINCombatSquadLayerPriority);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), stringList, settings.SAINExtractLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), stringList, settings.SAINCombatSoloLayerPriority);

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
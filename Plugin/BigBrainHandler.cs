using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Layers;
using SAIN.Layers.Combat.Solo;
using SAIN.Layers.Combat.Squad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string PMCRaider = "PMC";
            string Cultist = "SectantWarrior";
            string GoonsKnight = "Knight";
            string Rogue = "ExUsec";
            var Scavs = new List<string> 
            { 
                "Assault", 
                "CursAssault" 
            };
            var GoonsFollowers = new List<string> 
            { 
                "BigPipe", 
                "BirdEye" 
            };
            var TagillaKilla = new List<string> 
            { 
                "Tagilla", 
                "Killa" 
            };
            var NormalBosses = new List<string> 
            { 
                "BossBully",
                "BossSanitar", 
                "Tagilla", 
                "BossGluhar", 
                "BossKojaniy", 
                "SectantPriest" 
            };
            var NormalFollowers = new List<string> 
            { 
                "FollowerBully", 
                "FollowerSanitar", 
                "TagillaFollower", 
                "FollowerGluharAssault", 
                "FollowerGluharProtect", 
                "FollowerGluharScout" 
            };

            var AllBots = new List<string>();
            var AllBosses = new List<string>();
            var AllFollowers = new List<string>();
            var AllNormalBots = new List<string>();

            AllNormalBots.AddRange(Scavs);
            AllNormalBots.Add(PMCRaider);
            AllNormalBots.Add(Rogue);
            AllNormalBots.Add(Cultist);

            AllBosses.Add(GoonsKnight);
            AllBosses.AddRange(NormalBosses);
            AllBosses.AddRange(TagillaKilla);

            AllFollowers.AddRange(NormalFollowers);
            AllFollowers.AddRange(GoonsFollowers);

            AllBots.AddRange(AllNormalBots);
            AllBots.AddRange(AllFollowers);

            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), AllBots, 24);
            SAINLayers.Add(CombatSquadLayer.Name);

            BrainManager.AddCustomLayer(typeof(ExtractLayer), AllBots, 22);
            SAINLayers.Add(ExtractLayer.Name);

            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), AllBots, 20);
            SAINLayers.Add(CombatSoloLayer.Name);

            var LayersToRemove = new List<string> 
            { 
                "Help", 
                "AdvAssaultTarget", 
                "Hit", 
                "Pmc", 
                "AssaultHaveEnemy", 
                "Request" 
            };
            BrainManager.RemoveLayers(LayersToRemove, AllBots);
        }

        public static List<string> SAINLayers { get; private set; } = new List<string>();
    }
}

using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN
{
    public class BigBrainSAIN
    {
        public static bool IsBotUsingSAINLayer(BotOwner bot)
        {
            if (bot.Brain.Agent != null)
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
            CombineBigBrainLists();

            BrainManager.AddCustomLayer(typeof(SAINSquad), AllBots, 66);
            SAINLayers.Add(SAINSquad.Name);

            BrainManager.AddCustomLayer(typeof(SAINSolo), AllBots, 65);
            SAINLayers.Add(SAINSolo.Name);

            foreach (string layer in LayersToRemove)
            {
                BrainManager.RemoveLayer(layer, new List<string>(AllBots));
            }
        }

        private static void CombineBigBrainLists()
        {
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
            AllBots.AddRange(AllBosses);
        }

        public static string PMCRaider = "PMC";
        public static string Cultist = "SectantWarrior";
        public static string GoonsKnight = "Knight";
        public static string Rogue = "ExUsec";

        public static List<string> Scavs = new List<string> { "Assault", "CursAssault" };
        public static List<string> GoonsFollowers = new List<string> { "BigPipe", "BirdEye" };
        public static List<string> TagillaKilla = new List<string> { "Tagilla", "Killa" };
        public static List<string> NormalBosses = new List<string> { "BossBully", "BossSanitar", "Tagilla", "BossGluhar", "Killa", "BossKojaniy", "SectantPriest" };
        public static List<string> NormalFollowers = new List<string> { "FollowerBully", "FollowerSanitar", "TagillaFollower", "FollowerGluharAssault", "FollowerGluharProtect", "FollowerGluharScout" };

        public static List<string> AllBots = new List<string>();
        public static List<string> AllBosses = new List<string>();
        public static List<string> AllFollowers = new List<string>();
        public static List<string> AllNormalBots = new List<string>();

        public static List<string> SAINLayers { get; private set; } = new List<string>();
        public static List<string> LayersToRemove = new List<string> { "Help", "AdvAssaultTarget", "Hit", "Pmc", "AssaultHaveEnemy" };
    }
}

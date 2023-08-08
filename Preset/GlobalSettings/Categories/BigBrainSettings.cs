using EFT;
using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static SAIN.Helpers.EnumValues;

namespace SAIN.Preset.GlobalSettings.Categories
{
    public class BigBrainSettings
    {
        [AdvancedOptions(true, true)]
        public int SquadLayerPriority = 24;

        [AdvancedOptions(true, true)]
        public int ExtractLayerPriority = 22;

        [AdvancedOptions(true, true)]
        public int CombatSoloLayerPriority = 20;

        [AdvancedOptions(true, true)]
        public List<string> BotsWhoGetSAINLayers = new List<string>(AllBotBrains);

        [AdvancedOptions(true, true)]
        public List<string> LayersToRemove = new List<string>
            {
                "Help",
                "AdvAssaultTarget",
                "Hit",
                "Pmc",
                "AssaultHaveEnemy",
                "Request"
            };

        public static List<string> AllBotBrains = new List<string>
            {
                "PMC",
                "Assault",
                "CursAssault",
                "ExUsec",
                "Knight",
                "BigPipe",
                "BirdEye",
                "Tagilla",
                "Killa",
                "BossBully",
                "BossSanitar",
                "Tagilla",
                "BossGluhar",
                "BossKojaniy",
                "SectantPriest",
                "SectantWarrior",
                "FollowerBully",
                "FollowerSanitar",
                "TagillaFollower",
                "FollowerGluharAssault",
                "FollowerGluharProtect",
                "FollowerGluharScout"
            };

        public Dictionary<Brain, BigBrainConfigClass> BrainSettings = new Dictionary<Brain, BigBrainConfigClass>();
        /*
        GClass219 BotBaseBrainClass     return "ArenaFighter";
        GClass220 BotBaseBrainClass     return "BossBully";
        GClass238 GClass237		        return "BossGluhar";
        BossKnightBrainClass GClass227	return "Knight";
        GClass247 GClass246		        return "BossKojaniy";
        GClass221 BotBaseBrainClass		return "BossSanitar";
        GClass222 BotBaseBrainClass		return "Tagilla";
        GClass223 BotBaseBrainClass		return "BossTest";
        GClass224 BotBaseBrainClass		return "BossZryachiy";
        GClass225 BotBaseBrainClass		return "CursAssault";
        GClass226 BotBaseBrainClass		return "Obdolbs";
        ExUsecBrainClass GClass227		return "ExUsec";
        GClass230 BotBaseBrainClass		return "BigPipe";
        GClass231 BotBaseBrainClass		return "BirdEye";
        GClass232 BotBaseBrainClass		return "FollowerBully";
        GClass239 GClass237		        return "FollowerGluharAssault";
        GClass240 GClass237		        return "FollowerGluharProtect";
        GClass241 GClass237		        return "FollowerGluharScout";
        BossKojaniyBrainClass GClass246	return "FollowerKojaniy";
        GClass233 BotBaseBrainClass		return "FollowerSanitar";
        GClass234 BotBaseBrainClass		return "TagillaFollower";
        GClass235 BotBaseBrainClass		return "Fl_Zraychiy";
        GClass236 BotBaseBrainClass		return "Gifter";
        GClass242 BotBaseBrainClass		return "Killa";
        GClass243 BotBaseBrainClass		return "Marksman";
        GClass244 BotBaseBrainClass		return "PMC";
        GClass251 GClass246		        return "SectantPriest";
        GClass250 GClass249		        return "SectantWarrior";
        GClass245 BotBaseBrainClass		return "Assault";
        */
    }

    public class BigBrainConfigClass
    {
        [AdvancedOptions(true, true)]
        public string BrainName;

        [AdvancedOptions(true)]
        [DefaultValue(24)]
        [MinMaxRound(0, 100)]
        public int SquadLayerPriority = 24;

        [AdvancedOptions(true)]
        [DefaultValue(22)]
        [MinMaxRound(0, 100)]
        public int ExtractLayerPriority = 22;

        [AdvancedOptions(true)]
        [DefaultValue(20)]
        [MinMaxRound(0, 100)]
        public int CombatSoloLayerPriority = 20;

        [AdvancedOptions(true, true)]
        public List<string> LayersToRemove = new List<string>
            {
                "Help",
                "AdvAssaultTarget",
                "Hit",
                "Pmc",
                "AssaultHaveEnemy",
                "Request"
            };
    }

    public class BotBrains
    {
        public static string Parse(Brain brain)
        {
            return brain.ToString();
        }

        public static Brain Parse(string brain)
        {
            if (Enum.TryParse(brain, out Brain result))
            {
                return result;
            }
            return Brain.Assault;
        }

        public static readonly Brain[] AllBrains = Enum.GetValues(typeof(Brain)).Cast<Brain>().ToArray();

        public static readonly Brain[] Bosses =
        {
            Brain.BossBully,
            Brain.BossGluhar,
            Brain.Knight,
            Brain.BossKojaniy,
            Brain.BossSanitar,
            Brain.Tagilla,
            Brain.BossZryachiy,
            Brain.Killa,
            Brain.SectantPriest,
        };

        public static readonly Brain[] Followers =
        {
            Brain.FollowerBully,
            Brain.FollowerGluharAssault,
            Brain.FollowerGluharProtect,
            Brain.FollowerGluharScout,
            Brain.FollowerKojaniy,
            Brain.FollowerSanitar,
            Brain.TagillaFollower,
            Brain.Fl_Zraychiy,
            Brain.SectantWarrior,
            Brain.BigPipe,
            Brain.BirdEye,
        };

        public static readonly Brain[] Goons =
        {
            Brain.Knight,
            Brain.BigPipe,
            Brain.BirdEye,
        };

        public static readonly Brain[] Special =
        {
            Brain.BossTest,
            Brain.Obdolbs,
            Brain.Gifter,
            Brain.CursAssault,
        };
    }

    public enum Brain
    {
        ArenaFighter,
        BossBully,
        BossGluhar,
        Knight,
        BossKojaniy,
        BossSanitar,
        Tagilla,
        BossTest,
        BossZryachiy,
        Obdolbs,
        ExUsec,
        BigPipe,
        BirdEye,
        FollowerBully,
        FollowerGluharAssault,
        FollowerGluharProtect,
        FollowerGluharScout,
        FollowerKojaniy,
        FollowerSanitar,
        TagillaFollower,
        Fl_Zraychiy,
        Gifter,
        Killa,
        Marksman,
        PMC,
        SectantPriest,
        SectantWarrior,
        CursAssault,
        Assault,
    }

    public class DefaultBrainsClass
    {
        public DefaultBrainsClass()
        {
            foreach (Layer layer in GetEnum<Layer>())
            {
                var layerInfo = new LayerInfoClass();

                Dictionary<Brain, int> usedByBrains = new Dictionary<Brain, int>();
                List<WildSpawnType> usedByWST = new List<WildSpawnType>();
                foreach (var brain in BrainInfos)
                {
                    var usedLayers = brain.Value.Layers;
                    if (usedLayers.ContainsKey(layer))
                    {
                        int priority = usedLayers[layer];
                        usedByBrains.Add(brain.Key, priority);

                        usedByWST.AddRange(from wildSpawn in brain.Value.UsedByWildSpawns
                                           where !usedByWST.Contains(wildSpawn)
                                           select wildSpawn);
                    }
                }

                layerInfo.UsedByBrains = usedByBrains;
                layerInfo.UsedByWildSpawns = usedByWST.ToArray();

                if (LayersNames.ContainsKey(layer))
                {
                    layerInfo.Name = LayersNames[layer];
                }
                else
                {
                    layerInfo.Name = layer.ToString();
                    layerInfo.ConvertedToString = true;
                }
                LayerInfos.Add(layer, layerInfo);
            }

            JsonUtility.Save.SaveJson(BrainInfos, "BrainInfos", "BigBrain");
            JsonUtility.Save.SaveJson(LayerInfos, "LayerInfos", "BigBrain");
            JsonUtility.Save.SaveJson(LayersNames, "LayersNames", "BigBrain");
        }


        public Dictionary<Layer, string> LayersNames = new Dictionary<Layer, string>
        {
            { Layer.Kojaniy_Target, "Kojaniy Target" },
            { Layer.Follower_bully, "Follower bully" },
            { Layer.AdvAssaultTarget, "AdvAssaultTarget" },
            { Layer.Simple_Target , "Simple Target"},
            { Layer.ObdolbosFight , "ObdolbosFight"},
            { Layer.Leave_Map , "Leave Map"},
            { Layer.Pursuit , "Pursuit"},
            { Layer.Assault_Building , "Assault Building"},
            { Layer.Enemy_Building , "Enemy Building"},
            { Layer.AssaultHaveEnemy , "AssaultHaveEnemy"},
            { Layer.Bully_Layer , "Bully Layer"},
            { Layer.Kill_logic , "Kill logic"},
            { Layer.Debug , "Debug"},
            { Layer.FollowPlayer , "Follow Player"},
            { Layer.Khorovod , "Khorovod"},
            { Layer.Obd_Patrol , "Obd Ptrl"},
            { Layer.BirdHold , "BirdHold"},
            { Layer.PtrlBirdEye , "PtrlBirdEye"},
            { Layer.KnightFight , "KnightFight"},
            { Layer.StationaryWS , "StationaryWS"},
            { Layer.ExURequest , "ExURequest"},
            { Layer.FRequest , "FRequest"},
            { Layer.PatrolFollower , "PatrolFollower"},
            { Layer.Help , "Help"},
            { Layer.Gifter , "Gifter"},
            { Layer.FlGlPrTarget , "FlGlPrTarget"},
            { Layer.GlGoal , "GlGoal"},
            { Layer.GrenadeDanger , "GrenadeDanger"},
            { Layer.HoldOrCover , "HoldOrCover"},
            { Layer.KojaniyB_Enemy , "KojaniyB_Enemy"},
            { Layer.FolKojEnemy , "FolKojEnemy"},
            { Layer.Malfunction , "Malfunction"},
            { Layer.MarksmanEnemy , "MarksmanEnemy"},
            { Layer.MarksmanTarget , "MarksmanTarget"},
            { Layer.Panic , "Panic"},
            { Layer.Utility_peace , "Utility peace"},
            { Layer.BossSanitarFight , "BossSanitarFight"},
            { Layer.FlSanFight , "FlSanFight"},
            { Layer.SanitarGoal , "SanitarGoal"},
            { Layer.GrenSuicide , "GrenSuicide"},
            { Layer.RH_IN , "R&H_IN"},
            { Layer.RH_OUT , "R&H_OUT"},
            { Layer.MeleeS_IN , "MeleeS_IN"},
            { Layer.RunandStrike , "Run&Strike"},
            { Layer.SupShootSect_IN , "SupShootSect_IN"},
            { Layer.SupShootSect_OUT , "SupShootSect_OUT"},
            { Layer.CheckZryachiy , "CheckZryachiy"},
            { Layer.FlZryachFight , "FlZryachFight"},
            { Layer.LayingPatrol , "LayingPatrol"},
            { Layer.CloseZrFight , "CloseZrFight"},
            { Layer.ZryachiyFight , "ZryachiyFight"},
            { Layer.TestLayer , "TestLayer"},
            { Layer.TagillaAmbush , "TagillaAmbush"},
            { Layer.TagillaFollower , "TagillaFollower"},
            { Layer.TagillaMain , "TagillaMain"}
        };

        public Dictionary<Brain, BrainInfoClass> BrainInfos = new Dictionary<Brain, BrainInfoClass>
        {
            {
                Brain.Marksman, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.MarksmanEnemy, 30},
                        { Layer.MarksmanTarget, 20},
                        { Layer.StandBy, 3},
                        { Layer.PatrolAssault , 1},
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.marksman,
                    },
                }
            },
            {
                Brain.BossTest, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.TestLayer, 100},
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.bossTest,
                    },
                }
            },
            {
                Brain.BossBully, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        { Layer.Bully_Layer, 60},
                        { Layer.AssaultHaveEnemy , 50},
                        { Layer.AdvAssaultTarget , 9},
                        { Layer.PatrolAssault , 0},
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.followerBully,
                    },
                }
            },
            {
                Brain.FollowerBully, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        { Layer.FRequest, 70},
                        { Layer.Follower_bully, 60 },
                        { Layer.AssaultHaveEnemy , 50},
                        { Layer.Request , 30},
                        { Layer.AdvAssaultTarget , 9},
                        { Layer.PatrolFollower , 2},
                        { Layer.PatrolAssault , 0},
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.followerBully,
                    },
                }
            },
            {
                Brain.Killa, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78 },
                        { Layer.Kill_logic, 60 },
                        { Layer.Simple_Target, 9 },
                        { Layer.PatrolAssault , 0 },
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.bossKilla,
                    },
                }
            },
            {
                Brain.BossKojaniy, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction , 78 },
                        { Layer.KojaniyB_Enemy, 60 },
                        { Layer.Kojaniy_Target , 40 },
                        { Layer.StayAtPos, 11 },
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.bossKojaniy,
                    },
                }
            },
            {
                Brain.FollowerKojaniy, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction , 78 },
                        { Layer.FolKojEnemy, 60},
                        { Layer.AssaultHaveEnemy, 50},
                        { Layer.Kojaniy_Target, 40},
                        { Layer.StayAtPos , 11},
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.followerKojaniy,
                    },
                }
            },
            {
                Brain.PMC, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        {Layer.FRequest, 70 },
                        {Layer.Pmc, 60 },
                        {Layer.AssaultHaveEnemy, 50 },
                        {Layer.Request, 30 },
                        {Layer.Pursuit, 25 },
                        {Layer.AdvAssaultTarget, 9 },
                        {Layer.Utility_peace, 3 },
                        {Layer.PatrolFollower, 2 },
                        {Layer.PatrolAssault, 0 }
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.pmcBot,
                        WildSpawnType.arenaFighterEvent,
                        WildSpawnType.assaultGroup,
                    },
                }
            },
            {
                Brain.CursAssault, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        { Layer.Help, 65},
                        { Layer.GroupForce, 50 },
                        { Layer.Pursuit , 45},
                        { Layer.Request , 30},
                        { Layer.Leave_Map , 4},
                        { Layer.StandBy , 3},
                        { Layer.Utility_peace , 2},
                        { Layer.PatrolAssault , 1},
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.cursedAssault,
                    },
                }
            },
            {
                Brain.BossGluhar, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 90 },
                        { Layer.Malfunction, 88},
                        { Layer.BossGlFight, 65},
                        { Layer.SecurityGluhar , 60},
                        { Layer.AssaultHaveEnemy, 50 },
                        { Layer.AdvAssaultTarget , 9},
                        { Layer.PatrolAssault , 0},
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.bossGluhar,
                    },
                }
            },
            {
                Brain.FollowerGluharAssault, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        { Layer.FRequest, 70},
                        { Layer.GluhAssKilla, 70},
                        { Layer.GluharKilla , 65},
                        { Layer.AssaultHaveEnemy , 55},
                        { Layer.HoldOrCover, 47},
                        { Layer.Request, 30},
                        { Layer.GlGoal, 14},
                        { Layer.Simple_Target , 9},
                        { Layer.PatrolAssault, 0},
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.followerGluharAssault,
                    },
                }
            },
            {
                Brain.FollowerGluharProtect, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        { Layer.FRequest, 70},
                        { Layer.SecurityGluhar, 60},
                        { Layer.AssaultHaveEnemy , 50},
                        { Layer.FlGlPrTarget , 46},
                        { Layer.Request, 30},
                        { Layer.Simple_Target , 9},
                        { Layer.PatrolFollower, 2},
                        { Layer.PatrolAssault, 0},
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.followerGluharSecurity,
                        WildSpawnType.followerGluharSnipe,
                    },
                }
            },
            {
                Brain.FollowerGluharScout, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        { Layer.FRequest, 70},
                        { Layer.FlGlScout , 65},
                        { Layer.AssaultHaveEnemy , 50},
                        { Layer.Request, 30},
                        { Layer.Simple_Target , 9},
                        { Layer.PatrolAssault, 0},
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.followerGluharScout,
                    },
                }
            },
            {
                Brain.FollowerSanitar, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        { Layer.FRequest, 70},
                        { Layer.FlSanFight, 62},
                        { Layer.AssaultHaveEnemy, 50},
                        { Layer.Request, 30},
                        { Layer.SanitarGoal , 22},
                        { Layer.Simple_Target, 9},
                        { Layer.PatrolFollower, 2},
                        { Layer.PatrolAssault, 0},
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.followerSanitar,
                    },
                }
            },
            {
                Brain.BossSanitar, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        { Layer.BossSanitarFight, 62},
                        { Layer.AssaultHaveEnemy, 50},
                        { Layer.SanitarGoal , 22},
                        { Layer.Simple_Target, 9},
                        { Layer.Utility_peace, 2},
                        { Layer.PatrolAssault, 0},
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.bossSanitar,
                    },
                }
            },
            {
                Brain.SectantWarrior, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 200 },
                        { Layer.Malfunction, 198 },
                        { Layer.Kill_logic, 100 },
                        { Layer.RunandStrike, 90 },
                        { Layer.SupShootSect_IN, 80 },
                        { Layer.SupShootSect_OUT, 80 },
                        { Layer.MeleeS_IN, 70 },
                        { Layer.MeleeS_OUT, 70 },
                        { Layer.HoldOrCover, 47 },
                        { Layer.Utility_peace, 13 },
                        { Layer.Leave_Map, 12 },
                        { Layer.StayAtPosOpt, 11 },
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.sectantWarrior,
                    },
                }
            },
            {
                Brain.SectantPriest, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenSuicide, 150 },
                        { Layer.GrenadeDanger, 130 },
                        { Layer.Malfunction, 128 },
                        { Layer.RH_OUT, 120 },
                        { Layer.HoldOrCover, 47 },
                        { Layer.Leave_Map, 12 },
                        { Layer.StayAtPos, 11 },
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.sectantPriest,
                    },
                }
            },
            {
                Brain.Tagilla, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78 },
                        { Layer.FRequest, 70 },
                        { Layer.TagillaAmbush, 60 },
                        { Layer.TagillaMain, 50 },
                        { Layer.Request, 40 },
                        { Layer.Simple_Target, 9 },
                        { Layer.PatrolAssault, 0 },
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.bossTagilla,
                    },
                }
            },
            {
                Brain.TagillaFollower, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78 },
                        { Layer.FRequest, 70 },
                        { Layer.TagillaFollower, 65 },
                        { Layer.TagillaAmbush, 60 },
                        { Layer.TagillaMain, 50 },
                        { Layer.Request, 40 },
                        { Layer.Simple_Target, 9 },
                        { Layer.PatrolAssault, 0 },
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.followerTagilla,
                    },
                }
            },
            {
                Brain.ExUsec, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 100 },
                        { Layer.GroupForce, 95},
                        { Layer.ExURequest, 80},
                        { Layer.StationaryWS, 75},
                        { Layer.Malfunction , 73},
                        { Layer.FRequest , 70},
                        { Layer.Pmc, 60},
                        { Layer.AssaultHaveEnemy, 50},
                        { Layer.Request, 30},
                        { Layer.AdvAssaultTarget , 9},
                        { Layer.Utility_peace, 3},
                        { Layer.PatrolFollower, 2},
                        { Layer.PatrolAssault, 0},
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.exUsec,
                    },
                }
            },
            {
                Brain.Gifter, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        { Layer.Gifter, 70},
                        { Layer.FRequest, 65},
                        { Layer.AssaultHaveEnemy , 55},
                        { Layer.Request , 30},
                        { Layer.Simple_Target, 20},
                        { Layer.Leave_Map, 4},
                        { Layer.StandBy, 3},
                        { Layer.Utility_peace , 2},
                        { Layer.PatrolAssault, 1},
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.gifter,
                    },
                }
            },
            {
                Brain.Knight, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        { Layer.Assault_Building, 72 },
                        { Layer.Enemy_Building, 70 },
                        { Layer.KnightFight, 62 },
                        { Layer.AssaultHaveEnemy, 50 },
                        { Layer.Request, 30},
                        { Layer.PtrlBirdEye, 0 },
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.bossKnight,
                    },
                }
            },
            {
                Brain.BigPipe, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        { Layer.Assault_Building, 72 },
                        { Layer.Enemy_Building, 70 },
                        { Layer.Kill_logic, 60 },
                        { Layer.AssaultHaveEnemy, 50 },
                        { Layer.Request, 30},
                        { Layer.PtrlBirdEye, 0 },
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.followerBigPipe,
                    },
                }
            },
            {
                Brain.BirdEye, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        { Layer.Assault_Building, 72 },
                        { Layer.Enemy_Building, 70 },
                        { Layer.BirdHold, 55 },
                        { Layer.BirdEyeFight, 50 },
                        { Layer.AssaultHaveEnemy, 50 },
                        { Layer.Request, 30},
                        { Layer.PtrlBirdEye, 0 },
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.followerBirdEye,
                    },
                }
            },
            {
                Brain.BossZryachiy, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.CloseZrFight, 50},
                        { Layer.ZryachiyFight, 40 },
                        { Layer.CheckZryachiy, 30 },
                        { Layer.LayingPatrol, 0 },
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.bossZryachiy,
                    },
                }
            },
            {
                Brain.Fl_Zraychiy, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.CloseZrFight, 50},
                        { Layer.FlZryachFight, 40 },
                        { Layer.CheckZryachiy, 30 },
                        { Layer.LayingPatrol, 0 },
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.followerZryachiy,
                    },
                }
            },
            {
                Brain.ArenaFighter, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        { Layer.FRequest, 70 },
                        { Layer.Pmc, 60 },
                        { Layer.AssaultHaveEnemy, 50 },
                        { Layer.Request, 30 },
                        { Layer.AdvAssaultTarget, 9 },
                        { Layer.Utility_peace, 3 },
                        { Layer.PatrolFollower, 2 },
                        { Layer.PatrolAssault, 0 },
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.arenaFighter,
                    },
                }
            },
            {
                Brain.Obdolbs, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.ObdolbosFight, 72 },
                        { Layer.Obd_Patrol, 10 },
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.crazyAssaultEvent,
                    },
                }
            },
            {
                Brain.Assault, new BrainInfoClass()
                {
                    Layers = new Dictionary<Layer, int>
                    {
                        { Layer.GrenadeDanger, 80 },
                        { Layer.Malfunction, 78},
                        { Layer.Help, 70 },
                        { Layer.FRequest, 65 },
                        { Layer.AssaultHaveEnemy, 55 },
                        { Layer.Request, 30 },
                        { Layer.Pursuit, 25 },
                        { Layer.Simple_Target, 20 },
                        { Layer.Leave_Map, 4 },
                        { Layer.StandBy, 3 },
                        { Layer.Utility_peace, 2 },
                        { Layer.PatrolAssault, 1 },
                    },
                    UsedByWildSpawns = new WildSpawnType[]
                    {
                        WildSpawnType.assault,
                    },
                }
            },
        };

        public Dictionary<Layer, LayerInfoClass> LayerInfos = new Dictionary<Layer, LayerInfoClass>();
    }

    public sealed class BrainInfoClass
    {
        public string Name;
        public string Description;
        public Dictionary<Layer, int> Layers = new Dictionary<Layer, int>();
        public WildSpawnType[] UsedByWildSpawns;
    }
    public sealed class LayerInfoClass
    {
        public string Name;
        public bool ConvertedToString;
        public string Description;
        public Dictionary<Brain, int> UsedByBrains = new Dictionary<Brain, int>();
        public WildSpawnType[] UsedByWildSpawns;
    }

    public enum Layer
    {
        FlGlScout,
        GluhAssKilla,
        GluharKilla,
        SecurityGluhar,
        BossGlFight,
        Kojaniy_Target,
        StayAtPos,
        Follower_bully,
        StandBy,
        PatrolAssault,
        Request,
        Pmc,
        AdvAssaultTarget,
        Simple_Target,
        ObdolbosFight,
        Leave_Map,
        StayAtPosOpt,
        Pursuit,
        Assault_Building,
        Enemy_Building,
        AssaultHaveEnemy,
        Bully_Layer,
        Kill_logic,
        Debug,
        FollowPlayer,
        Khorovod,
        Obd_Patrol,
        BirdHold,
        BirdEyeFight,
        PtrlBirdEye,
        KnightFight,
        StationaryWS,
        ExURequest,
        FRequest,
        PatrolFollower,
        Help,
        Gifter,
        FlGlPrTarget,
        GlGoal,
        GrenadeDanger,
        HoldOrCover,
        KojaniyB_Enemy,
        FolKojEnemy,
        Malfunction,
        MarksmanEnemy,
        MarksmanTarget,
        Panic,
        Utility_peace,
        BossSanitarFight,
        FlSanFight,
        SanitarGoal,
        GrenSuicide,
        RH_IN,
        RH_OUT,
        MeleeS_IN,
        MeleeS_OUT,
        RunandStrike,
        SupShootSect_IN,
        SupShootSect_OUT,
        CheckZryachiy,
        FlZryachFight,
        LayingPatrol,
        CloseZrFight,
        ZryachiyFight,
        TestLayer,
        TagillaAmbush,
        TagillaFollower,
        TagillaMain,
        GroupForce
    }
}
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Layers;
using SAIN.Layers.Combat.Run;
using SAIN.Layers.Combat.Solo;
using SAIN.Layers.Combat.Squad;
using SAIN.Preset.GlobalSettings;
using SAIN.Preset.GlobalSettings.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAIN;

public static class BigBrainHandler
{
    public const bool INCLUDE_RAIDER_BRAIN_FOR_PMCS = true;

    private static readonly string[] _commonVanillaLayersToRemove =
    [
        "Help",
        "AdvAssaultTarget",
        "Hit",
        "Simple Target",
        "Pmc",
        "AssaultHaveEnemy",
        "Assault Building",
        "Enemy Building",
        "PushAndSup",
        "Pursuit",
    ];

    private static readonly List<Type> _SAINLayers = [];
    private static readonly List<string> _SAINLayerNames = [];

    public static List<string> SAINLayerNames => FindAllSAINLayers();
    public static List<Type> SAINLayers
    {
        get
        {
            if (_SAINLayers.Count == 0)
            {
                Type[] allTypes = typeof(SAINPlugin).Assembly.GetTypes();
                for (int i = 0; i < allTypes.Length; i++)
                {
                    Type type = allTypes[i];
                    if (type.IsSubclassOf(typeof(SAINLayer)))
                    {
                        _SAINLayers.Add(type);
                    }
                }
            }

            return _SAINLayers;
        }
    }

    public static void Init()
    {
        BrainAssignment.Init();
    }

    private static List<string> FindAllSAINLayers()
    {
        if (_SAINLayerNames.Count != 0)
        {
            return _SAINLayerNames;
        }

        foreach (Type layerType in SAINLayers)
        {
            FieldInfo nameFieldInfo = layerType.GetField("Name", BindingFlags.Public | BindingFlags.Static);
            if (nameFieldInfo == null)
            {
                Logger.LogError($"{layerType.Name} does not have a public static Name field. This is required for enabling vanilla layers!");
                continue;
            }

            _SAINLayerNames.Add((string)nameFieldInfo.GetValue(null));
        }

        return _SAINLayerNames;
    }

    public static bool BigBrainInitialized;

    public static class BrainAssignment
    {
        public static void Init()
        {
            AddCustomLayersToPMCs();
            AddCustomLayersToScavs();
            AddCustomLayersToRaiders([WildSpawnType.pmcBot]);
            AddCustomLayersToRogues();
            AddCustomLayersToBloodHounds();
            AddCustomLayersToBosses();
            AddCustomLayersToFollowers();
            AddCustomLayersToGoons();
            AddCustomLayersToOthers();

            ToggleVanillaLayersForPMCs(false);
            ToggleVanillaLayersForOthers(false);
            ToggleVanillaLayersForAllBots();
        }

        public static void ToggleVanillaLayersForAllBots()
        {
            ToggleVanillaLayersForScavs(VanillaBotSettings.VanillaScavs);
            ToggleVanillaLayersForRogues(VanillaBotSettings.VanillaRogues);
            ToggleVanillaLayersForRaiders([WildSpawnType.pmcBot], false); // _vanillaBotSettings.VanillaRaiders);
            ToggleVanillaLayersForBloodHounds(VanillaBotSettings.VanillaBloodHounds);
            ToggleVanillaLayersForBosses(VanillaBotSettings.VanillaBosses);
            ToggleVanillaLayersForFollowers(VanillaBotSettings.VanillaFollowers);
            ToggleVanillaLayersForGoons(VanillaBotSettings.VanillaGoons);
        }

        public static void ToggleVanillaLayersForPMCs(bool useVanillaLayers)
        {
            List<string> brainList = GetBrainList(AIBrains.PMCs);

            List<string> LayersToToggle =
            [
                "Request",
                //"FightReqNull",
                //"PeacecReqNull",
                "KnightFight",
                //"PtrlBirdEye",
				"PmcBear",
                "PmcUsec",
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);

            if (INCLUDE_RAIDER_BRAIN_FOR_PMCS)
            {
                ToggleVanillaLayersForRaiders([WildSpawnType.pmcBEAR, WildSpawnType.pmcUSEC], useVanillaLayers);
            }
        }

        public static void ToggleVanillaLayersForScavs(bool useVanillaLayers)
        {
            List<string> brainList = GetBrainList(AIBrains.Scavs);

            List<string> LayersToToggle =
            [
                //"FightReqNull",
                //"PeacecReqNull",
                "PmcBear",
                "PmcUsec",
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);

            ToggleVanillaLayersForRaiders([WildSpawnType.assaultGroup], useVanillaLayers);
        }

        public static void ToggleVanillaLayersForRaiders(List<WildSpawnType> roles, bool useVanillaLayers)
        {
            List<string> brainList = [nameof(EBrain.PMC)];

            List<string> LayersToToggle =
            [
                "Request",
                //"FightReqNull",
                //"PeacecReqNull",
                "KnightFight",
                //"PtrlBirdEye",
				"PmcBear",
                "PmcUsec",
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, roles, useVanillaLayers);
        }

        public static void ToggleVanillaLayersForOthers(bool useVanillaLayers)
        {
            List<string> brainList = GetBrainList(AIBrains.Others);

            List<string> LayersToToggle =
            [
                "Request",
                "KnightFight",
				"PmcBear",
                "PmcUsec",
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
        }

        public static void ToggleVanillaLayersForRogues(bool useVanillaLayers)
        {
            List<string> brainList = [nameof(EBrain.ExUsec)];

            List<string> LayersToToggle =
            [
                "Request",
                //"FightReqNull",
                //"PeacecReqNull",
                "KnightFight",
                //"PtrlBirdEye",
					"PmcBear",
                "PmcUsec",
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
        }

        public static void ToggleVanillaLayersForBloodHounds(bool useVanillaLayers)
        {
            List<string> brainList = [nameof(EBrain.ArenaFighter)];

            List<string> LayersToToggle =
            [
                "Request",
                //"FightReqNull",
                //"PeacecReqNull",
                "KnightFight",
                //"PtrlBirdEye",
				"PmcBear",
                "PmcUsec",
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
        }

        public static void ToggleVanillaLayersForBosses(bool useVanillaLayers)
        {
            List<string> brainList = GetBrainList(AIBrains.Bosses);

            List<string> LayersToToggle =
            [
                "KnightFight",
                "BirdEyeFight",
                "BossBoarFight",
                "KojaniyB_Enemy",
                "Bully Layer",
                "KlnSolo",
                "KolontayFight",
                "KlnTrg",
                "BossSanitarFight",
                .. _commonVanillaLayersToRemove,
            ];
            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);

        }

        public static void ToggleVanillaLayersForFollowers(bool useVanillaLayers)
        {
            List<string> brainList = GetBrainList(AIBrains.Followers);

            List<string> LayersToToggle =
            [
                "KnightFight",
                "BoarGrenadeDanger",
                "FBoarFght",
                "SecurityKln",
                "Kln_NIMH",
                "FolKojEnemy",
                "KlnForceAtk",
                "KolontayAP",
                "KlnTrg",
                "FlSanFight",
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
        }

        public static void ToggleVanillaLayersForGoons(bool useVanillaLayers)
        {
            List<string> brainList = GetBrainList(AIBrains.Goons);

            List<string> LayersToToggle =
            [
                //"FightReqNull",
                //"PeacecReqNull",
                "KnightFight",
                "BirdEyeFight",
                "Kill logic",
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
        }

        private static void ToggleVanillaLayers(List<string> brainNames, List<string> layerNames, bool useVanillaLayers)
        {
            if (useVanillaLayers)
            {
                BrainManager.RemoveLayers(SAINLayerNames, brainNames);
                BrainManager.RestoreLayers(layerNames, brainNames);
            }
            else
            {
                CheckExtractEnabled(layerNames);

                BrainManager.RestoreLayers(SAINLayerNames, brainNames);
                BrainManager.RemoveLayers(layerNames, brainNames);
            }
        }

        private static void ToggleVanillaLayers(List<string> brainNames, List<string> layerNames, List<WildSpawnType> roles, bool useVanillaLayers)
        {
            if (useVanillaLayers)
            {
                BrainManager.RemoveLayers(SAINLayerNames, brainNames, roles);
                BrainManager.RestoreLayers(layerNames, brainNames, roles);
            }
            else
            {
                CheckExtractEnabled(layerNames);

                BrainManager.RestoreLayers(SAINLayerNames, brainNames, roles);
                BrainManager.RemoveLayers(layerNames, brainNames, roles);
            }
        }

        private static void AddCustomLayersToPMCs()
        {
            List<string> pmcBrain = GetBrainList(AIBrains.PMCs);
            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;

            BrainManager.AddCustomLayer(typeof(DebugLayer), pmcBrain, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), pmcBrain, 80);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), pmcBrain, settings.SAINExtractLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), pmcBrain, settings.SAINCombatSquadLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), pmcBrain, settings.SAINCombatSoloLayerPriority);

            if (INCLUDE_RAIDER_BRAIN_FOR_PMCS)
            {
                AddCustomLayersToRaiders([WildSpawnType.pmcBEAR, WildSpawnType.pmcUSEC]);
            }
        }

        private static void AddCustomLayersToScavs()
        {
            List<string> brainList = GetBrainList(AIBrains.Scavs);
            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;

            //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);

            AddCustomLayersToRaiders([WildSpawnType.assaultGroup]);
        }

        private static void AddCustomLayersToRaiders(List<WildSpawnType> roles)
        {
            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
            List<string> raiderBrain = [nameof(EBrain.PMC)];

            BrainManager.AddCustomLayer(typeof(DebugLayer), raiderBrain, 99, roles);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), raiderBrain, 80, roles);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), raiderBrain, settings.SAINExtractLayerPriority, roles);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), raiderBrain, settings.SAINCombatSquadLayerPriority, roles);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), raiderBrain, settings.SAINCombatSoloLayerPriority, roles);
        }

        private static void AddCustomLayersToOthers()
        {
            List<string> brainList = GetBrainList(AIBrains.Others);

            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
            //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);
        }

        private static void AddCustomLayersToRogues()
        {
            List<string> brainList = [nameof(EBrain.ExUsec)];

            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
            //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);
        }

        private static void AddCustomLayersToBloodHounds()
        {
            List<string> brainList = [nameof(EBrain.ArenaFighter)];

            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
            //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);
        }

        private static void AddCustomLayersToBosses()
        {
            List<string> brainList = GetBrainList(AIBrains.Bosses);

            //var settings = SAINPlugin.LoadedPreset.GlobalSettings.General;
            //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, 70);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, 69);
        }

        private static void AddCustomLayersToFollowers()
        {
            List<string> brainList = GetBrainList(AIBrains.Followers);

            //var settings = SAINPlugin.LoadedPreset.GlobalSettings.General;
            //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, 70);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, 69);
        }

        private static void AddCustomLayersToGoons()
        {
            List<string> brainList = GetBrainList(AIBrains.Goons);

            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, 64);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, 62);
        }

        private static void CheckExtractEnabled(List<string> layersToRemove)
        {
            if (GlobalSettingsClass.Instance.General.Extract.SAIN_EXTRACT_TOGGLE)
            {
                layersToRemove.Add("Exfiltration");
            }
        }

        private static List<string> GetBrainList(List<EBrain> brains)
        {
            List<string> brainList = [];
            for (int i = 0; i < brains.Count; i++)
            {
                brainList.Add(brains[i].ToString());
            }
            return brainList;
        }

        private static VanillaBotSettings VanillaBotSettings => SAINPlugin.LoadedPreset.GlobalSettings.General.VanillaBots;
    }
}
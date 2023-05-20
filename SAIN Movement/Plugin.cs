using BepInEx;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes;
using SAIN.Components;
using SAIN.Layers;
using SAIN.UserSettings;
using System;
using System.Collections.Generic;

namespace SAIN
{
    [BepInPlugin("me.sol.sainlayers", "SAIN Layers", "1.0")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static List<string> NormalBots = new List<string> { "PMC", "Assault", "ExUsec", "CursAssault", "SectantWarrior" };
        public static List<string> Bosses = new List<string> { "Knight", "BossBully", "BossSanitar", "Tagilla", "BossGluhar", "Killa", "BossKojaniy", "SectantPriest" };
        public static List<string> Followers = new List<string> { 
            "BigPipe", "BirdEye", "FollowerBully", "FollowerSanitar", "TagillaFollower", 
            "FollowerGluharAssault", "FollowerGluharProtect","FollowerGluharScout" };

        private void Awake()
        {
            try
            {
                DebugConfig.Init(Config);
                CoverConfig.Init(Config);

                new Patches.AddComponentPatch().Enable();
                new Patches.DisposeComponentPatch().Enable();

                new Patches.GrenadeThrownActionPatch().Enable();

                new Patches.BotGlobalGrenadePatch().Enable();
                new Patches.BotGlobalMindPatch().Enable();
                new Patches.BotGlobalMovePatch().Enable();
                new Patches.BotGlobalCorePatch().Enable();
                new Patches.BotGlobalAimPatch().Enable();

                new Patches.KickPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            BrainManager.AddCustomLayer(typeof(SAINFight), new List<string>(NormalBots), 90);
            BrainManager.AddCustomLayer(typeof(SAINFight), new List<string>(Bosses), 90);
            BrainManager.AddCustomLayer(typeof(SAINFight), new List<string>(Followers), 90);
        }
    }
}
using BepInEx;
using DrakiaXYZ.VersionChecker;
using SAIN.UserSettings;
using System;
using System.Collections.Generic;
using SAIN.Components;
using SAIN.Editor;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using UnityEngine;
using Comfort.Common;

namespace SAIN
{
    [BepInPlugin("me.sol.sain", "SAIN Beta", "3.4")]
    [BepInDependency("xyz.drakia.bigbrain", "0.1.4")]
    [BepInDependency("xyz.drakia.waypoints", "1.1.2")]
    [BepInDependency("com.spt-aki.core", "3.5.8")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class SAINPlugin : BaseUnityPlugin
    {
        public static EditorGUI EditorGUI;
        private void Awake()
        {
            if (!TarkovVersion.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception($"Invalid EFT Version");
            }
            
            // If BigBrain isn't loaded, we need to exit too. Normally this would be handled via
            // the BepInDependency, but due to remapping between 3.5.7 and 3.5.8 we also have to/
            // manually check for now
            if (!Chainloader.PluginInfos.ContainsKey("xyz.drakia.bigbrain"))
            {
                throw new Exception("Missing BigBrain");
            }

            SAINBotPresetManager.Init();

            ConfigInit();
            EFTPatches.Init();
            BigBrainSAIN.Init();

            ModsCheckTimer = Time.time + 5f;
        }

        public static SAINEditor SAINEditor { get; private set; }
        private static GameObject EditorObject { get; set; }

        private void ConfigInit()
        {
            SAINConfig = Config;
            EditorSettings.Init();
            ExtractConfig.Init(Config);
            TalkConfig.Init(Config);
            VisionConfig.Init(Config);
            DebugConfig.Init(Config);
            CoverConfig.Init(Config);
            SoundConfig.Init(Config);
            DazzleConfig.Init(Config);
        }

        public static ConfigFile SAINConfig { get; private set; }

        private void Update()
        {
            if (SAINEditor == null)
            {
                EditorObject = new GameObject("SAINEditorObject");
                SAINEditor = EditorObject.GetOrAddComponent<SAINEditor>();
            }
            //EditorGUI.Update();
            BotControllerHandler.Update();

            if (!ModsChecked && ModsCheckTimer < Time.time)
            {
                ModsChecked = true;

                RealismLoaded = Chainloader.PluginInfos.ContainsKey("RealismMod");
                if (RealismLoaded)
                {
                    Logger.LogInfo("SAIN: Realism Mod Detected, Auto-Adjusting Recoil for Bots...");
                }
                LootingBotsLoaded = Chainloader.PluginInfos.ContainsKey("me.skwizzy.lootingbots");
                if (LootingBotsLoaded)
                {
                    Logger.LogInfo("SAIN: Looting Bots Detected.");
                }
            }
        }

        private float ModsCheckTimer;

        private bool ModsChecked = false;
        public static bool RealismLoaded { get; private set; } = false;
        public static bool LootingBotsLoaded { get; private set; } = false;

        public static List<string> SAINLayers => BigBrainSAIN.SAINLayers;
        public static SAINBotController BotController => BotControllerHandler.BotController;
    }
}
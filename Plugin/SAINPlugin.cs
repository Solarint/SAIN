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

namespace SAIN
{
    public static class PluginInfo
    {
        public const string GUID = "me.sol.sain";
        public const string Name = "SAIN Beta";
        public const string Version = "3.5";
    }

    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("xyz.drakia.bigbrain", "0.1.4")]
    [BepInDependency("xyz.drakia.waypoints", "1.1.2")]
    [BepInDependency("com.spt-aki.core", "3.5.8")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class SAINPlugin : BaseUnityPlugin
    {
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

            ConfigInit();
            EFTPatches.Init();
            BigBrainSAIN.Init();

            ModsCheckTimer = Time.time + 5f;
        }

        void Start()
        {
            SAINEditor.Init();
        }

        public static readonly SAINEditor SAINEditor = new SAINEditor();

        private void ConfigInit()
        {
            SAINConfig = Config;

            EditorSettings.Init();

            TalkConfig.Init(Config);
            VisionConfig.Init(Config);
            DebugConfig.Init(Config);
            CoverConfig.Init(Config);
            SoundConfig.Init(Config);
            DazzleConfig.Init(Config);
        }

        public static ConfigFile SAINConfig { get; private set; }

        void Update()
        {
            SAINEditor.Update();

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

        private void LateUpdate()
        {
            SAINEditor.LateUpdate();
        }

        void OnGUI()
        {
            SAINEditor.OnGUI();
        }

        private float ModsCheckTimer;

        private bool ModsChecked = false;
        public static bool RealismLoaded { get; private set; } = false;
        public static bool LootingBotsLoaded { get; private set; } = false;

        public static List<string> SAINLayers => BigBrainSAIN.SAINLayers;
        public static SAINBotController BotController => BotControllerHandler.BotController;
    }
}
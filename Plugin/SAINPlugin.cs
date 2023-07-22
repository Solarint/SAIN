using BepInEx;
using DrakiaXYZ.VersionChecker;
using SAIN.UserSettings;
using System;
using System.Collections.Generic;
using SAIN.Components;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using EFT.UI;
using UnityEngine;

namespace SAIN
{
    [BepInPlugin("me.sol.sain", "SAIN Beta", "3.4.5")]
    [BepInDependency("xyz.drakia.bigbrain", "0.2.0")]
    [BepInDependency("xyz.drakia.waypoints", "1.2.0")]
    [BepInDependency("com.spt-aki.core", "3.6.0")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class SAINPlugin : BaseUnityPlugin
    {
        public static Difficulty.Editor DifficultySettings;
        private void Awake()
        {
            if (!TarkovVersion.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception($"Invalid EFT Version");
            }

            ConfigInit();
            EditorInit();
            EFTPatches.Init();
            BigBrainSAIN.Init();
            DifficultySettings = new Difficulty.Editor();
            ModsCheckTimer = Time.time + 5f;
        }

        private void EditorInit()
        {
            ConsoleScreen.Processor.RegisterCommand("saineditor", new Action(Difficulty.Editor.OpenPanel));

            Difficulty.Editor.TogglePanel = Config.Bind(
                "SAIN Settings Editor",
                "",
                new KeyboardShortcut(KeyCode.Home),
                "The keyboard shortcut that toggles editor");
        }

        private void ConfigInit()
        {
            EditorSettings.Init(Config);
            ExtractConfig.Init(Config);
            TalkConfig.Init(Config);
            VisionConfig.Init(Config);
            DebugConfig.Init(Config);
            CoverConfig.Init(Config);
            SoundConfig.Init(Config);
            DazzleConfig.Init(Config);
        }

        private void Update()
        {
            DifficultySettings.Update();
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

        private void OnGUI()
        {
            DifficultySettings.OnGUI();
        }

        private float ModsCheckTimer;

        private bool ModsChecked = false;
        public static bool RealismLoaded { get; private set; } = false;
        public static bool LootingBotsLoaded { get; private set; } = false;

        public static List<string> SAINLayers => BigBrainSAIN.SAINLayers;
        public static SAINBotController BotController => BotControllerHandler.BotController;
    }
}
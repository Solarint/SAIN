using BepInEx;
using DrakiaXYZ.VersionChecker;
using SAIN.UserSettings;
using System;
using System.Collections.Generic;
using SAIN.Components;
using EFT;
using BepInEx.Bootstrap;
using Comfort.Common;
using BepInEx.Configuration;
using EFT.UI;
using UnityEngine;

namespace SAIN
{
    [BepInPlugin("me.sol.sain", "SAIN Beta 3", "2.0")]
    [BepInDependency("xyz.drakia.bigbrain", "0.1.4")]
    [BepInDependency("com.spt-aki.core", "3.5.8")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class SAINPlugin : BaseUnityPlugin
    {
        public static readonly Difficulty.Editor DifficultySettings = new Difficulty.Editor();
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
            EditorInit();
            EFTPatches.Init();
            BigBrainSAIN.Init();
        }

        public static bool ExtractingDisabled { get; private set; }

        private void EditorInit()
        {
            ConsoleScreen.Processor.RegisterCommand("saineditor", new Action(Difficulty.Editor.OpenPanel));

            Difficulty.Editor.TogglePanel = Config.Bind(
                "Difficulty Editor",
                "SAIN Bot Difficulty Editor",
                new KeyboardShortcut(KeyCode.Home),
                "The keyboard shortcut that toggles editor");
        }

        private void ConfigInit()
        {
            ExtractConfig.Init(Config);
            DifficultyConfig.Init(Config);
            TalkConfig.Init(Config);
            VisionConfig.Init(Config);
            DebugConfig.Init(Config);
            CoverConfig.Init(Config);
            ShootConfig.Init(Config);
            SoundConfig.Init(Config);
            DazzleConfig.Init(Config);
        }

        private void Update()
        {
            DifficultySettings.Update();
            BotControllerHandler.Update();
            if (Singleton<GameWorld>.Instance != null)
            {
                if (!WayPointsChecked)
                {
                    WayPointsChecked = true;
                    ExtractingDisabled = !Chainloader.PluginInfos.ContainsKey("xyz.drakia.waypoints");
                    if (ExtractingDisabled)
                    {
                        Console.WriteLine("SAIN: Waypoints Is Not Installed, Bot Extracts Disabled!");
                    }
                }
                if (!RealismChecked)
                {
                    RealismChecked = true;
                    RealismModLoaded = Chainloader.PluginInfos.ContainsKey("RealismMod");
                    if (RealismModLoaded)
                    {
                        Console.WriteLine("SAIN: Realism Mod Detected, auto-adjusting recoil for bots.");
                    }
                }
            }
        }

        private void OnGUI()
        {
            DifficultySettings.OnGUI();
        }

        private bool RealismChecked = false;
        public static bool RealismModLoaded = false;

        private bool WayPointsChecked = false;
        public static List<string> SAINLayers => BigBrainSAIN.SAINLayers;
        public static SAINBotController BotController => BotControllerHandler.BotController;
    }
}
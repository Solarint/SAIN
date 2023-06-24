using BepInEx;
using DrakiaXYZ.VersionChecker;
using SAIN.UserSettings;
using System;
using System.Collections.Generic;
using SAIN.Components;
using EFT;
using DrakiaXYZ.BigBrain.Brains;
using BepInEx.Bootstrap;
using Comfort.Common;

namespace SAIN
{
    [BepInPlugin("me.sol.sain", "SAIN Beta 2", "2.0")]
    [BepInDependency("xyz.drakia.bigbrain", "0.1.3")]
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
        }

        public static bool ExtractingDisabled { get; private set; }

        private void ConfigInit()
        {
            ExtractConfig.Init(Config);
            DifficultyConfig.Init(Config);
            TalkConfig.Init(Config);
            VisionConfig.Init(Config);
            DebugConfig.Init(Config);
            CoverConfig.Init(Config);
            BotShootConfig.Init(Config);
            SoundConfig.Init(Config);
            DazzleConfig.Init(Config);
        }

        private void Update()
        {
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
            }
        }

        private bool WayPointsChecked = false;
        public static List<string> SAINLayers => BigBrainSAIN.SAINLayers;
        public static SAINBotController BotController => BotControllerHandler.BotController;
    }
}
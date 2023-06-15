using BepInEx;
using DrakiaXYZ.VersionChecker;
using SAIN.UserSettings;
using System;
using System.Collections.Generic;
using SAIN.Components;
using Comfort.Common;
using EFT;

namespace SAIN
{
    [BepInPlugin("me.sol.sain", "SAIN AI Beta 2", "2.0")]
    [BepInDependency("xyz.drakia.bigbrain", "0.1.2")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class SAINPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            if (!TarkovVersion.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception($"Invalid EFT Version");
            }

            ConfigInit();
            EFTPatches.Init();
            BigBrainLayers.Init();
        }

        private void ConfigInit()
        {
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
        }

        public static List<string> SAINLayers => BigBrainLayers.SAINLayers;
        public static SAINBotController BotController => BotControllerHandler.BotController;
    }
}
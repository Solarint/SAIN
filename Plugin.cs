using BepInEx;
using DrakiaXYZ.VersionChecker;
using DrakiaXYZ.BigBrain.Brains;
using SAIN.Layers;
using SAIN.UserSettings;
using System;
using System.Collections.Generic;
using SAIN.Components;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace SAIN
{
    [BepInPlugin("me.sol.sain", "SAIN AI", "2.0")]
    [BepInDependency("xyz.drakia.bigbrain", "0.1.2")]
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
            if (!TarkovVersion.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception($"Invalid EFT Version");
            }

            try
            {
                TalkConfig.Init(Config);
                VisionConfig.Init(Config);
                DebugConfig.Init(Config);
                CoverConfig.Init(Config);
                BotShootConfig.Init(Config);
                SoundConfig.Init(Config);
                DazzleConfig.Init(Config);

                //new Patches.MoverPatch().Enable();
                new Patches.SprintPatch().Enable();

                new Patches.CheckFlashlightPatch().Enable();
                new Patches.DazzlePatch().Enable();

                new Patches.AddComponentPatch().Enable();
                new Patches.DisposeComponentPatch().Enable();
                new Patches.InitHelper().Enable();

                new Patches.InitiateShotPatch().Enable();
                new Patches.TryPlayShootSoundPatch().Enable();
                new Patches.HearingSensorPatch().Enable();

                new Patches.PlayerTalkPatch().Enable();
                new Patches.TalkDisablePatch1().Enable();
                new Patches.TalkDisablePatch2().Enable();
                new Patches.TalkDisablePatch3().Enable();
                new Patches.TalkDisablePatch4().Enable();

                new Patches.VisibleDistancePatch().Enable();
                new Patches.GainSightPatch().Enable();

                new Patches.KickPatch().Enable();
                new Patches.GrenadeThrownActionPatch().Enable();
                new Patches.AddEnemyToAllGroupsInBotZonePatch().Enable();
                new Patches.AddEnemyToAllGroupsPatch().Enable();

                new Patches.BotGlobalLookPatch().Enable();
                new Patches.BotGlobalShootPatch().Enable();
                new Patches.BotGlobalGrenadePatch().Enable();
                new Patches.BotGlobalMindPatch().Enable();
                new Patches.BotGlobalMovePatch().Enable();
                new Patches.BotGlobalCorePatch().Enable();
                new Patches.BotGlobalAimPatch().Enable();
                new Patches.BotGlobalScatterPatch().Enable();

                new Patches.AimOffsetPatch().Enable();
                new Patches.RecoilPatch().Enable();
                new Patches.LoseRecoilPatch().Enable();
                new Patches.EndRecoilPatch().Enable();
                new Patches.FullAutoPatch().Enable();
                new Patches.SemiAutoPatch().Enable();
                new Patches.FiremodePatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            BrainManager.AddCustomLayer(typeof(SAINCombatSquadLayer), new List<string>(NormalBots), 95);
            BrainManager.AddCustomLayer(typeof(SAINCombatSquadLayer), new List<string>(Bosses), 61);
            BrainManager.AddCustomLayer(typeof(SAINCombatSquadLayer), new List<string>(Followers), 95);

            BrainManager.AddCustomLayer(typeof(SAINCombatSoloLayer), new List<string>(NormalBots), 94);
            BrainManager.AddCustomLayer(typeof(SAINCombatSoloLayer), new List<string>(Bosses), 60);
            BrainManager.AddCustomLayer(typeof(SAINCombatSoloLayer), new List<string>(Followers), 94);
        }

        private void Update()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            MainPlayer = gameWorld?.MainPlayer;
            if (MainPlayer == null)
            {
                ComponentAdded = false;
                return;
            }

            // Add Components to main player
            if (!ComponentAdded)
            {
                gameWorld.GetOrAddComponent<LineOfSightManager>();
                gameWorld.GetOrAddComponent<BotController>();

                MainPlayer.GetOrAddComponent<PlayerTalkComponent>();
                MainPlayer.GetOrAddComponent<FlashLightComponent>();
                ComponentAdded = true;
            }

            if (UpdatePositionTimer < Time.time)
            {
                UpdatePositionTimer = Time.time + 0.15f;
                MainPlayerPosition = MainPlayer.Position;
            }
        }

        public static Vector3 MainPlayerPosition { get; private set; }
        public static Player MainPlayer { get; private set; }

        private bool ComponentAdded = false;
        private float UpdatePositionTimer = 0f;
    }
}
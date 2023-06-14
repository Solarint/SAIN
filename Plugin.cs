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
    [BepInPlugin("me.sol.sain", "SAIN AI", "2.1")]
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
            else
            {
                ConfigInit();

                GenericPatches();
                MovementPatches();
                ComponentPatches();
                HearingPatches();
                TalkPatches();
                VisionPatches();
                GlobalSettingsPatches();
                ShootPatches();

                AddLayers();
                RemoveLayers();
            }
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

        private void GenericPatches()
        {
            new Patches.GrenadeThrownActionPatch().Enable();
            new Patches.AddEnemyToAllGroupsInBotZonePatch().Enable();
            new Patches.AddEnemyToAllGroupsPatch().Enable();
        }

        private void ShootPatches()
        {
            new Patches.AimOffsetPatch().Enable();
            new Patches.RecoilPatch().Enable();
            new Patches.LoseRecoilPatch().Enable();
            new Patches.EndRecoilPatch().Enable();
            new Patches.FullAutoPatch().Enable();
            new Patches.SemiAutoPatch().Enable();
            new Patches.FiremodePatch().Enable();
        }

        private void MovementPatches()
        {
            new Patches.KickPatch().Enable();
            new Patches.SprintPatch1().Enable();
            new Patches.BotRunDisable().Enable();
            new Patches.TargetSpeedPatch1().Enable();
            new Patches.TargetSpeedPatch2().Enable();
            new Patches.SetPosePatch1().Enable();
            new Patches.SetPosePatch2().Enable();
            new Patches.HoldOrCoverPatch1().Enable();
            new Patches.HoldOrCoverPatch2().Enable();
        }

        private void ComponentPatches()
        {
            new Patches.AddComponentPatch().Enable();
            new Patches.DisposeComponentPatch().Enable();
            new Patches.InitHelper().Enable();
        }

        private void VisionPatches()
        {
            new Patches.VisibleDistancePatch().Enable();
            new Patches.GainSightPatch().Enable();
            new Patches.CheckFlashlightPatch().Enable();
            new Patches.DazzlePatch().Enable();
        }

        private void HearingPatches()
        {
            new Patches.InitiateShotPatch().Enable();
            new Patches.TryPlayShootSoundPatch().Enable();
            new Patches.HearingSensorPatch().Enable();
        }

        private void TalkPatches()
        {
            new Patches.PlayerTalkPatch().Enable();
            new Patches.TalkDisablePatch1().Enable();
            new Patches.TalkDisablePatch2().Enable();
            new Patches.TalkDisablePatch3().Enable();
            new Patches.TalkDisablePatch4().Enable();
        }

        private void GlobalSettingsPatches()
        {
            new Patches.BotGlobalLookPatch().Enable();
            new Patches.BotGlobalShootPatch().Enable();
            new Patches.BotGlobalGrenadePatch().Enable();
            new Patches.BotGlobalMindPatch().Enable();
            new Patches.BotGlobalMovePatch().Enable();
            new Patches.BotGlobalCorePatch().Enable();
            new Patches.BotGlobalAimPatch().Enable();
            new Patches.BotGlobalScatterPatch().Enable();
        }

        private void AddLayers()
        {
            // SAIN Squad
            BrainManager.AddCustomLayer(typeof(SAINSquad), new List<string>(NormalBots), 24);
            BrainManager.AddCustomLayer(typeof(SAINSquad), new List<string>(Bosses), 24);
            BrainManager.AddCustomLayer(typeof(SAINSquad), new List<string>(Followers), 24);
            SAINLayers.Add(SAINSquad.Name);

            // SAIN Solo
            BrainManager.AddCustomLayer(typeof(SAINSolo), new List<string>(NormalBots), 20);
            BrainManager.AddCustomLayer(typeof(SAINSolo), new List<string>(Bosses), 20);
            BrainManager.AddCustomLayer(typeof(SAINSolo), new List<string>(Followers), 20);
            SAINLayers.Add(SAINSolo.Name);
        }


        private void RemoveLayers()
        {
            BrainManager.RemoveLayer("AdvAssaultTarget", new List<string>(NormalBots));
            BrainManager.RemoveLayer("AdvAssaultTarget", new List<string>(Bosses));
            BrainManager.RemoveLayer("AdvAssaultTarget", new List<string>(Followers));
            BrainManager.RemoveLayer("Pmc", new List<string>(NormalBots));
            BrainManager.RemoveLayer("Pmc", new List<string>(Bosses));
            BrainManager.RemoveLayer("Pmc", new List<string>(Followers));
            BrainManager.RemoveLayer("AssaultHaveEnemy", new List<string>(NormalBots));
            BrainManager.RemoveLayer("AssaultHaveEnemy", new List<string>(Bosses));
            BrainManager.RemoveLayer("AssaultHaveEnemy", new List<string>(Followers));
        }

        public static List<string> SAINLayers { get; private set; } = new List<string>();

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
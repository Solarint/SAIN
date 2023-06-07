using BepInEx;
using DrakiaXYZ.VersionChecker;
using DrakiaXYZ.BigBrain.Brains;
using SAIN.Layers;
using SAIN.Helpers;
using SAIN.UserSettings;
using System;
using System.Collections.Generic;
using SAIN.Components;
using Comfort.Common;
using EFT;
using UnityEngine;
using EFT.Interactive;
using System.Collections;

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
                FullAutoConfig.Init(Config);
                RecoilScatterConfig.Init(Config);
                SemiAutoConfig.Init(Config);
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

            //BrainManager.AddCustomLayer(typeof(SAINRoamingLayer), new List<string>(NormalBots), 1);

            BrainManager.AddCustomLayer(typeof(SAINFightLayer), new List<string>(NormalBots), 80);
            BrainManager.AddCustomLayer(typeof(SAINFightLayer), new List<string>(Bosses), 80);
            BrainManager.AddCustomLayer(typeof(SAINFightLayer), new List<string>(Followers), 80);
        }

        private void Update()
        {
            // Add Components to main player

            if (Singleton<GameWorld>.Instance != null)
            {
                AddTalkComponent.AddSingleComponent<PlayerTalkComponent>();
                AddFlashlightComponent.AddSingleComponent<FlashLightComponent>();
                /*
                var player = Singleton<GameWorld>.Instance.MainPlayer;

                if (player != null)
                {
                    Vector3 botPos = player.MainParts[BodyPartType.head].Position;
                    Vector3 lookDir = player.LookDirection;
                    if (Physics.Raycast(botPos, lookDir, out var hit, 2f, LayerMaskClass.InteractiveMask))
                    {
                        Logger.LogWarning($"[{hit.transform.name}]");

                        if (hit.transform.name.ToLower().Contains("door"))
                        {
                            Door door = hit.transform.GetComponent<Door>();
                            if (door != null)
                            {
                                Logger.LogWarning($"CheckStuck(): Found Door Component");
                            }
                            else
                            {
                                Logger.LogWarning($"CheckStuck(): No Door Component");
                            }
                        }
                    }
                }
                */
            }
        }

        private readonly MainPlayerComponentSingle AddTalkComponent = new MainPlayerComponentSingle();
        private readonly MainPlayerComponentSingle AddFlashlightComponent = new MainPlayerComponentSingle();
    }
}
using BepInEx;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.UserSettings;
using SAIN.Layers;
using System;
using System.Collections.Generic;

namespace SAIN
{
    [BepInPlugin("me.sol.sainlayers", "SAIN Layers", "1.0")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            try
            {
                DogFightConfig.Init(Config);
                DebugConfig.Init(Config);
                CoverSystemConfig.Init(Config);

                new Patches.AddComponentPatch().Enable();
                new Patches.DisposeComponentPatch().Enable();

                new Patches.BotGlobalMindPatch().Enable();
                new Patches.BotGlobalMovePatch().Enable();
                new Patches.BotGlobalCorePatch().Enable();

                new Patches.KickPatch().Enable();

                BrainManager.AddCustomLayer(typeof(ScavLayer), new List<string>() { "Assault" }, 100);
                BrainManager.AddCustomLayer(typeof(PMCLayer), new List<string>() { "PMC" }, 100);
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }
        }

        private void Update()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld?.RegisteredPlayers == null || gameWorld?.MainPlayer?.Location == null || !Singleton<IBotGame>.Instantiated)
            {
                ComponentAdded = false;
                return;
            }

            if (!ComponentAdded)
            {
                gameWorld.MainPlayer.gameObject.AddComponent<CoverCentralComponent>();
                ComponentAdded = true;
            }
        }

        private bool ComponentAdded = false;
    }
}
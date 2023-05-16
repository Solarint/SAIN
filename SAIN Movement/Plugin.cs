using BepInEx;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes;
using SAIN.Components;
using SAIN.Layers;
using SAIN.UserSettings;
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
                CoverConfig.Init(Config);

                new Patches.AddComponentPatch().Enable();
                new Patches.DisposeComponentPatch().Enable();

                new Patches.BotGlobalMindPatch().Enable();
                new Patches.BotGlobalMovePatch().Enable();
                new Patches.BotGlobalCorePatch().Enable();

                new Patches.KickPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            BrainManager.AddCustomLayer(typeof(RetreatLayer), new List<string>() { "PMC", "Assault" }, 100);

            BrainManager.AddCustomLayer(typeof(DogFightLayer), new List<string>() { "PMC", "Assault" }, 95);

            BrainManager.AddCustomLayer(typeof(FightLayer), new List<string>() { "PMC", "Assault" }, 90);

            BrainManager.AddCustomLayer(typeof(SkirmishLayer), new List<string>() { "PMC", "Assault" }, 85);
        }

        private void Update()
        {
            /*
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld?.RegisteredPlayers == null || gameWorld?.MainPlayer?.Location == null || !Singleton<IBotGame>.Instantiated)
            {
                CoverCentral.NavmeshVertices = null;
                return;
            }

            foreach (var player in gameWorld.RegisteredPlayers)
            {
                player.GetOrAddComponent<PointGenerator>();
            }

            CoverCentral.Update();
            */
        }
    }
}
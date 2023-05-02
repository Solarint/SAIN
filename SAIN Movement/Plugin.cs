using BepInEx;
using DrakiaXYZ.BigBrain.Brains;
using DrakiaXYZ.VersionChecker;
using Movement.UserSettings;
using SAIN.Movement.Layers.DogFight;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Movement
{
    [BepInPlugin("me.sol.sainmove", "SAIN Movement", "2.0")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            CheckEftVersion();

            DogFight.Init(Config);
            UserSettings.Debug.Init(Config);

            try
            {
                new Patches.AddComponentPatch().Enable();
                new Patches.DisposeComponentPatch().Enable();

                new Patches.GlobalSettings.BotGlobalMindPatch().Enable();
                new Patches.GlobalSettings.BotGlobalMovePatch().Enable();
                new Patches.GlobalSettings.BotGlobalCorePatch().Enable();

                new Patches.DoorPatch.KickPatch().Enable();

                //new Patches.DogFight.BotLogicDecisionPatch().Enable();
                //new Patches.DogFight.DogFightGStructPatch().Enable();
                //new Patches.DogFight.EndShootFromPlacePatch().Enable();

                //new Patches.DogFight.TryStopReloadPatch().Enable();
                //new Patches.DogFight.IsInDogFightPatch().Enable();
                //new Patches.DogFight.StartPatch().Enable();
                //new Patches.DogFight.ManualUpdatePatch().Enable();
                //new Patches.DogFight.UpdatePatch().Enable();

                //new Patches.Heal.StopHealPatch1().Enable();
                //new Patches.Heal.StopHealPatch2().Enable();
                //new Patches.Heal.StopHealPatch3().Enable();
                //new Patches.Heal.StopHealPatch5().Enable();
                //new Patches.Heal.StopHealPatch6().Enable();
                //new Patches.Heal.StopHealPatch7().Enable();

            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            BrainManager.Instance.AddCustomLayer(typeof(DogFightLayer), new List<string>() { "Assault", "PMC" }, 100);
        }
        private void CheckEftVersion()
        {
            // Make sure the version of EFT being run is the correct version
            int currentVersion = FileVersionInfo.GetVersionInfo(BepInEx.Paths.ExecutablePath).FilePrivatePart;
            int buildVersion = TarkovVersion.BuildVersion;
            if (currentVersion != buildVersion)
            {
                Logger.LogError($"ERROR: This version of {Info.Metadata.Name} v{Info.Metadata.Version} was built for Tarkov {buildVersion}, but you are running {currentVersion}. Please download the correct plugin version.");
                throw new Exception($"Invalid EFT Version ({currentVersion} != {buildVersion})");
            }
        }
    }
}
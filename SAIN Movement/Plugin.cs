using BepInEx;
using DrakiaXYZ.VersionChecker;
using SAIN_Audio.Movement.Config;
using System;
using System.Diagnostics;

namespace SAIN_Audio.Movement
{
    [BepInPlugin("me.sol.sainmove", "SAIN Movement", "1.5")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class SAIN : BaseUnityPlugin
    {
        private void Awake()
        {
            CheckEftVersion();

            DogFighterConfig.Init(Config);
            DebugConfig.Init(Config);
            //DoorConfig.Init(Config);

            try
            {
                new Patches.AddComponentPatch().Enable();
                new Patches.BotGlobalsMindSettingsPatch().Enable();

                new Patches.MovementSpeed().Enable();
                new Patches.DogFight.Fight().Enable();
                //new Patches.DogFight.ManualUpdate().Enable();
                //new Patches.DogFight.Start().Enable();

                //new Patches.CoverPatch().Enable();
                //new Patches.DoorPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }
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
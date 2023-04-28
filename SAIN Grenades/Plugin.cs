using BepInEx;
using DrakiaXYZ.VersionChecker;
using SAIN_Grenades.Configs;
using System;
using System.Diagnostics;

namespace SAIN_Grenades
{
    [BepInPlugin("me.sol.saingrenades", "SAIN Grenades", "1.0")]
    public class AngryBotsPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            CheckEftVersion();

            GrenadeConfigs.Init(Config);

            try
            {
                new Patches.AddComponentPatch().Enable();

                new Patches.GlobalGrenadeSettingsPatch().Enable();

                new Patches.DisableGrenadePatch().Enable();

                //new Patches.GrenadeSoundPatch().Enable();
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
using BepInEx;
using DrakiaXYZ.VersionChecker;
using SAIN.Flashlights.Config;
using System;
using System.Diagnostics;

namespace SAIN.Flashlights
{
    [BepInPlugin("me.sol.sainflash", "SAIN Flashlights", "1.1")]
    public class FlashlightsPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            CheckEftVersion();

            DazzleConfig.Init(Config);

            try
            {
                new Patches.FlashLightDazzle().Enable();
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
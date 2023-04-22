using BepInEx;
using DrakiaXYZ.VersionChecker;
using SAIN.Config;
using System;
using System.Diagnostics;

namespace SAIN
{
    [BepInPlugin("me.sol.saincore", "SAIN Core", "1.6")]
    public class SAINCorePlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            CheckEftVersion();
            Template.Init(Config);

            try
            {
                new Patches.TemplatePatch().Enable();
                new Patches.TemplatePatch2().Enable();
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
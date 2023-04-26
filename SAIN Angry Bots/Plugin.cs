using BepInEx;
using DrakiaXYZ.VersionChecker;
using SAIN_AngryBots.Config;
using System;
using System.Diagnostics;

namespace SAIN_AngryBots
{
    [BepInPlugin("me.sol.sainangrybots", "SAIN Angry Bots", "1.0")]
    public class AngryBotsPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            //CheckEftVersion();
            AngryConfig.Init(Config);

            try
            {
                new Patches.PlayerTalkPatch().Enable();
                //new Patches.SetVisiblePatch().Enable();
                new Patches.GlobalTalkSettingsPatch().Enable();
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
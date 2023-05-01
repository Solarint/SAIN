using BepInEx;
using DrakiaXYZ.VersionChecker;
using Combat.UserSettings;
using System;
using System.Diagnostics;

namespace Combat
{
    [BepInPlugin("me.sol.sain", "SAIN Combat", "1.2")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            CheckEftVersion();

            FullAutoConfig.Init(Config);
            SemiAutoConfig.Init(Config);
            DebugConfig.Init(Config);
            RecoilScatterConfig.Init(Config);

            try
            {
                new Patches.AddComponentPatch().Enable();

                new Patches.BotGlobalScatterPatch().Enable();
                new Patches.BotGlobalShootDataPatch().Enable();
                new Patches.BotGlobalCorePatch().Enable();
                new Patches.BotGlobalAimingSettingsPatch().Enable();

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
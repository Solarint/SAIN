using BepInEx;
using DrakiaXYZ.VersionChecker;
using SAIN.Audio.Configs;
using System;
using System.Diagnostics;

namespace SAIN.Audio
{
    [BepInPlugin("me.sol.sainaudio", "SAIN Audio", "1.2")]
    public class AudioPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            CheckEftVersion();

            SoundConfig.Init(Config);

            try
            {
                new Patches.InitiateShotPatch().Enable();
                new Patches.TryPlayShootSoundPatch().Enable();
                new Patches.ComponentAddPatch().Enable();
                new Patches.HearingSensorDisablePatch().Enable();
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
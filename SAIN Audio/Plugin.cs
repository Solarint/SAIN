using BepInEx;
using DrakiaXYZ.VersionChecker;
using SAIN_Movement.Configs;
using System;
using System.Diagnostics;

namespace SAIN_Movement
{
    [BepInPlugin("me.sol.sainaudio", "SAIN Audio", "1.4")]
    public class AudioPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            if (!TarkovVersion.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception($"Invalid EFT Version");
            }

            SoundConfig.Init(Config);

            try
            {
                SoundConfig.Init(Config);
                new Patches.InitiateShotPatch().Enable();
                new Patches.TryPlayShootSoundPatch().Enable();

                new Patches.HearingSensorDisablePatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }
        }
    }
}
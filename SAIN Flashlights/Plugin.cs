using BepInEx;
using DrakiaXYZ.VersionChecker;
using Flashlights.Config;
using System;
using System.Diagnostics;

namespace Flashlights
{
    [BepInPlugin("me.sol.sainflash", "SAIN Flashlights", "2.0")]
    public class FlashlightsPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            if (!TarkovVersion.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception($"Invalid EFT Version");
            }

            DazzleConfig.Init(Config);

            try
            {
                new Patches.CheckFlashlightPatch().Enable();

                new Patches.DazzlePatch().Enable();

                new Patches.AddComponentPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }
        }
    }
}
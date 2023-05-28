using BepInEx;
using DrakiaXYZ.VersionChecker;
using Vision.UserSettings;
using System;
using System.Diagnostics;

namespace Vision
{
    [BepInPlugin("me.sol.sainvision", "SAIN Vision", "1.3")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            if (!TarkovVersion.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception($"Invalid EFT Version");
            }

            VisionConfig.Init(Config);

            try
            {
                new Patches.VisibleDistancePatch().Enable();
                new Patches.GainSightPatch().Enable();
                //new Patches.IsPartVisiblePatch().Enable();

                //new Patches.AddComponentPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }
        }
    }
}
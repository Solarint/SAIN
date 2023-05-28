using BepInEx;
using BepInEx.Configuration;
using DrakiaXYZ.VersionChecker;
using System;
using System.Diagnostics;

namespace SAIN
{
    [BepInPlugin("me.sol.saincore", "SAIN AI Core", "1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<float> VisionRaycast { get; private set; }
        public static ConfigEntry<float> ShootRaycast { get; private set; }
        public static ConfigEntry<float> CheckPath { get; private set; }

        private void Awake()
        {
            if (!TarkovVersion.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception($"Invalid EFT Version");
            }

            VisionRaycast = Config.Bind("Settings", "Raycast Vision Frequency", 0.15f,
                new ConfigDescription("How often to update the bool value of Bot's ability to see their enemy",
                new AcceptableValueRange<float>(0f, 0.5f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 11 }));

            ShootRaycast = Config.Bind("Settings", "Raycast Shoot Frequency", 0.15f,
                new ConfigDescription("How often to update the bool value of Bot's ability to shoot their enemy",
                new AcceptableValueRange<float>(0f, 0.5f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 10 }));

            CheckPath = Config.Bind("Settings", "Update NavMesh Path Frequency", 0.5f,
                new ConfigDescription("How often to calculate path between enemy and Bot",
                new AcceptableValueRange<float>(0f, 3f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 9 }));

            try
            {
                new AddComponentPatch().Enable();
                new DisposeComponentPatch().Enable();
                new InitHelper().Enable();
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
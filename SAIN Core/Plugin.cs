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
        public static ConfigEntry<float> CheckStatus { get; private set; }
        public static ConfigEntry<float> UnderFire { get; private set; }
        public static ConfigEntry<float> RefreshMeds { get; private set; }

        public static ConfigEntry<bool> DebugLogs { get; private set; }

        private void Awake()
        {
            CheckEftVersion();

            VisionRaycast = Config.Bind("Settings", "Raycast Vision Frequency", 0.15f,
                new ConfigDescription("How often to update the bool value of Bot's ability to see their enemy",
                new AcceptableValueRange<float>(0f, 0.5f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 11 }));

            ShootRaycast = Config.Bind("Settings", "Raycast Shoot Frequency", 0.25f,
                new ConfigDescription("How often to update the bool value of Bot's ability to shoot their enemy",
                new AcceptableValueRange<float>(0f, 0.5f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 10 }));

            CheckPath = Config.Bind("Settings", "Update NavMesh Path Frequency", 0.5f,
                new ConfigDescription("How often to calculate path between enemy and Bot",
                new AcceptableValueRange<float>(0f, 3f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 9 }));

            CheckStatus = Config.Bind("Settings", "Update Status Frequency", 0.25f,
                new ConfigDescription("How often to query Bot health status",
                new AcceptableValueRange<float>(0f, 3f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 9 }));

            UnderFire = Config.Bind("Settings", "Update UnderFire Frequency", 0.1f,
                new ConfigDescription("How often to update positions and timer when bot is under fire.",
                new AcceptableValueRange<float>(0f, 0.5f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 8 }));

            RefreshMeds = Config.Bind("Settings", "Update Meds Frequency", 3f,
                new ConfigDescription("How Often to update a bot's memory of their current meds and stims",
                new AcceptableValueRange<float>(0f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 7 }));

            DebugLogs = Config.Bind("Settings", "Debug Logs", false,
                new ConfigDescription("",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = -999 }));

            try
            {
                new AddComponentPatch().Enable();
                new DisposeComponentPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }
        }

        private void Update()
        {
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
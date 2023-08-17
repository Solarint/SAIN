using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using DrakiaXYZ.VersionChecker;
using SAIN.Components;
using SAIN.Editor;
using SAIN.Helpers;
using SAIN.Layers;
using SAIN.Plugin;
using SAIN.Preset;
using System;
using UnityEngine;
using static SAIN.AssemblyInfo;
using static SAIN.Editor.SAINLayout;

namespace SAIN
{
    public static class AssemblyInfo
    {
        public const string Title = SAINName;
        public const string Description = "Full Revamp of Escape from Tarkov's AI System.";
        public const string Configuration = SPTVersion;
        public const string Company = "";
        public const string Product = SAINName;
        public const string Copyright = "Copyright © 2023 Solarint";
        public const string Trademark = "";
        public const string Culture = "";

        // spt 3.6.0 == 25206
        public const int TarkovVersion = 25206;

        public const string EscapeFromTarkov = "EscapeFromTarkov.exe";

        public const string SAINGUID = "me.sol.sain";
        public const string SAINName = "SAIN";
        public const string SAINVersion = "3.5.3";

        public const string SPTGUID = "com.spt-aki.core";
        public const string SPTVersion = "3.6.0";

        public const string WaypointsGUID = "xyz.drakia.waypoints";
        public const string WaypointsVersion = "1.2.0";

        public const string BigBrainGUID = "xyz.drakia.bigbrain";
        public const string BigBrainVersion = "0.2.0";

        public const string LootingBots = "me.skwizzy.lootingbots";
        public const string Realism = "RealismMod";
    }

    [BepInPlugin(SAINGUID, SAINName, SAINVersion)]
    [BepInDependency(SPTGUID, SPTVersion)]
    [BepInDependency(BigBrainGUID, BigBrainVersion)]
    [BepInDependency(WaypointsGUID, WaypointsVersion)]
    [BepInProcess(EscapeFromTarkov)]
    public class SAINPlugin : BaseUnityPlugin
    {
        public static bool DebugModeEnabled = false;
        public static bool DrawDebugGizmos = false;
        public static SoloDecision ForceSoloDecision = SoloDecision.None;
        public static SquadDecision ForceSquadDecision = SquadDecision.None;
        public static SelfDecision ForceSelfDecision = SelfDecision.None;

        private void Awake()
        {
            if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
            {
                Sounds.PlaySound(EFT.UI.EUISoundType.ErrorMessage);
                throw new Exception("Invalid EFT Version");
            }

            //new DefaultBrainsClass();

            PresetHandler.Init();
            BindConfigs();
            Patches();
            BigBrainHandler.Init();
            Vector.Init();
        }

        private void BindConfigs()
        {
            string category = "SAIN Editor";

            NextDebugOverlay = Config.Bind(category, "Next Debug Overlay", new KeyboardShortcut(KeyCode.LeftBracket), "Change The Debug Overlay with DrakiaXYZs Debug Overlay");
            PreviousDebugOverlay = Config.Bind(category, "Previous Debug Overlay", new KeyboardShortcut(KeyCode.RightBracket), "Change The Debug Overlay with DrakiaXYZs Debug Overlay");

            OpenEditorButton = Config.Bind(category, "Open Editor", false, "Opens the Editor on press");
            OpenEditorConfigEntry = Config.Bind(category, "Open Editor Shortcut", new KeyboardShortcut(KeyCode.F6), "The keyboard shortcut that toggles editor");

            PauseConfigEntry = Config.Bind(category, "Pause Button", new KeyboardShortcut(KeyCode.Pause), "Pause The Game");
        }

        public static ConfigEntry<KeyboardShortcut> NextDebugOverlay { get; private set; }
        public static ConfigEntry<KeyboardShortcut> PreviousDebugOverlay { get; private set; }
        public static ConfigEntry<bool> OpenEditorButton { get; private set; }
        public static ConfigEntry<KeyboardShortcut> OpenEditorConfigEntry { get; private set; }
        public static ConfigEntry<KeyboardShortcut> PauseConfigEntry { get; private set; }

        private void Patches()
        {
            new UpdateEFTSettingsPatch().Enable();

            new Patches.Generic.KickPatch().Enable();
            new Patches.Generic.GetBotController().Enable();
            new Patches.Generic.GetBotSpawnerClass().Enable();
            new Patches.Generic.GrenadeThrownActionPatch().Enable();
            new Patches.Generic.GrenadeExplosionActionPatch().Enable();
            new Patches.Generic.BotGroupAddEnemyPatch().Enable();
            new Patches.Generic.BotMemoryAddEnemyPatch().Enable();

            new Patches.Hearing.TryPlayShootSoundPatch().Enable();
            new Patches.Hearing.HearingSensorPatch().Enable();
            new Patches.Hearing.BetterAudioPatch().Enable();

            new Patches.Talk.PlayerTalkPatch().Enable();
            new Patches.Talk.TalkDisablePatch1().Enable();
            new Patches.Talk.TalkDisablePatch2().Enable();
            new Patches.Talk.TalkDisablePatch3().Enable();
            new Patches.Talk.TalkDisablePatch4().Enable();

            new Patches.Vision.NoAIESPPatch().Enable();
            new Patches.Vision.VisionSpeedPatch().Enable();
            new Patches.Vision.VisibleDistancePatch().Enable();
            new Patches.Vision.CheckFlashlightPatch().Enable();

            new Patches.Shoot.AimTimePatch().Enable();
            new Patches.Shoot.AimOffsetPatch().Enable();
            new Patches.Shoot.RecoilPatch().Enable();
            new Patches.Shoot.LoseRecoilPatch().Enable();
            new Patches.Shoot.EndRecoilPatch().Enable();
            new Patches.Shoot.FullAutoPatch().Enable();
            new Patches.Shoot.SemiAutoPatch().Enable();
        }

        public static SAINPresetClass LoadedPreset => PresetHandler.LoadedPreset;

        public static SAINBotControllerComponent BotController => GameWorldHandler.SAINBotController;

        private void Update()
        {
            DebugGizmos.Update();
            DebugOverlay.Update();
            ModDetection.Update();
            SAINEditor.Update();
            GameWorldHandler.Update();

            LoadedPreset.GlobalSettings.Personality.Update();
        }

        private void Start() => SAINEditor.Init();

        private void LateUpdate() => SAINEditor.LateUpdate();

        private void OnGUI() => SAINEditor.OnGUI();
    }

    public static class ModDetection
    {
        static ModDetection()
        {
            ModsCheckTimer = Time.time + 5f;
        }

        public static void Update()
        {
            if (!ModsChecked && ModsCheckTimer < Time.time && ModsCheckTimer > 0)
            {
                ModsChecked = true;
                CheckPlugins();
            }
        }

        public static bool LootingBotsLoaded { get; private set; }
        public static bool RealismLoaded { get; private set; }

        public static void CheckPlugins()
        {
            if (Chainloader.PluginInfos.ContainsKey(LootingBots))
            {
                LootingBotsLoaded = true;
                Logger.LogInfo($"SAIN: Looting Bots Detected.");
            }
            if (Chainloader.PluginInfos.ContainsKey(Realism))
            {
                RealismLoaded = true;
                Logger.LogInfo($"SAIN: Realism Detected.");
            }
        }

        public static void ModDetectionGUI()
        {
            BeginVertical();

            BeginHorizontal();
            IsDetected(LootingBotsLoaded, "Looting Bots");
            IsDetected(RealismLoaded, "Realism Mod");
            EndHorizontal();

            EndVertical();
        }

        private static void IsDetected(bool value, string name)
        {
            Label(name);
            Box(value ? "Detected" : "Not Detected");
        }

        private static readonly float ModsCheckTimer = -1f;
        private static bool ModsChecked = false;
    }
}
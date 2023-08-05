using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using DrakiaXYZ.VersionChecker;
using SAIN.Components;
using SAIN.Editor;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.SAINPreset;
using System;
using UnityEngine;
using static SAIN.AssemblyInfo;

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
        public const string SAINVersion = "3.5.0";

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
        private void Awake()
        {
            if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
                throw new Exception("Invalid EFT Version");

            BindConfigs();
            PresetHandler.Init();
            Patches();
            BigBrainHandler.Init();
            VectorHelpers.Init();
            SAINEditor = new SAINEditor();
        }

        private void BindConfigs()
        {
            OpenEditorButton = Config.Bind("SAIN Editor", "Open Editor", false, "Opens the Editor on press");
            OpenEditorConfigEntry = Config.Bind("SAIN Editor", "Open Editor Shortcut", new KeyboardShortcut(KeyCode.F6), "The keyboard shortcut that toggles editor");
            PauseConfigEntry = Config.Bind("SAIN Editor", "PauseButton", new KeyboardShortcut(KeyCode.Pause), "Pause The Game");
        }

        private void Patches()
        {
            new Patches.Generic.KickPatch().Enable();
            new Patches.Generic.GetBotController().Enable();
            new Patches.Generic.GetBotSpawnerClass().Enable();
            new Patches.Generic.GrenadeThrownActionPatch().Enable();
            new Patches.Generic.GrenadeExplosionActionPatch().Enable();
            new Patches.Generic.BotGroupAddEnemyPatch().Enable();
            new Patches.Generic.BotMemoryAddEnemyPatch().Enable();

            new Patches.Hearing.InitiateShotPatch().Enable();
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

        public static ConfigEntry<bool> OpenEditorButton { get; private set; }

        public static ConfigEntry<KeyboardShortcut> OpenEditorConfigEntry { get; private set; }

        public static ConfigEntry<KeyboardShortcut> PauseConfigEntry { get; private set; }

        public static SAINEditor SAINEditor { get; private set; }

        public static SAINPresetClass LoadedPreset => PresetHandler.LoadedPreset;

        public static SAINBotController BotController => BotControllerHandler.BotController;


        private void Update()
        {
            ModDetection.Update();
            SAINEditor.Update();
            BotControllerHandler.Update();
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
                Logger.LogInfo($"SAIN: Looting Bots Detected.", typeof(ModDetection));
            }
            if (Chainloader.PluginInfos.ContainsKey(Realism))
            {
                LootingBotsLoaded = true;
                Logger.LogInfo($"SAIN: Realism Detected.", typeof(ModDetection));
            }
        }

        public static void ModDetectionGUI()
        {
            var Builder = SAINPlugin.SAINEditor.Builder;
            var Buttons = SAINPlugin.SAINEditor.Buttons;

            Builder.BeginVertical();
            Builder.BeginHorizontal();
            foreach (var plugin in Chainloader.PluginInfos)
            {
                string name = plugin.Value.Metadata.Name.ToLower();
                if (name.Contains("looting bots"))
                {

                }
            }

            Builder.EndHorizontal();
            Builder.EndVertical();
        }

        private static readonly float ModsCheckTimer = -1f;
        private static bool ModsChecked = false;
    }
}
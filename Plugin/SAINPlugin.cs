using Aki.Reflection.Patching;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using DrakiaXYZ.VersionChecker;
using HarmonyLib;
using SAIN.Components;
using SAIN.Editor;
using SAIN.Helpers;
using SAIN.Layers;
using SAIN.Plugin;
using SAIN.Preset;
using System;
using System.Collections.Generic;
using System.Reflection;
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

        public const int TarkovVersion = 26535;

        public const string EscapeFromTarkov = "EscapeFromTarkov.exe";

        public const string SAINGUID = "me.sol.sain";
        public const string SAINName = "SAIN";
        public const string SAINVersion = "2.1.7";
        public const string SAINPresetVersion = "2.1.6";

        public const string SPTGUID = "com.spt-aki.core";
        public const string SPTVersion = "3.7.4";

        public const string WaypointsGUID = "xyz.drakia.waypoints";
        public const string WaypointsVersion = "1.3.1";

        public const string BigBrainGUID = "xyz.drakia.bigbrain";
        public const string BigBrainVersion = "0.3.1";

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
        public static bool DebugMode => EditorDefaults.GlobalDebugMode;
        public static bool DrawDebugGizmos => EditorDefaults.DrawDebugGizmos;
        public static PresetEditorDefaults EditorDefaults => PresetHandler.EditorDefaults;

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
        }

        public static ConfigEntry<KeyboardShortcut> NextDebugOverlay { get; private set; }
        public static ConfigEntry<KeyboardShortcut> PreviousDebugOverlay { get; private set; }
        public static ConfigEntry<bool> OpenEditorButton { get; private set; }
        public static ConfigEntry<KeyboardShortcut> OpenEditorConfigEntry { get; private set; }

        private void Patches()
        {
            var patches = new List<Type>() {
                typeof(UpdateEFTSettingsPatch),
                typeof(Patches.Generic.KickPatch),
                typeof(Patches.Generic.GetBotController),
                typeof(Patches.Generic.GetBotSpawner),
                typeof(Patches.Generic.GrenadeThrownActionPatch),
                typeof(Patches.Generic.GrenadeExplosionActionPatch),
                typeof(Patches.Generic.BotGroupAddEnemyPatch),
                typeof(Patches.Generic.BotMemoryAddEnemyPatch),
                typeof(Patches.Hearing.TryPlayShootSoundPatch),
                typeof(Patches.Hearing.HearingSensorPatch),
                typeof(Patches.Hearing.BetterAudioPatch),
                typeof(Patches.Talk.PlayerTalkPatch),
                typeof(Patches.Talk.TalkDisablePatch1),
                typeof(Patches.Talk.TalkDisablePatch2),
                typeof(Patches.Talk.TalkDisablePatch3),
                typeof(Patches.Talk.TalkDisablePatch4),
                typeof(Patches.Vision.NoAIESPPatch),
                typeof(Patches.Vision.VisionSpeedPatch),
                typeof(Patches.Vision.VisibleDistancePatch),
                typeof(Patches.Vision.CheckFlashlightPatch),
                typeof(Patches.Shoot.AimTimePatch),
                typeof(Patches.Shoot.AimOffsetPatch),
                typeof(Patches.Shoot.RecoilPatch),
                typeof(Patches.Shoot.LoseRecoilPatch),
                typeof(Patches.Shoot.EndRecoilPatch),
                typeof(Patches.Shoot.FullAutoPatch),
                typeof(Patches.Shoot.SemiAutoPatch),
                typeof(Patches.Components.AddComponentPatch)
            };

            // Reflection go brrrrrrrrrrrrrr
            MethodInfo enableMethod = AccessTools.Method(typeof(ModulePatch), "Enable");
            foreach (var patch in patches)
            {
                if (!typeof(ModulePatch).IsAssignableFrom(patch))
                {
                    Logger.LogError($"Type {patch.Name} is not a ModulePatch");
                    continue;
                }

                try
                {
                    enableMethod.Invoke(Activator.CreateInstance(patch), null);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
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

                // If Realism mod is loaded, we need to adjust how powerlevel is calculated to take into account armor class going up to 10 instead of 6
                // 7 is the default
                EFTCoreSettings.UpdateArmorClassCoef(4f);
            }
            else
            {
                EFTCoreSettings.UpdateArmorClassCoef(7f);
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

using BepInEx;
using BepInEx.Configuration;
using DrakiaXYZ.VersionChecker;
using EFT;
using HarmonyLib;
using SAIN.Editor;
using SAIN.Helpers;
using SAIN.Patches.Generic;
using SAIN.Patches.Movement;
using SAIN.Patches.Shoot.Aim;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static SAIN.AssemblyInfoClass;

// advanced insurance mod
// track insured items, override default spt behavior
// if an item is dropped, track if the position it was dropped at is on the navmesh or not, if its in a bush or not, maybe figure out of way to detect "hiding spots"?
// if an item is dropped in a non-hidden area, track how long it was there before the player left the area, based return percentage off of that.
// if a player is killed, track the gear status of the person who killed them, base return percentage off of that.
// also could track backpack size/space available, maybe make it require looting bots for simpler code?
// if a player dies to landmines, ect, have a 100% return rate
// lower return percentage when killed by scavs or PMCs, very high from bosses, followers, rogues, ect

namespace SAIN
{
    [BepInPlugin(SAINGUID, SAINName, SAINVersion)]
    [BepInDependency(BigBrainGUID, BigBrainVersion)]
    [BepInDependency(SPTGUID, SPTVersion)]
    [BepInProcess(EscapeFromTarkov)]
    [BepInIncompatibility("com.dvize.BushNoESP")]
    [BepInIncompatibility("com.dvize.NoGrenadeESP")]
    public class SAINPlugin : BaseUnityPlugin
    {
        public static DebugSettings DebugSettings => LoadedPreset.GlobalSettings.General.Debug;
        public static bool DebugMode => DebugSettings.Logs.GlobalDebugMode;
        public static bool ProfilingMode => DebugSettings.Logs.GlobalProfilingToggle;
        public static bool DrawDebugGizmos => DebugSettings.Gizmos.DrawDebugGizmos;
        public static PresetEditorDefaults EditorDefaults => PresetHandler.EditorDefaults;

        public static ECombatDecision ForceSoloDecision = ECombatDecision.None;

        public static ESquadDecision ForceSquadDecision = ESquadDecision.None;

        public static ESelfDecision ForceSelfDecision = ESelfDecision.None;

        private void Awake()
        {
            if (!VersionChecker.CheckEftVersion(Logger, Info, Config)) {
                throw new Exception("Invalid EFT Version");
            }

            PresetHandler.Init();
            BindConfigs();
            InitPatches();
            BigBrainHandler.Init();
            Vector.Init();
        }

        private void BindConfigs()
        {
            string category = "SAIN Editor";
            OpenEditorButton = Config.Bind(category, "Open Editor", false, "Opens the Editor on press");
            OpenEditorConfigEntry = Config.Bind(category, "Open Editor Shortcut", new KeyboardShortcut(KeyCode.F6), "The keyboard shortcut that toggles editor");
        }

        public static ConfigEntry<bool> OpenEditorButton { get; private set; }

        public static ConfigEntry<KeyboardShortcut> OpenEditorConfigEntry { get; private set; }

        private List<Type> patches => new List<Type>() {
                typeof(Patches.Generic.StopRefillMagsPatch),
                typeof(Patches.Generic.SetEnvironmentPatch),
                typeof(Patches.Generic.SetPanicPointPatch),
                typeof(Patches.Generic.AddPointToSearchPatch),
                typeof(Patches.Generic.TurnDamnLightOffPatch),
                typeof(Patches.Generic.GrenadeThrownActionPatch),
                typeof(Patches.Generic.GrenadeExplosionActionPatch),
                typeof(Patches.Generic.ShallKnowEnemyPatch),
                typeof(Patches.Generic.ShallKnowEnemyLatePatch),
                typeof(Patches.Generic.HaveSeenEnemyPatch),
                typeof(Patches.Generic.AllowRequestPatch),
                typeof(Patches.Generic.FindRequestForMePatch),

                //typeof(Patches.Generic.Fixes.HealCancelPatch),
                typeof(Patches.Generic.Fixes.StopSetToNavMeshPatch),
                typeof(Patches.Generic.Fixes.FightShallReloadFixPatch),
                typeof(Patches.Generic.Fixes.EnableVaultPatch),
                typeof(Patches.Generic.Fixes.BotMemoryAddEnemyPatch),
                typeof(Patches.Generic.Fixes.BotGroupAddEnemyPatch),
                //typeof(Patches.Generic.Fixes.NoTeleportPatch),
                typeof(Patches.Generic.Fixes.FixItemTakerPatch),
                typeof(Patches.Generic.Fixes.FixItemTakerPatch2),
                //typeof(Patches.Generic.Fixes.FixPatrolDataPatch),
                typeof(Patches.Generic.Fixes.RotateClampPatch),

                typeof(Patches.Movement.EncumberedPatch),
                typeof(Patches.Movement.DoorOpenerPatch),
                typeof(Patches.Movement.DoorDisabledPatch),
                typeof(Patches.Movement.CrawlPatch),
                typeof(Patches.Movement.CrawlPatch2),
                typeof(Patches.Movement.PoseStaminaPatch),
                typeof(Patches.Movement.AimStaminaPatch),
                typeof(Patches.Movement.GlobalShootSettingsPatch),
                typeof(Patches.Movement.GlobalLookPatch),

                typeof(Patches.Hearing.TryPlayShootSoundPatch),
                typeof(Patches.Hearing.OnMakingShotPatch),
                typeof(Patches.Hearing.HearingSensorPatch),

                typeof(Patches.Hearing.VoicePatch),
                typeof(Patches.Hearing.GrenadeCollisionPatch),
                typeof(Patches.Hearing.GrenadeCollisionPatch2),

                typeof(Patches.Hearing.ToggleSoundPatch),
                typeof(Patches.Hearing.SpawnInHandsSoundPatch),
                typeof(Patches.Hearing.PlaySwitchHeadlightSoundPatch),
                typeof(Patches.Hearing.BulletImpactPatch),
                typeof(Patches.Hearing.TreeSoundPatch),
                typeof(Patches.Hearing.DoorBreachSoundPatch),
                typeof(Patches.Hearing.DoorOpenSoundPatch),
                typeof(Patches.Hearing.FootstepSoundPatch),
                typeof(Patches.Hearing.SprintSoundPatch),
                typeof(Patches.Hearing.GenericMovementSoundPatch),
                typeof(Patches.Hearing.JumpSoundPatch),
                typeof(Patches.Hearing.DryShotPatch),
                typeof(Patches.Hearing.ProneSoundPatch),
                typeof(Patches.Hearing.SoundClipNameCheckerPatch),
                typeof(Patches.Hearing.SoundClipNameCheckerPatch2),
                typeof(Patches.Hearing.AimSoundPatch),
                typeof(Patches.Hearing.LootingSoundPatch),
                typeof(Patches.Hearing.SetInHandsGrenadePatch),
                typeof(Patches.Hearing.SetInHandsFoodPatch),
                typeof(Patches.Hearing.SetInHandsMedsPatch),

                typeof(Patches.Talk.JumpPainPatch),
                typeof(Patches.Talk.PlayerHurtPatch),
                typeof(Patches.Talk.PlayerTalkPatch),
                typeof(Patches.Talk.BotTalkPatch),
                typeof(Patches.Talk.BotTalkManualUpdatePatch),

                typeof(Patches.Vision.DisableLookUpdatePatch),
                typeof(Patches.Vision.UpdateLightEnablePatch),
                typeof(Patches.Vision.UpdateLightEnablePatch2),
                typeof(Patches.Vision.ToggleNightVisionPatch),
                typeof(Patches.Vision.SetPartPriorityPatch),
                typeof(Patches.Vision.GlobalLookSettingsPatch),
                typeof(Patches.Vision.WeatherTimeVisibleDistancePatch),
                typeof(Patches.Vision.NoAIESPPatch),
                typeof(Patches.Vision.BotLightTurnOnPatch),
                typeof(Patches.Vision.VisionSpeedPatch),
                typeof(Patches.Vision.VisionDistancePatch),
                typeof(Patches.Vision.CheckFlashlightPatch),

                typeof(Patches.Shoot.Aim.DoHitAffectPatch),
                typeof(Patches.Shoot.Aim.HitAffectApplyPatch),
                typeof(Patches.Shoot.Aim.PlayerHitReactionDisablePatch),

                typeof(Patches.Shoot.Aim.SetAimStatusPatch),
                typeof(Patches.Shoot.Aim.AimOffsetPatch),
                typeof(Patches.Shoot.Aim.AimTimePatch),
                //typeof(Patches.Shoot.Aim.WeaponPresetPatch),
                typeof(Patches.Shoot.Aim.ForceNoHeadAimPatch),
                typeof(Patches.Shoot.Aim.AimRotateSpeedPatch),

                //typeof(Patches.Shoot.Grenades.DoThrowPatch),
                //typeof(Patches.Shoot.Grenades.DisableSpreadPatch),
                typeof(Patches.Shoot.Grenades.ResetGrenadePatch),
                typeof(Patches.Shoot.Grenades.SetGrenadePatch),

                typeof(Patches.Shoot.RateOfFire.FullAutoPatch),
                typeof(Patches.Shoot.RateOfFire.SemiAutoPatch),
                typeof(Patches.Shoot.RateOfFire.SemiAutoPatch2),
                typeof(Patches.Shoot.RateOfFire.SemiAutoPatch3),

                typeof(Patches.Components.AddBotComponentPatch),
                typeof(Patches.Components.AddGameWorldPatch),
                //typeof(Patches.Components.AddLightComponentPatch),
                //typeof(Patches.Components.AddLightComponentPatch2),
                typeof(Patches.Components.GetBotController),
                typeof(Patches.Components.GetBotSpawner),
            };

        private void InitPatches()
        {
            // Reflection go brrrrrrrrrrrrrr
            MethodInfo enableMethod = AccessTools.Method(typeof(ModulePatch), "Enable");
            foreach (var patch in patches) {
                if (!typeof(ModulePatch).IsAssignableFrom(patch)) {
                    Logger.LogError($"Type {patch.Name} is not a ModulePatch");
                    continue;
                }

                try {
                    enableMethod.Invoke(Activator.CreateInstance(patch), null);
                }
                catch (Exception ex) {
                    Logger.LogError(ex);
                }
            }
        }

        public static SAINPresetClass LoadedPreset => PresetHandler.LoadedPreset;

        private void Update()
        {
            ModDetection.Update();
            SAINEditor.Update();
        }

        private void Start() => SAINEditor.Init();

        private void LateUpdate() => SAINEditor.LateUpdate();

        private void OnGUI() => SAINEditor.OnGUI();

        public static bool IsBotExluded(BotOwner botOwner) => SAINEnableClass.IsSAINDisabledForBot(botOwner);
    }
}

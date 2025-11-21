global using EFTMath = GClass856;

using BepInEx;
using BepInEx.Configuration;
using EFT;
using SAIN.Editor;
using SAIN.Helpers;
using SAIN.Patches.Components;
using SAIN.Patches.Hearing;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.AssemblyInfoClass;

namespace SAIN;

[BepInPlugin(SAINGUID, SAINName, SAINVersion)]
[BepInDependency(BigBrainGUID, BigBrainVersion)]
//[BepInDependency(SPTGUID, SPTVersion)]
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

    public static ESelfActionType ForceSelfDecision = ESelfActionType.None;

    public void Awake()
    {
        /*
        if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
        {
            throw new Exception("Invalid EFT Version");
        }
        */

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

    private List<ModulePatch> SainPatches => [

        new WorldTickPatch(),
        new PlayerLateUpdatePatch(),
        new AddBotComponentPatch(),
        new ActivateBotComponentPatch(),
        new AddGameWorldPatch(),
        new GetBotController(),

        new Patches.Generic.BotOwnerActivatePatch(),
        new Patches.Generic.StopRequestExecutePatch(),
        new Patches.Generic.SetEnvironmentPatch(),
        new Patches.Generic.SetPanicPointPatch(),
        new Patches.Generic.AddPointToSearchPatch(),
        new Patches.Generic.TurnDamnLightOffPatch(),
        new Patches.Generic.GrenadeThrownActionPatch(),
        new Patches.Generic.GrenadeExplosionActionPatch(),

        new Patches.Generic.ShallKnowEnemyPatch(),
        new Patches.Generic.ShallKnowEnemyLatePatch(),
        new Patches.Generic.HaveSeenEnemyPatch(),

        new Patches.Generic.BlockVoiceRequestsPatch(),
        new Patches.Generic.BlockGrenadeThrowRequestsPatch(),
        //new Patches.Generic.BlockGrenadeThrowRequestsPatch2(),
        new Patches.Generic.BlockRequestPatch(),

        new Patches.Generic.SetInHands.SetInHands_Empty(),
        new Patches.Generic.SetInHands.SetInHands_Food_Patch(),
        new Patches.Generic.SetInHands.SetInHands_Grenade_Patch(),
        new Patches.Generic.SetInHands.SetInHands_Knife_Patch(),
        new Patches.Generic.SetInHands.SetInHands_Meds_Patch1(),
        new Patches.Generic.SetInHands.SetInHands_Meds_Patch2(),
        new Patches.Generic.SetInHands.SetInHands_QuickUse_Patch1(),
        new Patches.Generic.SetInHands.SetInHands_QuickUse_Patch2(),
        new Patches.Generic.SetInHands.SetInHands_Weapon_Patch(),
        new Patches.Generic.SetInHands.SetInHands_Weapon_Stationary_Patch(),

        new Patches.Generic.Fixes.StopSetToNavMeshPatch(),
        new Patches.Generic.Fixes.StopSetToNavMeshPatch2(),
        new Patches.Generic.Fixes.FightShallReloadFixPatch(),
        new Patches.Generic.Fixes.EnableVaultPatch(),
        new Patches.Generic.Fixes.BotMemoryAddEnemyPatch(),
        new Patches.Generic.Fixes.BotGroupAddEnemyPatch(),
        new Patches.Generic.Fixes.FixItemTakerPatch(),
        new Patches.Generic.Fixes.FixItemTakerPatch2(),
        new Patches.Generic.Fixes.RotateClampPatch(),
        new Patches.Generic.Fixes.RunToEnemyUpdatePatch(),
        new Patches.Generic.Fixes.DisableGrenadesPatch(),

        new Patches.Movement.EncumberedPatch(),
        new Patches.Movement.CrawlPatch(),
        new Patches.Movement.StopShootCauseAnimatorPatch(),
        new Patches.Movement.PoseStaminaPatch(),
        new Patches.Movement.AimStaminaPatch(),
        new Patches.Movement.GlobalShootSettingsPatch(),
        new Patches.Movement.MovementContextIsAIPatch(),
        new Patches.Movement.CanBeSnappedPatch(),
        new Patches.Movement.BotMoverManualUpdatePatch(),
        new Patches.Movement.BotMoverManualFixedUpdatePatch(),
        new Patches.Movement.SprintLookDirPatch(),
        new Patches.Movement.PlayerSetPosePatch(),

        new TryPlayShootSoundPatch(),
        new OnMakingShotPatch(),
        new RegisterShotPatch(),
        new OnWeaponModifiedPatch(),
        new HearingSensorPatch(),

        new GrenadeCollisionPatch(),
        new GrenadeCollisionPatch2(),

        new ToggleSoundPatch(),
        new SpawnInHandsSoundPatch(),
        new PlaySwitchHeadlightSoundPatch(),
        new BulletImpactPatch(),
        new TreeSoundPatch(),
        new DoorBreachSoundPatch(),
        new DoorOpenSoundPatch(),
        new FootstepSoundPatch(),
        new GenericMovementSoundPatch(),
        new SpecificStepAudioControllerPatch(),
        new DryShotPatch(),
        new ProneSoundPatch(),
        new SoundClipNameCheckerPatch(),
        new SoundClipNameCheckerPatch2(),
        new AimSoundPatch(),
        new LootingSoundPatch(),

        new Patches.Talk.PlayerHurtPatch(),
        new Patches.Talk.PlayerTalkPatch(),
        new Patches.Talk.BotTalkPatch(),
        new Patches.Talk.BotTalkManualUpdatePatch(),

        new Patches.Vision.DisableLookUpdatePatch(),
        new Patches.Vision.UpdateLightEnablePatch(),
        new Patches.Vision.UpdateLightEnablePatch2(),
        new Patches.Vision.ToggleNightVisionPatch(),
        //new Patches.Vision.SetPartPriorityPatch(),
        new Patches.Vision.GlobalLookSettingsPatch(),
        //new Patches.Vision.WeatherTimeVisibleDistancePatch(),
        new Patches.Vision.NoAIESPPatch(),
        new Patches.Vision.BotLightTurnOnPatch(),
        new Patches.Vision.VisionSpeedPatch(),
        new Patches.Vision.WeatherVisionPatch(),
        new Patches.Vision.IsPointInVisibleSectorPatch(),
        new Patches.Vision.VisionDistancePatch(),
        new Patches.Vision.CheckFlashlightPatch(),

        new Patches.Shoot.Aim.DoHitAffectPatch(),
        new Patches.Shoot.Aim.HitAffectApplyPatch(),
        new Patches.Shoot.Aim.PlayerHitReactionDisablePatch(),
        //new Patches.Shoot.Aim.WeaponMoAModificationPatch(),
        new Patches.Shoot.Aim.BotAimSteerPatch(),
        new Patches.Shoot.Aim.HardAimDisablePatch1(),
        new Patches.Shoot.Aim.HardAimDisablePatch2(),

        new Patches.Shoot.RateOfFire.BotShootPatch(),
        new Patches.Shoot.Aim.AimOffsetPatch(),
        new Patches.Shoot.Aim.AimTimePatch(),
        new Patches.Shoot.Aim.ForceNoHeadAimPatch(),
        new Patches.Shoot.Aim.SmoothTurnPatch(),
        new Patches.Shoot.Aim.BotSteeringPitchLimitPatch(),

        new Patches.Shoot.Grenades.ResetGrenadePatch(),
        new Patches.Shoot.Grenades.SetGrenadePatch(),
    ];

    private void InitPatches()
    {
        foreach (var patch in SainPatches)
        {
            patch.Enable();
        }
    }

    public static SAINPresetClass LoadedPreset => PresetHandler.LoadedPreset;

    public void Update()
    {
        ModDetection.ManualUpdate();
        SAINEditor.ManualUpdate();
        DebugGizmos.ManualUpdate();
    }

    public void Start() => SAINEditor.Init();

    public void LateUpdate() => SAINEditor.LateUpdate();

    public void OnGUI() => SAINEditor.OnGUI();
}
using CommonAssets.Scripts.Audio;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Components;
using SAIN.Components.Helpers;
using SAIN.Components.PlayerComponentSpace;
using SPT.Reflection.Patching;
using System.Reflection;
using Systems.Effects;
using UnityEngine;
using StepAudioController = LocalPlayerStepAudioControllerClass;

// this._specificStepAudioController = new GClass1184(surfaceSet, this, 0.1f, useOcclusion);

namespace SAIN.Patches.Hearing;

public class GrenadeCollisionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Grenade), nameof(Grenade.OnCollisionHandler));
    }

    [PatchPostfix]
    public static void Patch(Grenade __instance, SoundBank ___soundBank_0)
    {
        ___soundBank_0.Rolloff = _defaultRolloff * ROLLOFF_MULTI;
        //Logger.LogDebug($"Rolloff {_defaultRolloff} after {___soundBank_0.Rolloff}");
        BotManagerComponent.Instance?.GrenadeController.GrenadeCollided(__instance, 35);
    }

    private static float _defaultRolloff = 40;
    private const float ROLLOFF_MULTI = 1.5f;
}

public class GrenadeCollisionPatch2 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Throwable), nameof(Throwable.OnCollisionHandler));
    }

    [PatchPostfix]
    public static void Patch(ref float ___IgnoreCollisionTrackingTimer, Throwable __instance)
    {
        ___IgnoreCollisionTrackingTimer = Time.time + 0.35f;
    }
}

public class TreeSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TreeInteractive), nameof(TreeInteractive.method_0));
    }

    [PatchPostfix]
    public static void Patch(Vector3 soundPosition, BetterSource source, IPlayerOwner player, SoundBank ____soundBank)
    {
        if (player.iPlayer != null)
        {
            float baseRange = 50f;
            if (____soundBank != null)
            {
                baseRange = ____soundBank.Rolloff * player.SoundRadius * 0.8f;
            }
            //Logger.LogDebug($"Playing Bush Sound Range: {baseRange}");
            BotManagerComponent.Instance?.BotHearing.PlayAISound(player.iPlayer.ProfileId, SAINSoundType.Bush, soundPosition, baseRange, 1f);
        }
    }
}

public class DoorOpenSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.StartInteraction));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player ____player)
    {
        float baseRange = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.DOOR_OPEN_SOUND_RANGE;
        BotManagerComponent.Instance?.BotHearing.PlayAISound(____player.ProfileId, SAINSoundType.Door, ____player.Position, baseRange, 1f);
    }
}

public class DoorBreachSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.PlayBreachSound));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player ____player)
    {
        float baseRange = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.DOOR_KICK_SOUND_RANGE;
        BotManagerComponent.Instance?.BotHearing.PlayAISound(____player.ProfileId, SAINSoundType.Door, ____player.Position, baseRange, 1f);
    }
}

public class JumpSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.method_2));
    }

    [PatchPrefix]
    public static bool PatchPrefix(Player ____player, ref float ____nextJumpNoise)
    {
        //if (____player.AIData == null) {
        //    return false;
        //}
        //if (____player.AIData.IsAI && ____player.AIData.BotOwner.BotState != EBotState.Active) {
        //    return false;
        //}
        //if (Time.time > ____nextJumpNoise) {
        //    ____nextJumpNoise = Time.time + SAINPlugin.LoadedPreset.GlobalSettings.Hearing.JUMP_SOUND_INTERVAL;
        //    float baseRange = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.JUMP_SOUND_RANGE;
        //    SAINBotController.Instance?.BotHearing.PlayAISound(____player.ProfileId, SAINSoundType.Jump, ____player.Position, baseRange, 1f);
        //}
        return false;
    }
}

public class FootstepSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.PlayStepSound));
    }

    [PatchPostfix]
    public static void Patch(Player __instance, BetterSource ___NestedStepSoundSource, SurfaceSet ____currentSet)
    {
        ///// Most Copypasted from original function to replicate audio ranges that players experience. This could change in the future, so this function should be checked to make sure the code hasn't changed
        //SoundBank soundBank = (__instance.Pose == EPlayerPose.Duck) ? ____currentSet.DuckSoundBank : ____currentSet.RunSoundBank;
        //EAudioMovementState movementState = (__instance.Pose == EPlayerPose.Duck) ? EAudioMovementState.Duck : EAudioMovementState.Run;
        //float covertMovementVolumeBySpeed = __instance.MovementContext.CovertMovementVolumeBySpeed;
        //float num2 = __instance.method_55();
        //float num3 = __instance.method_62(movementState);
        //float num4 = (__instance.FirstPersonPointOfView || __instance.method_78()) ? soundBank.RandomVolume : 1f;
        //float num5 = covertMovementVolumeBySpeed * num2 * num3 * num4;
        //// End of copypaste
        //
        //float range = ___NestedStepSoundSource.MaxDistance;
        //float volume = num5;
        //SAINBotController.Instance?.BotHearing.PlayAISound(__instance.ProfileId, SAINSoundType.FootStep, __instance.Position, range, volume);
    }
}

public class FikaHeadlessTempFixPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.method_1));
    }

    [PatchPrefix]
    public static bool PatchPrefix(Player ____player, Vector3 motion, MovementContext __instance)
    {
        // TEMPORARY SOLUTION TO HEADLESS HAVING NO BOT HEARING OF FOOTSTEPS
        if (ModDetection.ProjectFikaHeadlessLoaded)
        {
            if (____player.AIData == null)
            {
                return false;
            }
            if (____player.AIData.IsAI && ____player.AIData.BotOwner.BotState != EBotState.Active)
            {
                return false;
            }
            if (Time.time > __instance.NextStepNoise)
            {
                if (motion.y < 0.2f && motion.y > -0.2f)
                {
                    motion.y = 0f;
                }
                __instance.NextStepNoise = Time.time + LocalBotSettingsProviderClass.Core.STEP_NOISE_DELTA;
                float num = ____player.Speed;
                if (____player.IsSprintEnabled)
                {
                    num = 2f;
                }
                float num2 = Mathf.Clamp(0.5f * ____player.PoseLevel + 0.5f, 0f, 1f);
                num *= num2;
                if (motion.sqrMagnitude < 1E-06f)
                {
                    return false;
                }
                float num3 = ____player.IsSprintEnabled ? 1f : __instance.CovertMovementVolumeBySpeed;
                float power = LocalBotSettingsProviderClass.Core.BASE_WALK_SPEREAD2 * (num3 + num) / 2f;
                BotManagerComponent.Instance?.BotHearing.PlayAISound(____player.ProfileId, SAINSoundType.FootStep, ____player.Position, power, 1f);
            }
        }
        return false;
    }
}

public class GenericMovementSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.DefaultPlay));
    }

    [PatchPostfix]
    public static void Patch(Player __instance, SoundBank bank, float volume, EAudioMovementState movementState)
    {
        //SAINSoundType soundType;
        //switch (movementState) {
        //    case EAudioMovementState.Run:
        //        soundType = SAINSoundType.FootStep;
        //        break;
        //
        //    case EAudioMovementState.Sprint:
        //        soundType = SAINSoundType.Sprint;
        //        break;
        //
        //    case EAudioMovementState.Land:
        //        soundType = SAINSoundType.Land;
        //        break;
        //
        //    case EAudioMovementState.Turn:
        //        soundType = SAINSoundType.TurnSound;
        //        break;
        //
        //    default:
        //        soundType = SAINSoundType.Generic;
        //        return;
        //}
        //
        //SAINBotController.Instance?.BotHearing.PlayAISound(__instance.ProfileId, soundType, __instance.Position, bank.Rolloff, volume);
    }
}

public class SpecificStepAudioControllerPatch : ModulePatch
{
    protected static readonly FieldInfo NestedStepSoundSourceField = AccessTools.Field(typeof(Player), "NestedStepSoundSource");

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(StepAudioController), nameof(StepAudioController.Play));
        // Taken From: this._specificStepAudioController = new GClass1117(surfaceSet, this, 0.1f, useOcclusion);
    }

    [PatchPrefix]
    public static bool Patch(StepAudioController __instance, EAudioMovementState movementState, EnvironmentType environment, float distance, float baseStepVolume, float blendParameter, bool stereo)
    {
        // TEMPORARY SOLUTION TO HEADLESS HAVING NO BOT HEARING OF FOOTSTEPS
        if (ModDetection.ProjectFikaHeadlessLoaded)
        {
            return true;
        }
        // COPIED FROM ORIGINAL FUNCTION
        if (movementState == EAudioMovementState.None)
        {
            return false;
        }
        float volume = baseStepVolume;
        bool IsUnderRoof = __instance.Bool_0;
        if (!IsUnderRoof && environment != EnvironmentType.Indoor && movementState != EAudioMovementState.None)
        {
            if (__instance.method_4(movementState, out SoundBank soundBank))
            {
                volume = __instance.CalculateFinalVolume(baseStepVolume, soundBank);
                soundBank.Play(__instance.BetterSource_0, EnvironmentType.Outdoor, distance, volume, blendParameter, stereo, true);
            }
            else
            {
                Debug.LogError(string.Format("Can't find bank for movement state: {0}", movementState));
            }
        }
        // END OF COPIED FUNCTION

        SAINSoundType soundType = movementState switch {
            EAudioMovementState.Run => SAINSoundType.FootStep,
            EAudioMovementState.Sprint => SAINSoundType.Sprint,
            EAudioMovementState.Land => SAINSoundType.Land,
            EAudioMovementState.Turn => SAINSoundType.TurnSound,
            EAudioMovementState.Drop => SAINSoundType.Land,
            _ => SAINSoundType.Generic,
        };

        if (__instance.Iplayer_0 is Player player)
        {
            if (NestedStepSoundSourceField != null)
            {
                object StepSourceObj = NestedStepSoundSourceField.GetValue(player);
                if (StepSourceObj != null)
                {
                    if (StepSourceObj is BetterSource NestedStepSoundSource)
                    {
                        BotManagerComponent.Instance?.BotHearing.PlayAISound(__instance.Iplayer_0.ProfileId, soundType, __instance.Iplayer_0.Position, NestedStepSoundSource.MaxDistance, volume);
                        //if (player.IsYourPlayer)
                        //    Logger.LogDebug($"SpecificStepAudioControllerPatch:: Played Sound [ Player: {___iplayer_0.Profile.Nickname}, MovementState: {movementState}, Environment: {environment}, Range: {NestedStepSoundSource.MaxDistance}, Volume: {volume}]");
                    }
                    else
                    {
                        Logger.LogError("StepSourceObj is not BetterSource NestedStepSoundSource");
                    }
                }
                else
                {
                    Logger.LogError("StepSourceObj is null");
                }
            }
            else
            {
                Logger.LogError("NestedStepSoundSourceField is null");
            }
        }
        else
        {
            Logger.LogError("___iplayer_0 is not Player player");
        }
        return false;
    }
}

public class DryShotPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.DryShot));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player ____player)
    {
        float baseRange = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_DryFire;
        BotManagerComponent.Instance?.BotHearing.PlayAISound(____player.ProfileId, SAINSoundType.DryFire, ____player.WeaponRoot.position, baseRange, 1f);
    }
}

public class HearingSensorPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotHearingSensor), nameof(BotHearingSensor.Init));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotHearingSensor __instance)
    {
        if (SAINEnableClass.IsSAINDisabledForBot(__instance.BotOwner))
        {
            return false;
        }
        return true;
    }
}

public class TryPlayShootSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PlayerAIDataClass), nameof(PlayerAIDataClass.TryPlayShootSound));
    }

    [PatchPrefix]
    public static bool PatchPrefix(PlayerAIDataClass __instance)
    {
        __instance.Boolean_0 = true;
        return false;
    }
}

public class OnMakingShotPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.OnMakingShot));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player __instance, IWeapon weapon, Vector3 force)
    {
        if (GameWorldComponent.TryGetPlayerComponent(__instance, out PlayerComponent PlayerComponent))
        {
            PlayerComponent.OnMakingShot(weapon, force);
        }
    }
}

public class RegisterShotPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.RegisterShot));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player ____player, Item weapon, EftBulletClass shot)
    {
        GameWorldComponent.Instance?.RegisterShot(____player, shot, weapon);
    }
}

public class OnWeaponModifiedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.WeaponModified));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player.FirearmController __instance, Player ____player)
    {
        if (GameWorldComponent.TryGetPlayerComponent(____player, out PlayerComponent PlayerComponent))
        {
            PlayerComponent.Equipment.WeaponModified(__instance.Weapon);
        }
    }
}

public class SoundClipNameCheckerPatch : ModulePatch
{
    private static MethodInfo _Player;
    private static FieldInfo _PlayerBridge;

    protected override MethodBase GetTargetMethod()
    {
        _PlayerBridge = AccessTools.Field(typeof(BaseSoundPlayer), "playersBridge");
        _Player = AccessTools.PropertyGetter(_PlayerBridge.FieldType, "iPlayer");
        return AccessTools.Method(typeof(BaseSoundPlayer), nameof(BaseSoundPlayer.SoundEventHandler));
    }

    [PatchPrefix]
    public static void PatchPrefix(string soundName, BaseSoundPlayer __instance)
    {
        if (BotManagerComponent.Instance != null)
        {
            object playerBridge = _PlayerBridge.GetValue(__instance);
            Player player = _Player.Invoke(playerBridge, null) as Player;
            SAINSoundTypeHandler.AISoundFileChecker(soundName, player);
        }
    }
}

public class SoundClipNameCheckerPatch2 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BaseSoundPlayer), nameof(BaseSoundPlayer.SoundAtPointEventHandler));
    }

    [PatchPrefix]
    public static void PatchPrefix(string soundName, BaseSoundPlayer __instance)
    {
        if (soundName == FUSE)
        {
            BaseSoundPlayer.SoundElement soundElement = __instance.AdditionalSounds.Find((BaseSoundPlayer.SoundElement elem) => elem.EventName == FUSE || elem.EventName == "Snd" + FUSE);
            if (soundElement != null)
            {
                soundElement.RollOff = 60;
                soundElement.Volume = 1;
            }
        }
    }

    private const string FUSE = "SndFuse";
}

public class ToggleSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.PlayToggleSound));
    }

    [PatchPostfix]
    public static void PatchPostfix(Player __instance, bool previousState, bool isOn, Vector3 ___SpeechLocalPosition)
    {
        if (previousState != isOn)
        {
            float baseRange = 10f;
            BotManagerComponent.Instance?.BotHearing.PlayAISound(__instance.ProfileId, SAINSoundType.GearSound, __instance.Position + ___SpeechLocalPosition, baseRange, 1f);
        }
    }
}

public class SpawnInHandsSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.SpawnInHands));
    }

    [PatchPostfix]
    public static void PatchPostfix(Player __instance, Item item)
    {
        //AudioClip itemClip = Singleton<GUISounds>.Instance.GetItemClip(item.ItemSound, EInventorySoundType.pickup);
        //if (itemClip != null) {
        //    SAINBotController.Instance?.BotHearing.PlayAISound(__instance.ProfileId, SAINSoundType.GearSound, __instance.Position, 30f, 1f);
        //}
    }
}

public class PlaySwitchHeadlightSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.PlayTacticalSound));
    }

    [PatchPostfix]
    public static void PatchPostfix(Player __instance, Vector3 ___SpeechLocalPosition)
    {
        BotManagerComponent.Instance?.BotHearing.PlayAISound(__instance.ProfileId, SAINSoundType.GearSound, __instance.Position + ___SpeechLocalPosition, 10f, 1f);
    }
}

public class LootingSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.method_46));
    }

    [PatchPostfix]
    public static void PatchPostfix(Player __instance, BetterSource ____searchSource)
    {
        if (____searchSource == null)
        {
            return;
        }
        float baseRange = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Looting;
        BotManagerComponent.Instance?.BotHearing.PlayAISound(__instance.ProfileId, SAINSoundType.Looting, __instance.Position, baseRange, 1f);
    }
}

public class ProneSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.PlaySoundBank));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player __instance, ref string soundBank, float ____runSurfaceCheck)
    {
        if (soundBank == "Prone"
            && __instance.SinceLastStep >= 0.5f
            && __instance.CheckSurface(____runSurfaceCheck))
        {
            float range = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Prone;
            BotManagerComponent.Instance?.BotHearing.PlayAISound(__instance.ProfileId, SAINSoundType.Prone, __instance.Position, range, 1f);
        }
    }
}

public class AimSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.method_60));
    }

    [PatchPrefix]
    public static void PatchPrefix(float volume, Player __instance)
    {
        float baseRange = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_AimingandGearRattle;
        BotManagerComponent.Instance?.BotHearing.PlayAISound(__instance.ProfileId, SAINSoundType.GearSound, __instance.Position, baseRange, volume);
    }
}

public class BulletImpactPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EffectsCommutator), nameof(EffectsCommutator.PlayHitEffect));
    }

    [PatchPostfix]
    public static void PatchPostfix(EftBulletClass info)
    {
        if (BotManagerComponent.Instance != null)
        {
            BotManagerComponent.Instance.BotHearing.BulletImpacted(info);
        }
    }
}
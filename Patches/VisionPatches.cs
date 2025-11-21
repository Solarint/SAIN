using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SAIN.Patches.Vision;

public class UpdateLightEnablePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotLight), nameof(BotLight.UpdateLightEnable));
    }

    [PatchPrefix]
    public static bool PatchPrefix(
        float curLightDist,
        ref float __result,
        BotLight __instance)
    {
        __result = curLightDist;
        if (__instance.BotOwner_0.FlashGrenade.IsFlashed)
        {
            return false;
        }
        if (!__instance.HaveLight)
        {
            return false;
        }
        __instance.CurLightDist = curLightDist;

        float timeModifier = BotManagerComponent.Instance.TimeVision.TimeVisionDistanceModifier;
        var lookSettings = GlobalSettingsClass.Instance.Look.Light;
        float turnOnRatio = lookSettings.LightOnRatio;
        float turnOffRatio = lookSettings.LightOffRatio;

        bool isOn = __instance.IsEnable;
        bool wantOn = !isOn && timeModifier <= turnOnRatio && __instance.BotOwner_0.Memory.IsPeace;
        bool wantOff = isOn && timeModifier >= turnOffRatio;
        __instance.CanUseNow = timeModifier < turnOffRatio;

        if (wantOn)
        {
            try
            {
                __instance.TurnOn(true);
            }
            catch { }
        }
        if (wantOff)
        {
            try
            {
                __instance.TurnOff(true, true);
            }
            catch { }
#if DEBUG
            try
            {
                __instance.TurnOff(true, true);
            }
            catch (Exception e)
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogError(e);
                }
            }
#endif

            if (__instance.IsEnable)
            {
                var gameworld = GameWorldComponent.Instance;
                if (gameworld == null)
                {
#if DEBUG
                    Logger.LogError($"GameWorldComponent is null, cannot check if bot has flashlight on!");
#endif
                    return false;
                }
                PlayerComponent playerComponent = gameworld.PlayerTracker.GetPlayerComponent(__instance.BotOwner_0.ProfileId);
                if (playerComponent == null)
                {
#if DEBUG
                    Logger.LogError($"Player Component is null, cannot check if bot has flashlight on!");
#endif
                    return false;
                }
                if (playerComponent.Flashlight.WhiteLight ||
                    (__instance.BotOwner_0.NightVision.UsingNow && playerComponent.Flashlight.IRLight))
                {
                    float min = __instance.BotOwner_0.Settings.FileSettings.Look.VISIBLE_DISNACE_WITH_LIGHT;
                    __result = Mathf.Clamp(curLightDist, min, float.MaxValue);
                }
            }
        }
        return false;
    }
}

public class UpdateLightEnablePatch2 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotLight), nameof(BotLight.method_0));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotLight __instance)
    {
        if (!__instance.IsEnable)
        {
            return false;
        }
        float timeModifier = BotManagerComponent.Instance.TimeVision.TimeVisionDistanceModifier;
        float turnOffRatio = GlobalSettingsClass.Instance.Look.Light.LightOffRatio;
        bool wantOff = timeModifier >= turnOffRatio;
        if (wantOff)
        {
            __instance.TurnOff(true, true);
        }
        return false;
    }
}

public class ToggleNightVisionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotNightVisionData), nameof(BotNightVisionData.method_0));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotNightVisionData __instance)
    {
        if (__instance.BotOwner_0.FlashGrenade.IsFlashed)
        {
            return false;
        }

        float timeModifier = BotManagerComponent.Instance.TimeVision.TimeVisionDistanceModifier;
        var lookSettings = GlobalSettingsClass.Instance.Look.Light;
        float turnOnRatio = lookSettings.NightVisionOnRatio;
        float turnOffRatio = lookSettings.NightVisionOffRatio;

        if (__instance.NightVisionAtPocket)
        {
            if (timeModifier < turnOnRatio)
            {
                __instance.method_4();
                return false;
            }
        }
        else
        {
            if (timeModifier < turnOnRatio)
            {
                __instance.method_5();
            }
            if (timeModifier >= turnOffRatio)
            {
                __instance.method_1();
            }
        }
        return false;
    }
}

public class SetPartPriorityPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EnemyInfo), nameof(EnemyInfo.method_5));
    }

    [PatchPrefix]
    public static bool PatchPrefix(EnemyInfo __instance)
    {
        bool isAI = __instance.Person?.IsAI == true;

        if (isAI)
        {
            if (!__instance.HaveSeenPersonal || Time.time - __instance.TimeLastSeenReal > 5f)
            {
                __instance.ActiveParts = __instance.Maxparts;
            }
            else
            {
                __instance.ActiveParts = __instance.MiddleParts;
            }
            return false;
        }

        if (!isAI &&
            SAINEnableClass.GetSAIN(__instance.Owner.ProfileId, out BotComponent botComponent))
        {
            Enemy enemy = botComponent.EnemyController.CheckAddEnemy(__instance.Person);
            if (enemy != null)
            {
                if (enemy.IsCurrentEnemy)
                {
                    __instance.ActiveParts = __instance.Maxparts;
                    return false;
                }
                if (enemy.Status.ShotAtMeRecently ||
                    enemy.Status.PositionalFlareEnabled)
                {
                    __instance.ActiveParts = __instance.Maxparts;
                    return false;
                }
            }
        }

        return true;
    }
}

/// <summary>
/// Disable the ai task registration of SAIN bots for vision updates.
/// </summary>
public class DisableLookUpdatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LookSensor), nameof(LookSensor.Activate));
    }

    [PatchPrefix]
    public static bool Patch(LookSensor __instance)
    {
        if (SAINEnableClass.IsBotExcluded(__instance.BotOwner)) return true;
        __instance.method_2();
        return false;
    }
}

public class GlobalLookSettingsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotGlobalLookData), nameof(BotGlobalLookData.Update));
    }

    [PatchPostfix]
    public static void Patch(BotGlobalLookData __instance)
    {
        __instance.CHECK_HEAD_ANY_DIST = true;
        __instance.MIDDLE_DIST_CAN_SHOOT_HEAD = true;
        __instance.SHOOT_FROM_EYES = false;
    }
}

public class NoAIESPPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BotOwner).GetMethod(nameof(BotOwner.IsEnemyLookingAtMe), BindingFlags.Instance | BindingFlags.Public, null, [typeof(IPlayer)], null);
    }

    [PatchPrefix]
    public static bool PatchPrefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}

public class BotLightTurnOnPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotLight), nameof(BotLight.TurnOn));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotLight __instance)
    {
        if (__instance.IsInDarkPlace_1 && !SAINPlugin.LoadedPreset.GlobalSettings.General.Flashlight.AllowLightOnForDarkBuildings)
        {
            __instance.IsInDarkPlace_1 = false;
        }
        if (__instance.IsInDarkPlace_1 || __instance.BotOwner_0.Memory.GoalEnemy != null)
        {
            return true;
        }
        if (!ShallTurnLightOff(__instance.BotOwner_0.Profile.Info.Settings.Role))
        {
            return true;
        }
        __instance.BotOwner_0.BotLight.TurnOff(false, true);
        return false;
    }

    private static bool ShallTurnLightOff(WildSpawnType wildSpawnType)
    {
        FlashlightSettings settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Flashlight;
        if (EnumValues.WildSpawn.IsScav(wildSpawnType))
        {
            return settings.TurnLightOffNoEnemySCAV;
        }
        if (wildSpawnType.IsPmcBot())
        {
            return settings.TurnLightOffNoEnemyPMC;
        }
        if (EnumValues.WildSpawn.IsGoons(wildSpawnType))
        {
            return settings.TurnLightOffNoEnemyGOONS;
        }
        if (wildSpawnType.IsBoss())
        {
            return settings.TurnLightOffNoEnemyBOSS;
        }
        if (wildSpawnType.IsFollower())
        {
            return settings.TurnLightOffNoEnemyFOLLOWER;
        }
        return settings.TurnLightOffNoEnemyRAIDERROGUE;
    }
}

public class VisionSpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EnemyInfo), nameof(EnemyInfo.method_9));
    }

    [PatchPostfix]
    public static void PatchPostfix(ref float __result, EnemyInfo __instance)
    {
        if (SAINEnableClass.GetSAIN(__instance.Owner.ProfileId, out var sain))
        {
            Enemy enemy = sain.EnemyController.GetEnemy(__instance.ProfileId, false);
            enemy ??= sain.EnemyController.CheckAddEnemy(__instance.Person);
            if (enemy != null)
            {
                float sainMod = EnemyGainSightClass.GetGainSightModifier(enemy);
                __result /= sainMod;
                enemy.Vision.LastGainSightResult = __result;
            }
        }
    }
}

public class WeatherVisionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EnemyInfo), nameof(EnemyInfo.method_11));
    }

    [PatchPrefix]
    public static bool PatchPrefix(EnemyInfo __instance, ref float __result)
    {
        if (SAINEnableClass.IsBotExcluded(__instance.Owner))
        {
            return true;
        }

        __result = 1f;
        return false;
    }
}


//public class CheckPartLineOfSightPatch : ModulePatch
//{
//    protected override MethodBase GetTargetMethod()
//    {
//        return AccessTools.Method(typeof(EnemyInfo), nameof(EnemyInfo.CheckPartLineOfSight));
//    }
//
//    [PatchPrefix]
//    public static bool PatchPrefix(EnemyInfo __instance, ref bool __result, KeyValuePair<EnemyPart, EnemyPartData> part, LayerMask lookSensorMask, float addSensorDistance, ref float visibilityChangeSpeedK)
//    {
//        if (SAINEnableClass.GetSAIN(__instance.Owner, out var sain))
//        {
//            Enemy enemy = sain.EnemyController.GetEnemy(__instance.ProfileId, true);
//            if (enemy != null)
//            {
//                __result = enemy.Vision.Angles.CanBeSeen;
//                return false;
//            }
//        }
//        return true;
//    }
//}

public class IsPointInVisibleSectorPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LookSensor), nameof(LookSensor.IsPointInVisibleSector));
    }

    [PatchPrefix]
    public static bool PatchPrefix(LookSensor __instance, ref bool __result)
    {
        if (SAINEnableClass.GetSAIN(__instance.BotOwner.ProfileId, out var sain))
        {
            Enemy enemy = sain.EnemyController.GetEnemy(__instance.BotOwner.ProfileId, false);
            if (enemy != null)
            {
                __result = enemy.Vision.Angles.CanBeSeen && enemy.Vision.EnemyParts.CanBeSeen;
                return false;
            }
        }
        return true;
    }
}

public class VisionDistancePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EnemyInfo), nameof(EnemyInfo.method_4));
    }

    [PatchPostfix]
    public static void PatchPrefix(ref float __result, EnemyInfo __instance)
    {
        if (SAINEnableClass.GetSAIN(__instance.Owner.ProfileId, out var sain))
        {
            Enemy enemy = sain.EnemyController.GetEnemy(__instance.ProfileId, false);
            if (enemy != null)
            {
                __result += enemy.Vision._visionDistance.Value;
            }
        }
    }
}

public class CheckFlashlightPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.SetLightsState));
    }

    [PatchPostfix]
    public static void PatchPostfix(Player ____player)
    {
        PlayerComponent playerComponent = GameWorldComponent.Instance?.PlayerTracker.GetPlayerComponent(____player?.ProfileId);
        if (playerComponent != null)
        {
            BotManagerComponent.Instance.BotHearing.PlayAISound(playerComponent, SAINSoundType.GearSound, playerComponent.Player.WeaponRoot.position, 35f, 1f, true);
            var flashLight = playerComponent.Flashlight;
            flashLight.CheckDevice();

            if (!flashLight.WhiteLight && !flashLight.Laser)
            {
                (____player.AIData as PlayerAIDataClass).UsingLight = false;
            }
        }
    }
}
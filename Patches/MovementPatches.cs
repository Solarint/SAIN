using Comfort.Common;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Preset.GlobalSettings;
using SPT.Reflection.Patching;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace SAIN.Patches.Movement;

/// <summary>
/// stops sideways sprinting
/// </summary>
public class SprintLookDirPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotMover), nameof(BotMover.Sprint));
    }

    [PatchPrefix]
    public static bool Patch(BotMover __instance, bool val)
    {
        if (__instance.Sprinting == val)
        {
            return false;
        }
        BotOwner botOwner = __instance.BotOwner_0;
        if (SAINEnableClass.IsBotInCombat(botOwner)) return false;
        if (__instance.NoSprint)
        {
            __instance.Player.EnableSprint(false);
            return false;
        }
        if (val && botOwner.Mover.HasPathAndNoComplete)
        {
            Vector3 destinationDirection = botOwner.Mover.RealDestPoint - botOwner.Position;
            destinationDirection.y = 0;
            destinationDirection.Normalize();
            Vector3 lookDirection = botOwner.LookDirection;
            lookDirection.y = 0;
            lookDirection.Normalize();
            if (Vector3.Angle(lookDirection, destinationDirection) > 20f)
            {
                val = false;
            }
        }
        __instance.Sprinting = val;
        if (val)
        {
            botOwner.SetTargetMoveSpeed(1f);
        }
        __instance.Player.EnableSprint(val);
        return false;
    }
}

/// <summary>
/// Blocks pose changes when a player is sprinting for ai
/// </summary>
public class PlayerSetPosePatch : ModulePatch
{
    private static readonly FieldInfo playerField = AccessTools.Field(typeof(MovementContext), "_player");

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(RunStateClass), nameof(RunStateClass.ChangePose));
    }

    [PatchPrefix]
    public static bool Patch(RunStateClass __instance)
    {
        if (playerField.GetValue(__instance.MovementContext) is Player player)
        {
            if (player.IsAI && __instance.MovementContext.IsSprintEnabled)
            {
                __instance.MovementContext.SetPoseLevel(1f, true);
                return false;
            }
            return true;
        }
        else
        {
#if DEBUG
            Logger.LogError("nope");
#endif
        }
        return true;
    }
}

/// <summary>
/// Disables the check for is ai in movement context. could break things in the future
/// </summary>
public class MovementContextIsAIPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(MovementContext), nameof(MovementContext.IsAI));
    }

    [PatchPrefix]
    public static bool Patch(ref bool __result)
    {
        __result = false;
        return false;
    }
}

/// <summary>
/// Disables can be snapped for all players, its only used for door opening and ai
/// </summary>
public class CanBeSnappedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(Player), nameof(Player.CanBeSnapped));
    }

    [PatchPrefix]
    public static bool Patch(ref bool __result)
    {
        __result = false;
        return false;
    }
}

public class GlobalShootSettingsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotGlobalShootData), nameof(BotGlobalShootData.Update));
    }

    [PatchPostfix]
    public static void PatchPrefix(BotGlobalShootData __instance)
    {
        //__instance.CAN_STOP_SHOOT_CAUSE_ANIMATOR = true;
        __instance.MAX_DIST_COEF = 100f;
    }
}

public class StopShootCauseAnimatorPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ShootData), nameof(ShootData.method_1));
    }

    [PatchPrefix]
    public static bool PatchPostfix(ShootData __instance)
    {
        __instance.CanShootByState = true;
        return false;
    }
}

public class PoseStaminaPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PlayerPhysicalClass), nameof(PlayerPhysicalClass.ConsumePoseLevelChange));
    }

    [PatchPrefix]
    public static bool PatchPrefix(PlayerPhysicalClass __instance)
    {
        if (__instance.Player_0.IsAI)
        {
            return false;
        }
        return true;
    }
}

/// <summary>
/// Disable specific functions in Manual Update that might be causing erratic movement in sain bots.
/// </summary>
public class BotMoverManualUpdatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotMover), nameof(BotMover.ManualUpdate));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotMover __instance)
    {
        if (!SAINEnableClass.IsBotInCombat(__instance.BotOwner_0))
        {
            return true;
        }
        __instance.LocalAvoidance.DropOffset();
        __instance.PositionOnWayInner = __instance.BotOwner_0.Position;

        //__instance.method_16();
        //__instance.method_15();
        __instance.method_14();
        //__instance.LocalAvoidance.ManualUpdate();
        return false;
    }
}

/// <summary>
/// Disable specific functions in Manual Update that might be causing erratic movement in sain bots if they are in combat.
/// </summary>
public class BotMoverManualFixedUpdatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotMover), nameof(BotMover.ManualFixedUpdate));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotMover __instance)
    {
        if (SAINEnableClass.IsBotInCombat(__instance.BotOwner_0))
        {
            return false;
        }
        return true;
    }
}

public class AimStaminaPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PlayerPhysicalClass), nameof(PlayerPhysicalClass.Aim));
    }

    [PatchPrefix]
    public static bool PatchPrefix(PlayerPhysicalClass __instance)
    {
        if (__instance.Player_0.IsAI)
        {
            return false;
        }
        return true;
    }
}

public class CrawlPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotPathFinderClass), nameof(BotPathFinderClass.method_0));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotPathFinderClass __instance, ref bool __result)
    {
        if (!SAINEnableClass.IsBotInCombat(__instance.BotOwner_0))
        {
            return true;
        }
        __result = false;
        return false;
    }
}

public class EncumberedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BasePhysicalClass), nameof(BasePhysicalClass.UpdateWeightLimits));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BasePhysicalClass __instance)
    {
        //if (___bool_7)
        //{
        //    return true;
        //}
        IPlayer player = __instance.IobserverToPlayerBridge_0.iPlayer;
        if (player == null)
        {
#if DEBUG
            Logger.LogWarning($"Player is Null, can't set weight limits for AI.");
#endif
            return true;
        }
        if (!player.IsAI)
        {
            return true;
        }
        if (SAINEnableClass.IsSAINDisabledForBot(player.AIData.BotOwner))
        {
            return true;
        }

        // Copy Pasted from original EFT code, there is a check to not enable weight limits for AI
        BackendConfigSettingsClass.InertiaSettings inertia = Singleton<BackendConfigSettingsClass>.Instance.Inertia;
        Vector3 b2 = new(inertia.InertiaLimitsStep * (float)__instance.IobserverToPlayerBridge_0.Skills.Strength.SummaryLevel, inertia.InertiaLimitsStep * (float)__instance.IobserverToPlayerBridge_0.Skills.Strength.SummaryLevel, 0f);
        __instance.BaseInertiaLimits = inertia.InertiaLimits + b2;
        //__instance.WalkOverweightLimits = stamina.WalkOverweightLimits * d + b;
        //__instance.BaseOverweightLimits = stamina.BaseOverweightLimits * d + b;
        //__instance.SprintOverweightLimits = stamina.SprintOverweightLimits * d + b;
        //__instance.WalkSpeedOverweightLimits = stamina.WalkSpeedOverweightLimits * d + b;
        __instance.WalkOverweightLimits.Set(9000f, 10000f);
        __instance.BaseOverweightLimits.Set(9000f, 10000f);
        __instance.SprintOverweightLimits.Set(9000f, 10000f);
        __instance.WalkSpeedOverweightLimits.Set(9000f, 10000f);
        // End of CopyPaste

        return false;
    }
}

//public class DoorDisabledPatch : ModulePatch
//{
//    protected override MethodBase GetTargetMethod()
//    {
//        return AccessTools.Method(typeof(WorldInteractiveObject), nameof(WorldInteractiveObject.method_4));
//    }
//
//    [PatchPrefix]
//    public static bool PatchPrefix(WorldInteractiveObject __instance)
//    {
//        if (!__instance.enabled || !__instance.gameObject.activeInHierarchy)
//        {
//            return false;
//        }
//        return true;
//    }
//}
using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Mover;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace SAIN.Patches.Generic.Fixes;

internal class RunToEnemyUpdatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotMeleeWeaponData), nameof(BotMeleeWeaponData.RunToEnemyUpdate));
    }

    [PatchPrefix]
    public static bool Patch(BotMeleeWeaponData __instance)
    {
        if (SAINEnableClass.GetSAIN(__instance.BotOwner_0.ProfileId, out BotComponent bot) && bot.SAINLayersActive)
        {
            Enemy enemy = bot.GoalEnemy;
            if (enemy == null)
            {
                return false;
            }
            __instance.ShallEndRun = false;
            if (!__instance.BotOwner_0.WeaponManager.IsMelee)
            {
                if (!__instance.BotOwner_0.WeaponManager.Selector.CanChangeToMeleeWeapons)
                {
                    __instance.ShallEndRun = true;
                    return false;
                }
                __instance.BotOwner_0.WeaponManager.Selector.ChangeToMelee();
            }
            if (__instance.BotOwner_0.BotLay.IsLay)
            {
                __instance.BotOwner_0.BotLay.GetUp(false);
            }
            bot.Mover.SetTargetPose(1f);
            EnemyInfo goalEnemy = enemy.EnemyInfo;
            bool flag;
            if (flag = (goalEnemy.Distance < __instance.Single_0))
            {
                bot.Steering.LookToPoint(goalEnemy.GetBodyPartPosition());
                if (goalEnemy.Person.AIData.Player.MovementContext.IsInPronePose)
                {
                    bot.Mover.SetTargetPose(0f);
                }
            }
            else
            {
                bot.Steering.LookToMovingDirection(true);
            }

            if (__instance.Running && goalEnemy.Distance > __instance.Single_1)
            {
                bot.Mover.ActivePath?.RequestStartSprint(ESprintUrgency.High, "melee");
            }
            if (__instance.NextTryHitTime < Time.time)
            {
                __instance.method_0((flag && __instance.method_2(goalEnemy)) ? 10f : __instance.TRY_HIT_PERIOD_FALSE);
            }
            if (bot.Mover.Running)
            {
                if (__instance.RunPathCheck < Time.time)
                {
                    float num;
                    if (__instance.UseZigZag)
                    {
                        num = ((goalEnemy.Distance > __instance.FAR_DIST) ? __instance.FarRecalc : ((goalEnemy.Distance > __instance.MID_DIST) ? __instance.MidRecalcZZ : __instance.CloseRecalcZZ));
                    }
                    else
                    {
                        num = (goalEnemy.Distance > __instance.FAR_DIST) ? __instance.FarRecalc : ((goalEnemy.Distance > __instance.MID_DIST) ? __instance.MidRecalc : __instance.CloseRecalc);
                    }
                    __instance.RunPathCheck = Time.time + num;
                    if (!__instance.CanRunToEnemyToHit(goalEnemy, out Vector3[] way))
                    {
                        __instance.ShallEndRun = true;
                        return false;
                    }
                    if (goalEnemy.Distance < __instance.BotOwner_0.Settings.FileSettings.Shoot.MELEE_STOP_MOVE_DISTANCE)
                    {
                        bot.Mover.ActivePath.Cancel(0.1f);
                    }
                    else
                    {
                        bot.Mover.RunToPoint(goalEnemy.CurrPosition, true, -1, SAINComponent.Classes.Mover.ESprintUrgency.High, true);
                    }
                }
            }
            else
            {
                if (!__instance.CanRunToEnemyToHit(goalEnemy, out Vector3[] way2))
                {
                    __instance.ShallEndRun = true;
                    return false;
                }
                if (goalEnemy.Distance < __instance.BotOwner_0.Settings.FileSettings.Shoot.MELEE_STOP_MOVE_DISTANCE)
                {
                    bot.Mover.ActivePath?.Cancel(0.1f);
                }
                else
                {
                    bot.Mover.RunToPoint(goalEnemy.CurrPosition, true, -1, SAINComponent.Classes.Mover.ESprintUrgency.High, true);
                }
            }
            return false;
        }
        return false;
    }
}

internal class EnableVaultPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.InitVaultingComponent));
    }

    [PatchPrefix]
    public static void Patch(Player __instance, ref bool aiControlled)
    {
        if (__instance.UsedSimplifiedSkeleton)
            return;

        aiControlled = false;
    }
}

internal class DisableGrenadesPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotGrenadeToPortal), nameof(BotGrenadeToPortal.method_0));
    }

    [PatchPrefix]
    public static bool Patch(BotGrenadeToPortal __instance)
    {
        var settings = GlobalSettingsClass.Instance.General;
        if (!settings.BotsUseGrenades) return false;
        if (SAINEnableClass.GetSAIN(__instance.BotOwner_0.ProfileId, out BotComponent bot))
        {
            var goalEnemy = bot.EnemyController.GoalEnemy;
            if (goalEnemy == null) return false;
            if (!settings.BotVsBotGrenade && goalEnemy.IsAI) return false;
        }
        return true;
    }
}

internal class FightShallReloadFixPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotReload), nameof(BotReload.FightShallReload));
    }

    [PatchPrefix]
    public static bool Patch(BotReload __instance, ref bool __result)
    {
        if (SAINEnableClass.IsBotInCombat(__instance.BotOwner_0))
        {
            __result = true;
            return false;
        }
        return true;
    }
}

internal class FixItemTakerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotItemTaker), nameof(BotItemTaker.method_12));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotItemTaker __instance)
    {
        return GenericHelpers.CheckNotNull(__instance.BotOwner_0);
    }
}

internal class FixItemTakerPatch2 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotItemTaker), nameof(BotItemTaker.RefreshClosestItems));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotItemTaker __instance)
    {
        return GenericHelpers.CheckNotNull(__instance.BotOwner_0);
    }
}

internal class RotateClampPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.Rotate));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player __instance, ref bool ignoreClamp)
    {
        if (__instance?.IsAI == true && __instance.IsSprintEnabled && SAINEnableClass.IsBotInCombat(__instance))
        {
            ignoreClamp = true;
        }
    }
}

internal class BotGroupAddEnemyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.AddEnemy));
    }

    [PatchPrefix]
    public static bool PatchPrefix(IPlayer person)
    {
        if (person == null || person.IsAI && person.AIData?.BotOwner?.GetPlayer == null)
        {
            return false;
        }

        return true;
    }
}

internal class BotMemoryAddEnemyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotMemoryClass), nameof(BotMemoryClass.AddEnemy));
    }

    [PatchPrefix]
    public static bool PatchPrefix(IPlayer enemy)
    {
        if (enemy == null || enemy.IsAI && enemy.AIData?.BotOwner?.GetPlayer == null)
        {
            return false;
        }

        return true;
    }
}

public class StopSetToNavMeshPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotMover), nameof(BotMover.method_9));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotMover __instance)
    {
        if (SAINEnableClass.IsBotInCombat(__instance.BotOwner_0))
        {
            __instance.PositionOnWayInner = __instance.BotOwner_0.Position;
            __instance.BotOwner_0.Mover.LocalAvoidance.DropOffset();
            return false;
        }
        return true;
    }
}

public class StopSetToNavMeshPatch2 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GClass494), nameof(GClass494.method_21));
    }

    [PatchPrefix]
    public static bool PatchPrefix(GClass494 __instance)
    {
        if (SAINEnableClass.IsBotInCombat(__instance.BotOwner_0))
        {
            return false;
        }
        return true;
    }
}
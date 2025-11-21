using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Components;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using static SAIN.Helpers.Shoot;

namespace SAIN.Patches.Shoot.RateOfFire;

public class BotShootPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ShootData), nameof(ShootData.Shoot));
    }

    [PatchPrefix]
    public static bool PatchPrefix(ShootData __instance, ref bool __result)
    {
        BotOwner botOwner = __instance.Owner;
        if (!SAINEnableClass.GetSAIN(botOwner.ProfileId, out BotComponent bot))
        {
            return true;
        }
        __result = false;
        if (__instance.ShootController == null)
        {
            return false;
        }
        BotUnderbarrelLauncherController underbarrelLauncherController = botOwner.WeaponManager.UnderbarrelLauncherController;
        if (underbarrelLauncherController.IsActive)
        {
            if (underbarrelLauncherController.NeedToReload() && !underbarrelLauncherController.TryReload(null))
            {
                underbarrelLauncherController.TryDisable(null);
                return false;
            }
            if (!underbarrelLauncherController.CheckShootAttemptAndDisableIfNeeded())
            {
                return false;
            }
            __instance.NextFingerDownCan = Time.time - 0.1f;
        }
        if (!__instance.Shooting && __instance.NextFingerDownCan < Time.time)
        {
            bool fullAuto = bot.Info.WeaponInfo.SelectedFireMode == Weapon.EFireMode.fullauto;
            if (fullAuto) __instance.NextFingerUpTime = Time.time + FullAutoBurstLength(bot, bot.DistanceToAimTarget);
            __instance.NextFingerDownCan = Time.time + bot.Info.WeaponInfo.Firerate.CalcFirerateInterval();
            __instance.Shooting = true;
            __instance.TimeFingerDown = Time.time;
            __instance.LastTriggerPressd = Time.time;
            __instance.ShootController.IsInLauncherMode();
            __instance.ShootController.SetTriggerPressed(true);
            botOwner.AimingManager.CurrentAiming.TriggerPressedDone();
            __result = true;
            return false;
        }
        return false;
    }
}
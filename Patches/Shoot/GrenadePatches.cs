using EFT;
using HarmonyLib;
using SAIN.Components;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace SAIN.Patches.Shoot.Grenades;

public class SetGrenadePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotGrenadeController), nameof(BotGrenadeController.method_3));
    }

    [PatchPostfix]
    public static void Patch(ThrowWeapItemClass potentialGrenade, BotGrenadeController __instance)
    {
        if (potentialGrenade == null)
        {
            return;
        }
        if (!BotManagerComponent.Instance.GetSAIN(__instance.BotOwner_0, out var botComponent))
        {
            return;
        }
        //__instance.Mass = potentialGrenade.Weight;
        botComponent.Grenade.MyGrenade = potentialGrenade;
    }
}

public class ResetGrenadePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotGrenadeController), nameof(BotGrenadeController.method_2));
    }

    [PatchPostfix]
    public static void Patch(BotGrenadeController __instance)
    {
        if (!BotManagerComponent.Instance.GetSAIN(__instance.BotOwner_0, out var botComponent))
        {
            return;
        }
        botComponent.Grenade.MyGrenade = __instance.Grenade;
    }
}
using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Combat.Components;
using SAIN.Combat.Helpers;
using System.Reflection;
using UnityEngine;
using static SAIN.Combat.Configs.AimingConfig;
using static SAIN.Combat.Configs.DebugConfig;

namespace SAIN.Combat.Patches
{
    public class AddComponentPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotOwner), "PreActivate");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner __instance)
        {
            __instance.gameObject.AddComponent<WeaponInfo>();
        }
    }
}
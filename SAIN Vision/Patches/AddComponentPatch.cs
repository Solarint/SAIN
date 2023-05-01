using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SAIN_Helpers;
using System.Reflection;
using UnityEngine;
using Vision.Helpers;

namespace Vision.Patches
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
            if (__instance?.GetPlayer != null)
            {
                __instance.gameObject.AddComponent<BotLineObject>();
            }
        }
    }
}
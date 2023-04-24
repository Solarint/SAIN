using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Flashlights.Components;
using SAIN.Flashlights.Helpers;
using System.Reflection;
using UnityEngine;
using static SAIN.Flashlights.Config.DazzleConfig;

namespace SAIN.Flashlights.Patches
{
    public class AddComponentPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(AiDataClass), "CalcPower");
        }

        [PatchPostfix]
        public static void PatchPostfix(AiDataClass __instance)
        {
            __instance.Player.gameObject.AddComponent<FlashLightDetection>();
        }
    }
}
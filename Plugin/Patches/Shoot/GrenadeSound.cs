using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Helpers;
using System.Reflection;
using UnityEngine;

namespace SAIN.Patches
{
    public class GrenadeSoundPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GrenadeCartridge), "method_2");
        }

        [PatchPostfix]
        public static void PatchPostfix(GrenadeCartridge __instance)
        {
            try
            {
                Singleton<GClass635>.Instance.PlaySound(null, __instance.transform.position, 20f, AISoundType.gun);
                Logger.LogInfo($"Played AISound for grenade bounce");
            }
            catch
            {
                Logger.LogError($"Failed to play grenade collision sound for AI");
            }
        }
    }
}
using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN_Movement.Components;
using System.Reflection;

namespace SAIN_Movement.Patches
{
    public class ComponentAddPatch : ModulePatch
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
                __instance.gameObject.AddComponent<SolarintAudio>();
            }
        }
    }

    public class HearingSensorDisablePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("HearingSensor")?.PropertyType?.GetMethod("Init");
        }
        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return false;
        }
    }
}

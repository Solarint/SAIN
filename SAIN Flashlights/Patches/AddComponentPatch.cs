using Aki.Reflection.Patching;
using HarmonyLib;
using Flashlights.Components;
using System.Reflection;

namespace Flashlights.Patches
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
            __instance.Player.gameObject.AddComponent<SAIN_Flashlight_Component>();
        }
    }
}
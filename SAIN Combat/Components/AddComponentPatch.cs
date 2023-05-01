using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using Combat.Components;
using System.Reflection;

namespace Combat.Patches
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
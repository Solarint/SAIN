using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Audio.Components;
using System.Reflection;

namespace SAIN.Audio.Patches
{
    // Adds a dictionary with bot Id for later reference by other patches
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
}

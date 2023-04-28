using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SAIN_Grenades.Components;
using SAIN_Helpers;
using System.Reflection;
using UnityEngine;

namespace SAIN_Grenades.Patches
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
                __instance.gameObject.AddComponent<GrenadeComponent>();
            }
        }
    }
    public class DisableGrenadePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotControllerClass), "method_4");
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotControllerClass __instance, Grenade grenade, Vector3 position, Vector3 force, float mass)
        {
            foreach (BotOwner bot in __instance.Bots.BotOwners)
            {
                var GrenadeTracker = bot.GetComponent<GrenadeComponent>();
                GrenadeTracker.GrenadeThrown(grenade, position, force, mass);
            }
            return false;
        }
    }
    public class GlobalGrenadeSettingsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalsGrenadeSettings), "Update");
        }

        [PatchPostfix]
        public static void PatchPostfix(BotGlobalsGrenadeSettings __instance)
        {
            __instance.CHANCE_TO_NOTIFY_ENEMY_GR_100 = 100f;
            __instance.DELTA_GRENADE_START_TIME = 0.0f;
            __instance.BEWARE_TYPE = 3;
        }
    }
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
                Singleton<GClass629>.Instance.PlaySound(null, __instance.transform.position, 20f, AISoundType.gun);
                DebugDrawer.Sphere(__instance.transform.position, 1.0f, Color.black, 5f);
                Logger.LogInfo($"Played AISound for grenade bounce");
            }
            catch
            {
                Logger.LogError($"Failed to play grenade collision sound for AI");
            }
        }
    }
}
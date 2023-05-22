using Aki.Reflection.Patching;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SAIN.Components;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Patches
{
    public class BotGlobalMindPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalsMindSettings), "Update");
        }

        [PatchPostfix]
        public static void PatchPostfix(BotGlobalsMindSettings __instance)
        {
            __instance.DOG_FIGHT_IN = 0f;
            __instance.DOG_FIGHT_OUT = 0.1f;
            __instance.DIST_TO_STOP_RUN_ENEMY = 0f;
            __instance.NO_RUN_AWAY_FOR_SAFE = false;
            __instance.SURGE_KIT_ONLY_SAFE_CONTAINER = false;
            __instance.CAN_USE_MEDS = true;
            __instance.CAN_USE_FOOD_DRINK = true;
        }
    }

    public class BotGlobalAimPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalAimingSettings), "Update");
        }

        [PatchPostfix]
        public static void PatchPostfix(BotGlobalAimingSettings __instance)
        {
            __instance.SHPERE_FRIENDY_FIRE_SIZE = 0.66f;
        }
    }

    public class BotGlobalMovePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalsMoveSettings), "Update");
        }

        [PatchPostfix]
        public static void PatchPostfix(BotGlobalsMoveSettings __instance)
        {
            __instance.SEC_TO_CHANGE_TO_RUN = 1f;
            __instance.RUN_TO_COVER_MIN = 0f;
        }
    }

    public class BotGlobalCorePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass559), "Update");
        }

        [PatchPostfix]
        public static void PatchPostfix(GClass559 __instance)
        {
            __instance.MIN_DIST_TO_STOP_RUN = 0f;
        }
    }

    public class BotGlobalGrenadePatch : ModulePatch
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
}

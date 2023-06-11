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
            __instance.GROUP_ANY_PHRASE_DELAY = 5f;
            __instance.GROUP_EXACTLY_PHRASE_DELAY = 5f;
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
            //__instance.SHPERE_FRIENDY_FIRE_SIZE = 0.33f;
            __instance.RECALC_MUST_TIME = 1;
            __instance.RECALC_MUST_TIME_MIN = 1;
            __instance.RECALC_MUST_TIME_MAX = 2;
            __instance.BASE_HIT_AFFECTION_DELAY_SEC = 0.5f;
            __instance.BASE_HIT_AFFECTION_MIN_ANG = 3f;
            __instance.BASE_HIT_AFFECTION_MAX_ANG = 5f;
        }
    }

    public class BotGlobalShootPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalShootData), "Update");
        }

        [PatchPostfix]
        public static void PatchPostfix(BotGlobalShootData __instance)
        {
            __instance.MAX_DIST_COEF = 1.75f;
            __instance.CHANCE_TO_CHANGE_TO_AUTOMATIC_FIRE_100 = 100f;
            __instance.AUTOMATIC_FIRE_SCATTERING_COEF = 1.5f;
            __instance.BASE_AUTOMATIC_TIME = 0.5f;
            __instance.RECOIL_DELTA_PRESS = 0.1f;
        }
    }

    public class BotGlobalScatterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalsScatteringSettings), "Update");
        }
        [PatchPostfix]
        public static void PatchPostfix(BotGlobalsScatteringSettings __instance)
        {
            __instance.MinScatter = 0.15f;
            __instance.WorkingScatter = 0.25f;
            __instance.MaxScatter = 0.33f;
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
            __instance.SEC_TO_CHANGE_TO_RUN = 9999f;
            __instance.RUN_TO_COVER_MIN = 9999f;
            __instance.BASE_ROTATE_SPEED = 250f;
            __instance.FIRST_TURN_SPEED = 250f;
            __instance.FIRST_TURN_BIG_SPEED = 250f;
            __instance.TURN_SPEED_ON_SPRINT = 250f;
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
            __instance.MIN_DIST_TO_STOP_RUN = 9999f;
            //__instance.SHOOT_TO_CHANGE_RND_PART_DELTA = 0.01f;
            //__instance.CAN_SHOOT_TO_HEAD = false;
            __instance.ARMOR_CLASS_COEF = 7f;
            __instance.SHOTGUN_POWER = 70f;
            __instance.RIFLE_POWER = 50f;
            __instance.PISTOL_POWER = 30f;
            __instance.SMG_POWER = 100f;
            __instance.SNIPE_POWER = 20f;
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

    public class BotGlobalLookPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalLookData), "Update");
        }

        [PatchPostfix]
        public static void PatchPostfix(BotGlobalLookData __instance)
        {
            //__instance.MAX_DIST_CLAMP_TO_SEEN_SPEED = 1000f;

            __instance.NIGHT_VISION_ON = 75f;
            __instance.NIGHT_VISION_OFF = 125f;
            __instance.NIGHT_VISION_DIST = 125f;
            __instance.VISIBLE_ANG_NIGHTVISION = 90f;

            //__instance.LOOK_THROUGH_PERIOD_BY_HIT = 0f;

            __instance.LightOnVisionDistance = 40f;
            __instance.VISIBLE_ANG_LIGHT = 30f;
            __instance.VISIBLE_DISNACE_WITH_LIGHT = 50f;

            __instance.GOAL_TO_FULL_DISSAPEAR = 0.5f;
            __instance.GOAL_TO_FULL_DISSAPEAR_GREEN = 0.25f;
            __instance.GOAL_TO_FULL_DISSAPEAR_SHOOT = 0.0001f;

            //__instance.MAX_VISION_GRASS_METERS = 1f;
            //__instance.MAX_VISION_GRASS_METERS_OPT = 1f;
            //__instance.MAX_VISION_GRASS_METERS_FLARE = 4f;
            //__instance.MAX_VISION_GRASS_METERS_FLARE_OPT = 0.25f;

            //__instance.NO_GREEN_DIST = 3f;
            //__instance.NO_GRASS_DIST = 3f;
        }
    }
}

using Aki.Reflection.Patching;
using HarmonyLib;
using System.Reflection;

namespace Combat.Patches
{
    public class BotGlobalAimingSettingsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalAimingSettings), "Update");
        }
        [PatchPostfix]
        public static void PatchPostfix(BotGlobalAimingSettings __instance)
        {
            __instance.RECALC_MUST_TIME = 1;
            __instance.RECALC_MUST_TIME_MIN = 1;
            __instance.RECALC_MUST_TIME_MAX = 2;
            __instance.BASE_HIT_AFFECTION_DELAY_SEC = 0.5f;
            __instance.BASE_HIT_AFFECTION_MIN_ANG = 3f;
            __instance.BASE_HIT_AFFECTION_MAX_ANG = 5f;
        }
    }

    public class BotGlobalShootDataPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalShootData), "Update");
        }
        [PatchPostfix]
        public static void PatchPostfix(BotGlobalShootData __instance)
        {
            __instance.CHANCE_TO_CHANGE_TO_AUTOMATIC_FIRE_100 = 100f;
            __instance.AUTOMATIC_FIRE_SCATTERING_COEF = 2f;
            __instance.BASE_AUTOMATIC_TIME = 0.5f;
            __instance.RECOIL_DELTA_PRESS = 0f;
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
            __instance.WorkingScatter = 0.35f;
            __instance.MaxScatter = 0.5f;
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
            __instance.SHOOT_TO_CHANGE_RND_PART_DELTA = 0.01f;
            __instance.CAN_SHOOT_TO_HEAD = false;
            __instance.ARMOR_CLASS_COEF = 7f;
            __instance.SHOTGUN_POWER = 70f;
            __instance.RIFLE_POWER = 50f;
            __instance.PISTOL_POWER = 30f;
            __instance.SMG_POWER = 100f;
            __instance.SNIPE_POWER = 20f;
        }
    }
}

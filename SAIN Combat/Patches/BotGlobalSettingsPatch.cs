using Aki.Reflection.Patching;
using HarmonyLib;
using System.Reflection;

namespace SAIN_Audio.Combat.Patches
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
            //__instance.MAX_AIM_PRECICING = 5f;
            //__instance.BETTER_PRECICING_COEF = 0.5f;
            //__instance.RECLC_Y_DIST = 0.25f;
            //__instance.RECALC_DIST = 0.5f;
            //__instance.RECALC_SQR_DIST = 0.25f;
            //__instance.HARD_AIM_CHANCE_100 = 0;
            __instance.RECALC_MUST_TIME = 1;
            __instance.RECALC_MUST_TIME_MIN = 1;
            __instance.RECALC_MUST_TIME_MAX = 2;
            //__instance.DAMAGE_TO_DISCARD_AIM_0_100 = 1f;
            //__instance.MIN_TIME_DISCARD_AIM_SEC = 0.75f;
            //__instance.MAX_TIME_DISCARD_AIM_SEC = 1.25f;
            //__instance.MAX_AIMING_UPGRADE_BY_TIME = 0f;
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

            // NONE OF THESE ARE USED, THEY LEAD TO UN-USED CLASS FILES
            //__instance.SpeedUp = 0.15f;
            //__instance.SpeedUpAim = 1.1f;
            //__instance.SpeedDown = -0.3f;
            //__instance.FromShot = 0.1f;

            //__instance.ToSlowBotSpeed = 0.1f;
            //__instance.ToLowBotSpeed = 0.1f;
            //__instance.ToUpBotSpeed = 0.1f;
            //__instance.MovingSlowCoef = 2.5f;

            //__instance.ToLowBotAngularSpeed = 80f;
            //__instance.ToStopBotAngularSpeed = 40f;

            //__instance.TracerCoef = 1f;
            //__instance.HandDamageScatteringMinMax = 3.0f;
            //__instance.HandDamageAccuracySpeed = 3f;
            //__instance.BloodFall = 1.45f;
            //__instance.ToCaution = 1f;

            //__instance.RecoilControlCoefShootDone = 0.1f; // 0.0003f;
            //__instance.RecoilControlCoefShootDoneAuto = 0.1f; // 0.00015f;

            //__instance.DIST_FROM_OLD_POINT_TO_NOT_AIM_SQRT = 0.25f * 0.25f;

            //__instance.AMPLITUDE_FACTOR = 0.5f;
            //__instance.AMPLITUDE_SPEED = 0.1f;
            //__instance.DIST_NOT_TO_SHOOT = 0.1f;
            //__instance.LayFactor = 1f;
            //__instance.RecoilYCoef = 0.0005f;
            //__instance.RecoilYCoefSppedDown = -0.52f;
            //__instance.RecoilYMax = 1f;
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

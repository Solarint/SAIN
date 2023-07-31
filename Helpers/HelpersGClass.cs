using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using Interpolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN.Helpers
{
    internal class HelpersGClass
    {
        public DateTime UTCNow => GClass1292.UtcNow;
        public static GClass563 BotCoreSettings => GClass564.Core;
        public static float LAY_DOWN_ANG_SHOOT => BotCoreSettings.LAY_DOWN_ANG_SHOOT;
        public static float Gravity => BotCoreSettings.G;
        public static float SMOKE_GRENADE_RADIUS_COEF => BotCoreSettings.SMOKE_GRENADE_RADIUS_COEF;

        public static GClass635 SoundPlayer => Singleton<GClass635>.Instance;

        public static void PlaySound(IAIDetails player, Vector3 pos, float range, AISoundType soundtype)
        {
            SoundPlayer?.PlaySound(player, pos, range, soundtype);
        }

        public class BotStatGClassWrapper
        {
            public BotStatGClassWrapper(float precision, float accuracySpeed, float gainSight, float scatter, float priorityScatter)
            {
                Modifiers = new GClass561
                {
                    PrecicingSpeedCoef = precision,
                    AccuratySpeedCoef = accuracySpeed,
                    GainSightCoef = gainSight,
                    ScatteringCoef = scatter,
                    PriorityScatteringCoef = priorityScatter,
                };
            }

            public GClass561 Modifiers;
        }
    }

    public class BotGlobalSettingsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass563), "Update");
        }

        [PatchPostfix]
        public static void PatchPostfix(GClass563 __instance)
        {
            var settings = __instance;
            VectorHelpers.Gravity = settings.G;

            //__instance.CARE_ENEMY_ONLY_TIME = 120f;
            settings.SCAV_GROUPS_TOGETHER = false;
            settings.DIST_NOT_TO_GROUP = 50f;
            settings.DIST_NOT_TO_GROUP_SQR = 50f * 50f;
            settings.MIN_DIST_TO_STOP_RUN = 0f;
            settings.CAN_SHOOT_TO_HEAD = false;
            settings.ARMOR_CLASS_COEF = 7f;
            settings.SHOTGUN_POWER = 40f;
            settings.RIFLE_POWER = 50f;
            settings.PISTOL_POWER = 20f;
            settings.SMG_POWER = 60f;
            settings.SNIPE_POWER = 5f;

        }
    }
}

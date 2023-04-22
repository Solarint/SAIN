using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Combat.Components;
using SAIN.Combat.Helpers;
using System.Reflection;
using UnityEngine;
using static SAIN.Combat.Configs.AimingConfig;
using static SAIN.Combat.Configs.DebugConfig;

namespace SAIN.Combat.Patches
{
    public class AimPatch : ModulePatch
    {
        private static MethodInfo _LastAimTime;
        private static PropertyInfo _Boolean_0;

        protected override MethodBase GetTargetMethod()
        {
            _Boolean_0 = AccessTools.Property(typeof(GClass544), "Boolean_0");
            _LastAimTime = AccessTools.PropertySetter(typeof(GClass544), "LastAimTime");
            return AccessTools.Method(typeof(GClass544), "method_7");
        }
        [PatchPrefix]
        public static bool PatchPrefix(
            GClass544 __instance, ref BotDifficultySettingsClass ___gclass561_0, ref BotOwner ___botOwner_0, // Class References
            ref bool ___bool_1, ref float ___float_10, // Field References
            float dist, float ang, ref float __result) // Original Input Parameters
        {
            // assign values to new ones for better readability
            float aimDelay = ___float_10;
            bool isMoving = ___bool_1;

            // Take longer to aim if moving
            float aimMoveCoef = 1f;
            if (isMoving)
            {
                aimMoveCoef *= TIME_COEF_IF_MOVE.Value;
            }

            // Assign worse aim if bot is panicing
            float aimPanicCoef = (bool)_Boolean_0.GetValue(__instance) ? Configs.AimingConfig.PANIC_COEF.Value : 1f;

            // Evaluate aim based on animation curves
            float aimAngleCoef = ___gclass561_0.Curv.AimAngCoef.Evaluate(ang);
            float aimTimeDistance = ___gclass561_0.Curv.AimTime2Dist.Evaluate(dist);

            // Assign better aim if bot is in cover
            float aimCoef = 1f;
            if (___botOwner_0.Memory.IsInCover)
            {
                aimCoef = Configs.AimingConfig.COEF_FROM_COVER.Value;
            }

            // MATH
            float aimBottomCoef = aimCoef * BOTTOM_COEF.Value;

            float currentAccuracySpeed = AccuratySpeed.Value + ___botOwner_0.Settings.Current._accuratySpeedCoef;
            float baseAimTime = aimAngleCoef * aimTimeDistance * currentAccuracySpeed * aimPanicCoef;

            float finalAimTime = (aimBottomCoef + baseAimTime + aimDelay) * aimMoveCoef;

            float max_AIM_TIME = MAX_AIM_TIME.Value;

            // Clamp Max Aim
            if (finalAimTime > max_AIM_TIME)
            {
                finalAimTime = max_AIM_TIME;
            }

            ___float_10 = 0f;
            _LastAimTime.Invoke(__instance, new object[] { finalAimTime });
            __result = finalAimTime;

            //if (Config.Debug.)

            return false;
        }
    }
    public class ScatterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // This is not a field, but a property, and it is private, so we must use reflection to read its value later
            _Boolean_0 = AccessTools.Property(typeof(GClass544), "Boolean_0");

            // These properties are "read only" so we must use reflection
            _LastSpreadCount = AccessTools.PropertySetter(typeof(GClass544), "LastSpreadCount");
            return AccessTools.Method(typeof(GClass544), "method_9");
        }

        // Save the method/property info
        private static MethodInfo _LastSpreadCount;
        private static PropertyInfo _Boolean_0;

        [PatchPrefix]
        public static bool PatchPrefix(float dist, float angCoef, float additionCoef,
            GClass544 __instance, ref BotOwner ___botOwner_0,
            ref bool ___bool_1, ref bool ___bool_3,
            ref Vector3 ___vector3_5, ref Vector3 __result)
        {

            // Apply config values 
            float XZ_COEF = SAIN.Combat.Configs.AimingConfig.XZ_COEF.Value;
            float BAD_SHOOTS_MAIN_COEF = SAIN.Combat.Configs.AimingConfig.BAD_SHOOTS_MAIN_COEF.Value;
            float BAD_SHOOTS_OFFSET = SAIN.Combat.Configs.AimingConfig.BAD_SHOOTS_OFFSET.Value;
            float COEF_IF_MOVE = SAIN.Combat.Configs.AimingConfig.COEF_IF_MOVE.Value;
            float NEXT_SHOT_MISS_Y_OFFSET = SAIN.Combat.Configs.AimingConfig.NEXT_SHOT_MISS_Y_OFFSET.Value;
            float PANIC_ACCURATY_COEF = SAIN.Combat.Configs.AimingConfig.PANIC_ACCURATY_COEF.Value;
            float Y_BOTTOM_OFFSET_COEF = SAIN.Combat.Configs.AimingConfig.Y_BOTTOM_OFFSET_COEF.Value;
            float Y_TOP_OFFSET_COEF = SAIN.Combat.Configs.AimingConfig.Y_TOP_OFFSET_COEF.Value;

            bool amIMoving = ___bool_1;
            float distance = dist;
            var goalEnemy = ___botOwner_0.Memory.GoalEnemy;

            // Calculate "bad shoot". If we are going to miss?
            bool badshoots = false;
            if (goalEnemy != null)
            {
                int shootByTarget = ___botOwner_0.BotPersonalStats.GetShootByTarget(goalEnemy);

                int num = BSGMethods.BadShoot(distance, BAD_SHOOTS_MAIN_COEF);

                if (shootByTarget < num)
                {
                    badshoots = true;
                }
            }

            // Calculate what scatter should start as
            float lastspread = BSGMethods.ScatterSetting(distance, ___botOwner_0) * additionCoef;

            bool isPanicing = (bool)_Boolean_0.GetValue(__instance);
            if (isPanicing)
            {
                lastspread *= PANIC_ACCURATY_COEF;
            }
            if (__instance.HardAim)
            {
                lastspread *= HARD_AIM.Value;
            }
            if (___botOwner_0.BotLay.IsLay)
            {
                lastspread *= 0.66f;
            }
            if (amIMoving)
            {
                lastspread *= COEF_IF_MOVE;
            }

            // Update Spread with reflection
            _LastSpreadCount.Invoke(__instance, new object[] { lastspread });

            // Generate new location to aim at based on scatter
            float x = 0f;
            float y = 0f;
            float z = 0f;

            Weapon weapon = ___botOwner_0.WeaponManager.CurrentWeapon;
            if (weapon.SelectedFireMode == Weapon.EFireMode.fullauto || weapon.SelectedFireMode == Weapon.EFireMode.burst)
            {
                XZ_COEF *= 1.5f;
                NEXT_SHOT_MISS_Y_OFFSET *= 1.5f;
            }

            float badShootOffset = BAD_SHOOTS_OFFSET;
            float angClamped = Mathf.Clamp(angCoef, 0f, 60f);
            float calcScatter = 2f * distance * Mathf.Sin(0.017453292f * angClamped / 2f) * XZ_COEF + __instance.LastSpreadCount;
            float yTopOffset = __instance.LastSpreadCount * Y_TOP_OFFSET_COEF;
            float yBottomOffset = -__instance.LastSpreadCount * Y_BOTTOM_OFFSET_COEF;

            // Not sure what this does exactly. What is the difference between "normal" and "regular"
            AimingType aimingType = ___botOwner_0.Settings.FileSettings.Core.AimingType;
            if (aimingType != AimingType.normal)
            {
                if (aimingType == AimingType.regular)
                {
                    x = MathHelpers.Random(-calcScatter, calcScatter);
                    y = MathHelpers.Random(yBottomOffset, yTopOffset);
                    z = MathHelpers.Random(-calcScatter, calcScatter);
                }
            }
            else
            {
                x = MathHelpers.RandomNormal(-calcScatter, calcScatter);
                y = MathHelpers.RandomNormal(yBottomOffset, yTopOffset);
                z = MathHelpers.RandomNormal(-calcScatter, calcScatter);
            }

            // If next shot miss
            if (___bool_3)
            {
                y = NEXT_SHOT_MISS_Y_OFFSET;
                ___bool_3 = false;
            }

            // If we "bad shoot". If we miss?
            if (badshoots)
            {
                float x2 = (x > 0f) ? badShootOffset : (-badShootOffset);
                float z2 = (z > 0f) ? badShootOffset : (-badShootOffset);
                ___vector3_5 = new Vector3(x2, 0f, z2);
            }
            else
            {
                ___vector3_5 = Vector3.zero;
            }

            __result = new Vector3(x, y, z);

            return false;
        }
    }
    public class BSGMethods
    {
        // Method_10
        public static int BadShoot(float distance, float badshootcoef)
        {
            int badshootmaxmin = MathHelpers.RandomInclude(1, 2);
            return badshootmaxmin + (int)(BAD_SHOOTS_MAIN_COEF.Value * Mathf.Log(1.2f + distance * 0.2f));
        }
        // Method_8
        public static float ScatterSetting(float distance, BotOwner bot)
        {
            float f = BASE_SHIEF.Value + distance;
            float p = bot.WeaponManager.IsCloseWeapon ? SCATTERING_DIST_MODIF_CLOSE.Value : SCATTERING_DIST_MODIF.Value;
            float num = Mathf.Pow(f, p);
            float num2 = bot.WeaponManager.IsCloseWeapon ? bot.Settings.Current.CurrentScatteringClose : bot.Settings.Current.CurrentScattering;
            return num * num2;
        }
    }
}
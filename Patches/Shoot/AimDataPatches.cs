using EFT;
using HarmonyLib;
using RootMotion.FinalIK;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.BotSettings.SAINSettings.Categories;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Text;
using UnityEngine;
using HitAffectClass = GClass568;

namespace SAIN.Patches.Shoot.Aim
{
    internal class PlayerHitReactionDisablePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HitReaction), "Hit");
        }

        [PatchPrefix]
        public static bool Patch()
        {
            if (!GlobalSettingsClass.Instance.Aiming.HitEffects.HIT_REACTION_TOGGLE) {
                return true;
            }
            return false;
        }
    }

    internal class HitAffectApplyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HitAffectClass), "Affect");
        }

        [PatchPrefix]
        public static bool Patch(BotOwner ___botOwner_0, ref Vector3 __result, Vector3 dir)
        {
            if (!GlobalSettingsClass.Instance.Aiming.HitEffects.HIT_REACTION_TOGGLE) {
                return true;
            }
            if (SAINEnableClass.GetSAIN(___botOwner_0, out var bot)) {
                __result = bot.Medical.HitReaction.AimHitEffect.ApplyEffect(dir);
                return false;
            }
            return true;
        }
    }

    internal class DoHitAffectPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HitAffectClass), "DoAffection");
        }

        [PatchPrefix]
        public static bool Patch(BotOwner ___botOwner_0)
        {
            if (!GlobalSettingsClass.Instance.Aiming.HitEffects.HIT_REACTION_TOGGLE) {
                return true;
            }
            if (SAINEnableClass.GetSAIN(___botOwner_0, out var bot)) {
                return false;
            }
            return true;
        }
    }

    internal class SetAimStatusPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertySetter(typeof(BotAimingClass), "Status");
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotAimingClass __instance, AimStatus value, BotOwner ___botOwner_0, ref AimStatus ___aimStatus_0, float ___float_7)
        {
            if (___aimStatus_0 == value || ___botOwner_0.BotState != EBotState.Active) {
                return false;
            }
            if (SAINEnableClass.GetSAIN(___botOwner_0, out var bot)) {
                ___aimStatus_0 = value;
                //bool flag;
                //if ((flag = (((this.bool_0 && this.botOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Attack)) || this.botOwner_0.Memory.IsInCover || this.method_1()) && this.aimStatus_0 != AimStatus.NoTarget && this.method_0())) != this.botOwner_0.WeaponManager.ShootController.IsAiming)
                //{
                //	this.botOwner_0.WeaponManager.ShootController.SetAim(flag);
                //}
                //this.HardAim = flag;
                if (value == AimStatus.AimComplete) {
                    ___botOwner_0.BotPersonalStats.Aim(__instance.EndTargetPoint, ___float_7);
                }
                return false;
            }
            return true;
        }
    }

    internal class AimOffsetPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            _endTargetPointProp = AccessTools.Property(HelpersGClass.AimDataType, "EndTargetPoint");
            return AccessTools.Method(typeof(BotAimingClass), "method_13");
            //return AccessTools.Method(HelpersGClass.AimDataType, "method_13");
        }

        private static PropertyInfo _endTargetPointProp;

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref Vector3 ___vector3_5, ref Vector3 ___vector3_4, ref float ___float_13)
        {
            if (!SAINEnableClass.GetSAIN(___botOwner_0, out var bot)) {
                return true;
            }

            Vector3 realTargetPoint = ___botOwner_0.AimingManager.CurrentAiming.RealTargetPoint;

            if (bot.IsCheater) {
                _endTargetPointProp.SetValue(___botOwner_0.AimingManager.CurrentAiming, realTargetPoint);
                return false;
            }

            Enemy enemy = bot.Shoot.LastShotEnemy ?? bot.Enemy ?? bot.LastEnemy;
            if (enemy == null) {
                return true;
            }

            float aimUpgradeByTime = ___float_13;
            Vector3 badShootOffset = ___vector3_5;
            Vector3 recoilOffset = bot.Info.WeaponInfo.Recoil.CurrentRecoilOffset;

            // Applies aiming offset, recoil offset, and scatter offsets
            // Default Setup :: Vector3 finalTarget = __instance.RealTargetPoint + badShootOffset + (AimUpgradeByTime * (AimOffset + ___botOwner_0.RecoilData.RecoilOffset));

            Vector3 aimOffset;
            if (___botOwner_0.Settings.FileSettings.Aiming.DIST_TO_SHOOT_NO_OFFSET > enemy.RealDistance) {
                aimOffset = Vector3.zero;
            }
            else {
                float spread = aimUpgradeByTime / enemy.Aim.AimAndScatterMultiplier;
                spread = Mathf.Clamp(spread, 0f, 3f);
                aimOffset = ___vector3_4 * spread;
            }

            if (bot.Info.Profile.IsPMC || bot.Info.Profile.WildSpawnType.isGoons()) {
                badShootOffset = Vector3.zero;
            }

            Vector3 finalOffset = badShootOffset + aimOffset + recoilOffset;
            if (!enemy.IsAI &&
                SAINPlugin.LoadedPreset.GlobalSettings.Look.NotLooking.NotLookingToggle) {
                finalOffset += NotLookingOffset(enemy.EnemyPerson.IPlayer, ___botOwner_0);
            }

            Vector3 result = realTargetPoint + finalOffset;

            if (SAINPlugin.LoadedPreset.GlobalSettings.General.Debug.Gizmos.DebugDrawAimGizmos &&
                enemy.EnemyPerson.IPlayer.IsYourPlayer == true) {
                Vector3 weaponRoot = ___botOwner_0.WeaponRoot.position;
                DebugGizmos.Line(weaponRoot, result, Color.red, 0.02f, true, 0.25f, true);
                DebugGizmos.Sphere(result, 0.025f, Color.red, true, 10f);

                DebugGizmos.Line(weaponRoot, realTargetPoint, Color.white, 0.02f, true, 0.25f, true);
                DebugGizmos.Sphere(realTargetPoint, 0.025f, Color.white, true, 10f);
            }
            //if (SAINPlugin.DebugSettings.Gizmos.DebugDrawRecoilGizmos &&
            //    enemy.EnemyPerson.IPlayer.IsYourPlayer == true &&
            //    ___botOwner_0.ShootData.Shooting) {
            //    DebugGizmos.Sphere(recoilOffset + realTargetPoint, 0.035f, Color.red, true, 10f);
            //    DebugGizmos.Line(recoilOffset + realTargetPoint, realTargetPoint, Color.red, 0.02f, true, 10f, true);
            //}

            _endTargetPointProp.SetValue(___botOwner_0.AimingManager.CurrentAiming, result);
            return false;
        }

        private static Vector3 NotLookingOffset(IPlayer person, BotOwner botOwner)
        {
            float ExtraSpread = SAINNotLooking.GetSpreadIncrease(person, botOwner);
            if (ExtraSpread > 0) {
                Vector3 vectorSpread = UnityEngine.Random.insideUnitSphere * ExtraSpread;
                vectorSpread.y = 0;
                return vectorSpread;
            }
            return Vector3.zero;
        }
    }

    internal class ScatterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(HelpersGClass.AimDataType, "method_9");
        }

        [PatchPrefix]
        public static void PatchPrefix(BotOwner ___botOwner_0, ref float additionCoef)
        {
            if (SAINEnableClass.GetSAIN(___botOwner_0, out var bot)) {
            }
        }
    }

    internal class HitEffectPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(HelpersGClass.AimDataType, "GetHit");
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotOwner ___botOwner_0, DamageInfoStruct DamageInfoStruct)
        {
            if (SAINPlugin.IsBotExluded(___botOwner_0)) {
                return true;
            }

            return false;
        }
    }

    internal class WeaponPresetPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotWeaponManager), "UpdateHandsController");
        }

        [PatchPostfix]
        public static void Patch(BotOwner ___botOwner_0, IHandsController handsController)
        {
            IFirearmHandsController firearmHandsController;
            if ((firearmHandsController = (handsController as IFirearmHandsController)) != null) {
                SAINBotController.Instance?.BotChangedWeapon(___botOwner_0, firearmHandsController);
            }
        }
    }

    public class AimTimePatch : ModulePatch
    {
        private static PropertyInfo _PanicingProp;

        protected override MethodBase GetTargetMethod()
        {
            _PanicingProp = AccessTools.Property(HelpersGClass.AimDataType, "Boolean_0");
            return AccessTools.Method(HelpersGClass.AimDataType, "method_7");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, float dist, float ang, ref bool ___bool_1, ref float ___float_10, ref float __result)
        {
            if (!SAINEnableClass.GetSAIN(___botOwner_0, out var bot)) {
                return true;
            }

            float aimDelay = ___float_10;
            bool moving = ___bool_1;
            bool panicing = (bool)_PanicingProp.GetValue(___botOwner_0.AimingManager.CurrentAiming);

            __result = calculateAim(bot, dist, ang, moving, panicing, aimDelay);
            bot.Aim.LastAimTime = __result;

            return false;
        }

        private static float calculateAim(BotComponent botComponent, float distance, float angle, bool moving, bool panicing, float aimDelay)
        {
            BotOwner botOwner = botComponent.BotOwner;
            StringBuilder stringBuilder = SAINPlugin.LoadedPreset.GlobalSettings.General.Debug.Logs.DebugAimCalculations ? new StringBuilder() : null;
            stringBuilder?.AppendLine($"Aim Time Calculation for [{botOwner?.name} : {botOwner?.Profile?.Info?.Settings?.Role} : {botOwner?.Profile?.Info?.Settings?.BotDifficulty}]");

            SAINAimingSettings sainAimSettings = botComponent.Info.FileSettings.Aiming;
            BotSettingsComponents fileSettings = botOwner.Settings.FileSettings;

            float baseAimTime = fileSettings.Aiming.BOTTOM_COEF;
            stringBuilder?.AppendLine($"baseAimTime [{baseAimTime}]");
            baseAimTime = calcCoverMod(baseAimTime, botOwner, botComponent, fileSettings, stringBuilder);
            BotCurvSettings curve = botOwner.Settings.Curv;
            float angleTime = calcCurveOutput(curve.AimAngCoef, angle, sainAimSettings.AngleAimTimeMultiplier, stringBuilder, "Angle");
            float distanceTime = calcCurveOutput(curve.AimTime2Dist, distance, sainAimSettings.DistanceAimTimeMultiplier, stringBuilder, "Distance");
            float calculatedAimTime = calcAimTime(angleTime, distanceTime, botOwner, stringBuilder);
            calculatedAimTime = calcPanic(panicing, calculatedAimTime, fileSettings, stringBuilder);

            float timeToAimResult = (baseAimTime + calculatedAimTime + aimDelay);
            stringBuilder?.AppendLine($"timeToAimResult [{timeToAimResult}] (baseAimTime + calculatedAimTime + aimDelay)");

            timeToAimResult = calcMoveModifier(moving, timeToAimResult, fileSettings, stringBuilder);
            timeToAimResult = calcADSModifier(botOwner.WeaponManager?.ShootController?.IsAiming == true, timeToAimResult, stringBuilder);
            timeToAimResult = clampAimTime(timeToAimResult, fileSettings, stringBuilder);
            timeToAimResult = calcFasterCQB(distance, timeToAimResult, sainAimSettings, stringBuilder);
            timeToAimResult = calcAttachmentMod(botComponent, timeToAimResult, stringBuilder);

            if (stringBuilder != null &&
                botOwner?.Memory?.GoalEnemy?.Person?.IsYourPlayer == true) {
                Logger.LogDebug(stringBuilder.ToString());
            }
            return timeToAimResult;
        }

        private static float calcAimTime(float angleTime, float distanceTime, BotOwner botOwner, StringBuilder stringBuilder)
        {
            float accuracySpeed = botOwner.Settings.Current.CurrentAccuratySpeed;
            stringBuilder?.AppendLine($"accuracySpeed [{accuracySpeed}]");

            float calculatedAimTime = angleTime * distanceTime * accuracySpeed;
            stringBuilder?.AppendLine($"calculatedAimTime [{calculatedAimTime}] (angleTime * distanceTime * accuracySpeed)");
            return calculatedAimTime;
        }

        private static float calcCoverMod(float baseAimTime, BotOwner botOwner, BotComponent botComponent, BotSettingsComponents fileSettings, StringBuilder stringBuilder)
        {
            CoverPoint coverInUse = botComponent?.Cover.CoverInUse;
            bool inCover = botOwner.Memory.IsInCover || coverInUse?.BotInThisCover == true;
            if (inCover) {
                baseAimTime *= fileSettings.Aiming.COEF_FROM_COVER;
                stringBuilder?.AppendLine($"In Cover: [{baseAimTime}] : COEF_FROM_COVER [{fileSettings.Aiming.COEF_FROM_COVER}]");
            }
            return baseAimTime;
        }

        private static float calcCurveOutput(AnimationCurve aimCurve, float input, float modifier, StringBuilder stringBuilder, string curveType)
        {
            float result = aimCurve.Evaluate(input);
            result *= modifier;
            stringBuilder?.AppendLine($"{curveType} Curve Output [{result}] : input [{input}] : Multiplier: [{modifier}]");
            return result;
        }

        private static float calcMoveModifier(bool moving, float timeToAimResult, BotSettingsComponents fileSettings, StringBuilder stringBuilder)
        {
            if (moving) {
                timeToAimResult *= fileSettings.Aiming.COEF_IF_MOVE;
                stringBuilder?.AppendLine($"Moving [{timeToAimResult}] : Moving Coef [{fileSettings.Aiming.COEF_IF_MOVE}]");
            }
            return timeToAimResult;
        }

        private static float calcADSModifier(bool aiming, float timeToAimResult, StringBuilder stringBuilder)
        {
            if (aiming) {
                float adsMulti = SAINPlugin.LoadedPreset.GlobalSettings.Aiming.AimDownSightsAimTimeMultiplier;
                timeToAimResult *= adsMulti;
                stringBuilder?.AppendLine($"Aiming Down Sights [{timeToAimResult}] : ADS Multiplier [{adsMulti}]");
            }
            return timeToAimResult;
        }

        private static float clampAimTime(float timeToAimResult, BotSettingsComponents fileSettings, StringBuilder stringBuilder)
        {
            float clampedResult = Mathf.Clamp(timeToAimResult, 0f, fileSettings.Aiming.MAX_AIM_TIME);
            if (clampedResult != timeToAimResult) {
                stringBuilder?.AppendLine($"Clamped Aim Time [{clampedResult}] : MAX_AIM_TIME [{fileSettings.Aiming.MAX_AIM_TIME}]");
            }
            return clampedResult;
        }

        private static float calcPanic(bool panicing, float calculatedAimTime, BotSettingsComponents fileSettings, StringBuilder stringBuilder)
        {
            if (panicing) {
                calculatedAimTime *= fileSettings.Aiming.PANIC_COEF;
                stringBuilder?.AppendLine($"Panicing [{calculatedAimTime}] : Panic Coef [{fileSettings.Aiming.PANIC_COEF}]");
            }
            return calculatedAimTime;
        }

        private static float calcFasterCQB(float distance, float aimTimeResult, SAINAimingSettings aimSettings, StringBuilder stringBuilder)
        {
            if (!SAINPlugin.LoadedPreset.GlobalSettings.Aiming.FasterCQBReactionsGlobal) {
                return aimTimeResult;
            }
            if (aimSettings?.FasterCQBReactions == true &&
                distance <= aimSettings.FasterCQBReactionsDistance) {
                float ratio = distance / aimSettings.FasterCQBReactionsDistance;
                float fasterTime = aimTimeResult * ratio;
                fasterTime = Mathf.Clamp(fasterTime, aimSettings.FasterCQBReactionsMinimum, aimTimeResult);
                stringBuilder?.AppendLine($"Faster CQB Aim Time: Result [{fasterTime}] : Original [{aimTimeResult}] : At Distance [{distance}] with maxDist [{aimSettings.FasterCQBReactionsDistance}]");
                return fasterTime;
            }
            return aimTimeResult;
        }

        private static float calcAttachmentMod(BotComponent bot, float aimTimeResult, StringBuilder stringBuilder)
        {
            Enemy enemy = bot?.Enemy;
            if (enemy != null) {
                float modifier = enemy.Aim.AimAndScatterMultiplier;
                stringBuilder?.AppendLine($"Bot Attachment Mod: Result [{aimTimeResult / modifier}] : Original [{aimTimeResult}] : Modifier [{modifier}]");
                aimTimeResult /= modifier;
            }
            return aimTimeResult;
        }
    }

    public class AimRotateSpeedPatch : ModulePatch
    {
        static AimRotateSpeedPatch()
        {
            PresetHandler.OnPresetUpdated += updateSettings;
            updateSettings(SAINPresetClass.Instance);
        }

        private static void updateSettings(SAINPresetClass preset)
        {
            _aimTurnSpeed = preset.GlobalSettings.Move.AimTurnSpeed;
        }

        private static float _aimTurnSpeed = 300f;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(HelpersGClass.AimDataType, "method_11");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref Vector3 ___vector3_2, ref Vector3 ___vector3_0, Vector3 dir)
        {
            ___vector3_2 = dir;
            ___botOwner_0.Steering.LookToDirection(dir, _aimTurnSpeed);
            ___botOwner_0.Steering.SetYByDir(___vector3_0);
            return false;
        }
    }

    internal class ForceNoHeadAimPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EnemyInfo), "method_7");
        }

        [PatchPrefix]
        public static void PatchPrefix(ref bool withLegs, ref bool canBehead, EnemyInfo __instance)
        {
            if (!__instance.Person.IsAI) {
                var aim = GlobalSettingsClass.Instance.Aiming;
                canBehead = EFTMath.RandomBool(aim.PMCAimForHeadChance) && aim.PMCSAimForHead && isPMC(__instance);
                withLegs = true;
            }
            else {
                canBehead = true;
                withLegs = true;
            }
        }

        private static bool isPMC(EnemyInfo __instance)
        {
            return EnumValues.WildSpawn.IsPMC(__instance.Owner.Profile.Info.Settings.Role);
        }
    }
}
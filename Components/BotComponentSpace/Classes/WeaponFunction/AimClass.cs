using EFT;
using HarmonyLib;
using SAIN.Preset;
using SAIN.Preset.BotSettings.SAINSettings.Categories;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using System;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.WeaponFunction
{
    public class TimeToAimClass : BotBase, IBotClass
    {
        public TimeToAimClass(BotComponent bot) : base(bot)
        {
            var curv = bot.BotOwner.Settings.Curv;
            _aimAngleCurve = curv.AimAngCoef;
            _aimDistanceCurve = curv.AimTime2Dist;
        }

        public void Init()
        {
            base.SubscribeToPreset(updateSettings);
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        private void updateSettings(SAINPresetClass preset)
        {
            var aimSettings = Bot.Info.FileSettings.Aiming;
            AIMTIME_MULTIPLIER_ANGLE = aimSettings.AngleAimTimeMultiplier;
            AIMTIME_MULTIPLIER_DISTANCE = aimSettings.DistanceAimTimeMultiplier;

            var eftSettings = BotOwner.Settings.FileSettings.Aiming;
            AIMTIME_BASE = eftSettings.BOTTOM_COEF;
            AIMTIME_COVER_COEF = eftSettings.COEF_FROM_COVER;
        }

        private float AIMTIME_MULTIPLIER_ANGLE = 1f;
        private float AIMTIME_MULTIPLIER_DISTANCE = 1f;
        private float AIMTIME_BASE = 1f;
        private float AIMTIME_COVER_COEF = 1f;

        private AnimationCurve _aimAngleCurve;
        private AnimationCurve _aimDistanceCurve;

        private float calculateAim(float distance, float angle, bool moving, bool panicing, float aimDelay)
        {
            StringBuilder stringBuilder = SAINPlugin.LoadedPreset.GlobalSettings.General.Debug.Logs.DebugAimCalculations ? new StringBuilder() : null;
            stringBuilder?.AppendLine($"Aim Time Calculation for [{BotOwner?.name} : {BotOwner?.Profile?.Info?.Settings?.Role} : {BotOwner?.Profile?.Info?.Settings?.BotDifficulty}]");

            SAINAimingSettings sainAimSettings = Bot.Info.FileSettings.Aiming;
            BotSettingsComponents fileSettings = BotOwner.Settings.FileSettings;

            float baseAimTime = AIMTIME_BASE;
            stringBuilder?.AppendLine($"baseAimTime [{baseAimTime}]");

            if (Bot.Cover.InCover) {
                baseAimTime *= AIMTIME_COVER_COEF;
            }

            float angleTime =
                calcCurveOutput(_aimAngleCurve, angle, AIMTIME_MULTIPLIER_ANGLE, stringBuilder, "Angle");
            float distanceTime =
                calcCurveOutput(_aimDistanceCurve, distance, AIMTIME_MULTIPLIER_DISTANCE, stringBuilder, "Distance");
            float calculatedAimTime =
                calcAimTime(angleTime, distanceTime, BotOwner, stringBuilder);
            calculatedAimTime =
                calcPanic(panicing, calculatedAimTime, fileSettings, stringBuilder);

            float timeToAimResult = (baseAimTime + calculatedAimTime + aimDelay);
            stringBuilder?.AppendLine($"timeToAimResult [{timeToAimResult}] (baseAimTime + calculatedAimTime + aimDelay)");

            timeToAimResult = calcMoveModifier(moving, timeToAimResult, fileSettings, stringBuilder);
            timeToAimResult = calcADSModifier(BotOwner.WeaponManager?.ShootController?.IsAiming == true, timeToAimResult, stringBuilder);
            timeToAimResult = clampAimTime(timeToAimResult, fileSettings, stringBuilder);
            timeToAimResult = calcFasterCQB(distance, timeToAimResult, sainAimSettings, stringBuilder);
            timeToAimResult = calcAttachmentMod(Bot, timeToAimResult, stringBuilder);

            if (stringBuilder != null &&
                BotOwner?.Memory?.GoalEnemy?.Person?.IsYourPlayer == true) {
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
            bool inCover = coverInUse?.BotInThisCover == true;
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

    public class AimClass : BotBase, IBotClass
    {
        public event Action<bool> OnAimAllowedOrBlocked;

        public bool CanAim { get; private set; }

        public float LastAimTime { get; set; }

        public AimStatus AimStatus {
            get
            {
                object aimStatus = aimStatusField.GetValue(BotOwner.AimingManager.CurrentAiming);
                if (aimStatus == null) {
                    return AimStatus.NoTarget;
                }

                var status = (AimStatus)aimStatus;

                //if (status != AimStatus.NoTarget &&
                //    Bot.Enemy?.IsVisible == false &&
                //    Bot.LastEnemy?.IsVisible == false)
                //{
                //    return AimStatus.NoTarget;
                //}
                return status;
            }
        }

        public AimClass(BotComponent sain) : base(sain)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
            checkCanAim();
            checkLoseTarget();
        }

        public void LateUpdate()
        {
        }

        private void checkCanAim()
        {
            bool couldAim = CanAim;
            CanAim = canAim();
            if (couldAim != CanAim) {
                OnAimAllowedOrBlocked?.Invoke(CanAim);
            }
        }

        private bool canAim()
        {
            var aimData = BotOwner.AimingManager.CurrentAiming;
            if (aimData == null) {
                //return false;
            }
            if (Player.IsSprintEnabled) {
                return false;
            }
            if (BotOwner.WeaponManager.Reload.Reloading) {
                //return false;
            }
            if (!Bot.HasEnemy) {
                //return false;
            }
            return true;
        }

        private void checkLoseTarget()
        {
            if (!CanAim) {
                BotOwner.AimingManager.CurrentAiming?.LoseTarget();
                return;
            }
        }

        public void Dispose()
        {
        }

        static AimClass()
        {
            aimStatusField = AccessTools.Field(Helpers.HelpersGClass.AimDataType, "aimStatus_0");
        }

        private static FieldInfo aimStatusField;
    }
}
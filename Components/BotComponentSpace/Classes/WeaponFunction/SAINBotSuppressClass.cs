using EFT;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using SAIN.Preset.GlobalSettings.Categories;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Info;
using System;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.WeaponFunction
{
    public class SAINBotSuppressClass : BotBase, IBotClass
    {
        public event Action<ESuppressionState> OnSuppressionStateChanged;

        public Enemy LastSuppressByEnemy { get; private set; }

        public ESuppressionState CurrentState { get; private set; }
        public ESuppressionState LastState { get; private set; }
        public float SuppressionNumber { get; private set; }
        public bool IsSuppressed => CurrentState == ESuppressionState.Medium;
        public bool IsHeavySuppressed => CurrentState == ESuppressionState.Heavy || CurrentState == ESuppressionState.Extreme;

        public SAINBotSuppressClass(BotComponent sain) : base(sain)
        {
        }

        public void Init()
        {
            base.SubscribeToPreset(null);
            Bot.EnemyController.Events.OnEnemyRemoved += clearLastSuppEnemy;
        }

        public void Update()
        {
            checkState();
            decaySuppression();
        }

        public void Dispose()
        {
            Bot.EnemyController.Events.OnEnemyRemoved -= clearLastSuppEnemy;
        }

        public void CheckAddSuppression(Enemy enemy, float distance, float amount = -1)
        {
            if (!_settings.SUPP_TOGGLE) {
                return;
            }
            float resistance = getResistance();
            if (resistance >= 1) {
                return;
            }

            if (amount <= 0) {
                amount = getSuppNum(enemy);
            }

            float scaledSupNum = scaleSuppDist(amount * _settings.SUPP_AMOUNT_MULTI, distance);
            if (scaledSupNum <= 0) {
                return;
            }

            LastSuppressByEnemy = enemy;
            float resistedAmount = calcResistance(scaledSupNum, resistance);
            clampAndUpdateSuppression(resistedAmount);
        }

        private static float scaleSuppDist(float suppNum, float distance)
        {
            MindSettings settings = GlobalSettingsClass.Instance.Mind;
            if (distance < settings.SUPP_DISTANCE_AMP_DIST) {
                return suppNum * settings.SUPP_DISTANCE_AMP_AMOUNT;
            }
            float max = settings.SUPP_DISTANCE_SCALE_END;
            float min = settings.SUPP_DISTANCE_SCALE_START;
            float ratio = (distance - min) / (max - min);
            return Mathf.Lerp(suppNum, 0f, ratio);
        }

        private float getResistance()
        {
            var mindSettings = Bot.Info.FileSettings.Mind;
            if (mindSettings == null) {
                return 1f;
            }

            var persSettings = Bot.Info.PersonalitySettings.General;
            if (persSettings == null) {
                return Mathf.Clamp01(mindSettings.SuppressionResistance);
            }

            return Mathf.Lerp(
                Mathf.Clamp01(mindSettings.SuppressionResistance),
                Mathf.Clamp01(persSettings.SuppressionResistance),
                0.5f).Round100();
        }

        private float getSuppNum(Enemy enemy)
        {
            const float defaultNum = 2f;

            WeaponInfo weapon = enemy.EnemyPlayerComponent.Equipment.CurrentWeapon;
            if (weapon == null) {
                if (SAINPlugin.DebugMode) {
                    Logger.LogWarning($"Could not find Weapon to check suppression amount!");
                }
                return defaultNum;
            }

            if (GlobalSettings.Mind.SUPP_AMOUNTS.TryGetValue(weapon.AmmoCaliber, out float result)) {
                return result;
            }
            if (SAINPlugin.DebugMode) {
                Logger.LogWarning($"Could not find [{weapon.AmmoCaliber}] to check suppression amount!");
            }

            if (GlobalSettings.Mind.SUPP_AMOUNTS.TryGetValue(ECaliber.Default, out result)) {
                return result;
            }
            if (SAINPlugin.DebugMode) {
                Logger.LogWarning($"Could not find Default Caliber Value to check suppression amount!");
            }
            return defaultNum;
        }

        private void checkState()
        {
            if (_tickTime < Time.time) {
                _tickTime = Time.time + _settings.SUP_CHECK_FREQ;

                if (SuppressionNumber <= 0) {
                    if (CurrentState != ESuppressionState.None) {
                        applyNewState(ESuppressionState.None, null);
                    }
                    return;
                }

                ESuppressionState newState = SuppressionHelpers.FindActiveState(SuppressionNumber, out SuppressionConfig config);
                if (CurrentState == newState) {
                    return;
                }
                applyNewState(newState, config);
            }
        }

        private void decaySuppression()
        {
            if (SuppressionNumber > 0 &&
                _decayTime < Time.time) {
                _decayTime = Time.time + _settings.SUP_DECAY_FREQ;
                clampAndUpdateSuppression(-_settings.SUP_DECAY_AMOUNT);
            }
        }

        private void clampAndUpdateSuppression(float addAmount)
        {
            SuppressionNumber = Mathf.Clamp(SuppressionNumber + addAmount, 0f, _settings.SUPP_MAX_NUM);
        }

        private void applyNewState(ESuppressionState newState, SuppressionConfig config)
        {
            LastState = CurrentState;
            CurrentState = newState;
            clearModifiers();

            if (newState == ESuppressionState.None || config == null) {
                return;
            }

            _temporaryStatModifiers = createMods(config);
            BotOwner?.Settings?.Current?.Apply(_temporaryStatModifiers.Modifiers);
            OnSuppressionStateChanged?.Invoke(newState);
        }

        private void clearModifiers()
        {
            if (_temporaryStatModifiers != null) {
                if (_temporaryStatModifiers.Modifiers.IsApplyed) {
                    BotOwner.Settings.Current.Dismiss(_temporaryStatModifiers.Modifiers);
                }
                _temporaryStatModifiers = null;
            }
        }

        private static TemporaryStatModifiers createMods(SuppressionConfig config)
        {
            float multiplier = Mathf.Clamp(GlobalSettingsClass.Instance.Mind.SUPP_STRENGTH_MULTI, 0.01f, 100f);
            return new TemporaryStatModifiers(
                (config.PrecisionSpeedCoef * multiplier).Round100(),
                (config.AccuracySpeedCoef * multiplier).Round100(),
                (config.GainSightCoef * multiplier).Round100(),
                (config.ScatteringCoef * multiplier).Round100(),
                (config.ScatteringCoef * multiplier).Round100(),
                config.VisibleDistCoef,
                config.HearingDistCoef);
        }

        private static float calcResistance(float value, float resistance)
        {
            if (value == 1f) {
                return 1f;
            }
            resistance = Mathf.Clamp01(resistance);
            return Mathf.Lerp(value, 0f, resistance);
        }

        private void clearLastSuppEnemy(string profileId, Enemy enemy)
        {
            if (LastSuppressByEnemy != null && LastSuppressByEnemy.IsSame(enemy)) {
                LastSuppressByEnemy = null;
            }
        }

        private MindSettings _settings => GlobalSettings.Mind;
        private float _decayTime;
        private float _tickTime;
        private TemporaryStatModifiers _temporaryStatModifiers;
    }
}
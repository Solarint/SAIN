using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Preset.GlobalSettings;
using SAIN.Preset.GlobalSettings.Categories;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Info;
using System;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.WeaponFunction;

public class SAINBotSuppressClass : BotComponentClassBase
{
    public event Action<ESuppressionState> OnSuppressionStateChanged;

    public Enemy LastSuppressByEnemy { get; private set; }

    public ESuppressionState CurrentState { get; private set; }
    public ESuppressionState LastState { get; private set; }
    public float SuppressionNumber { get; private set; }
    public bool IsSuppressed => CurrentState >= ESuppressionState.Medium;
    public bool IsHeavySuppressed => CurrentState >= ESuppressionState.Heavy;

    public SAINBotSuppressClass(BotComponent sain) : base(sain)
    {
        TickRequirement = ESAINTickState.OnlyNoSleep;
    }

    public override void Init()
    {
        Bot.EnemyController.Events.OnEnemyRemoved += EnemyRemoved;
        base.Init();
    }

    public override void ManualUpdate()
    {
        checkState();
        decaySuppression();
        CheckEndSuppression();
        base.ManualUpdate();
    }

    private void CheckEndSuppression()
    {
        if (SuppressingTarget)
        {
            if (EnemyBeingSuppressed?.EnemyPlayer?.HealthController?.IsAlive != true)
            {
                ResetSuppressing();
            }
            else if (!Bot.HasEnemy)
            {
                ResetSuppressing();
            }
            else if (_suppressTime < Time.time)
            {
                ResetSuppressing();
            }
            else
            {
                UpdateSuppressionVector();
            }
        }
    }

    private void UpdateSuppressionVector()
    {
        Enemy enemy = EnemyBeingSuppressed;
        if (enemy == null)
        {
            ResetSuppressing();
            return;
        }
        var suppressionTarget = enemy.SuppressionTarget;
        if (suppressionTarget == null)
        {
            ResetSuppressing();
            return;
        }
        Bot.ManualShoot.ShootPosition = suppressionTarget.Value;
    }

    public override void Dispose()
    {
        Bot.EnemyController.Events.OnEnemyRemoved -= EnemyRemoved;
        base.Dispose();
    }

    private Enemy EnemyBeingSuppressed;

    public bool TrySuppressAnyEnemy(Enemy priorityEnemy, EnemyList knownEnemies, float minimumAmmoRatio = 0.33f, int minimumBullets = 2, bool withBehaviorChecks = true)
    {
        if (!GlobalSettingsClass.Instance.Mind.TARGET_SUPPRESS_TOGGLE)
        {
            if (SuppressingTarget) ResetSuppressing();
            return false;
        }
        if (!Bot.Info.PersonalitySettings.General.TARGET_SUPPRESS_TOGGLE)
        {
            if (SuppressingTarget) ResetSuppressing();
            return false;
        }
        if (Bot.Decision.CurrentSelfDecision == ESelfActionType.Reload)
        {
            if (SuppressingTarget) ResetSuppressing();
            return false;
        }
        if (Bot.Mover.Running &&
            (Bot.Mover.ActivePath.CurrentSprintStatus == Mover.EBotSprintStatus.Running ||
            Bot.Mover.ActivePath.CurrentSprintStatus == Mover.EBotSprintStatus.Turning))
        {
            if (SuppressingTarget) ResetSuppressing();
            return false;
        }

        float ratio = CalcAmmoRatio(BotOwner, out int currentAmmo);
        bool ammoGoodForSupp = ratio >= minimumAmmoRatio && currentAmmo >= minimumBullets;
        if (!ammoGoodForSupp)
        {
            ResetSuppressing();
            return false;
        }

        if (SuppressingTarget)
        {
            CheckEndSuppression();
        }

        if (SuppressingTarget && _suppressTime > Time.time)
        {
            return true;
        }

        // Always check priority enemy first
        if (TrySuppressEnemy(priorityEnemy, withBehaviorChecks))
        {
            _nextChangeSuppTargetTime = Time.time + CHANGE_SUPP_TARGET_FREQ;
            return true;
        }

        // Try to keep suppressing the same target if possible
        if (LastSuppressedEnemy != null)
        {
            if (!LastSuppressedEnemy.EnemyKnown || !Enemy.IsEnemyValid(Bot.ProfileId, LastSuppressedEnemy.EnemyPlayerComponent))
            {
                LastSuppressedEnemy = null;
            }
            else if (TrySuppressEnemy(LastSuppressedEnemy, withBehaviorChecks))
            {
                _nextChangeSuppTargetTime = Time.time + CHANGE_SUPP_TARGET_FREQ;
                return true;
            }
        }

        // Frequency limit to stop rapidly changing targets
        if (_nextChangeSuppTargetTime > Time.time)
        {
            ResetSuppressing();
            return false;
        }

        knownEnemies.SortBy(EnemyList.EBotListSortType.VisiblePathPointDistanceToEnemy);
        foreach (Enemy enemy in knownEnemies)
        {
            if (TrySuppressEnemy(enemy, withBehaviorChecks))
            {
                _nextChangeSuppTargetTime = Time.time + CHANGE_SUPP_TARGET_FREQ;
                return true;
            }
        }
        ResetSuppressing();
        return false;
    }

    private const float CHANGE_SUPP_TARGET_FREQ = 0.5f;
    private float _nextChangeSuppTargetTime;

    public static float CalcAmmoRatio(BotOwner botowner, out int currentAmmoCount)
    {
        var weaponManager = botowner?.WeaponManager;
        if (weaponManager == null || weaponManager.Reload == null)
        {
            currentAmmoCount = 0;
            return 0f;
        }
        // eft code lacks a null ref check
        var item = weaponManager.ShootController?.Item;
        if (item == null)
        {
            currentAmmoCount = 0;
            return 0f;
        }
        currentAmmoCount = weaponManager.Reload.BulletCount;
        return (float)currentAmmoCount / weaponManager.Reload.MaxBulletCount;
    }

    public float TimeSinceSeenToSuppress => Bot.Info.PersonalitySettings.General.TimeSinceSeenToSuppress;
    public float TimeSinceShotAtToSuppress => Bot.Info.PersonalitySettings.General.TimeSinceShotToSuppress;
    public float TimeSinceShotToSuppress => Bot.Info.PersonalitySettings.General.TimeSinceShotToSuppress;

    public bool TrySuppressEnemy(Enemy Enemy, bool withBehaviorChecks = true)
    {
        if (Enemy != null && !Enemy.IsZombie && !Enemy.IsVisible)
        {
            if (withBehaviorChecks && !CanSuppressEnemy(Enemy))
            {
                return false;
            }
            Vector3? suppressTarget = Enemy.SuppressionTarget;
            if (suppressTarget != null)
            {
                return Bot.Suppression.SuppressPosition(suppressTarget.Value, Enemy);
            }
        }
        return false;
    }

    private bool CanSuppressEnemy(Enemy Enemy)
    {
        if (Enemy.Seen && Enemy.TimeSinceSeen <= TimeSinceSeenToSuppress)
        {
            return true;
        }
        var status = Enemy.Status;
        if (status.ShotAtMe && Time.time - status.TimeLastShotAtMe <= TimeSinceShotAtToSuppress)
        {
            return true;
        }
        if (status.ShotMe && Time.time - status.TimeLastShotMe <= TimeSinceShotToSuppress)
        {
            return true;
        }
        return false;
    }

    public bool SuppressPosition(Vector3 position, Enemy Enemy)
    {
        if (Enemy == null ||
            !Bot.ManualShoot.TryShoot(Enemy, position, true, EShootReason.Suppress))
        {
            ResetSuppressing();
            return false;
        }

        SuppressingTarget = true;
        Enemy.Status.EnemyIsSuppressed = true;
        EnemyBeingSuppressed = Enemy;
        if (_suppressTime < Time.time)
        {
            float timeAdd;
            bool isMachineGun = Bot.Info.WeaponInfo.EWeaponClass == EWeaponClass.machinegun;
            timeAdd = isMachineGun ? 0.1f : 0.25f;
            _suppressTime = Time.time + timeAdd * UnityEngine.Random.Range(0.66f, 1.33f);
        }
        return true;
    }

    public void ResetSuppressing()
    {
        //if (EnemyBeingSuppressed != null || SuppressingTarget)
        //{
        if (EnemyBeingSuppressed != null)
        {
            EnemyBeingSuppressed.Status.EnemyIsSuppressed = false;
            LastSuppressedEnemy = EnemyBeingSuppressed;
            EnemyBeingSuppressed = null;
        }
        SuppressingTarget = false;
        Bot.ManualShoot.Reset();
        _suppressTime = 0;
        //}
    }

    private Enemy LastSuppressedEnemy;

    public bool SuppressingTarget;
    private float _suppressTime;

    public void CheckAddSuppression(Enemy enemy, float distance, float amount = -1)
    {
        if (!_settings.SUPP_TOGGLE)
        {
            return;
        }
        float resistance = getResistance();
        if (resistance >= 1)
        {
            return;
        }

        if (amount <= 0)
        {
            amount = getSuppNum(enemy);
        }

        float scaledSupNum = scaleSuppDist(amount * _settings.SUPP_AMOUNT_MULTI, distance);
        if (scaledSupNum <= 0)
        {
            return;
        }

        LastSuppressByEnemy = enemy;
        float resistedAmount = calcResistance(scaledSupNum, resistance);
        clampAndUpdateSuppression(resistedAmount);
    }

    private static float scaleSuppDist(float suppNum, float distance)
    {
        MindSettings settings = GlobalSettingsClass.Instance.Mind;
        if (distance < settings.SUPP_DISTANCE_AMP_DIST)
        {
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
        if (mindSettings == null)
        {
            return 1f;
        }

        var persSettings = Bot.Info.PersonalitySettings.General;
        if (persSettings == null)
        {
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

        WeaponInfo weapon = enemy.EnemyPlayerComponent.Equipment.CurrentWeaponInfo;
        if (weapon == null)
        {
#if DEBUG
            if (SAINPlugin.DebugMode)
            {
                Logger.LogWarning($"Could not find Weapon to check suppression amount!");
            }
#endif
            return defaultNum;
        }

        if (GlobalSettings.Mind.SUPP_AMOUNTS.TryGetValue(weapon.AmmoCaliber, out float result))
        {
            return result;
        }
#if DEBUG
        if (SAINPlugin.DebugMode)
        {
            Logger.LogWarning($"Could not find [{weapon.AmmoCaliber}] to check suppression amount!");
        }
#endif

        if (GlobalSettings.Mind.SUPP_AMOUNTS.TryGetValue(ECaliber.Default, out result))
        {
            return result;
        }
#if DEBUG
        if (SAINPlugin.DebugMode)
        {
            Logger.LogWarning($"Could not find Default Caliber Value to check suppression amount!");
        }
#endif
        return defaultNum;
    }

    private void checkState()
    {
        if (_tickTime < Time.time)
        {
            _tickTime = Time.time + _settings.SUP_CHECK_FREQ;

            if (SuppressionNumber <= 0)
            {
                if (CurrentState != ESuppressionState.None)
                {
                    applyNewState(ESuppressionState.None, null);
                }
                return;
            }

            ESuppressionState newState = SuppressionHelpers.FindActiveState(SuppressionNumber, out SuppressionConfig config);
            if (CurrentState == newState)
            {
                return;
            }
            applyNewState(newState, config);
        }
    }

    private void decaySuppression()
    {
        if (SuppressionNumber > 0 &&
            _decayTime < Time.time)
        {
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

        if (newState == ESuppressionState.None || config == null)
        {
            return;
        }

        _temporaryStatModifiers = createMods(config);
        BotOwner?.Settings?.Current?.Apply(_temporaryStatModifiers.Modifiers);
        OnSuppressionStateChanged?.Invoke(newState);
    }

    private void clearModifiers()
    {
        if (_temporaryStatModifiers != null)
        {
            if (_temporaryStatModifiers.Modifiers.IsApplyed)
            {
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
        if (value == 1f)
        {
            return 1f;
        }
        resistance = Mathf.Clamp01(resistance);
        return Mathf.Lerp(value, 0f, resistance);
    }

    private void EnemyRemoved(string profileId, Enemy enemy)
    {
        if (LastSuppressByEnemy == enemy)
        {
            LastSuppressByEnemy = null;
            ResetSuppressing();
        }
    }

    private MindSettings _settings => GlobalSettings.Mind;
    private float _decayTime;
    private float _tickTime;
    private TemporaryStatModifiers _temporaryStatModifiers;
}
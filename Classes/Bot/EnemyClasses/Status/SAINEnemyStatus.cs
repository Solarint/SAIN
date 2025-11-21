using EFT;
using SAIN.Models.Enums;
using System;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class SAINEnemyStatus(EnemyData enemyData) : EnemyBase(enemyData, enemyData.Enemy.Bot)
{
    public event Action<Enemy> OnEnemyShotAtMe;

    public ETagStatus EnemyHealthStatus { get; private set; }
    public EEnemyAction VulnerableAction { get; private set; }

    public void TickEnemy(float currentTime)
    {
        if (Enemy.EnemyKnown)
        {
            const float DOT_THRESHOLD_LOOK_AT_ME = 0.75f;
            const float DOT_THRESHOLD_POINTWEAPON_AT_ME = 0.75f;
            if (_nextCheckEnemyLookTime < currentTime)
            {
                _nextCheckEnemyLookTime = currentTime + 0.25f;
                EnemyLookAtMe = Vector3.Dot((Bot.Position - EnemyTransform.WeaponRoot).normalized, EnemyTransform.LookDirection) >= DOT_THRESHOLD_LOOK_AT_ME;
                PointingWeaponAtMe = Enemy.IsShooter() && Vector3.Dot((Bot.Position - EnemyTransform.WeaponData.FirePort).normalized, EnemyTransform.WeaponData.PointDirection) < DOT_THRESHOLD_POINTWEAPON_AT_ME;
            }
            UpdateVulnerableState();
            updateHealthStatus();
        }
    }

    private void updateHealthStatus()
    {
        if (_nextCheckHealthTime > Time.time)
        {
            return;
        }
        _nextCheckHealthTime = Time.time + 0.5f;

        ETagStatus lastStatus = EnemyHealthStatus;
        EnemyHealthStatus = EnemyPlayer.HealthStatus;
        if (lastStatus != EnemyHealthStatus)
        {
            Enemy.Events.HealthStatusChanged(EnemyHealthStatus);
        }
    }

    protected override void OnEnemyKnownChanged(bool known, Enemy enemy)
    {
        if (!known)
        {
            SetVulnerableAction(EEnemyAction.None);
        }
    }

    private EEnemyAction CheckVulnerableAction()
    {
        if (EnemyUsingSurgery)
        {
            return EEnemyAction.UsingSurgery;
        }
        if (EnemyIsReloading)
        {
            return EEnemyAction.Reloading;
        }
        if (EnemyHasGrenadeOut)
        {
            return EEnemyAction.HasGrenade;
        }
        if (EnemyIsHealing)
        {
            return EEnemyAction.Healing;
        }
        if (EnemyIsLooting)
        {
            return EEnemyAction.Looting;
        }
        return EEnemyAction.None;
    }

    private void UpdateVulnerableState()
    {
        VulnerableAction = CheckVulnerableAction();
    }

    public void SetVulnerableAction(EEnemyAction action)
    {
        if (action != VulnerableAction)
        {
            VulnerableAction = action;
            switch (action)
            {
                case EEnemyAction.None:
                    ResetActions();
                    break;

                case EEnemyAction.Reloading:
                    EnemyIsReloading = true;
                    break;

                case EEnemyAction.HasGrenade:
                    EnemyHasGrenadeOut = true;
                    break;

                case EEnemyAction.Healing:
                    EnemyIsHealing = true;
                    break;

                case EEnemyAction.Looting:
                    EnemyIsLooting = true;
                    break;

                case EEnemyAction.UsingSurgery:
                    EnemyUsingSurgery = true;
                    break;

                default:
                    break;
            }
        }
    }

    private void ResetActions()
    {
        HeardRecently = false;
        EnemyLookAtMe = false;
        PointingWeaponAtMe = false;
        ShotMeRecently = false;
        EnemyUsingSurgery = false;
        EnemyIsLooting = false;
        EnemyHasGrenadeOut = false;
        EnemyIsHealing = false;
        EnemyIsReloading = false;
        LastShotPosition = null;
    }

    public bool PositionalFlareEnabled =>
        Enemy.EnemyKnown &&
        Enemy.KnownPlaces.EnemyDistanceFromLastKnown < _maxDistFromPosFlareEnabled;

    public bool HeardRecently {
        get
        {
            return _heardRecently.Value;
        }
        set
        {
            _heardRecently.Value = value;
        }
    }

    public bool ShotMeRecently {
        get
        {
            return _shotByEnemy.Value;
        }
        set
        {
            if (value)
            {
                if (!ShotMe)
                {
                    ShotMe = true;
                    TimeFirstShot = Time.time;
                }
            }
            _shotByEnemy.Value = value;
        }
    }

    private void UpdateShotStatus()
    {
        if (!ShotMe)
        {
            ShotMe = true;
            TimeFirstShot = Time.time;
        }
    }

    private void UpdateShotPos()
    {
    }

    public bool PointingWeaponAtMe {
        get
        {
            return _pointWeaponAtMe.Value;
        }
        private set
        {
            _pointWeaponAtMe.Value = value;
        }
    }

    public bool EnemyUsingSurgery {
        get
        {
            return _enemySurgery.Value;
        }
        set
        {
            _enemySurgery.Value = value;
        }
    }

    public bool EnemyIsLooting {
        get
        {
            return _enemyLooting.Value;
        }
        set
        {
            _enemyLooting.Value = value;
        }
    }

    public bool SearchingBecauseLooting { get; set; }

    public bool EnemyIsSuppressed {
        get
        {
            return _enemyIsSuppressed.Value;
        }
        set
        {
            _enemyIsSuppressed.Value = value;
        }
    }

    public bool ShotAtMeRecently {
        get
        {
            return _enemyShotAtMe.Value;
        }
        set
        {
            _enemyShotAtMe.Value = value;
        }
    }

    public bool EnemyIsReloading {
        get
        {
            return _enemyIsReloading.Value;
        }
        set
        {
            _enemyIsReloading.Value = value;
        }
    }

    public bool EnemyHasGrenadeOut {
        get
        {
            return _enemyHasGrenade.Value;
        }
        set
        {
            _enemyHasGrenade.Value = value;
        }
    }

    public bool EnemyIsHealing {
        get
        {
            return _enemyIsHealing.Value;
        }
        set
        {
            _enemyIsHealing.Value = value;
        }
    }

    public bool EnemyLookAtMe {
        get
        {
            return _lookAtMe.Value;
        }
        set
        {
            _lookAtMe.Value = value;
        }
    }

    public int NumberOfSearchesStarted { get; set; }

    public void GetHit(DamageInfoStruct DamageInfoStruct)
    {
        IPlayer player = DamageInfoStruct.Player?.iPlayer;
        if (player != null && (object)player == Enemy.EnemyPlayer)
        {
            if (!ShotMe)
            {
                ShotMe = true;
                TimeFirstShot = Time.time;
            }
            TimeLastShotMe = Time.time;
            ShotMeRecently = true;
            Enemy.Events.ShotByEnemy();
        }
    }

    public bool ShotMe { get; private set; }
    public float TimeFirstShot { get; private set; } = -1f;
    public float TimeLastShotMe { get; private set; } = -1f;
    public Vector3? LastShotPosition { get; private set; }

    public bool ShotAtMe { get; private set; }
    public float TimeFirstShotAtMe { get; private set; } = -1f;
    public float TimeLastShotAtMe { get; private set; } = -1f;

    public void RegisterEnemyFlyBy()
    {
        float time = Time.time;
        if (!ShotAtMe)
        {
            TimeFirstShotAtMe = time;
            ShotAtMe = true;
        }
        ShotAtMeRecently = true;
        TimeLastShotAtMe = time;
        OnEnemyShotAtMe?.Invoke(Enemy);
    }


    private readonly ExpirableBool _heardRecently = new(2f, 0.5f, 1.5f);
    private readonly ExpirableBool _enemyIsReloading = new(4f, 0.5f, 1.5f);
    private readonly ExpirableBool _enemyHasGrenade = new(4f, 0.5f, 1.5f);
    private readonly ExpirableBool _enemyIsHealing = new(6f, 0.5f, 1.5f);
    private readonly ExpirableBool _enemyShotAtMe = new(5f, 0.5f, 1.5f);
    private readonly ExpirableBool _enemyIsSuppressed = new(4f, 0.5f, 1.5f);
    private readonly ExpirableBool _enemyLooting = new(15f, 0.5f, 1.5f);
    private readonly ExpirableBool _enemySurgery = new(8f, 0.5f, 1.5f);
    private readonly ExpirableBool _shotByEnemy = new(15f, 0.5f, 1.5f);
    private readonly ExpirableBool _lookAtMe = new(2f, 0.5f, 2f);
    private readonly ExpirableBool _pointWeaponAtMe = new(2f, 0.5f, 2f);
    private float _nextCheckEnemyLookTime;
    private const float _maxDistFromPosFlareEnabled = 10f;
    private float _nextCheckHealthTime;
}
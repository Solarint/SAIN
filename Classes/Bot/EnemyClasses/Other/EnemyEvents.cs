using EFT;
using SAIN.Helpers.Events;
using SAIN.Models.Enums;
using System;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class EnemyEvents
{
    public EnemyToggleEventTimeTracked OnEnemyLineOfSightChanged { get; }
    public EnemyToggleEventTimeTracked OnEnemyKnownChanged { get; }
    public EnemyToggleEventTimeTracked OnActiveThreatChanged { get; }
    public EnemyToggleEventTimeTracked OnVisionChange { get; }
    public EnemyToggleEventTimeTracked OnSearch { get; }
    public EnemyToggleEventTimeTracked OnEnemyCanShootChanged { get; }

    public event Action<Enemy> OnEnemyLocationsSearched;
    public event Action<Enemy> OnFirstSeen;
    public event Action<Enemy> OnEnemyShot;
    public event Action<Enemy> OnBeingShotByEnemy;
    public event Action<Enemy, EnemyPlace> OnPositionUpdated;
    public event Action<Enemy, SAINSoundType, bool, EnemyPlace> OnEnemyHeard;
    public event Action<Enemy, ETagStatus> OnHealthStatusChanged;

    public EnemyEvents(EnemyData enemy)
    {
        Enemy = enemy.Enemy;
        OnEnemyLineOfSightChanged = new EnemyToggleEventTimeTracked(enemy.Enemy, false);
        OnEnemyKnownChanged = new EnemyToggleEventTimeTracked(enemy.Enemy, false);
        OnActiveThreatChanged = new EnemyToggleEventTimeTracked(enemy.Enemy, false);
        OnVisionChange = new EnemyToggleEventTimeTracked(enemy.Enemy, false);
        OnSearch = new EnemyToggleEventTimeTracked(enemy.Enemy, false);
        OnEnemyCanShootChanged = new EnemyToggleEventTimeTracked(enemy.Enemy, false);
    }

    private readonly Enemy Enemy;

    public void Init(Player enemyPlayer)
    {
        enemyPlayer.BeingHitAction += enemyHit;
    }

    public void Dispose(Player enemyPlayer)
    {
        if (enemyPlayer != null) enemyPlayer.BeingHitAction -= enemyHit;
    }

    public void EnemyLocationsSearched()
    {
        OnEnemyLocationsSearched?.Invoke(Enemy);
    }

    public void LastKnownUpdated(EnemyPlace place, float currentTime)
    {
        OnPositionUpdated?.Invoke(Enemy, place);
        OnEnemyKnownChanged.CheckToggle(true, currentTime);
    }

    public void ShotByEnemy()
    {
        OnBeingShotByEnemy?.Invoke(Enemy);
    }

    public void HealthStatusChanged(ETagStatus status)
    {
        OnHealthStatusChanged?.Invoke(Enemy, status);
    }

    public void EnemyFirstSeen()
    {
        OnFirstSeen?.Invoke(Enemy);
    }

    public void EnemyHeard(SAINSoundType type, bool gunFire, EnemyPlace place)
    {
        OnEnemyHeard?.Invoke(Enemy, type, gunFire, place);
    }

    private void enemyHit(DamageInfoStruct damage, EBodyPart _, float _2)
    {
        var damageSource = damage.Player?.iPlayer;
        if (damageSource == null)
        {
            return;
        }
        if (damageSource.ProfileId == Enemy.Bot.ProfileId)
        {
            OnEnemyShot?.Invoke(Enemy);
        }
    }

    public class EnemyToggleEvent : ToggleEventForObject<Enemy>
    {
        public EnemyToggleEvent(Enemy enemy, bool defaultValue) : base(enemy, defaultValue)
        {
        }
    }

    public class EnemyToggleEventTimeTracked : ToggleEventForObjectTimeTracked<Enemy>
    {
        public EnemyToggleEventTimeTracked(Enemy enemy, bool defaultValue) : base(enemy, defaultValue)
        {
        }
    }
}
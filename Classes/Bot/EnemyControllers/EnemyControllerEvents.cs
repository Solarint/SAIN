using EFT;
using SAIN.Helpers.Events;
using SAIN.Models.Enums;
using System;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class EnemyControllerEvents : BotSubClass<SAINEnemyController>, IBotClass
{
    public ToggleEventTimeTracked OnPeaceChanged { get; } = new ToggleEventTimeTracked(true);
    public ToggleEvent ActiveHumanEnemyEvent { get; } = new ToggleEvent();
    public ToggleEvent HumanInLineOfSightEvent { get; } = new ToggleEvent();

    public event Action<Enemy> OnEnemyHit;

    public event Action<Enemy> OnEnemyAdded;

    public event Action<string, Enemy> OnEnemyRemoved;

    public event Action<Player> OnEnemyKilled;

    public event Action<Enemy, SAINSoundType, bool, EnemyPlace> OnEnemyHeard;

    public event Action<bool, Enemy> OnEnemyKnownChanged;

    public event Action<Enemy, Enemy> OnEnemyChanged;

    public event Action<ETagStatus, Enemy> OnEnemyHealthChanged;

    public EnemyControllerEvents(SAINEnemyController controller) : base(controller)
    {
    }

    public override void Init()
    {
        var knownEnemies = BaseClass.KnownEnemies;
        knownEnemies.OnListEmptyOrGetFirst += OnPeaceChanged.CheckToggle;
        knownEnemies.OnListEmptyOrGetFirstHuman += ActiveHumanEnemyEvent.CheckToggle;

        var enemiesInLOS = BaseClass.EnemiesInLineOfSight;
        enemiesInLOS.OnListEmptyOrGetFirstHuman += HumanInLineOfSightEvent.CheckToggle;
        base.Init();
    }

    public override void Dispose()
    {
        Bot.EnemyController.KnownEnemies.OnListEmptyOrGetFirst -= OnPeaceChanged.CheckToggle;
        Bot.EnemyController.KnownEnemies.OnListEmptyOrGetFirstHuman -= ActiveHumanEnemyEvent.CheckToggle;
        Bot.EnemyController.EnemiesInLineOfSight.OnListEmptyOrGetFirstHuman -= HumanInLineOfSightEvent.CheckToggle;
        base.Dispose();
    }

    private void enemyHealthChanged(Enemy enemy, ETagStatus health)
    {
        OnEnemyHealthChanged?.Invoke(health, enemy);
    }

    public void EnemyChanged(Enemy enemy, Enemy lastEnemy)
    {
        OnEnemyChanged?.Invoke(enemy, lastEnemy);
    }

    public void EnemyAdded(Enemy enemy)
    {
        enemy.EnemyPlayer.OnPlayerDead += enemyKilled;
        enemy.Events.OnEnemyHeard += enemyHeard;
        enemy.Events.OnEnemyKnownChanged.OnToggle += enemyKnownChanged;
        enemy.Events.OnEnemyShot += enemyHit;
        enemy.Events.OnHealthStatusChanged += enemyHealthChanged;
        OnEnemyAdded?.Invoke(enemy);
    }

    public void EnemyRemoved(string profileID, Enemy enemy)
    {
        if (enemy != null)
        {
            enemy.Events.OnEnemyHeard -= enemyHeard;
            enemy.Events.OnEnemyKnownChanged.OnToggle -= enemyKnownChanged;
            enemy.Events.OnEnemyShot -= enemyHit;
            enemy.Events.OnHealthStatusChanged -= enemyHealthChanged;
        }
        OnEnemyRemoved?.Invoke(profileID, enemy);
    }

    private void enemyKnownChanged(bool value, Enemy enemy)
    {
        OnEnemyKnownChanged?.Invoke(value, enemy);
    }

    private void enemyHit(Enemy enemy)
    {
        OnEnemyHit?.Invoke(enemy);
    }

    private void enemyKilled(Player player, IPlayer lastAggressor, DamageInfoStruct lastDamageInfoStruct, EBodyPart lastBodyPart)
    {
        if (player != null)
        {
            player.OnPlayerDead -= enemyKilled;
            if (lastAggressor != null &&
                lastAggressor.ProfileId == Bot.ProfileId)
            {
                OnEnemyKilled?.Invoke(player);
            }
        }
    }

    private void enemyHeard(Enemy enemy, SAINSoundType soundType, bool isDanger, EnemyPlace place)
    {
        OnEnemyHeard?.Invoke(enemy, soundType, isDanger, place);
    }
}
using EFT;
using SAIN.Classes.Transform;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings;
using UnityEngine;
using UnityEngine.UIElements;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public readonly struct EnemyData(Enemy enemy)
{
    public readonly Enemy Enemy = enemy;
    public readonly EnemyInfo EnemyInfo  = enemy.EnemyInfo;
    public readonly PlayerTransformClass EnemyTransform  = enemy.EnemyTransform;
    public readonly PlayerComponent EnemyPlayerComponent = enemy.EnemyPlayerComponent;
    public readonly Player EnemyPlayer = enemy.EnemyPlayer;
}

public abstract class EnemyBase
{
    protected EnemyBase(EnemyData enemyData, BotComponent bot)
    {
        Enemy = enemyData.Enemy;
        EnemyInfo = enemyData.EnemyInfo;
        EnemyTransform = enemyData.EnemyTransform;
        EnemyPlayerComponent = enemyData.EnemyPlayerComponent;
        EnemyPlayer = enemyData.EnemyPlayer;

        Bot = bot;
        PlayerComponent = bot.PlayerComponent;
        BotOwner = bot.BotOwner;
        Player = bot.PlayerComponent.Player;
    }

    protected Enemy Enemy { get; }
    protected EnemyInfo EnemyInfo { get; }
    protected PlayerTransformClass EnemyTransform { get; }
    protected PlayerComponent EnemyPlayerComponent { get; }
    protected Player EnemyPlayer { get; }

    public BotComponent Bot { get; }
    public PlayerComponent PlayerComponent  { get; }
    public BotOwner BotOwner  { get; }
    public Player Player { get; }

    public virtual void Init()
    {
        Enemy.OnEnemyDisposed += Dispose;
        PresetHandler.OnPresetUpdated += UpdatePresetSettings;
        Enemy.Events.OnEnemyKnownChanged.OnToggle += OnEnemyKnownChanged;
    }

    protected virtual void OnEnemyKnownChanged(bool known, Enemy enemy)
    {
    }

    protected virtual void UpdatePresetSettings(SAINPresetClass preset)
    {
    }

    public virtual void Dispose()
    {
        PresetHandler.OnPresetUpdated -= UpdatePresetSettings;
        Enemy.Events.OnEnemyKnownChanged.OnToggle -= OnEnemyKnownChanged;
        Enemy.OnEnemyDisposed -= Dispose;
    }

    protected Vector3 EnemyCurrentPosition => EnemyTransform.Position;
}
using SAIN.Components;
using SAIN.Preset;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System;

namespace SAIN;

public enum ESAINTickState
{
    NeverUpdate,
    AlwaysUpdate,
    OnlyBotActive,
    OnlyNoSleep,
    OnlyBotInCombat,
}

/// <summary>
/// A generic interface for any class owned by a bot.
/// </summary>
public interface IBotClass : IDisposable
{
    void Init();

    void ManualUpdate();

    bool ShallTick(float CurrentTime);

    BotComponent Bot { get; }
    ESAINTickState TickRequirement { get; }
    bool CanEverTick { get; }
    float TickInterval { get; }
    float LastTickTime { get; }
}

public interface IBotDecisionClass
{
    bool GetDecision(Enemy enemy, out string reason);
}

public interface IBotEnemyClass : IBotClass
{
    void OnEnemyKnownChanged(bool known, Enemy enemy);
}
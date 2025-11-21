using EFT;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent;

/// <summary>
/// This class serves as an abstract class for all the different SAIN classes to inherit from, mostly serves as a way to store a reference to the bot that owns the class, and shortcuts to properties related to the bot.
/// </summary>
public abstract class BotBase(BotComponent bot) : IBotClass
{
    public BotComponent Bot { get; } = bot;
    public PlayerComponent PlayerComponent => Bot.PlayerComponent;
    public BotOwner BotOwner => Bot.BotOwner;
    public Player Player => Bot.Player;

    protected static GlobalSettingsClass GlobalSettings => GlobalSettingsClass.Instance;

    public virtual void Init()
    {
        PresetHandler.OnPresetUpdated += UpdatePresetSettings;
    }

    /// <summary>
    /// Check if we should tick this frame, Set Last Tick Time if return true
    /// </summary>
    public virtual bool ShallTick(float CurrentTime)
    {
        if (CanEverTick && LastTickTime + TickInterval < CurrentTime)
        {
            LastTickTime = Time.time;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Set Last Tick Time
    /// </summary>
    public virtual void ManualUpdate()
    {
    }

    protected virtual void UpdatePresetSettings(SAINPresetClass preset)
    {
    }

    public virtual void Dispose()
    {
        PresetHandler.OnPresetUpdated -= UpdatePresetSettings;
    }

    public ESAINTickState TickRequirement { get; protected set; } = ESAINTickState.AlwaysUpdate;
    public bool CanEverTick { get; protected set; } = true;
    public float TickInterval { get; protected set; }
    public float LastTickTime { get; protected set; }
}

/// <summary>
/// This class serves as an abstract class for SAIN classes that exist as a property on a BotComponent to inherit from, mostly serves as a way to store a reference to the bot that owns the class, and shortcuts to properties related to the bot.
/// </summary>
public abstract class BotComponentClassBase : BotBase
{
    protected BotComponentClassBase(BotComponent bot) : base(bot)
    {
        bot.AddBotClass(this);
    }

    public override void Init()
    {
        Bot.AddBotTickClass(this);
        foreach (IBotClass Class in SubClasses)
        {
            Class.Init();
        }

        base.Init();
    }

    public override void ManualUpdate()
    {
        float time = Time.time;
        foreach (IBotClass Class in SubClasses)
        {
            Class.ManualUpdate();
        }

        base.ManualUpdate();
    }

    public override void Dispose()
    {
        foreach (IBotClass Class in SubClasses)
        {
            Class?.Dispose();
        }

        base.Dispose();
    }

    protected void AddSubClass(IBotClass Class)
    {
        SubClasses.Add(Class);
    }

    protected readonly List<IBotClass> SubClasses = [];
}

public abstract class BotSubClass<T>(T sainClass) : BotBase(sainClass.Bot) where T : IBotClass
{
    protected T BaseClass { get; } = sainClass;
}
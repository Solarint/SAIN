using SAIN.Components;
using System;

namespace SAIN.SAINComponent.Classes;

public class BotGlobalEventsClass : BotComponentClassBase
{
    public event Action<BotComponent> OnEnterPeace;

    public event Action<BotComponent> OnExitPeace;

    public BotGlobalEventsClass(BotComponent sain) : base(sain)
    {
        CanEverTick = false;
    }

    public override void Init()
    {
        Bot.EnemyController.Events.OnPeaceChanged.OnToggle += PeaceChanged;
        base.Init();
    }

    public override void Dispose()
    {
        Bot.EnemyController.Events.OnPeaceChanged.OnToggle -= PeaceChanged;
        base.Dispose();
    }

    public void PeaceChanged(bool value)
    {
        if (value)
            OnEnterPeace?.Invoke(Bot);
        else
            OnExitPeace?.Invoke(Bot);
    }
}
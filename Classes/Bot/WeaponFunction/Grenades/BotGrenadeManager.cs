using SAIN.Components;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.WeaponFunction;

public class BotGrenadeManager : BotComponentClassBase
{
    public ThrowWeapItemClass MyGrenade { get; set; }
    public Vector3? GrenadeDangerPoint => GrenadeReactionClass.GrenadeDangerPoint;

    public GrenadeThrowDecider GrenadeThrowDecider { get; }
    public GrenadeReactionClass GrenadeReactionClass { get; }

    public BotGrenadeManager(BotComponent sain) : base(sain)
    {
        TickRequirement = ESAINTickState.OnlyNoSleep;
        GrenadeThrowDecider = new GrenadeThrowDecider(this);
        GrenadeReactionClass = new GrenadeReactionClass(this);
    }

    public override void Init()
    {
        GrenadeThrowDecider.Init();
        GrenadeReactionClass.Init();
        base.Init();
    }

    public override void ManualUpdate()
    {
        GrenadeThrowDecider.ManualUpdate();
        GrenadeReactionClass.ManualUpdate();
        base.ManualUpdate();
    }

    public override void Dispose()
    {
        GrenadeThrowDecider.Dispose();
        GrenadeReactionClass.Dispose();
        base.Dispose();
    }
}
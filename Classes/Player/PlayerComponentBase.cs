using EFT;
using SAIN.Classes.Transform;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Components.PlayerComponentSpace.PersonClasses;
using System;
using UnityEngine;

namespace SAIN.SAINComponent;

public abstract class PlayerComponentBase(PlayerComponent player) : IDisposable
{
    public PlayerComponent PlayerComponent { get; private set; } = player;

    public Vector3 Position => PlayerComponent.Position;
    public Vector3 LookDirection => PlayerComponent.LookDirection;
    public PlayerTransformClass Transform => PlayerComponent.Transform;
    public Player Player => PlayerComponent.Player;

    public virtual void Dispose() { }
}
using EFT;
using SAIN.Classes.Transform;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Components.PlayerComponentSpace.PersonClasses;
using System;
using UnityEngine;

namespace SAIN.Components;

public abstract class BotComponentBase : MonoBehaviour, IDisposable
{
    public event Action OnDispose;

    public string ProfileId { get; private set; }

    public PlayerComponent PlayerComponent { get; private set; }
    public BotOwner BotOwner { get; private set; }
    public Player Player { get; private set; }
    public PlayerTransformClass Transform { get; private set; }

    public Vector3 Position => Transform.Position;
    public Vector3 LookDirection => Transform.LookDirection;

    public virtual bool Init(PlayerComponent playerComponent, BotOwner botOwner)
    {
        PlayerComponent = playerComponent;
        Player = playerComponent.Player;
        BotOwner = botOwner;
        Transform = playerComponent.Transform;
        ProfileId = Player.ProfileId;
        Player.ActiveHealthController.SetDamageCoeff(1f);
        return true;
    }

    public virtual void Dispose()
    {
        OnDispose?.Invoke();
    }
}
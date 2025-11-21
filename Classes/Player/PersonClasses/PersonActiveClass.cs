using EFT;
using System;
using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace.PersonClasses;

public class PersonActiveClass(PlayerComponent playerComponent)
{
    public event Action<bool> OnPlayerActiveChanged;

    public bool PlayerActive { get; private set; }
    public bool IsAlive { get; private set; } = true;

    public void CheckActive(PlayerComponent playerComponent)
    {
        if (IsAlive) 
            IsAlive = CheckAlive(playerComponent);

        bool wasActive = PlayerActive;
        PlayerActive = IsAlive && playerComponent.gameObject.activeInHierarchy;
        if (wasActive != PlayerActive)
        {
            OnPlayerActiveChanged?.Invoke(PlayerActive);
            //Logger.LogDebug($"Player {_person.Nickname} Active [{PlayerActive}]");
        }
    }

    public void Disable()
    {
        bool wasActive = PlayerActive;
        PlayerActive = false;
        if (wasActive != PlayerActive)
        {
            OnPlayerActiveChanged?.Invoke(PlayerActive);
        }
    }

    private static bool CheckAlive(PlayerComponent playerComponent)
    {
        if (playerComponent == null)
        {
            return false;
        }
        Player player = playerComponent.Player;
        if (player == null || player.gameObject == null || player.Transform?.Original == null)
        {
            return false;
        }
        if (player.HealthController?.IsAlive != true)
        {
            return false;
        }
        if (player.IsAI)
        {
            BotOwner botOwner = player.AIData?.BotOwner;
            if (botOwner == null || botOwner.Transform?.Original == null)
            {
                return false;
            }
        }
        return true;
    }
}
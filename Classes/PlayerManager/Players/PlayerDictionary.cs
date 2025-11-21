using EFT;
using System;
using System.Collections.Generic;

namespace SAIN.Components.PlayerComponentSpace;

public class PlayerDictionary : Dictionary<string, PlayerComponent>
{
    public event Action<string> OnPlayerComponentRemoved;
    
    public PlayerComponent GetPlayerComponent(IPlayer Player)
    {
        if (Player != null && 
            this.TryGetValue(Player.ProfileId, out PlayerComponent component))
        {
            return component;
        }
        return null;
    }

    public PlayerComponent GetPlayerComponent(string profileId)
    {
        if (!profileId.IsNullOrEmpty() &&
            this.TryGetValue(profileId, out PlayerComponent component))
        {
            return component;
        }
        return null;
    }

    public bool TryRemove(string profileId, out bool destroyedComponent)
    {
        destroyedComponent = false;
        if (profileId.IsNullOrEmpty())
        {
            ClearNullPlayers();
            return false;
        }
        if (this.TryGetValue(profileId, out PlayerComponent playerComponent))
        {
            OnPlayerComponentRemoved?.Invoke(profileId);
            if (playerComponent != null)
            {
                destroyedComponent = true;
                playerComponent.Dispose();
            }
            this.Remove(profileId);
            return true;
        }
        return false;
    }

    public void ClearNullPlayers()
    {
        foreach (KeyValuePair<string, PlayerComponent> kvp in this)
        {
            PlayerComponent component = kvp.Value;
            if (component == null ||
                component.Player == null)
            {
                _ids.Add(kvp.Key);
                if (component.Player != null)
                {
                    Logger.LogDebug($"Removing {component.Player.Profile?.Nickname} from player dictionary");
                }
            }
        }
        if (_ids.Count > 0)
        {
            Logger.LogDebug($"Removing {_ids.Count} null players");
            foreach (var id in _ids)
            {
                TryRemove(id, out _);
            }
            _ids.Clear();
        }
    }

    private readonly List<string> _ids = new();
}
using EFT;
using SAIN.Components.PlayerComponentSpace.PersonClasses;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings.Categories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using static DrakiaXYZ.BigBrain.Brains.BrainManager;

namespace SAIN.Components.PlayerComponentSpace;

public class PlayerSpawnTracker
{
    public event Action<PlayerComponent> OnPlayerAdded;
    public event Action<string, PlayerComponent> OnPlayerRemoved;
    public readonly HashSet<PlayerComponent> AlivePlayerArray = [];
    public readonly Dictionary<string, PlayerComponent> AlivePlayersDictionary = [];
    public readonly List<IPlayer> DeadPlayers = [];
    
    public PlayerComponent GetPlayerComponent(IPlayer Player)
    {
        if (Player != null && 
            AlivePlayersDictionary.TryGetValue(Player.ProfileId, out PlayerComponent component))
        {
            return component;
        }
        return null;
    }

    public PlayerComponent GetPlayerComponent(string profileId)
    {
        if (!profileId.IsNullOrEmpty() &&
            AlivePlayersDictionary.TryGetValue(profileId, out PlayerComponent component))
        {
            return component;
        }
        return null;
    }

    public PlayerComponent FindClosestHumanPlayer(out float closestPlayerSqrMag, Vector3 targetPosition, out Player player)
    {
        PlayerComponent closestPlayer = null;
        closestPlayerSqrMag = float.MaxValue;
        player = null;

        foreach (var component in AlivePlayersDictionary.Values)
        {
            if (component != null &&
                component.Player != null &&
                !component.IsAI)
            {
                float sqrMag = (component.Position - targetPosition).sqrMagnitude;
                if (sqrMag < closestPlayerSqrMag)
                {
                    player = component.Player;
                    closestPlayer = component;
                    closestPlayerSqrMag = sqrMag;
                }
            }
        }
        return closestPlayer;
    }

    public PlayerComponent FindClosestHumanPlayer(out float distance, PlayerComponent quierrier, out Player player)
    {
        List<OtherPlayerData> otherPlayers = quierrier.OtherPlayersData.DataList;
        otherPlayers.Sort((x, y) => x.DistanceData.Distance.CompareTo(y.DistanceData.Distance));
        for (int i = 0; i < otherPlayers.Count; i++)
        {
            OtherPlayerData otherPlayer = otherPlayers[i];
            if (otherPlayer != null && !otherPlayer.OtherPlayerComponent.IsAI)
            {
                distance = otherPlayer.DistanceData.Distance;
                player = otherPlayer.OtherPlayerComponent.Player;
                return otherPlayer.OtherPlayerComponent;
            }
        }
        distance = float.MaxValue;
        player = null;
        return null;
    }

    public PlayerComponent AddPlayerManual(IPlayer player)
    {
        if (player == null)
        {
            return null;
        }
        //Logger.LogDebug($"Manually trying to recreate Player Component for [{player.Profile?.Nickname} : {player.ProfileId}]");
        AddPlayer(player);
        if (AlivePlayersDictionary.TryGetValue(player.ProfileId, out var component))
        {
#if DEBUG
            Logger.LogDebug($"Successfully created new Player Component for [{player.Profile?.Nickname} : {player.ProfileId}]");
#endif
            return component;
        }
        return null;
    }

    private void AddPlayer(IPlayer iPlayer)
    {
        if (iPlayer == null)
        {
#if DEBUG
            Logger.LogError($"Could not add PlayerComponent for Null IPlayer.");
#endif
            return;
        }

        string profileId = iPlayer.ProfileId;
        Player player = iPlayer as Player;
        if (player == null)
        {
#if DEBUG
            Logger.LogError($"Could not add PlayerComponent for Null Player. IPlayer: {iPlayer.Profile?.Nickname} : {profileId}");
#endif
            return;
        }
        if (player.gameObject == null)
        {
#if DEBUG
            Logger.LogError($"Player Has null gameobject? IPlayer: {iPlayer.Profile?.Nickname} : {profileId}");
#endif
            return;
        }

        if (TryRemove(profileId, out bool compDestroyed))
        {
#if DEBUG
            Logger.LogWarning($"PlayerComponent already exists for Player: {player.name} : {player.Profile?.Nickname} : {profileId}");
            if (compDestroyed)
            {
                Logger.LogWarning($"Destroyed old Component for: {player.name} : {player.Profile?.Nickname} : {profileId}");
            }
#endif
        }
        if (TryAddPlayerComponent(player))
        {
#if DEBUG
            Logger.LogDebug($"Added New Player [{player.name}] : [{player.Profile.Nickname}]");
#endif
        }
    }

    private void RemovePerson(IPlayer player)
    {
        if (player == null)
        {
#if DEBUG
            Logger.LogError("Can't Remove player. Player Null");
#endif
            return;
        }
        if (TryRemove(player.ProfileId, out _))
        {
#if DEBUG
            Logger.LogDebug($"Removed Player Component [{player.Profile.Nickname}]]");
#endif
        }
        else
        {
#if DEBUG
            Logger.LogWarning("Could not find player in Player Component Dictionary!");
#endif
        }
#if DEBUG
#endif
    }

    public PlayerSpawnTracker(GameWorldComponent sainGameWorld)
    {
        _sainGameWorld = sainGameWorld;
        sainGameWorld.GameWorld.OnPersonAdd += AddPlayer;
    }

    public void Dispose()
    {
        var gameWorld = _sainGameWorld?.GameWorld;
        if (gameWorld != null)
        {
            gameWorld.OnPersonAdd -= AddPlayer;
        }
        foreach (var player in AlivePlayersDictionary)
        {
            player.Value?.Dispose();
        }
        AlivePlayersDictionary.Clear();
    }

    private bool TryAddPlayerComponent(Player player)
    {
        PlayerComponent component = player.gameObject.AddComponent<PlayerComponent>();
        if (component?.Init(player) == true)
        {
            player.OnIPlayerDeadOrUnspawn += RemovePerson;
            AlivePlayersDictionary.Add(player.ProfileId, component);
            AlivePlayerArray.Add(component);
            OnPlayerAdded?.Invoke(component);
#if DEBUG
            Logger.LogDebug($"Initialized Player Component {player.name} : {player.ProfileId}");
#endif
            return true;
        }
        else
        {
#if DEBUG
            Logger.LogError($"Init PlayerComponent Failed for {player.name} : {player.ProfileId}");
#endif
            GameObject.Destroy(component);
            return false;
        }
    }

    private bool TryRemove(string profileId, out bool destroyedComponent)
    {
        destroyedComponent = false;
        if (profileId.IsNullOrEmpty())
        {
            ClearNullPlayers();
            return false;
        }
        if (AlivePlayersDictionary.TryGetValue(profileId, out PlayerComponent playerComponent))
        {
            OnPlayerRemoved?.Invoke(profileId, playerComponent);
            if (playerComponent != null)
            {
                destroyedComponent = true;
                playerComponent.Dispose();
            }
            AlivePlayersDictionary.Remove(profileId);
            return true;
        }
        return false;
    }

    private void ClearNullPlayers()
    {
        foreach (KeyValuePair<string, PlayerComponent> kvp in AlivePlayersDictionary)
        {
            PlayerComponent component = kvp.Value;
            if (component == null ||
                component.Player == null)
            {
                _ids.Add(kvp.Key);
                if (component.Player != null)
                {
#if DEBUG
                    Logger.LogDebug($"Removing {component.Player.Profile?.Nickname} from player dictionary");
#endif
                }
            }
        }
        if (_ids.Count > 0)
        {
#if DEBUG
            Logger.LogDebug($"Removing {_ids.Count} null players");
#endif
            foreach (var id in _ids)
            {
                TryRemove(id, out _);
            }
            _ids.Clear();
        }
    }

    private readonly List<string> _ids = [];

    private readonly GameWorldComponent _sainGameWorld;
}
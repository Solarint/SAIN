using SAIN.SAINComponent;
using System.Collections.Generic;

namespace SAIN.Components.PlayerComponentSpace;

public class OtherPlayersData : PlayerComponentBase
{
    public Dictionary<string, OtherPlayerData> DataDictionary { get; } = [];
    public HashSet<OtherPlayerData> DataHashSet { get; } = [];
    public List<OtherPlayerData> DataList { get; } = [];

    public OtherPlayersData(PlayerComponent playerComponent) : base(playerComponent)
    {
        var playerTracker = GameWorldComponent.Instance?.PlayerTracker;
        if (playerTracker == null)
        {
#if DEBUG
            Logger.LogError("player tracker null");
#endif
            return;
        }
        // Subscribe to player added or removed events
        playerTracker.OnPlayerAdded += PlayerAdded;
        playerTracker.OnPlayerRemoved += PlayerRemoved;
        // Add any already existing player.
        foreach (PlayerComponent player in playerTracker.AlivePlayerArray)
            PlayerAdded(player);
    }

    public override void Dispose()
    {
        var playerTracker = GameWorldComponent.Instance?.PlayerTracker;
        if (playerTracker != null)
        {
            playerTracker.OnPlayerAdded -= PlayerAdded;
            playerTracker.OnPlayerRemoved -= PlayerRemoved;
        }

        foreach (OtherPlayerData data in DataHashSet)
        {
            data.Dispose();
        }
        DataDictionary.Clear();
        DataHashSet.Clear();
        DataList.Clear();
    }

    private void PlayerAdded(PlayerComponent playerComp)
    {
        if (playerComp == PlayerComponent)
        {
            return;
        }
        if (playerComp == null)
        {
            //Logger.LogWarning("Player Component is null. Cannot add other player data!");
            return;
        }
        if (playerComp.Player?.HealthController?.IsAlive != true)
        {
            //Logger.LogWarning("Player is dead. Cannot add other player data!");
            return;
        }
        string profileId = playerComp.ProfileId;
        if (profileId == PlayerComponent.ProfileId)
        {
            return;
        }

        // Just in-case make sure we remove any data under this profile id
        if (DataDictionary.TryGetValue(profileId, out OtherPlayerData Data))
        {
            DataHashSet.Remove(Data);
            DataDictionary.Remove(profileId);
            //Logger.LogWarning($"Removed Existing Playerdata for profile ID [{profileId}] : Old Data Nickname: [{Data.OtherPlayerComponent?.Player?.Profile.Nickname}] : New Data Nickname: [{playerComp.Player?.Profile.Nickname}]");
        }

        Data = new(profileId, playerComp);
        DataHashSet.Add(Data);
        DataDictionary.Add(profileId, Data);
        DataList.Add(Data);
    }

    private void PlayerRemoved(string profileId, PlayerComponent playerComp)
    {
        if (DataDictionary.TryGetValue(profileId, out OtherPlayerData Data))
        {
            Data.Dispose();
            DataHashSet.Remove(Data);
            DataDictionary.Remove(profileId);
            DataList.Remove(Data);
        }
    }
}
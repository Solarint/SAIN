using Comfort.Common;
using EFT;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections.Generic;

namespace SAIN.Helpers;

internal class GameWorldInfo
{
    public static bool IsEnemyMainPlayer(Enemy enemy)
    {
        Player player = enemy?.EnemyPlayer;
        Player mainPlayer = GameWorld?.MainPlayer;
        return
            player != null &&
            mainPlayer != null &&
            player.ProfileId == mainPlayer.ProfileId;
    }

    public static Player GetAlivePlayer(IPlayer person) => GetAlivePlayer(person?.ProfileId);

    public static Player GetAlivePlayer(string profileID) => GameWorld?.GetAlivePlayerByProfileID(profileID);

    public static GameWorld GameWorld => Singleton<GameWorld>.Instance;
    public static List<Player> AlivePlayers => GameWorld?.AllAlivePlayersList;
    public static Dictionary<string, Player> AlivePlayersDictionary => GameWorld?.allAlivePlayersByID;
}

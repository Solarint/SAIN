using Comfort.Common;
using EFT;
using SAIN.Classes;
using SAIN.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.Helpers
{
    internal class EFTInfo
    {
        public static bool IsEnemyMainPlayer(BotOwner bot)
        {
            Player player = GetPlayer(bot?.Memory?.GoalEnemy?.Person);
            return IsPlayerMainPlayer(player);
        }
        public static bool IsEnemyMainPlayer(SAINEnemy enemy)
        {
            Player player = enemy?.EnemyPlayer;
            return IsPlayerMainPlayer(player);
        }
        public static bool IsEnemyMainPlayer(SAINComponent bot)
        {
            return IsEnemyMainPlayer(bot?.Enemy);
        }
        public static bool IsPlayerMainPlayer(Player player)
        {
            return player != null && Compare(player, MainPlayer);
        }
        public static bool IsPlayerMainPlayer(IAIDetails player)
        {
            return player != null && Compare(player, MainPlayer);
        }

        public static string ProfileId(DamageInfo damage) => damage.Player?.iPlayer?.ProfileId;
        public static string ProfileId(IAIDetails player) => player?.ProfileId;
        public static string ProfileId(Player player) => player?.ProfileId;
        public static string ProfileId(BotOwner bot) => bot?.ProfileId;

        public static Player GetPlayer(SAINComponent bot) => GetPlayer(bot?.ProfileId);
        public static Player GetPlayer(BotOwner bot) => GetPlayer(bot?.ProfileId);
        public static Player GetPlayer(IAIDetails person) => GetPlayer(person?.ProfileId);
        public static Player GetPlayer(string profileID) => GameWorld?.GetAlivePlayerByProfileID(profileID);

        public static bool Compare(IAIDetails A, IAIDetails B) => Compare(A?.ProfileId, B?.ProfileId);
        public static bool Compare(Player A, IAIDetails B) => Compare(A?.ProfileId, B?.ProfileId);
        public static bool Compare(IAIDetails A, Player B) => Compare(A?.ProfileId, B?.ProfileId);

        public static bool Compare(Player A, Player B) => Compare(A?.ProfileId, B?.ProfileId);
        public static bool Compare(Player A, string B) => Compare(A?.ProfileId, B);
        public static bool Compare(string A, Player B) => Compare(A, B);

        public static bool Compare(SAINComponent A, SAINComponent B) => Compare(A?.ProfileId, B?.ProfileId);
        public static bool Compare(SAINComponent A, BotOwner B) => Compare(A?.ProfileId, B?.ProfileId);
        public static bool Compare(BotOwner A, SAINComponent B) => Compare(A?.ProfileId, B?.ProfileId);

        public static bool Compare(SAINComponent A, Player B) => Compare(A?.ProfileId, B?.ProfileId);
        public static bool Compare(Player A, SAINComponent B) => Compare(A?.ProfileId, B?.ProfileId);

        public static bool Compare(BotOwner A, BotOwner B) => Compare(A?.ProfileId, B?.ProfileId);
        public static bool Compare(BotOwner A, Player B) => Compare(A?.ProfileId, B?.ProfileId);
        public static bool Compare(Player A, BotOwner B) => Compare(A?.ProfileId, B?.ProfileId);

        public static bool Compare(string A, string B) => A == B;

        public static GameWorld GameWorld => Singleton<GameWorld>.Instance;
        public static Player MainPlayer => GameWorld?.MainPlayer;
        public static List<IAIDetails> AllPlayers => GameWorld?.RegisteredPlayers;
        public static List<Player> AlivePlayers => GameWorld?.AllAlivePlayersList;
        public static Dictionary<string, Player> AlivePlayersDictionary => GameWorld?.allAlivePlayersByID;
    }
}

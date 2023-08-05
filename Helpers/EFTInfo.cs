using Comfort.Common;
using EFT;

using SAIN.Components;
using System;
using System.Collections.Generic;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;

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
        public static bool IsEnemyMainPlayer(SAINComponentClass bot)
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

        public static Player GetPlayer(SAINComponentClass bot) => GetPlayer(bot?.ProfileId);
        public static Player GetPlayer(BotOwner bot) => GetPlayer(bot?.ProfileId);
        public static Player GetPlayer(IAIDetails person) => GetPlayer(person?.ProfileId);
        public static Player GetPlayer(string profileID) => GameWorld?.GetAlivePlayerByProfileID(profileID);

        public static bool Compare(IAIDetails A, IAIDetails B) => Compare(A?.ProfileId, B?.ProfileId);
        public static bool Compare(Player A, IAIDetails B) => Compare(A?.ProfileId, B?.ProfileId);
        public static bool Compare(IAIDetails A, Player B) => Compare(A?.ProfileId, B?.ProfileId);

        public static bool Compare(Player A, Player B) => Compare(A?.ProfileId, B?.ProfileId);
        public static bool Compare(Player A, string B) => Compare(A?.ProfileId, B);
        public static bool Compare(string A, Player B) => Compare(A, B);

        public static bool Compare(string A, string B) => A == B;

        public static GameWorld GameWorld => Singleton<GameWorld>.Instance;
        public static Player MainPlayer => GameWorld?.MainPlayer;
        public static List<IAIDetails> AllPlayers => GameWorld?.RegisteredPlayers;
        public static List<Player> AlivePlayers => GameWorld?.AllAlivePlayersList;
        public static Dictionary<string, Player> AlivePlayersDictionary => GameWorld?.allAlivePlayersByID;
    }
}

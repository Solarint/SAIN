using EFT;
using SAIN.SAINPreset.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAIN.Helpers
{
    internal class EnumValues
    {
        internal class WildSpawn
        {
            static WildSpawn()
            {
                Bosses = CheckAdd("boss");
                Followers = CheckAdd("follower");
            }

            static List<WildSpawnType> CheckAdd(string search)
            {
                var list = new List<WildSpawnType>();
                foreach (WildSpawnType type in GetEnum<WildSpawnType>())
                {
                    if (type.ToString().ToLower().StartsWith(search))
                    {
                        list.Add(type);
                    }
                }
                return list;
            }

            public static bool IsFollower(WildSpawnType type) => Followers.Contains(type);
            public static bool IsBoss(WildSpawnType type) => Bosses.Contains(type);
            public static bool IsScav(WildSpawnType type) => Scavs.Contains(type);
            public static bool IsPMC(WildSpawnType type) => PMCs.Contains(type);

            public static WildSpawnType[] PMCs =
            {
                Usec,
                Bear
            };

            public static WildSpawnType[] Scavs =
            {
                WildSpawnType.assault,
                WildSpawnType.crazyAssaultEvent,
                WildSpawnType.cursedAssault,
                WildSpawnType.marksman
            };

            public static readonly WildSpawnType Usec = Parse<WildSpawnType>("sptUsec");
            public static readonly WildSpawnType Bear = Parse<WildSpawnType>("sptBear");

            public static List<WildSpawnType> Bosses;
            public static List<WildSpawnType> Followers;
        }

        public static T Parse<T>(string value) => (T)Enum.Parse(typeof(T), value);

        public static BotDifficulty[] Difficulties => GetEnum<BotDifficulty>();
        public static WildSpawnType[] WildSpawnTypes => GetEnum<WildSpawnType>();
        public static Caliber[] AmmoCalibers => GetEnum<Caliber>();
        public static WeaponClass[] WeaponClasses => GetEnum<WeaponClass>();
        public static SAINPersonality[] Personalities => GetEnum<SAINPersonality>();

        public static T[] GetEnum<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}
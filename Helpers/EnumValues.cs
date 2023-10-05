using EFT;
using System;
using System.Collections.Generic;
using System.Linq;

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

            private static List<WildSpawnType> CheckAdd(string search)
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

            public static bool IsPMC(WildSpawnType type)
            {
                return type.ToString() == "sptUsec" || type.ToString() == "sptBear";
            }

            public static WildSpawnType[] Scavs =
            {
                WildSpawnType.assault,
                WildSpawnType.assaultGroup,
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

        public static readonly BotDifficulty[] Difficulties = { BotDifficulty.easy, BotDifficulty.normal, BotDifficulty.hard, BotDifficulty.impossible };
        public static readonly WildSpawnType[] WildSpawnTypes = GetEnum<WildSpawnType>();

        public static readonly ICaliber[] AmmoCalibers = GetEnum<ICaliber>();
        public static readonly IWeaponClass[] WeaponClasses = GetEnum<IWeaponClass>();

        public static readonly IPersonality[] Personalities = GetEnum<IPersonality>();

        public static readonly SoloDecision[] SoloDecisions = GetEnum<SoloDecision>();
        public static readonly SquadDecision[] SquadDecisions = GetEnum<SquadDecision>();
        public static readonly SelfDecision[] SelfDecisions = GetEnum<SelfDecision>();

        public static ICaliber ParseCaliber(string caliber)
        {
            if (Enum.TryParse(caliber, out ICaliber result))
            {
                return result;
            }
            Logger.LogError(caliber);
            return ICaliber.Default;
        }

        public static IWeaponClass ParseWeaponClass(string weaponClass)
        {
            if (Enum.TryParse(weaponClass, out IWeaponClass result))
            {
                return result;
            }
            Logger.LogError(weaponClass);
            return IWeaponClass.Default;
        }

        public static T[] GetEnum<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}
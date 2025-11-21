using EFT;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAIN.Helpers;

internal static class EnumValues
{
    internal static class WildSpawn
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

        public static bool IsScav(WildSpawnType type) => Scavs.Contains(type);

        public static bool IsGoons(WildSpawnType type)
        {
            return Goons.Contains(type);
        }

        public static WildSpawnType[] Scavs =
        [
            WildSpawnType.assault,
            WildSpawnType.assaultGroup,
            WildSpawnType.crazyAssaultEvent,
            WildSpawnType.cursedAssault,
            WildSpawnType.marksman
        ];

        public static WildSpawnType[] Goons =
        [
            WildSpawnType.bossKnight,
            WildSpawnType.followerBigPipe,
            WildSpawnType.followerBirdEye,
        ];

        public static List<WildSpawnType> Bosses;
        public static List<WildSpawnType> Followers;
    }

    public static T Parse<T>(string value) => (T)Enum.Parse(typeof(T), value);

    public static readonly BotDifficulty[] Difficulties = [BotDifficulty.easy, BotDifficulty.normal, BotDifficulty.hard, BotDifficulty.impossible];
    public static readonly WildSpawnType[] WildSpawnTypes = GetEnum<WildSpawnType>();

    public static readonly ECaliber[] AmmoCalibers = GetEnum<ECaliber>();
    public static readonly EWeaponClass[] WeaponClasses = GetEnum<EWeaponClass>();

    public static readonly EPersonality[] Personalities = GetEnum<EPersonality>();

    public static readonly ECombatDecision[] SoloDecisions = GetEnum<ECombatDecision>();
    public static readonly ESquadDecision[] SquadDecisions = GetEnum<ESquadDecision>();
    public static readonly ESelfActionType[] SelfDecisions = GetEnum<ESelfActionType>();

    public static ECaliber ParseCaliber(string caliber)
    {
        if (Enum.TryParse(caliber, out ECaliber result))
        {
            return result;
        }
        Logger.LogError($"Caliber [{caliber}] does not exist in Caliber Enum!");
        return ECaliber.Default;
    }

    public static EWeaponClass ParseWeaponClass(string weaponClass)
    {
        if (Enum.TryParse(weaponClass, out EWeaponClass result))
        {
            return result;
        }
        Logger.LogError($"Weapon Class [{weaponClass}] does not exist in IWeaponClass Enum!");
        return EWeaponClass.Default;
    }

    public static T TryParse<T>(string _string) where T : struct, Enum
    {
        if (Enum.TryParse(_string, out T result))
        {
            return result;
        }
        Logger.LogError($"[{_string}] does not exist in [{typeof(T)}] Enum!");
        return default;
    }

    public static T[] GetEnum<T>()
    {
        return (T[])Enum.GetValues(typeof(T));
    }
}
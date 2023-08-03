using EFT;
using SAIN.SAINPreset.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.Helpers
{
    internal class EnumValues
    {
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

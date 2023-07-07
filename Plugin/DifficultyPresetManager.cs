using EFT;
using SAIN.Helpers;
using System;
using System.Collections.Generic;

//using System.Linq;
using System.Reflection;

namespace SAIN
{
    public class SAINBotPresetManager
    {
        public static string[] WildSpawnTypes { get; private set; }
        public static string[] ConvertedWildSpawnTypes { get; private set; }

        public static string[] Difficulties = { "easy", "normal", "hard", "impossible" };
        public static string[] PropertyNames { get; private set; }

        public static Action<WildSpawnType, BotDifficulty, SAINBotPreset> PresetUpdated { get; set; }

        public static void Init()
        {
            CreateJsons();
            AssignProperties();
        }

        public static void SavePreset(SAINBotPreset preset)
        {
            UpdatePreset(preset);
        }

        public static void ClonePresetList(List<SAINBotPreset> presets, SAINBotPreset clone, BotDifficulty difficulty)
        {
            for (int i = 0; i < presets.Count; i++)
            {
                presets[i] = CopyAllProperties(presets[i], clone, difficulty);
                UpdatePreset(presets[i]);
            }
        }

        public static SAINBotPreset LoadPreset(KeyValuePair<WildSpawnType, BotDifficulty> keypair)
        {
            if (Presets.ContainsKey(keypair))
            {
                return Presets[keypair];
            }
            return null;
        }

        public static SAINBotPreset LoadPreset(WildSpawnType type, BotDifficulty difficulty)
        {
            foreach (var pair in TypeDiffs)
            {
                if (pair.Key == type && pair.Value == difficulty)
                {
                    return LoadPreset(pair);
                }
            }
            return null;
        }

        public static SAINBotPreset LoadPreset(string type, string difficulty)
        {
            return LoadPreset(GetType(type), GetDiff(difficulty));
        }

        public static SAINBotPreset LoadPreset(WildSpawnType type, string difficulty)
        {
            return LoadPreset(type, GetDiff(difficulty));
        }

        public static SAINBotPreset LoadPreset(string type, BotDifficulty difficulty)
        {
            return LoadPreset(GetType(type), difficulty);
        }

        public static WildSpawnType GetType(string type)
        {
            return (WildSpawnType)Enum.Parse(typeof(WildSpawnType), type);
        }

        public static BotDifficulty GetDiff(string diff)
        {
            return (BotDifficulty)Enum.Parse(typeof(BotDifficulty), diff);
        }

        public static void CreateJsons()
        {
            List<string> strings = new List<string>();
            foreach (WildSpawnType type in GetTypeEnums())
            {
                strings.Add(type.ToString());
                foreach (BotDifficulty diff in Enum.GetValues(typeof(BotDifficulty)))
                {
                    TypeDiffs.Add(new KeyValuePair<WildSpawnType, BotDifficulty>(type, diff));
                }
            }

            CreateWildSpawnTypeArray(strings);

            foreach (var pair in TypeDiffs)
            {
                SAINBotPreset preset = JsonUtility.LoadFromJson.DifficultyPreset(pair) ?? new SAINBotPreset(pair);
                Presets.Add(pair, preset);
            }
            foreach (var preset in Presets)
            {
                JsonUtility.SaveToJson.DifficultyPreset(preset.Value);
            }
        }

        private static WildSpawnType[] GetTypeEnums()
        {
            List<WildSpawnType> types = new List<WildSpawnType>();
            foreach (WildSpawnType type in Enum.GetValues(typeof(WildSpawnType)))
            {
                if (!TypeExclusions.Contains(type))
                {
                    types.Add(type);
                }
            }
            return types.ToArray();
        }

        private static void CreateWildSpawnTypeArray(List<string> types)
        {
            WildSpawnTypes = types.ToArray();
            Array.Sort(WildSpawnTypes);

            List<string> list = new List<string>(WildSpawnTypes);

            foreach (var kvp in TypeIndexes)
            {
                int currentIndex = list.IndexOf(kvp.Key);
                MoveEntryToIndex(list, currentIndex, kvp.Value);
            }

            WildSpawnTypes = list.ToArray();
            ConvertedWildSpawnTypes = ConvertNames(WildSpawnTypes, false);
        }

        private static void MoveEntryToIndex(List<string> list, int currentIndex, int desiredIndex)
        {
            string entry = list[currentIndex];
            list.RemoveAt(currentIndex);
            list.Insert(desiredIndex, entry);
        }

        public static string[] ConvertNames(string[] array, bool revert)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = GetConvertedName(array[i], revert);
            }

            return array;
        }

        public static string GetConvertedName(string name, bool getConvertedName)
        {
            if (getConvertedName)
            {
                foreach (var kvp in NameConversions)
                {
                    if (kvp.Value == name)
                    {
                        name = kvp.Key;
                        break;
                    }
                }
            }
            else
            {
                if (NameConversions.ContainsKey(name))
                {
                    name = NameConversions[name];
                }
            }
            return name;
        }

        private static readonly List<WildSpawnType> TypeExclusions = new List<WildSpawnType>
        {
            WildSpawnType.test,
            WildSpawnType.bossTest,
            WildSpawnType.followerTest,
            WildSpawnType.gifter,
            WildSpawnType.assaultGroup
        };

        private static readonly Dictionary<string, int> TypeIndexes = new Dictionary<string, int>
            {
                { "assault", 0 },
                { "sptUsec", 1 },
                { "sptBear", 2 }
            };


        //   WildSpawnType                                      // DisplayName             // Section       // Description
        public static BotType[] BotTypes =
        {
            new BotType( WildSpawnType.assault,                 "Scav",                     "Scavs" ,       "Scavs!" ),
            new BotType( WildSpawnType.marksman,                "Scav Sniper",              "Scavs" ,       "The Scav Snipers that spawn on rooftops on certain maps" ),
            new BotType( WildSpawnType.cursedAssault,           "Tagged and Cursed Scav",   "Scavs" ,       "The type a scav is assigned when the player is marked as Tagged and Cursed" ),

            new BotType( WildSpawnType.bossKnight,              "Knight",                   "Goons" ,       "Goons leader. Close proximity to the goons has been noted to cause smashed keyboards" ),
            new BotType( WildSpawnType.followerBigPipe,         "BigPipe" ,                 "Goons" ,       "Goons follower. Close proximity to the goons has been noted to cause smashed keyboards\"" ),
            new BotType( WildSpawnType.followerBirdEye,         "BirdEye",                  "Goons" ,       "Goons follower. Close proximity to the goons has been noted to cause smashed keyboards\"" ),

            new BotType( "sptUsec",                             "PMC Usec",                 "PMCs" ,        "A PMC of the Usec Faction" ),
            new BotType( "sptBear",                             "PMC Bear",                 "PMCs" ,        "A PMC of the Bear Faction" ),

            new BotType( WildSpawnType.exUsec,                  "Rogue",                    "Other" ,       "Ex Usec Personel on Lighthouse usually found around the water treatment plant" ),
            new BotType( WildSpawnType.pmcBot,                  "Raider",                   "Other" ,       "Heavily armed scavs typically found on reserve and Labs by default" ),
            new BotType( WildSpawnType.arenaFighterEvent,       "Bloodhound",               "Other" ,       "From the Live Event, nearly identical to raiders except with different voicelines and better gear. Found in" ),

            new BotType( WildSpawnType.sectantPriest,           "Cultist Priest",           "Other" ,       "Found on Customs, Woods, Factory, Shoreline at night" ),
            new BotType( WildSpawnType.sectantWarrior,          "Cultist",                  "Other" ,       "Found on Customs, Woods, Factory, Shoreline at night" ),

            new BotType( WildSpawnType.bossKilla,               "Killa",                    "Bosses" ,      "He shoot. Found on Interchange and Streets" ),

            new BotType( WildSpawnType.bossBully,               "Rashala",                  "Bosses" ,      "Customs Boss" ),
            new BotType( WildSpawnType.followerBully,           "Rashala Guard",            "Followers" ,   "Customs Boss Follower" ),

            new BotType( WildSpawnType.bossKojaniy,             "Shturman",                 "Bosses" ,      "Woods Boss" ),
            new BotType( WildSpawnType.followerKojaniy,         "Shturman Guard",           "Followers" ,   "Woods Boss Follower" ),

            new BotType( WildSpawnType.bossTagilla,             "Tagilla",                  "Bosses" ,      "He Smash" ),
            new BotType( WildSpawnType.followerTagilla,         "Tagilla Guard",            "Followers" ,   "They Smash Too?" ),

            new BotType( WildSpawnType.bossSanitar,             "Sanitar",                  "Bosses" ,      "Shoreline Boss" ),
            new BotType( WildSpawnType.followerSanitar,         "Sanitar Guard",            "Followers" ,   "Shoreline Boss Follower" ),

            new BotType( WildSpawnType.bossGluhar,              "Gluhar",                   "Bosses" ,      "Reserve Boss. Also can be found on Streets." ),
            new BotType( WildSpawnType.followerGluharSnipe,     "Gluhar Guard Snipe",       "Followers" ,   "Reserve Boss Follower" ),
            new BotType( WildSpawnType.followerGluharScout,     "Gluhar Guard Scout",       "Followers" ,   "Reserve Boss Follower" ),
            new BotType( WildSpawnType.followerGluharSecurity,  "Gluhar Guard Security",    "Followers" ,   "Reserve Boss Follower" ),
            new BotType( WildSpawnType.followerGluharAssault,   "Gluhar Guard Assault",     "Followers" ,   "Reserve Boss Follower" ),

            new BotType( WildSpawnType.bossZryachiy,            "Zryachiy",                 "Bosses" ,      "Lighthouse Island Sniper Boss" ),
            new BotType( WildSpawnType.followerZryachiy,        "Zryachiy Guard",           "Followers" ,   "Lighthouse Island Sniper Boss Follower" )
        };

        public static readonly Dictionary<string, string> NameConversions = new Dictionary<string, string>
            {
                { "assault", "Scav" },
                { "marksman", "Scav Sniper" },
                { "cursedAssault", "Tagged and Cursed Scav" },

                { "bossKnight", "Knight" },
                { "followerBigPipe", "BigPipe" },
                { "followerBirdEye", "BirdEye" },

                { "sptUsec", "PMC Usec" },
                { "sptBear", "PMC Bear" },
                { "exUsec", "Rogue" },
                { "pmcBot", "Raider" },
                { "sectantPriest", "Cultist Priest" },
                { "sectantWarrior", "Cultist" },
                { "arenaFighterEvent", "Bloodhound" },
                { "bossBully", "Rashala" },
                { "followerBully", "Rashala Guard" },
                { "bossKojaniy", "Shturman" },
                { "followerKojaniy", "Shturman Guard" },

                { "bossTagilla", "Tagilla" },
                { "followerTagilla", "Tagilla Guard" },
                { "followerSanitar", "Sanitar Guard" },
                { "bossSanitar", "Sanitar" },
                { "followerGluharSnipe", "Gluhar Guard Snipe" },
                { "followerGluharScout", "Gluhar Guard Scout" },
                { "followerGluharSecurity", "Gluhar Guard Security" },
                { "followerGluharAssault", "Gluhar Guard Assault" },
                { "bossGluhar", "Gluhar" },
                { "bossKilla", "Killa" },
                { "bossZryachiy", "Zryachiy" },
                { "followerZryachiy", "Zryachiy Guard" }
            };

        public static void UpdatePreset(SAINBotPreset preset)
        {
            var Pair = preset.KeyPair;
            if (Presets.ContainsKey(Pair))
            {
                Presets[Pair] = preset;
                JsonUtility.SaveToJson.DifficultyPreset(preset);
                if (SAINPlugin.BotController != null && SAINPlugin.BotController.Bots.Count > 0)
                {
                    PresetUpdated(Pair.Key, Pair.Value, preset);
                }
            }
        }

        private static void AssignProperties()
        {
            if (Properties == null)
            {
                var list = new List<PropertyInfo>();
                var names = new List<string>();
                PropertyInfo[] properties = typeof(SAINBotPreset).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo property in properties)
                {
                    Type propertyType = property.PropertyType;
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(SAINProperty<>))
                    {
                        names.Add(property.Name);
                        list.Add(property);
                    }
                }
                PropertyNames = names.ToArray();
                Properties = list.ToArray();
            }
        }

        public static SAINBotPreset CopyAllProperties(SAINBotPreset target, SAINBotPreset source, BotDifficulty difficulty)
        {
            foreach (PropertyInfo property in Properties)
            {
                Copy(target, source, property, difficulty);
            }
            return target;
        }

        public static SAINBotPreset Copy(SAINBotPreset target, SAINBotPreset source, PropertyInfo property, BotDifficulty difficulty)
        {
            var targetProp = property.GetValue(target);
            var sourceProp = property.GetValue(source);
            Copy(targetProp, sourceProp, difficulty);
            return target;
        }

        public static void Copy(object targetProp, object sourceProp, BotDifficulty difficulty)
        {
            if (targetProp is SAINProperty<float> targetFloat && sourceProp is SAINProperty<float> sourceFloat)
            {
                targetFloat.SetValue(difficulty, sourceFloat.GetValue(difficulty));
            }
            if (targetProp is SAINProperty<int> targetInt && sourceProp is SAINProperty<int> sourceInt)
            {
                targetInt.SetValue(difficulty, sourceInt.GetValue(difficulty));
            }
            if (targetProp is SAINProperty<bool> targetBool && sourceProp is SAINProperty<bool> sourceBool)
            {
                targetBool.SetValue(difficulty, sourceBool.GetValue(difficulty));
            }
        }

        public static void UpdatePropertyValue<T>(List<SAINBotPreset> presets, PropertyInfo property, T value, BotDifficulty difficulty)
        {
            for (int i = 0; i < presets.Count; i++)
            {
                UpdatePropertyValue(presets[i], property, value, difficulty);
            }
        }

        public static void UpdatePropertyValue<T>(SAINBotPreset preset, PropertyInfo property, T value, BotDifficulty difficulty)
        {
            var targetProp = property.GetValue(preset);
            if (targetProp is SAINProperty<T> prop)
            {
                UpdatePropertyValue(prop, value, difficulty);
            }
        }

        public static void UpdatePropertyValue<T>(SAINProperty<T> property, T value, BotDifficulty difficulty)
        {
            property.SetValue(difficulty, value);
        }

        public static PropertyInfo[] Properties { get; private set; }

        private static readonly List<KeyValuePair<WildSpawnType, BotDifficulty>> TypeDiffs = new List<KeyValuePair<WildSpawnType, BotDifficulty>>();
        public static readonly Dictionary<KeyValuePair<WildSpawnType, BotDifficulty>, SAINBotPreset> Presets = new Dictionary<KeyValuePair<WildSpawnType, BotDifficulty>, SAINBotPreset>();
    }

    public sealed class BotType
    {
        public BotType(WildSpawnType type, string name, string section, string description)
        {
            OriginalName = type.ToString();
            Name = name;
            Description = description;
            Section = section;
            WildSpawnType = type;
        }

        public BotType(string originalName, string name, string section, string description)
        {
            OriginalName = originalName;
            Name = name;
            Description = description;
            Section = section;
            WildSpawnType = SAINBotPresetManager.GetType(originalName);
        }

        public SAINBotPreset Preset { get; set; }

        public PropertyInfo[] PresetProperties => SAINBotPresetManager.Properties;

        public string OriginalName { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Section { get; private set; }

        public WildSpawnType WildSpawnType { get; private set; }
    }
}
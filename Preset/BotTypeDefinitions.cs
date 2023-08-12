using EFT;
using Newtonsoft.Json;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.Preset
{
    public sealed class BotType
    {
        public string Name;
        public string Description;
        public string Section;
        public WildSpawnType WildSpawnType;
    }

    public class BotTypeDefinitions
    {
        public static Dictionary<WildSpawnType, BotType> BotTypes;
        public static List<BotType> BotTypesList;

        static BotTypeDefinitions()
        {
            BotTypesList = CreateBotTypes();
            BotTypes = new Dictionary<WildSpawnType, BotType>();

            for (int i = 0; i < BotTypesList.Count; i++)
            {
                BotTypes.Add(BotTypesList[i].WildSpawnType, BotTypesList[i]);
            }
        }

        static List<BotType> CreateBotTypes()
        {
            return new List<BotType>
            {
            new BotType{ WildSpawnType = WildSpawnType.assault,                 Name = "Scav",                     Section = "Scavs" ,       Description = "Scavs!" },
            new BotType{ WildSpawnType = WildSpawnType.crazyAssaultEvent,       Name = "Crazy Scav Event",         Section = "Scavs" ,       Description = "Scavs!" },
            new BotType{ WildSpawnType = EnumValues.WildSpawn.Usec,             Name = "Usec",                     Section = "PMCs" ,        Description = "A PMC of the Usec Faction" },
            new BotType{ WildSpawnType = EnumValues.WildSpawn.Bear,             Name = "Bear",                     Section = "PMCs" ,        Description = "A PMC of the Bear Faction" },
            new BotType{ WildSpawnType = WildSpawnType.marksman,                Name = "Scav Sniper",              Section = "Scavs" ,       Description = "The Scav Snipers that spawn on rooftops on certain maps" },
            new BotType{ WildSpawnType = WildSpawnType.cursedAssault,           Name = "Tagged and Cursed Scav",   Section = "Scavs" ,       Description = "The type a scav is assigned when the player is marked as Tagged and Cursed" },
            new BotType{ WildSpawnType = WildSpawnType.bossKnight,              Name = "Knight",                   Section = "Goons" ,       Description = "Goons leader. Close proximity to the goons has been noted to cause smashed keyboards" },
            new BotType{ WildSpawnType = WildSpawnType.followerBigPipe,         Name = "BigPipe" ,                 Section = "Goons" ,       Description = "Goons follower. Close proximity to the goons has been noted to cause smashed keyboards\"" },
            new BotType{ WildSpawnType = WildSpawnType.followerBirdEye,         Name = "BirdEye",                  Section = "Goons" ,       Description = "Goons follower. Close proximity to the goons has been noted to cause smashed keyboards\"" },
            new BotType{ WildSpawnType = WildSpawnType.exUsec,                  Name = "Rogue",                    Section = "Other" ,       Description = "Ex Usec Personel on Lighthouse usually found around the water treatment plant" },
            new BotType{ WildSpawnType = WildSpawnType.pmcBot,                  Name = "Raider",                   Section = "Other" ,       Description = "Heavily armed scavs typically found on reserve and Labs by default" },
            new BotType{ WildSpawnType = WildSpawnType.arenaFighterEvent,       Name = "Bloodhound",               Section = "Other" ,       Description = "From the Live Event, nearly identical to raiders except with different voicelines and better gear. Found in" },
            new BotType{ WildSpawnType = WildSpawnType.sectantPriest,           Name = "Cultist Priest",           Section = "Other" ,       Description = "Found on Customs, Woods, Factory, Shoreline at night" },
            new BotType{ WildSpawnType = WildSpawnType.sectantWarrior,          Name = "Cultist",                  Section = "Other" ,       Description = "Found on Customs, Woods, Factory, Shoreline at night" },
            new BotType{ WildSpawnType = WildSpawnType.bossKilla,               Name = "Killa",                    Section = "Bosses" ,      Description = "He shoot. Found on Interchange and Streets" },
            new BotType{ WildSpawnType = WildSpawnType.bossBully,               Name = "Rashala",                  Section = "Bosses" ,      Description = "Customs Boss" },
            new BotType{ WildSpawnType = WildSpawnType.followerBully,           Name = "Rashala Guard",            Section = "Followers" ,   Description = "Customs Boss Follower" },
            new BotType{ WildSpawnType = WildSpawnType.bossKojaniy,             Name = "Shturman",                 Section = "Bosses" ,      Description = "Woods Boss" },
            new BotType{ WildSpawnType = WildSpawnType.followerKojaniy,         Name = "Shturman Guard",           Section = "Followers" ,   Description = "Woods Boss Follower" },
            new BotType{ WildSpawnType = WildSpawnType.bossTagilla,             Name = "Tagilla",                  Section = "Bosses" ,      Description = "He Smash" },
            new BotType{ WildSpawnType = WildSpawnType.followerTagilla,         Name = "Tagilla Guard",            Section = "Followers" ,   Description = "They Smash Too?" },
            new BotType{ WildSpawnType = WildSpawnType.bossSanitar,             Name = "Sanitar",                  Section = "Bosses" ,      Description = "Shoreline Boss" },
            new BotType{ WildSpawnType = WildSpawnType.followerSanitar,         Name = "Sanitar Guard",            Section = "Followers" ,   Description = "Shoreline Boss Follower" },
            new BotType{ WildSpawnType = WildSpawnType.bossGluhar,              Name = "Gluhar",                   Section = "Bosses" ,      Description = "Reserve Boss. Also can be found on Streets." },
            new BotType{ WildSpawnType = WildSpawnType.followerGluharSnipe,     Name = "Gluhar Guard Snipe",       Section = "Followers" ,   Description = "Reserve Boss Follower" },
            new BotType{ WildSpawnType = WildSpawnType.followerGluharScout,     Name = "Gluhar Guard Scout",       Section = "Followers" ,   Description = "Reserve Boss Follower" },
            new BotType{ WildSpawnType = WildSpawnType.followerGluharSecurity,  Name = "Gluhar Guard Security",    Section = "Followers" ,   Description = "Reserve Boss Follower" },
            new BotType{ WildSpawnType = WildSpawnType.followerGluharAssault,   Name = "Gluhar Guard Assault",     Section = "Followers" ,   Description = "Reserve Boss Follower" },
            new BotType{ WildSpawnType = WildSpawnType.bossZryachiy,            Name = "Zryachiy",                 Section = "Bosses" ,      Description = "Lighthouse Island Sniper Boss" },
            new BotType{ WildSpawnType = WildSpawnType.followerZryachiy,        Name = "Zryachiy Guard",           Section = "Followers" ,   Description = "Lighthouse Island Sniper Boss Follower" }
            };
        }
    }
}

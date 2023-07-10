using EFT;

namespace SAIN.BotPresets
{
    public class BotTypeDefinitions
    {
        //   WildSpawnType                                      // DisplayName             // Section       // Description
        public static readonly BotType[] BotTypes =
        {
            new BotType( WildSpawnType.assault,                 "Scav",                     "Scavs" ,       "Scavs!" ),

            new BotType( "sptUsec",                             "Usec",                     "PMCs" ,        "A PMC of the Usec Faction" ),
            new BotType( "sptBear",                             "Bear",                     "PMCs" ,        "A PMC of the Bear Faction" ),

            new BotType( WildSpawnType.marksman,                "Scav Sniper",              "Scavs" ,       "The Scav Snipers that spawn on rooftops on certain maps" ),
            new BotType( WildSpawnType.cursedAssault,           "Tagged and Cursed Scav",   "Scavs" ,       "The type a scav is assigned when the player is marked as Tagged and Cursed" ),

            new BotType( WildSpawnType.bossKnight,              "Knight",                   "Goons" ,       "Goons leader. Close proximity to the goons has been noted to cause smashed keyboards" ),
            new BotType( WildSpawnType.followerBigPipe,         "BigPipe" ,                 "Goons" ,       "Goons follower. Close proximity to the goons has been noted to cause smashed keyboards\"" ),
            new BotType( WildSpawnType.followerBirdEye,         "BirdEye",                  "Goons" ,       "Goons follower. Close proximity to the goons has been noted to cause smashed keyboards\"" ),

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

    }
}

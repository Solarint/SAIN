import { ApplicationContext } from "@spt-aki/context/ApplicationContext";
import { BotGenerator } from "@spt-aki/generators/BotGenerator";
import { BotDifficultyHelper } from "@spt-aki/helpers/BotDifficultyHelper";
import { BotHelper } from "@spt-aki/helpers/BotHelper";
import { ProfileHelper } from "@spt-aki/helpers/ProfileHelper";
import { IGenerateBotsRequestData } from "@spt-aki/models/eft/bot/IGenerateBotsRequestData";
import { IBotBase } from "@spt-aki/models/eft/common/tables/IBotBase";
import { IBotCore } from "@spt-aki/models/eft/common/tables/IBotCore";
import { Difficulty } from "@spt-aki/models/eft/common/tables/IBotType";
import { IBotConfig } from "@spt-aki/models/spt/config/IBotConfig";
import { IPmcConfig } from "@spt-aki/models/spt/config/IPmcConfig";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { ConfigServer } from "@spt-aki/servers/ConfigServer";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { BotGenerationCacheService } from "@spt-aki/services/BotGenerationCacheService";
import { LocalisationService } from "@spt-aki/services/LocalisationService";
import { MatchBotDetailsCacheService } from "@spt-aki/services/MatchBotDetailsCacheService";
import { JsonUtil } from "@spt-aki/utils/JsonUtil";
export declare class BotController {
    protected logger: ILogger;
    protected databaseServer: DatabaseServer;
    protected botGenerator: BotGenerator;
    protected botHelper: BotHelper;
    protected botDifficultyHelper: BotDifficultyHelper;
    protected botGenerationCacheService: BotGenerationCacheService;
    protected matchBotDetailsCacheService: MatchBotDetailsCacheService;
    protected localisationService: LocalisationService;
    protected profileHelper: ProfileHelper;
    protected configServer: ConfigServer;
    protected applicationContext: ApplicationContext;
    protected jsonUtil: JsonUtil;
    protected botConfig: IBotConfig;
    protected pmcConfig: IPmcConfig;
    constructor(logger: ILogger, databaseServer: DatabaseServer, botGenerator: BotGenerator, botHelper: BotHelper, botDifficultyHelper: BotDifficultyHelper, botGenerationCacheService: BotGenerationCacheService, matchBotDetailsCacheService: MatchBotDetailsCacheService, localisationService: LocalisationService, profileHelper: ProfileHelper, configServer: ConfigServer, applicationContext: ApplicationContext, jsonUtil: JsonUtil);
    /**
     * Return the number of bot loadout varieties to be generated
     * @param type bot Type we want the loadout gen count for
     * @returns number of bots to generate
     */
    getBotPresetGenerationLimit(type: string): number;
    /**
     * Handle singleplayer/settings/bot/difficulty
     * Get the core.json difficulty settings from database\bots
     * @returns IBotCore
     */
    getBotCoreDifficulty(): IBotCore;
    /**
     * Get bot difficulty settings
     * adjust PMC settings to ensure they engage the correct bot types
     * @param type what bot the server is requesting settings for
     * @param difficulty difficulty level server requested settings for
     * @returns Difficulty object
     */
    getBotDifficulty(type: string, difficulty: string): Difficulty;
    /**
     * Generate bot profiles and store in cache
     * @param sessionId Session id
     * @param info bot generation request info
     * @returns IBotBase array
     */
    generate(sessionId: string, info: IGenerateBotsRequestData): IBotBase[];
    /**
     * Get the difficulty passed in, if its not "asoline", get selected difficulty from config
     * @param requestedDifficulty
     * @returns
     */
    getPMCDifficulty(requestedDifficulty: string): string;
    /**
     * Get the max number of bots allowed on a map
     * Looks up location player is entering when getting cap value
     * @returns cap number
     */
    getBotCap(): number;
    getAiBotBrainTypes(): any;
}

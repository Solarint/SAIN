import { FenceBaseAssortGenerator } from "@spt-aki/generators/FenceBaseAssortGenerator";
import { ProfileHelper } from "@spt-aki/helpers/ProfileHelper";
import { TraderAssortHelper } from "@spt-aki/helpers/TraderAssortHelper";
import { TraderHelper } from "@spt-aki/helpers/TraderHelper";
import { ITraderAssort, ITraderBase } from "@spt-aki/models/eft/common/tables/ITrader";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { FenceService } from "@spt-aki/services/FenceService";
import { TraderAssortService } from "@spt-aki/services/TraderAssortService";
import { TraderPurchasePersisterService } from "@spt-aki/services/TraderPurchasePersisterService";
import { JsonUtil } from "@spt-aki/utils/JsonUtil";
export declare class TraderController {
    protected logger: ILogger;
    protected databaseServer: DatabaseServer;
    protected traderAssortHelper: TraderAssortHelper;
    protected profileHelper: ProfileHelper;
    protected traderHelper: TraderHelper;
    protected traderAssortService: TraderAssortService;
    protected traderPurchasePersisterService: TraderPurchasePersisterService;
    protected fenceService: FenceService;
    protected fenceBaseAssortGenerator: FenceBaseAssortGenerator;
    protected jsonUtil: JsonUtil;
    constructor(logger: ILogger, databaseServer: DatabaseServer, traderAssortHelper: TraderAssortHelper, profileHelper: ProfileHelper, traderHelper: TraderHelper, traderAssortService: TraderAssortService, traderPurchasePersisterService: TraderPurchasePersisterService, fenceService: FenceService, fenceBaseAssortGenerator: FenceBaseAssortGenerator, jsonUtil: JsonUtil);
    /**
     * Runs when onLoad event is fired
     * Iterate over traders, ensure an unmolested copy of their assorts is stored in traderAssortService
     * Store timestamp of next assort refresh in nextResupply property of traders .base object
     */
    load(): void;
    /**
     * Runs when onUpdate is fired
     * If current time is > nextResupply(expire) time of trader, refresh traders assorts and
     * Fence is handled slightly differently
     * @returns has run
     */
    update(): boolean;
    /**
     * Handle client/trading/api/traderSettings
     * Return an array of all traders
     * @param sessionID Session id
     * @returns array if ITraderBase objects
     */
    getAllTraders(sessionID: string): ITraderBase[];
    /**
     * Order traders by their traderId (Ttid)
     * @param traderA First trader to compare
     * @param traderB Second trader to compare
     * @returns 1,-1 or 0
     */
    protected sortByTraderId(traderA: ITraderBase, traderB: ITraderBase): number;
    /** Handle client/trading/api/getTrader */
    getTrader(sessionID: string, traderID: string): ITraderBase;
    /** Handle client/trading/api/getTraderAssort */
    getAssort(sessionId: string, traderId: string): ITraderAssort;
}

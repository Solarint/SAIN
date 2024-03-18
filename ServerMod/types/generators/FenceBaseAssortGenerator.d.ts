import { HandbookHelper } from "@spt-aki/helpers/HandbookHelper";
import { ItemHelper } from "@spt-aki/helpers/ItemHelper";
import { ITemplateItem } from "@spt-aki/models/eft/common/tables/ITemplateItem";
import { ITraderConfig } from "@spt-aki/models/spt/config/ITraderConfig";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { ConfigServer } from "@spt-aki/servers/ConfigServer";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { ItemFilterService } from "@spt-aki/services/ItemFilterService";
import { SeasonalEventService } from "@spt-aki/services/SeasonalEventService";
export declare class FenceBaseAssortGenerator {
    protected logger: ILogger;
    protected databaseServer: DatabaseServer;
    protected handbookHelper: HandbookHelper;
    protected itemHelper: ItemHelper;
    protected itemFilterService: ItemFilterService;
    protected seasonalEventService: SeasonalEventService;
    protected configServer: ConfigServer;
    protected traderConfig: ITraderConfig;
    constructor(logger: ILogger, databaseServer: DatabaseServer, handbookHelper: HandbookHelper, itemHelper: ItemHelper, itemFilterService: ItemFilterService, seasonalEventService: SeasonalEventService, configServer: ConfigServer);
    /**
     * Create base fence assorts dynamically and store in db
     */
    generateFenceBaseAssorts(): void;
    /**
     * Check if item is valid for being added to fence assorts
     * @param item Item to check
     * @returns true if valid fence item
     */
    protected isValidFenceItem(item: ITemplateItem): boolean;
}

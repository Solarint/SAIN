import { ItemHelper } from "@spt-aki/helpers/ItemHelper";
import { IPreset } from "@spt-aki/models/eft/common/IGlobals";
import { Item } from "@spt-aki/models/eft/common/tables/IItem";
import { IRagfairConfig } from "@spt-aki/models/spt/config/IRagfairConfig";
import { ConfigServer } from "@spt-aki/servers/ConfigServer";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { SeasonalEventService } from "@spt-aki/services/SeasonalEventService";
import { HashUtil } from "@spt-aki/utils/HashUtil";
import { JsonUtil } from "@spt-aki/utils/JsonUtil";
export declare class RagfairAssortGenerator {
    protected jsonUtil: JsonUtil;
    protected hashUtil: HashUtil;
    protected itemHelper: ItemHelper;
    protected databaseServer: DatabaseServer;
    protected seasonalEventService: SeasonalEventService;
    protected configServer: ConfigServer;
    protected generatedAssortItems: Item[];
    protected ragfairConfig: IRagfairConfig;
    constructor(jsonUtil: JsonUtil, hashUtil: HashUtil, itemHelper: ItemHelper, databaseServer: DatabaseServer, seasonalEventService: SeasonalEventService, configServer: ConfigServer);
    /**
     * Get an array of unique items that can be sold on the flea
     * @returns array of unique items
     */
    getAssortItems(): Item[];
    /**
     * Check internal generatedAssortItems array has objects
     * @returns true if array has objects
     */
    protected assortsAreGenerated(): boolean;
    /**
     * Generate an array of items the flea can sell
     * @returns array of unique items
     */
    protected generateRagfairAssortItems(): Item[];
    /**
     * Get presets from globals.json
     * @returns Preset object array
     */
    protected getPresets(): IPreset[];
    /**
     * Get default presets from globals.json
     * @returns Preset object array
     */
    protected getDefaultPresets(): IPreset[];
    /**
     * Create a base assort item and return it with populated values + 999999 stack count + unlimited count = true
     * @param tplId tplid to add to item
     * @param id id to add to item
     * @returns hydrated Item object
     */
    protected createRagfairAssortItem(tplId: string, id?: string): Item;
}

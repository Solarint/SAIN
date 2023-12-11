import { PresetBuildCallbacks } from "@spt-aki/callbacks/PresetBuildCallbacks";
import { HandledRoute, ItemEventRouterDefinition } from "@spt-aki/di/Router";
import { IPmcData } from "@spt-aki/models/eft/common/IPmcData";
import { IItemEventRouterResponse } from "@spt-aki/models/eft/itemEvent/IItemEventRouterResponse";
export declare class PresetBuildItemEventRouter extends ItemEventRouterDefinition {
    protected presetBuildCallbacks: PresetBuildCallbacks;
    constructor(presetBuildCallbacks: PresetBuildCallbacks);
    getHandledRoutes(): HandledRoute[];
    handleItemEvent(url: string, pmcData: IPmcData, body: any, sessionID: string): IItemEventRouterResponse;
}

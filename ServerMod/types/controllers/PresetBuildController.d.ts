import { ItemHelper } from "@spt-aki/helpers/ItemHelper";
import { IPmcData } from "@spt-aki/models/eft/common/IPmcData";
import { IItemEventRouterResponse } from "@spt-aki/models/eft/itemEvent/IItemEventRouterResponse";
import { IPresetBuildActionRequestData } from "@spt-aki/models/eft/presetBuild/IPresetBuildActionRequestData";
import { IRemoveBuildRequestData } from "@spt-aki/models/eft/presetBuild/IRemoveBuildRequestData";
import { IUserBuilds } from "@spt-aki/models/eft/profile/IAkiProfile";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { EventOutputHolder } from "@spt-aki/routers/EventOutputHolder";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { SaveServer } from "@spt-aki/servers/SaveServer";
import { HashUtil } from "@spt-aki/utils/HashUtil";
import { JsonUtil } from "@spt-aki/utils/JsonUtil";
export declare class PresetBuildController {
    protected logger: ILogger;
    protected hashUtil: HashUtil;
    protected eventOutputHolder: EventOutputHolder;
    protected jsonUtil: JsonUtil;
    protected databaseServer: DatabaseServer;
    protected itemHelper: ItemHelper;
    protected saveServer: SaveServer;
    constructor(logger: ILogger, hashUtil: HashUtil, eventOutputHolder: EventOutputHolder, jsonUtil: JsonUtil, databaseServer: DatabaseServer, itemHelper: ItemHelper, saveServer: SaveServer);
    /** Handle client/handbook/builds/my/list */
    getUserBuilds(sessionID: string): IUserBuilds;
    /** Handle SaveWeaponBuild event */
    saveWeaponBuild(pmcData: IPmcData, body: IPresetBuildActionRequestData, sessionId: string): IItemEventRouterResponse;
    /** Handle SaveEquipmentBuild event */
    saveEquipmentBuild(pmcData: IPmcData, body: IPresetBuildActionRequestData, sessionID: string): IItemEventRouterResponse;
    protected saveBuild(pmcData: IPmcData, body: IPresetBuildActionRequestData, sessionID: string, buildType: string): IItemEventRouterResponse;
    /** Handle RemoveWeaponBuild event*/
    removeBuild(pmcData: IPmcData, body: IRemoveBuildRequestData, sessionID: string): IItemEventRouterResponse;
    /** Handle RemoveWeaponBuild event*/
    removeWeaponBuild(pmcData: IPmcData, body: IPresetBuildActionRequestData, sessionID: string): IItemEventRouterResponse;
    /** Handle RemoveEquipmentBuild event*/
    removeEquipmentBuild(pmcData: IPmcData, body: IPresetBuildActionRequestData, sessionID: string): IItemEventRouterResponse;
    protected removePlayerBuild(pmcData: IPmcData, id: string, sessionID: string): IItemEventRouterResponse;
}

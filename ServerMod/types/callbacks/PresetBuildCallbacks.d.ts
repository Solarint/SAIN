import { PresetBuildController } from "@spt-aki/controllers/PresetBuildController";
import { IEmptyRequestData } from "@spt-aki/models/eft/common/IEmptyRequestData";
import { IPmcData } from "@spt-aki/models/eft/common/IPmcData";
import { IGetBodyResponseData } from "@spt-aki/models/eft/httpResponse/IGetBodyResponseData";
import { IItemEventRouterResponse } from "@spt-aki/models/eft/itemEvent/IItemEventRouterResponse";
import { IPresetBuildActionRequestData } from "@spt-aki/models/eft/presetBuild/IPresetBuildActionRequestData";
import { IRemoveBuildRequestData } from "@spt-aki/models/eft/presetBuild/IRemoveBuildRequestData";
import { IUserBuilds } from "@spt-aki/models/eft/profile/IAkiProfile";
import { HttpResponseUtil } from "@spt-aki/utils/HttpResponseUtil";
export declare class PresetBuildCallbacks {
    protected httpResponse: HttpResponseUtil;
    protected presetBuildController: PresetBuildController;
    constructor(httpResponse: HttpResponseUtil, presetBuildController: PresetBuildController);
    /** Handle client/handbook/builds/my/list */
    getHandbookUserlist(url: string, info: IEmptyRequestData, sessionID: string): IGetBodyResponseData<IUserBuilds>;
    /** Handle SaveWeaponBuild event */
    saveWeaponBuild(pmcData: IPmcData, body: IPresetBuildActionRequestData, sessionID: string): IItemEventRouterResponse;
    /** Handle removeBuild event*/
    removeBuild(pmcData: IPmcData, body: IRemoveBuildRequestData, sessionID: string): IItemEventRouterResponse;
    /** Handle RemoveWeaponBuild event*/
    removeWeaponBuild(pmcData: IPmcData, body: IPresetBuildActionRequestData, sessionID: string): IItemEventRouterResponse;
    /** Handle SaveEquipmentBuild event */
    saveEquipmentBuild(pmcData: IPmcData, body: IPresetBuildActionRequestData, sessionID: string): IItemEventRouterResponse;
    /** Handle RemoveEquipmentBuild event*/
    removeEquipmentBuild(pmcData: IPmcData, body: IPresetBuildActionRequestData, sessionID: string): IItemEventRouterResponse;
}

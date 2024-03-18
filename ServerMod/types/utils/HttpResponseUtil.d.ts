import { IGetBodyResponseData } from "@spt-aki/models/eft/httpResponse/IGetBodyResponseData";
import { INullResponseData } from "@spt-aki/models/eft/httpResponse/INullResponseData";
import { IItemEventRouterResponse } from "@spt-aki/models/eft/itemEvent/IItemEventRouterResponse";
import { BackendErrorCodes } from "@spt-aki/models/enums/BackendErrorCodes";
import { LocalisationService } from "@spt-aki/services/LocalisationService";
import { JsonUtil } from "@spt-aki/utils/JsonUtil";
export declare class HttpResponseUtil {
    protected jsonUtil: JsonUtil;
    protected localisationService: LocalisationService;
    constructor(jsonUtil: JsonUtil, localisationService: LocalisationService);
    protected clearString(s: string): any;
    /**
     * Return passed in data as JSON string
     * @param data
     * @returns
     */
    noBody(data: any): any;
    /**
     * Game client needs server responses in a particular format
     * @param data
     * @param err
     * @param errmsg
     * @returns
     */
    getBody<T>(data: T, err?: number, errmsg?: any): IGetBodyResponseData<T>;
    getUnclearedBody(data: any, err?: number, errmsg?: any): string;
    emptyResponse(): IGetBodyResponseData<string>;
    nullResponse(): INullResponseData;
    emptyArrayResponse(): IGetBodyResponseData<any[]>;
    appendErrorToOutput(output: IItemEventRouterResponse, message?: string, errorCode?: BackendErrorCodes): IItemEventRouterResponse;
}

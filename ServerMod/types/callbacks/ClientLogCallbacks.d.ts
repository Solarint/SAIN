import { ClientLogController } from "@spt-aki/controllers/ClientLogController";
import { INullResponseData } from "@spt-aki/models/eft/httpResponse/INullResponseData";
import { IClientLogRequest } from "@spt-aki/models/spt/logging/IClientLogRequest";
import { HttpResponseUtil } from "@spt-aki/utils/HttpResponseUtil";
/** Handle client logging related events */
export declare class ClientLogCallbacks {
    protected httpResponse: HttpResponseUtil;
    protected clientLogController: ClientLogController;
    constructor(httpResponse: HttpResponseUtil, clientLogController: ClientLogController);
    /**
     * Handle /singleplayer/log
     */
    clientLog(url: string, info: IClientLogRequest, sessionID: string): INullResponseData;
}

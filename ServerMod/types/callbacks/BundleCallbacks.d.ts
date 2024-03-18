import { BundleLoader } from "@spt-aki/loaders/BundleLoader";
import { IHttpConfig } from "@spt-aki/models/spt/config/IHttpConfig";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { ConfigServer } from "@spt-aki/servers/ConfigServer";
import { HttpFileUtil } from "@spt-aki/utils/HttpFileUtil";
import { HttpResponseUtil } from "@spt-aki/utils/HttpResponseUtil";
export declare class BundleCallbacks {
    protected logger: ILogger;
    protected httpResponse: HttpResponseUtil;
    protected httpFileUtil: HttpFileUtil;
    protected bundleLoader: BundleLoader;
    protected configServer: ConfigServer;
    protected httpConfig: IHttpConfig;
    constructor(logger: ILogger, httpResponse: HttpResponseUtil, httpFileUtil: HttpFileUtil, bundleLoader: BundleLoader, configServer: ConfigServer);
    sendBundle(sessionID: string, req: any, resp: any, body: any): any;
    /**
     * Handle singleplayer/bundles
     */
    getBundles(url: string, info: any, sessionID: string): string;
    getBundle(url: string, info: any, sessionID: string): string;
}

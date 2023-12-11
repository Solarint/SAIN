import { PresetBuildCallbacks } from "@spt-aki/callbacks/PresetBuildCallbacks";
import { StaticRouter } from "@spt-aki/di/Router";
export declare class PresetStaticRouter extends StaticRouter {
    protected presetCallbacks: PresetBuildCallbacks;
    constructor(presetCallbacks: PresetBuildCallbacks);
}

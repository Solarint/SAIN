import { PresetHelper } from "@spt-aki/helpers/PresetHelper";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
export declare class PresetController {
    protected presetHelper: PresetHelper;
    protected databaseServer: DatabaseServer;
    constructor(presetHelper: PresetHelper, databaseServer: DatabaseServer);
    initialize(): void;
}

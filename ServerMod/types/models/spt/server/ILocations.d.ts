import { ILocationBase } from "@spt-aki/models/eft/common/ILocationBase";
import { ILooseLoot } from "@spt-aki/models/eft/common/ILooseLoot";
import { ILocationsBase } from "@spt-aki/models/eft/common/tables/ILocationsBase";
export interface ILocations {
    bigmap?: ILocationData;
    develop?: ILocationData;
    factory4_day?: ILocationData;
    factory4_night?: ILocationData;
    hideout?: ILocationData;
    interchange?: ILocationData;
    laboratory?: ILocationData;
    lighthouse?: ILocationData;
    privatearea?: ILocationData;
    rezervbase?: ILocationData;
    shoreline?: ILocationData;
    suburbs?: ILocationData;
    tarkovstreets?: ILocationData;
    terminal?: ILocationData;
    town?: ILocationData;
    woods?: ILocationData;
    base?: ILocationsBase;
}
export interface ILocationData {
    base: ILocationBase;
    looseLoot?: ILooseLoot;
}

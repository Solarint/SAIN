import { IBotBase, IEftStats } from "@spt-aki/models/eft/common/tables/IBotBase";
export interface IPmcData extends IBotBase {
}
export interface IPostRaidPmcData extends IBotBase {
    /** Only found in profile we get from client post raid */
    EftStats: IEftStats;
}

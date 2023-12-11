import { IBaseConfig } from "@spt-aki/models/spt/config/IBaseConfig";
export interface ICoreConfig extends IBaseConfig {
    kind: "aki-core";
    akiVersion: string;
    projectName: string;
    compatibleTarkovVersion: string;
    serverName: string;
    profileSaveIntervalSeconds: number;
    sptFriendNickname: string;
    fixes: IGameFixes;
    features: IServerFeatures;
    /** Commit hash build server was created from */
    commit?: string;
    /** Timestamp of server build */
    buildTime?: string;
}
export interface IGameFixes {
    /** Shotguns use a different value than normal guns causing huge pellet dispersion  */
    fixShotgunDispersion: boolean;
    /** Remove items added by mods when the mod no longer exists - can fix dead profiles stuck at game load*/
    removeModItemsFromProfile: boolean;
    /** Fix issues that cause the game to not start due to inventory item issues */
    fixProfileBreakingInventoryItemIssues: boolean;
}
export interface IServerFeatures {
    autoInstallModDependencies: boolean;
}

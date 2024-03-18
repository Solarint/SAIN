import { ItemHelper } from "@spt-aki/helpers/ItemHelper";
import { IPmcData } from "@spt-aki/models/eft/common/IPmcData";
import { Common, CounterKeyValue, Stats } from "@spt-aki/models/eft/common/tables/IBotBase";
import { IAkiProfile } from "@spt-aki/models/eft/profile/IAkiProfile";
import { IValidateNicknameRequestData } from "@spt-aki/models/eft/profile/IValidateNicknameRequestData";
import { SkillTypes } from "@spt-aki/models/enums/SkillTypes";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { SaveServer } from "@spt-aki/servers/SaveServer";
import { LocalisationService } from "@spt-aki/services/LocalisationService";
import { ProfileSnapshotService } from "@spt-aki/services/ProfileSnapshotService";
import { JsonUtil } from "@spt-aki/utils/JsonUtil";
import { TimeUtil } from "@spt-aki/utils/TimeUtil";
import { Watermark } from "@spt-aki/utils/Watermark";
export declare class ProfileHelper {
    protected logger: ILogger;
    protected jsonUtil: JsonUtil;
    protected watermark: Watermark;
    protected timeUtil: TimeUtil;
    protected saveServer: SaveServer;
    protected databaseServer: DatabaseServer;
    protected itemHelper: ItemHelper;
    protected profileSnapshotService: ProfileSnapshotService;
    protected localisationService: LocalisationService;
    constructor(logger: ILogger, jsonUtil: JsonUtil, watermark: Watermark, timeUtil: TimeUtil, saveServer: SaveServer, databaseServer: DatabaseServer, itemHelper: ItemHelper, profileSnapshotService: ProfileSnapshotService, localisationService: LocalisationService);
    /**
     * Remove/reset a completed quest condtion from players profile quest data
     * @param sessionID Session id
     * @param questConditionId Quest with condition to remove
     */
    removeCompletedQuestConditionFromProfile(pmcData: IPmcData, questConditionId: Record<string, string>): void;
    /**
     * Get all profiles from server
     * @returns Dictionary of profiles
     */
    getProfiles(): Record<string, IAkiProfile>;
    getCompleteProfile(sessionID: string): IPmcData[];
    /**
     * Fix xp doubling on post-raid xp reward screen by sending a 'dummy' profile to the post-raid screen
     * Server saves the post-raid changes prior to the xp screen getting the profile, this results in the xp screen using
     * the now updated profile values as a base, meaning it shows x2 xp gained
     * Instead, clone the post-raid profile (so we dont alter its values), apply the pre-raid xp values to the cloned objects and return
     * Delete snapshot of pre-raid profile prior to returning profile data
     * @param sessionId Session id
     * @param output pmc and scav profiles array
     * @param pmcProfile post-raid pmc profile
     * @param scavProfile post-raid scav profile
     * @returns updated profile array
     */
    protected postRaidXpWorkaroundFix(sessionId: string, output: IPmcData[], pmcProfile: IPmcData, scavProfile: IPmcData): IPmcData[];
    /**
     * Check if a nickname is used by another profile loaded by the server
     * @param nicknameRequest
     * @param sessionID Session id
     * @returns True if already used
     */
    isNicknameTaken(nicknameRequest: IValidateNicknameRequestData, sessionID: string): boolean;
    protected profileHasInfoProperty(profile: IAkiProfile): boolean;
    protected nicknameMatches(profileName: string, nicknameRequest: string): boolean;
    protected sessionIdMatchesProfileId(profileId: string, sessionId: string): boolean;
    /**
     * Add experience to a PMC inside the players profile
     * @param sessionID Session id
     * @param experienceToAdd Experience to add to PMC character
     */
    addExperienceToPmc(sessionID: string, experienceToAdd: number): void;
    getProfileByPmcId(pmcId: string): IPmcData;
    getExperience(level: number): number;
    getMaxLevel(): number;
    getDefaultAkiDataObject(): any;
    getFullProfile(sessionID: string): IAkiProfile;
    getPmcProfile(sessionID: string): IPmcData;
    getScavProfile(sessionID: string): IPmcData;
    /**
     * Get baseline counter values for a fresh profile
     * @returns Stats
     */
    getDefaultCounters(): Stats;
    protected isWiped(sessionID: string): boolean;
    protected getServerVersion(): string;
    /**
     * Iterate over player profile inventory items and find the secure container and remove it
     * @param profile Profile to remove secure container from
     * @returns profile without secure container
     */
    removeSecureContainer(profile: IPmcData): IPmcData;
    /**
     *  Flag a profile as having received a gift
     * Store giftid in profile aki object
     * @param playerId Player to add gift flag to
     * @param giftId Gift player received
     */
    addGiftReceivedFlagToProfile(playerId: string, giftId: string): void;
    /**
     * Check if profile has recieved a gift by id
     * @param playerId Player profile to check for gift
     * @param giftId Gift to check for
     * @returns True if player has recieved gift previously
     */
    playerHasRecievedGift(playerId: string, giftId: string): boolean;
    /**
     * Find Stat in profile counters and increment by one
     * @param counters Counters to search for key
     * @param keyToIncrement Key
     */
    incrementStatCounter(counters: CounterKeyValue[], keyToIncrement: string): void;
    /**
     * Check if player has a skill at elite level
     * @param skillType Skill to check
     * @param pmcProfile Profile to find skill in
     * @returns True if player has skill at elite level
     */
    hasEliteSkillLevel(skillType: SkillTypes, pmcProfile: IPmcData): boolean;
    /**
     * Add points to a specific skill in player profile
     * @param skill Skill to add points to
     * @param pointsToAdd Points to add
     * @param pmcProfile Player profile with skill
     * @param useSkillProgressRateMultipler Skills are multiplied by a value in globals, default is off to maintain compatibility with legacy code
     * @returns
     */
    addSkillPointsToPlayer(pmcProfile: IPmcData, skill: SkillTypes, pointsToAdd: number, useSkillProgressRateMultipler?: boolean): void;
    getSkillFromProfile(pmcData: IPmcData, skill: SkillTypes): Common;
}

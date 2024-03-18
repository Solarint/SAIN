import { DialogueHelper } from "@spt-aki/helpers/DialogueHelper";
import { ItemHelper } from "@spt-aki/helpers/ItemHelper";
import { ProfileHelper } from "@spt-aki/helpers/ProfileHelper";
import { TraderHelper } from "@spt-aki/helpers/TraderHelper";
import { IPmcData } from "@spt-aki/models/eft/common/IPmcData";
import { Item } from "@spt-aki/models/eft/common/tables/IItem";
import { IGetInsuranceCostRequestData } from "@spt-aki/models/eft/insurance/IGetInsuranceCostRequestData";
import { IGetInsuranceCostResponseData } from "@spt-aki/models/eft/insurance/IGetInsuranceCostResponseData";
import { IInsureRequestData } from "@spt-aki/models/eft/insurance/IInsureRequestData";
import { IItemEventRouterResponse } from "@spt-aki/models/eft/itemEvent/IItemEventRouterResponse";
import { ISystemData, Insurance } from "@spt-aki/models/eft/profile/IAkiProfile";
import { IInsuranceConfig } from "@spt-aki/models/spt/config/IInsuranceConfig";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { EventOutputHolder } from "@spt-aki/routers/EventOutputHolder";
import { ConfigServer } from "@spt-aki/servers/ConfigServer";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { SaveServer } from "@spt-aki/servers/SaveServer";
import { InsuranceService } from "@spt-aki/services/InsuranceService";
import { MailSendService } from "@spt-aki/services/MailSendService";
import { PaymentService } from "@spt-aki/services/PaymentService";
import { RandomUtil } from "@spt-aki/utils/RandomUtil";
import { TimeUtil } from "@spt-aki/utils/TimeUtil";
export declare class InsuranceController {
    protected logger: ILogger;
    protected randomUtil: RandomUtil;
    protected eventOutputHolder: EventOutputHolder;
    protected timeUtil: TimeUtil;
    protected saveServer: SaveServer;
    protected databaseServer: DatabaseServer;
    protected itemHelper: ItemHelper;
    protected profileHelper: ProfileHelper;
    protected dialogueHelper: DialogueHelper;
    protected traderHelper: TraderHelper;
    protected paymentService: PaymentService;
    protected insuranceService: InsuranceService;
    protected mailSendService: MailSendService;
    protected configServer: ConfigServer;
    protected insuranceConfig: IInsuranceConfig;
    protected roubleTpl: string;
    constructor(logger: ILogger, randomUtil: RandomUtil, eventOutputHolder: EventOutputHolder, timeUtil: TimeUtil, saveServer: SaveServer, databaseServer: DatabaseServer, itemHelper: ItemHelper, profileHelper: ProfileHelper, dialogueHelper: DialogueHelper, traderHelper: TraderHelper, paymentService: PaymentService, insuranceService: InsuranceService, mailSendService: MailSendService, configServer: ConfigServer);
    /**
     * Process insurance items of all profiles prior to being given back to the player through the mail service.
     *
     * @returns void
     */
    processReturn(): void;
    /**
     * Process insurance items of a single profile prior to being given back to the player through the mail service.
     *
     * @returns void
     */
    processReturnByProfile(sessionID: string): void;
    /**
     * Get all insured items that are ready to be processed in a specific profile.
     *
     * @param sessionID Session ID of the profile to check.
     * @param time The time to check ready status against. Current time by default.
     * @returns All insured items that are ready to be processed.
     */
    protected filterInsuredItems(sessionID: string, time?: number): Insurance[];
    /**
     * This method orchestrates the processing of insured items in a profile.
     *
     * @param insuranceDetails The insured items to process.
     * @param sessionID The session ID that should receive the processed items.
     * @returns void
     */
    protected processInsuredItems(insuranceDetails: Insurance[], sessionID: string): void;
    /**
     * Remove an insurance package from a profile using the package's system data information.
     *
     * @param sessionID The session ID of the profile to remove the package from.
     * @param index The array index of the insurance package to remove.
     * @returns void
     */
    protected removeInsurancePackageFromProfile(sessionID: string, packageInfo: ISystemData): void;
    /**
     * Finds the items that should be deleted based on the given Insurance object.
     *
     * @param insured The insurance object containing the items to evaluate for deletion.
     * @returns A Set containing the IDs of items that should be deleted.
     */
    protected findItemsToDelete(insured: Insurance): Set<string>;
    /**
     * Populate a Map object of items for quick lookup by their ID.
     *
     * @param insured The insurance object containing the items to populate the map with.
     * @returns A Map where the keys are the item IDs and the values are the corresponding Item objects.
     */
    protected populateItemsMap(insured: Insurance): Map<string, Item>;
    /**
     * Initialize a Map object that holds main-parents to all of their attachments. Note that "main-parent" in this
     * context refers to the parent item that an attachment is attached to. For example, a suppressor attached to a gun,
     * not the backpack that the gun is located in (the gun's parent).
     *
     * @param insured - The insurance object containing the items to evaluate.
     * @param itemsMap - A Map object for quick item look-up by item ID.
     * @returns A Map object containing parent item IDs to arrays of their attachment items.
     */
    protected populateParentAttachmentsMap(insured: Insurance, itemsMap: Map<string, Item>): Map<string, Item[]>;
    /**
     * Process "regular" insurance items. Any insured item that is not an attached, attachment is considered a "regular"
     * item. This method iterates over them, preforming item deletion rolls to see if they should be deleted. If so,
     * they (and their attached, attachments, if any) are marked for deletion in the toDelete Set.
     *
     * @param insured The insurance object containing the items to evaluate.
     * @param toDelete A Set to keep track of items marked for deletion.
     * @returns void
     */
    protected processRegularItems(insured: Insurance, toDelete: Set<string>): void;
    /**
     * Process parent items and their attachments, updating the toDelete Set accordingly.
     *
     * This method iterates over a map of parent items to their attachments and performs evaluations on each.
     * It marks items for deletion based on certain conditions and updates the toDelete Set accordingly.
     *
     * @param mainParentToAttachmentsMap A Map object containing parent item IDs to arrays of their attachment items.
     * @param itemsMap A Map object for quick item look-up by item ID.
     * @param traderId The trader ID from the Insurance object.
     * @param toDelete A Set object to keep track of items marked for deletion.
     */
    protected processAttachments(mainParentToAttachmentsMap: Map<string, Item[]>, itemsMap: Map<string, Item>, traderId: string, toDelete: Set<string>): void;
    /**
     * Takes an array of attachment items that belong to the same main-parent item, sorts them in descending order by
     * their maximum price. For each attachment, a roll is made to determine if a deletion should be made. Once the
     * number of deletions has been counted, the attachments are added to the toDelete Set, starting with the most
     * valuable attachments first.
     *
     * @param attachments The array of attachment items to sort, filter, and roll.
     * @param traderId The ID of the trader to that has ensured these items.
     * @param toDelete The array that accumulates the IDs of the items to be deleted.
     * @returns void
     */
    protected processAttachmentByParent(attachments: Item[], traderId: string, toDelete: Set<string>): void;
    /**
     * Sorts the attachment items by their max price in descending order.
     *
     * @param attachments The array of attachments items.
     * @returns An array of items enriched with their max price and common locale-name.
     */
    protected sortAttachmentsByPrice(attachments: Item[]): EnrichedItem[];
    /**
     * Logs the details of each attachment item.
     *
     * @param attachments The array of attachment items.
     */
    protected logAttachmentsDetails(attachments: EnrichedItem[]): void;
    /**
     * Counts the number of successful rolls for the attachment items.
     *
     * @param attachments The array of attachment items.
     * @param traderId The ID of the trader that has insured these attachments.
     * @returns The number of successful rolls.
     */
    protected countSuccessfulRolls(attachments: Item[], traderId: string): number;
    /**
     * Marks the most valuable attachments for deletion based on the number of successful rolls made.
     *
     * @param attachments The array of attachment items.
     * @param successfulRolls The number of successful rolls.
     * @param toDelete The array that accumulates the IDs of the items to be deleted.
     */
    protected attachmentDeletionByValue(attachments: EnrichedItem[], successfulRolls: number, toDelete: Set<string>): void;
    /**
     * Remove items from the insured items that should not be returned to the player.
     *
     * @param insured The insured items to process.
     * @param toDelete The items that should be deleted.
     * @returns void
     */
    protected removeItemsFromInsurance(insured: Insurance, toDelete: Set<string>): void;
    /**
     * Adopts orphaned items by resetting them as base-level items. Helpful in situations where a parent has been
     * deleted from insurance, but any insured items within the parent should remain. This method will remove the
     * reference from the children to the parent and set item properties to main-level values.
     *
     * @param insured Insurance object containing items.
     */
    protected adoptOrphanedItems(insured: Insurance): void;
    /**
     * Fetches the parentId property of an item with a slotId "hideout". Not sure if this is actually dynamic, but this
     * method should be a reliable way to fetch it, if it ever does change.
     *
     * @param items Array of items to search through.
     * @returns The parentId of an item with slotId 'hideout'. Empty string if not found.
     */
    protected fetchHideoutItemParent(items: Item[]): string;
    /**
     * Handle sending the insurance message to the user that potentially contains the valid insurance items.
     *
     * @param sessionID The session ID that should receive the insurance message.
     * @param insurance The context of insurance to use.
     * @param noItems Whether or not there are any items to return to the player.
     * @returns void
     */
    protected sendMail(sessionID: string, insurance: Insurance, noItems: boolean): void;
    /**
     * Determines whether a insured item should be removed from the player's inventory based on a random roll and
     * trader-specific return chance.
     *
     * @param traderId The ID of the trader who insured the item.
     * @param insuredItem Optional. The item to roll for. Only used for logging.
     * @returns true if the insured item should be removed from inventory, false otherwise.
     */
    protected rollForDelete(traderId: string, insuredItem?: Item): boolean;
    /**
     * Handle Insure event
     * Add insurance to an item
     *
     * @param pmcData Player profile
     * @param body Insurance request
     * @param sessionID Session id
     * @returns IItemEventRouterResponse object to send to client
     */
    insure(pmcData: IPmcData, body: IInsureRequestData, sessionID: string): IItemEventRouterResponse;
    /**
     * Handle client/insurance/items/list/cost
     * Calculate insurance cost
     *
     * @param request request object
     * @param sessionID session id
     * @returns IGetInsuranceCostResponseData object to send to client
     */
    cost(request: IGetInsuranceCostRequestData, sessionID: string): IGetInsuranceCostResponseData;
}
interface EnrichedItem extends Item {
    name: string;
    maxPrice: number;
}
export {};

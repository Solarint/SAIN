import { HandbookHelper } from "@spt-aki/helpers/HandbookHelper";
import { IPmcData } from "@spt-aki/models/eft/common/IPmcData";
import { InsuredItem } from "@spt-aki/models/eft/common/tables/IBotBase";
import { Item, Repairable } from "@spt-aki/models/eft/common/tables/IItem";
import { IStaticAmmoDetails } from "@spt-aki/models/eft/common/tables/ILootBase";
import { ITemplateItem } from "@spt-aki/models/eft/common/tables/ITemplateItem";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { ItemBaseClassService } from "@spt-aki/services/ItemBaseClassService";
import { ItemFilterService } from "@spt-aki/services/ItemFilterService";
import { LocaleService } from "@spt-aki/services/LocaleService";
import { LocalisationService } from "@spt-aki/services/LocalisationService";
import { HashUtil } from "@spt-aki/utils/HashUtil";
import { JsonUtil } from "@spt-aki/utils/JsonUtil";
import { MathUtil } from "@spt-aki/utils/MathUtil";
import { ObjectId } from "@spt-aki/utils/ObjectId";
import { RandomUtil } from "@spt-aki/utils/RandomUtil";
declare class ItemHelper {
    protected logger: ILogger;
    protected hashUtil: HashUtil;
    protected jsonUtil: JsonUtil;
    protected randomUtil: RandomUtil;
    protected objectId: ObjectId;
    protected mathUtil: MathUtil;
    protected databaseServer: DatabaseServer;
    protected handbookHelper: HandbookHelper;
    protected itemBaseClassService: ItemBaseClassService;
    protected itemFilterService: ItemFilterService;
    protected localisationService: LocalisationService;
    protected localeService: LocaleService;
    protected readonly defaultInvalidBaseTypes: string[];
    constructor(logger: ILogger, hashUtil: HashUtil, jsonUtil: JsonUtil, randomUtil: RandomUtil, objectId: ObjectId, mathUtil: MathUtil, databaseServer: DatabaseServer, handbookHelper: HandbookHelper, itemBaseClassService: ItemBaseClassService, itemFilterService: ItemFilterService, localisationService: LocalisationService, localeService: LocaleService);
    /**
     * Checks if an id is a valid item. Valid meaning that it's an item that be stored in stash
     * @param       {string}    tpl       the template id / tpl
     * @returns                             boolean; true for items that may be in player possession and not quest items
     */
    isValidItem(tpl: string, invalidBaseTypes?: string[]): boolean;
    /**
     * Check if the tpl / template Id provided is a descendent of the baseclass
     *
     * @param   {string}    tpl             the item template id to check
     * @param   {string}    baseClassTpl    the baseclass to check for
     * @return  {boolean}                   is the tpl a descendent?
     */
    isOfBaseclass(tpl: string, baseClassTpl: string): boolean;
    /**
     * Check if item has any of the supplied base classes
     * @param tpl Item to check base classes of
     * @param baseClassTpls base classes to check for
     * @returns true if any supplied base classes match
     */
    isOfBaseclasses(tpl: string, baseClassTpls: string[]): boolean;
    /**
     * Returns the item price based on the handbook or as a fallback from the prices.json if the item is not
     * found in the handbook. If the price can't be found at all return 0
     * @param tpl Item to look price up of
     * @returns Price in roubles
     */
    getItemPrice(tpl: string): number;
    /**
     * Returns the item price based on the handbook or as a fallback from the prices.json if the item is not
     * found in the handbook. If the price can't be found at all return 0
     * @param tpl Item to look price up of
     * @returns Price in roubles
     */
    getItemMaxPrice(tpl: string): number;
    /**
     * Get the static (handbook) price in roubles for an item by tpl
     * @param tpl Items tpl id to look up price
     * @returns Price in roubles (0 if not found)
     */
    getStaticItemPrice(tpl: string): number;
    /**
     * Get the dynamic (flea) price in roubles for an item by tpl
     * @param tpl Items tpl id to look up price
     * @returns Price in roubles (undefined if not found)
     */
    getDynamicItemPrice(tpl: string): number;
    /**
     * Update items upd.StackObjectsCount to be 1 if its upd is missing or StackObjectsCount is undefined
     * @param item Item to update
     * @returns Fixed item
     */
    fixItemStackCount(item: Item): Item;
    /**
     * AmmoBoxes contain StackSlots which need to be filled for the AmmoBox to have content.
     * Here's what a filled AmmoBox looks like:
     *   {
     *       "_id": "b1bbe982daa00ac841d4ae4d",
     *       "_tpl": "57372c89245977685d4159b1",
     *       "parentId": "5fe49a0e2694b0755a504876",
     *       "slotId": "hideout",
     *       "location": {
     *           "x": 3,
     *           "y": 4,
     *           "r": 0
     *       },
     *       "upd": {
     *           "StackObjectsCount": 1
     *       }
     *   },
     *   {
     *       "_id": "b997b4117199033afd274a06",
     *       "_tpl": "56dff061d2720bb5668b4567",
     *       "parentId": "b1bbe982daa00ac841d4ae4d",
     *       "slotId": "cartridges",
     *       "location": 0,
     *       "upd": {
     *           "StackObjectsCount": 30
     *       }
     *   }
     * Given the AmmoBox Item (first object) this function generates the StackSlot (second object) and returns it.
     * StackSlots are only used for AmmoBoxes which only have one element in StackSlots. However, it seems to be generic
     * to possibly also have more than one StackSlot. As good as possible, without seeing items having more than one
     * StackSlot, this function takes account of this and creates and returns an array of StackSlotItems
     *
     * @param {object}      item            The item template of the AmmoBox as given in items.json
     * @param {string}      parentId        The id of the AmmoBox instance these StackSlotItems should be children of
     * @returns {array}                     The array of StackSlotItems
     */
    generateItemsFromStackSlot(item: ITemplateItem, parentId: string): Item[];
    /**
     * Get cloned copy of all item data from items.json
     * @returns array of ITemplateItem objects
     */
    getItems(): ITemplateItem[];
    /**
     * Gets item data from items.json
     * @param tpl items template id to look up
     * @returns bool - is valid + template item object as array
     */
    getItem(tpl: string): [boolean, ITemplateItem];
    isItemInDb(tpl: string): boolean;
    /**
     * get normalized value (0-1) based on item condition
     * @param item
     * @returns number between 0 and 1
     */
    getItemQualityModifier(item: Item): number;
    /**
     * Get a quality value based on a repairable items (weapon/armor) current state between current and max durability
     * @param itemDetails Db details for item we want quality value for
     * @param repairable Repairable properties
     * @param item Item quality value is for
     * @returns A number between 0 and 1
     */
    protected getRepairableItemQualityValue(itemDetails: ITemplateItem, repairable: Repairable, item: Item): number;
    /**
     * Recursive function that looks at every item from parameter and gets their childrens Ids + includes parent item in results
     * @param items Array of items (item + possible children)
     * @param itemId Parent items id
     * @returns an array of strings
     */
    findAndReturnChildrenByItems(items: Item[], itemId: string): string[];
    /**
     * A variant of findAndReturnChildren where the output is list of item objects instead of their ids.
     * @param items
     * @param baseItemId
     * @returns An array of Item objects
     */
    findAndReturnChildrenAsItems(items: Item[], baseItemId: string): Item[];
    /**
     * Find children of the item in a given assort (weapons parts for example, need recursive loop function)
     * @param itemIdToFind Template id of item to check for
     * @param assort Array of items to check in
     * @returns Array of children of requested item
     */
    findAndReturnChildrenByAssort(itemIdToFind: string, assort: Item[]): Item[];
    /**
     * Check if the passed in item has buy count restrictions
     * @param itemToCheck Item to check
     * @returns true if it has buy restrictions
     */
    hasBuyRestrictions(itemToCheck: Item): boolean;
    /**
     * is the passed in template id a dog tag
     * @param tpl Template id to check
     * @returns true if it is a dogtag
     */
    isDogtag(tpl: string): boolean;
    /**
     * Gets the identifier for a child using slotId, locationX and locationY.
     * @param item
     * @returns "slotId OR slotid,locationX,locationY"
     */
    getChildId(item: Item): string;
    /**
     * Can the passed in item be stacked
     * @param tpl item to check
     * @returns true if it can be stacked
     */
    isItemTplStackable(tpl: string): boolean;
    /**
     * split item stack if it exceeds its items StackMaxSize property
     * @param itemToSplit Item to split into smaller stacks
     * @returns Array of split items
     */
    splitStack(itemToSplit: Item): Item[];
    /**
     * Find Barter items from array of items
     * @param {string} by tpl or id
     * @param {Item[]} items Array of items to iterate over
     * @param {string} barterItemId
     * @returns Array of Item objects
     */
    findBarterItems(by: "tpl" | "id", items: Item[], barterItemId: string): Item[];
    /**
     * Regenerate all guids with new ids, exceptions are for items that cannot be altered (e.g. stash/sorting table)
     * @param pmcData Player profile
     * @param items Items to adjust ID values of
     * @param insuredItems insured items to not replace ids for
     * @param fastPanel
     * @returns Item[]
     */
    replaceIDs(pmcData: IPmcData, items: Item[], insuredItems?: InsuredItem[], fastPanel?: any): Item[];
    /**
     * WARNING, SLOW. Recursively loop down through an items hierarchy to see if any of the ids match the supplied list, return true if any do
     * @param {string} tpl Items tpl to check parents of
     * @param {Array} tplsToCheck Tpl values to check if parents of item match
     * @returns boolean Match found
     */
    doesItemOrParentsIdMatch(tpl: string, tplsToCheck: string[]): boolean;
    /**
     * Check if item is quest item
     * @param tpl Items tpl to check quest status of
     * @returns true if item is flagged as quest item
     */
    isQuestItem(tpl: string): boolean;
    /**
     * Checks to see if the item is *actually* moddable in-raid. Checks include the items existence in the database, the
     * parent items existence in the database, the existence (and value) of the items RaidModdable property, and that
     * the parents slot-required property exists, matches that of the item, and it's value.
     *
     * Note: this function does not preform any checks to see if the item and parent are *actually* related.
     *
     * @param item The item to be checked
     * @param parent The parent of the item to be checked
     * @returns True if the item is actually moddable, false if it is not, and null if the check cannot be performed.
     */
    isRaidModdable(item: Item, parent: Item): boolean | null;
    /**
     * Retrieves the main parent item for a given attachment item.
     *
     * This method traverses up the hierarchy of items starting from a given `itemId`, until it finds the main parent
     * item that is not an attached attachment itself. In other words, if you pass it an item id of a suppressor, it
     * will traverse up the muzzle brake, barrel, upper receiver, and return the gun that the suppressor is ultimately
     * attached to, even if that gun is located within multiple containers.
     *
     * It's important to note that traversal is expensive, so this method requires that you pass it a Map of the items
     * to traverse, where the keys are the item IDs and the values are the corresponding Item objects. This alleviates
     * some of the performance concerns, as it allows for quick lookups of items by ID.
     *
     * To generate the map:
     * ```
     * const itemsMap = new Map<string, Item>();
     * items.forEach(item => itemsMap.set(item._id, item));
     * ```
     *
     * @param itemId - The unique identifier of the item for which to find the main parent.
     * @param itemsMap - A Map containing item IDs mapped to their corresponding Item objects for quick lookup.
     * @returns The Item object representing the top-most parent of the given item, or `null` if no such parent exists.
     */
    getAttachmentMainParent(itemId: string, itemsMap: Map<string, Item>): Item | null;
    /**
     * Determines if an item is an attachment that is currently attached to it's parent item.
     *
     * @param item The item to check.
     * @returns true if the item is attached attachment, otherwise false.
     */
    isAttachmentAttached(item: Item): boolean;
    /**
     * Get the inventory size of an item
     * @param items Item with children
     * @param rootItemId
     * @returns ItemSize object (width and height)
     */
    getItemSize(items: Item[], rootItemId: string): ItemHelper.ItemSize;
    /**
     * Get a random cartridge from an items Filter property
     * @param item Db item template to look up Cartridge filter values from
     * @returns Caliber of cartridge
     */
    getRandomCompatibleCaliberTemplateId(item: ITemplateItem): string;
    /**
     * Add cartridges to the ammo box with correct max stack sizes
     * @param ammoBox Box to add cartridges to
     * @param ammoBoxDetails Item template from items db
     */
    addCartridgesToAmmoBox(ammoBox: Item[], ammoBoxDetails: ITemplateItem): void;
    /**
     * Check if item is stored inside of a container
     * @param item Item to check is inside of container
     * @param desiredContainerSlotId Name of slot to check item is in e.g. SecuredContainer/Backpack
     * @param items Inventory with child parent items to check
     * @returns True when item is in container
     */
    itemIsInsideContainer(item: Item, desiredContainerSlotId: string, items: Item[]): boolean;
    /**
     * Add child items (cartridges) to a magazine
     * @param magazine Magazine to add child items to
     * @param magTemplate Db template of magazine
     * @param staticAmmoDist Cartridge distribution
     * @param caliber Caliber of cartridge to add to magazine
     * @param minSizePercent % the magazine must be filled to
     */
    fillMagazineWithRandomCartridge(magazine: Item[], magTemplate: ITemplateItem, staticAmmoDist: Record<string, IStaticAmmoDetails[]>, caliber?: string, minSizePercent?: number): void;
    /**
     * Add child items to a magazine of a specific cartridge
     * @param magazine Magazine to add child items to
     * @param magTemplate Db template of magazine
     * @param cartridgeTpl Cartridge to add to magazine
     * @param minSizePercent % the magazine must be filled to
     */
    fillMagazineWithCartridge(magazine: Item[], magTemplate: ITemplateItem, cartridgeTpl: string, minSizePercent?: number): void;
    /**
     * Choose a random bullet type from the list of possible a magazine has
     * @param magTemplate Magazine template from Db
     * @returns Tpl of cartridge
     */
    protected getRandomValidCaliber(magTemplate: ITemplateItem): string;
    /**
     * Chose a randomly weighted cartridge that fits
     * @param caliber Desired caliber
     * @param staticAmmoDist Cartridges and thier weights
     * @returns Tpl of cartridge
     */
    protected drawAmmoTpl(caliber: string, staticAmmoDist: Record<string, IStaticAmmoDetails[]>): string;
    /**
     * Create a basic cartrige object
     * @param parentId container cartridges will be placed in
     * @param ammoTpl Cartridge to insert
     * @param stackCount Count of cartridges inside parent
     * @param location Location inside parent (e.g. 0, 1)
     * @returns Item
     */
    createCartridges(parentId: string, ammoTpl: string, stackCount: number, location: number): Item;
    /**
     * Get the size of a stack, return 1 if no stack object count property found
     * @param item Item to get stack size of
     * @returns size of stack
     */
    getItemStackSize(item: Item): number;
    /**
     * Get the name of an item from the locale file using the item tpl
     * @param itemTpl Tpl of item to get name of
     * @returns Name of item
     */
    getItemName(itemTpl: string): string;
    getItemTplsOfBaseType(desiredBaseType: string): string[];
}
declare namespace ItemHelper {
    interface ItemSize {
        width: number;
        height: number;
    }
}
export { ItemHelper };

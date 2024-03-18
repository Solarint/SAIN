import { ContainerHelper } from "@spt-aki/helpers/ContainerHelper";
import { InventoryHelper } from "@spt-aki/helpers/InventoryHelper";
import { ItemHelper } from "@spt-aki/helpers/ItemHelper";
import { WeightedRandomHelper } from "@spt-aki/helpers/WeightedRandomHelper";
import { Inventory } from "@spt-aki/models/eft/common/tables/IBotBase";
import { GenerationData } from "@spt-aki/models/eft/common/tables/IBotType";
import { Item } from "@spt-aki/models/eft/common/tables/IItem";
import { Grid, ITemplateItem } from "@spt-aki/models/eft/common/tables/ITemplateItem";
import { EquipmentSlots } from "@spt-aki/models/enums/EquipmentSlots";
import { ItemAddedResult } from "@spt-aki/models/enums/ItemAddedResult";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { LocalisationService } from "@spt-aki/services/LocalisationService";
import { HashUtil } from "@spt-aki/utils/HashUtil";
import { RandomUtil } from "@spt-aki/utils/RandomUtil";
export declare class BotWeaponGeneratorHelper {
    protected logger: ILogger;
    protected databaseServer: DatabaseServer;
    protected itemHelper: ItemHelper;
    protected randomUtil: RandomUtil;
    protected hashUtil: HashUtil;
    protected inventoryHelper: InventoryHelper;
    protected weightedRandomHelper: WeightedRandomHelper;
    protected localisationService: LocalisationService;
    protected containerHelper: ContainerHelper;
    constructor(logger: ILogger, databaseServer: DatabaseServer, itemHelper: ItemHelper, randomUtil: RandomUtil, hashUtil: HashUtil, inventoryHelper: InventoryHelper, weightedRandomHelper: WeightedRandomHelper, localisationService: LocalisationService, containerHelper: ContainerHelper);
    /**
     * Get a randomized number of bullets for a specific magazine
     * @param magCounts Weights of magazines
     * @param magTemplate magazine to generate bullet count for
     * @returns bullet count number
     */
    getRandomizedBulletCount(magCounts: GenerationData, magTemplate: ITemplateItem): number;
    /**
     * Get a randomized count of magazines
     * @param magCounts min and max value returned value can be between
     * @returns numerical value of magazine count
     */
    getRandomizedMagazineCount(magCounts: GenerationData): number;
    /**
     * Is this magazine cylinder related (revolvers and grenade launchers)
     * @param magazineParentName the name of the magazines parent
     * @returns true if it is cylinder related
     */
    magazineIsCylinderRelated(magazineParentName: string): boolean;
    /**
     * Create a magazine using the parameters given
     * @param magazineTpl Tpl of the magazine to create
     * @param ammoTpl Ammo to add to magazine
     * @param magTemplate template object of magazine
     * @returns Item array
     */
    createMagazineWithAmmo(magazineTpl: string, ammoTpl: string, magTemplate: ITemplateItem): Item[];
    /**
     * Add a specific number of cartridges to a bots inventory (defaults to vest and pockets)
     * @param ammoTpl Ammo tpl to add to vest/pockets
     * @param cartridgeCount number of cartridges to add to vest/pockets
     * @param inventory bot inventory to add cartridges to
     * @param equipmentSlotsToAddTo what equipment slots should bullets be added into
     */
    addAmmoIntoEquipmentSlots(ammoTpl: string, cartridgeCount: number, inventory: Inventory, equipmentSlotsToAddTo?: EquipmentSlots[]): void;
    /**
     * Get a weapons default magazine template id
     * @param weaponTemplate weapon to get default magazine for
     * @returns tpl of magazine
     */
    getWeaponsDefaultMagazineTpl(weaponTemplate: ITemplateItem): string;
    /**
     * TODO - move into BotGeneratorHelper, this is not the class for it
     * Adds an item with all its children into specified equipmentSlots, wherever it fits.
     * @param equipmentSlots Slot to add item+children into
     * @param parentId
     * @param parentTpl
     * @param itemWithChildren Item to add
     * @param inventory Inventory to add item+children into
     * @returns a `boolean` indicating item was added
     */
    addItemWithChildrenToEquipmentSlot(equipmentSlots: string[], parentId: string, parentTpl: string, itemWithChildren: Item[], inventory: Inventory): ItemAddedResult;
    /**
     * is the provided item allowed inside a container
     * @param slot location item wants to be placed in
     * @param itemTpl item being placed
     * @returns true if allowed
     */
    protected itemAllowedInContainer(slot: Grid, itemTpl: string): boolean;
}

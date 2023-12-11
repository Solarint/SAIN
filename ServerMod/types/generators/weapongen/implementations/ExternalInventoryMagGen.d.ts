import { IInventoryMagGen } from "@spt-aki/generators/weapongen/IInventoryMagGen";
import { InventoryMagGen } from "@spt-aki/generators/weapongen/InventoryMagGen";
import { BotWeaponGeneratorHelper } from "@spt-aki/helpers/BotWeaponGeneratorHelper";
import { ItemHelper } from "@spt-aki/helpers/ItemHelper";
import { ITemplateItem } from "@spt-aki/models/eft/common/tables/ITemplateItem";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { LocalisationService } from "@spt-aki/services/LocalisationService";
import { RandomUtil } from "@spt-aki/utils/RandomUtil";
export declare class ExternalInventoryMagGen implements IInventoryMagGen {
    protected logger: ILogger;
    protected itemHelper: ItemHelper;
    protected localisationService: LocalisationService;
    protected botWeaponGeneratorHelper: BotWeaponGeneratorHelper;
    protected randomUtil: RandomUtil;
    constructor(logger: ILogger, itemHelper: ItemHelper, localisationService: LocalisationService, botWeaponGeneratorHelper: BotWeaponGeneratorHelper, randomUtil: RandomUtil);
    getPriority(): number;
    canHandleInventoryMagGen(inventoryMagGen: InventoryMagGen): boolean;
    process(inventoryMagGen: InventoryMagGen): void;
    /**
     * Get a random compatible external magazine for a weapon, excluses internal magazines from possible pool
     * @param weaponTpl Weapon to get mag for
     * @returns tpl of magazine
     */
    protected getRandomExternalMagazineForInternalMagazineGun(weaponTpl: string, magazineBlacklist: string[]): ITemplateItem;
}

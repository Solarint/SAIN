import { ItemHelper } from "@spt-aki/helpers/ItemHelper";
import { Item } from "@spt-aki/models/eft/common/tables/IItem";
export interface OwnerInventoryItems {
    from: Item[];
    to: Item[];
    sameInventory: boolean;
    isMail: boolean;
}
export declare class SecureContainerHelper {
    protected itemHelper: ItemHelper;
    constructor(itemHelper: ItemHelper);
    getSecureContainerItems(items: Item[]): string[];
}

import { OwnerInfo } from "@spt-aki/models/eft/common/request/IBaseInteractionRequestData";
export interface ISellScavItemsToFenceRequestData {
    Action: "SellAllFromSavage";
    fromOwner: OwnerInfo;
    toOwner: OwnerInfo;
}

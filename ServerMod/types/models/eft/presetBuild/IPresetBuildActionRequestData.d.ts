import { Item } from "@spt-aki/models/eft/common/tables/IItem";
export interface IPresetBuildActionRequestData {
    Action: string;
    id: string;
    name: string;
    root: string;
    items: Item[];
}

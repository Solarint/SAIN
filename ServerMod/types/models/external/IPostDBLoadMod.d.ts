import { DependencyContainer } from "@spt-aki/models/external/tsyringe";
export interface IPostDBLoadMod {
    postDBLoad(container: DependencyContainer): void;
}

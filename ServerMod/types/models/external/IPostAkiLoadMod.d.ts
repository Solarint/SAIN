import { DependencyContainer } from "@spt-aki/models/external/tsyringe";
export interface IPostAkiLoadMod {
    postAkiLoad(container: DependencyContainer): void;
}

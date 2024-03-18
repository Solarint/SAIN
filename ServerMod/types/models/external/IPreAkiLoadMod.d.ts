import { DependencyContainer } from "@spt-aki/models/external/tsyringe";
export interface IPreAkiLoadMod {
    preAkiLoad(container: DependencyContainer): void;
}

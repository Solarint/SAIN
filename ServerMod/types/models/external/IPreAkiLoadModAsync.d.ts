import { DependencyContainer } from "@spt-aki/models/external/tsyringe";
export interface IPreAkiLoadModAsync {
    preAkiLoadAsync(container: DependencyContainer): Promise<void>;
}

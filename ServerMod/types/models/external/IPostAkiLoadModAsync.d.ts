import { DependencyContainer } from "@spt-aki/models/external/tsyringe";
export interface IPostAkiLoadModAsync {
    postAkiLoadAsync(container: DependencyContainer): Promise<void>;
}
